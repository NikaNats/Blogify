using System.Net.Http.Json;
using Blogify.Api;
using Blogify.Application.Abstractions.Data;
using Blogify.FunctionalTests.Users;
using Blogify.Infrastructure;
using Blogify.Infrastructure.Authentication;
using Blogify.Infrastructure.Data;
using Blogify.Infrastructure.Outbox;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Blogify.FunctionalTests.Infrastructure;

public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static string GetRealmExportPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var directory = new DirectoryInfo(baseDir);

        while (directory != null)
        {
            var targetFile = Path.Combine(directory.FullName, "test", "Blogify.FunctionalTests", ".files", "blogify-realm-export.json");
            if (File.Exists(targetFile)) return targetFile;

            var localFile = Path.Combine(directory.FullName, ".files", "blogify-realm-export.json");
            if (File.Exists(localFile)) return localFile;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find 'blogify-realm-export.json' in any parent directory structures.");
    }

    // Keep image versions in sync with docker-compose/docker-compose.yml.
    private const string PostgresImage = "postgres:18.7";
    private const string RedisImage = "redis:8.10";
    // Must match the Keycloak version the realm export was produced with.
    private const string KeycloakImage = "quay.io/keycloak/keycloak:26.0.7";

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(PostgresImage)
        .WithDatabase("blogify")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly KeycloakContainer _keycloakContainer;

    private readonly RedisContainer _redisContainer = new RedisBuilder(RedisImage)
        .Build();

    public FunctionalTestWebAppFactory()
    {
        _keycloakContainer = new KeycloakBuilder(KeycloakImage)
            .WithResourceMapping(
                new FileInfo(GetRealmExportPath()),
                new FileInfo("/opt/keycloak/data/import/realm.json"))
            .WithCommand("--import-realm")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _keycloakContainer.StartAsync());

        await InitializeTestUserAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _keycloakContainer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            var connectionString = $"{_dbContainer.GetConnectionString()};Pooling=False";

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(connectionString)
                    .UseSnakeCaseNamingConvention());

            services.RemoveAll(typeof(ISqlConnectionFactory));

            services.AddSingleton<ISqlConnectionFactory>(_ =>
                new SqlConnectionFactory(connectionString));

            services.Configure<RedisCacheOptions>(redisCacheOptions =>
                redisCacheOptions.Configuration = _redisContainer.GetConnectionString());

            var keycloakAddress = _keycloakContainer.GetBaseAddress();

            services.Configure<KeycloakOptions>(o =>
            {
                o.AdminUrl = $"{keycloakAddress}admin/realms/Blogify/";
                o.TokenUrl = $"{keycloakAddress}realms/Blogify/protocol/openid-connect/token";
            });

            services.Configure<AuthenticationOptions>(o =>
            {
                o.Issuer = $"{keycloakAddress}realms/Blogify/";
                o.MetadataUrl = $"{keycloakAddress}realms/Blogify/.well-known/openid-configuration";
            });

            services.RemoveAll(typeof(Microsoft.Extensions.Options.IOptions<OutboxOptions>));
            services.AddSingleton<Microsoft.Extensions.Options.IOptions<OutboxOptions>>(_ =>
                new Microsoft.Extensions.Options.OptionsWrapper<OutboxOptions>(new OutboxOptions
                {
                    IntervalInSeconds = 1,
                    BatchSize = 10
                }));
        });
    }

    private async Task InitializeTestUserAsync()
    {
        using var httpClient = CreateClient();
        var response = await httpClient.PostAsJsonAsync("api/v1/users/register", UserData.RegisterTestUserRequest);

        // Fail fast: a failed seed registration would otherwise surface as confusing test failures later.
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"CRITICAL ERROR: InitializeTestUserAsync failed! Status: {response.StatusCode}, Details: {errorContent}");
        }
    }
}