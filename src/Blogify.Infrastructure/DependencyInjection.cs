using Microsoft.Extensions.Http.Resilience;
using Asp.Versioning;
using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Clock;
using Blogify.Application.Abstractions.Cryptography;
using Blogify.Application.Abstractions.Data;
using Blogify.Application.Abstractions.Email;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using Blogify.Domain.Users;
using Blogify.Infrastructure.Authentication;
using Blogify.Infrastructure.Authorization;
using Blogify.Infrastructure.Caching;
using Blogify.Infrastructure.Clock;
using Blogify.Infrastructure.Cryptography;
using Blogify.Infrastructure.Data;
using Blogify.Infrastructure.Email;
using Blogify.Infrastructure.Interceptors;
using Blogify.Infrastructure.Outbox;
using Blogify.Infrastructure.Repositories;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using StackExchange.Redis;
using AuthenticationOptions = Blogify.Infrastructure.Authentication.AuthenticationOptions;
using AuthenticationService = Blogify.Infrastructure.Authentication.AuthenticationService;
using IAuthenticationService = Blogify.Application.Abstractions.Authentication.IAuthenticationService;

namespace Blogify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IEmailService, EmailService>();


        AddPersistence(services, configuration);

        AddCaching(services, configuration);

        AddDataProtection(services);

        AddAuthentication(services, configuration);

        AddAuthorization(services);

        AddHealthChecks(services, configuration);

        AddApiVersioning(services);

        AddBackgroundJobs(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") ??
                               throw new ArgumentNullException(nameof(configuration));

        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseNpgsql(connectionString)
                .AddInterceptors(sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IPostRepository, PostRepository>();

        services.AddScoped<ICommentRepository, CommentRepository>();

        services.AddScoped<ITagRepository, TagRepository>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<ISqlConnectionFactory>(_ =>
            new SqlConnectionFactory(connectionString));

        services.AddTransient<IDapperQueryExecutor, DapperQueryExecutor>();

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));

        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));

        services.AddMemoryCache();
        services.AddTransient<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient<IAuthenticationService, AuthenticationService>((serviceProvider, httpClient) =>
            {
                var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

                httpClient.BaseAddress = new Uri(keycloakOptions.AdminUrl);
            })
            .AddHttpMessageHandler<AdminAuthorizationDelegatingHandler>()
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(20);
                // Never auto-retry user creation/deletion: a retried POST could create a duplicate identity.
                options.Retry.DisableForUnsafeHttpMethods();
            });

        services.AddHttpClient<IJwtService, JwtService>()
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(20);
                // Password-grant logins are not retried: a 401 is a real answer and a replay would hammer lockouts.
                options.Retry.DisableForUnsafeHttpMethods();
            });

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddScoped<AuthorizationService>();

        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Cache") ??
                               throw new ArgumentNullException(nameof(configuration));

        services.AddStackExchangeRedisCache(options => options.Configuration = connectionString);

        // One multiplexer per process, shared by the distributed cache and the Data Protection key ring.
        // Resolved lazily from RedisCacheOptions so configuration overrides (e.g. in tests) apply.
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            ConnectionMultiplexer.Connect(
                serviceProvider.GetRequiredService<IOptions<RedisCacheOptions>>().Value.Configuration!));

        services.AddOptions<RedisCacheOptions>()
            .Configure<IServiceProvider>((options, serviceProvider) =>
                options.ConnectionMultiplexerFactory = () =>
                    Task.FromResult(serviceProvider.GetRequiredService<IConnectionMultiplexer>()));

        services.AddSingleton<ICacheService, CacheService>();
    }

    private static void AddDataProtection(IServiceCollection services)
    {
        // Persist keys in Redis so protected payloads (e.g. pending registration passwords in the
        // outbox) survive restarts and can be unprotected by any instance.
        services.AddDataProtection()
            .SetApplicationName("Blogify");

        services.AddOptions<KeyManagementOptions>()
            .Configure<IConnectionMultiplexer>((options, multiplexer) =>
                options.XmlRepository = new RedisXmlRepository(
                    () => multiplexer.GetDatabase(),
                    "Blogify:DataProtection:Keys"));

        services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
    }

    private static void AddHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!, tags: ["ready"])
            .AddRedis(configuration.GetConnectionString("Cache")!, tags: ["ready"])
            .AddUrlGroup(new Uri(configuration["Keycloak:BaseUrl"]!), HttpMethod.Get, "keycloak", tags: ["ready"]);
    }

    private static void AddApiVersioning(IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));

        services.AddQuartz(quartz =>
        {
            const string jobName = nameof(ProcessOutboxMessagesJob);

            quartz.AddJob<ProcessOutboxMessagesJob>(job => job.WithIdentity(jobName));

            // Resolve the interval lazily so IOptions<OutboxOptions> overrides (e.g. in tests) are honored.
            quartz.AddTrigger<ProcessOutboxMessagesJob>((serviceProvider, trigger) =>
            {
                var outboxOptions = serviceProvider.GetRequiredService<IOptions<OutboxOptions>>().Value;

                trigger
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithInterval(TimeSpan.FromSeconds(outboxOptions.IntervalInSeconds)).RepeatForever());
            });

            quartz.AddJob<OutboxCleanupJob>(job => job.WithIdentity(nameof(OutboxCleanupJob)));
            quartz.AddTrigger<OutboxCleanupJob>((_, trigger) => trigger
                .ForJob(nameof(OutboxCleanupJob))
                .WithSimpleSchedule(schedule => schedule.WithInterval(TimeSpan.FromHours(1)).RepeatForever()));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        // Outbox data access abstraction (enables unit tests without a real DB)
        services.AddTransient<IOutboxDataAccess, DapperOutboxDataAccess>();
    }
}