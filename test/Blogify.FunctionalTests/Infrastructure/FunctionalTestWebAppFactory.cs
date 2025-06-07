using System.Net.Http.Json;
using Blogify.Api;
using Blogify.Application.Abstractions.Data;
using Blogify.FunctionalTests.Users;
using Blogify.Infrastructure;
using Blogify.Infrastructure.Authentication;
using Blogify.Infrastructure.Data;
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
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("blogify")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder()
        .WithImage("quay.io/keycloak/keycloak:latest")
        .WithResourceMapping(
            new FileInfo(".files/blogify-realm-export.json"),
            new FileInfo("/opt/keycloak/data/import/realm.json"))
        .WithCommand("--import-realm")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _keycloakContainer.StartAsync();

        await InitializeTestUserAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
        await _keycloakContainer.StopAsync();
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
        });
    }

    private async Task InitializeTestUserAsync()
    {
        try
        {
            using var httpClient = CreateClient();

            await httpClient.PostAsJsonAsync("api/v1/users/register", UserData.RegisterTestUserRequest);
        }
        catch
        {
            // Do nothing.
        }
    }
}