using System.Net.Http.Json;
using Blogify.Api.Controllers.Users;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Data;
using Blogify.Application.Users.LogInUser;
using Blogify.FunctionalTests.Users;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Blogify.FunctionalTests.Infrastructure;

[CollectionDefinition("FunctionalTests")]
public class FunctionalTestsCollection : ICollectionFixture<FunctionalTestWebAppFactory>
{
}

[Collection("FunctionalTests")]
public abstract class BaseFunctionalTest(FunctionalTestWebAppFactory factory)
{
    protected readonly HttpClient HttpClient = factory.CreateClient();

    protected readonly ISqlConnectionFactory SqlConnectionFactory =
        factory.Services.GetRequiredService<ISqlConnectionFactory>();

    protected Guid AuthenticatedUserId { get; private set; }

    /// <summary>
    ///     Clears cached query results (and tag sets) so a test never sees data cached by an earlier test.
    ///     The Data Protection key ring lives in the same Redis and must survive, or pending outbox passwords
    ///     could no longer be decrypted.
    /// </summary>
    protected async Task ResetCacheAsync()
    {
        const string dataProtectionKey = "Blogify:DataProtection:Keys";
        var redis = factory.Services.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>();
        var database = redis.GetDatabase();

        foreach (var endpoint in redis.GetEndPoints())
        {
            var server = redis.GetServer(endpoint);
            if (server.IsReplica) continue;

            await foreach (var key in server.KeysAsync(pattern: "*"))
                if (key != dataProtectionKey)
                    await database.KeyDeleteAsync(key);
        }
    }

    protected async Task WaitUntilUserActiveAsync(string email)
    {
        var timeout = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < timeout)
        {
            using var connection = SqlConnectionFactory.CreateConnection();
            var status = await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT status FROM users WHERE email = @Email",
                new { Email = email });

            // A NULL status means the user row was never written: fail fast.
            if (status == null)
            {
                throw new Exception($"CRITICAL ERROR: User {email} DOES NOT EXIST in the database! Registration completely failed.");
            }

            if (status == 1) return; // UserStatus.Active
            await Task.Delay(150);
        }
        throw new TimeoutException($"User {email} was found, but remained 'Pending' and was not activated in time by Outbox processor.");
    }

    /// <summary>Registers (once) and logs in a plain Registered user with no administrator role.</summary>
    protected async Task<(string Token, Guid UserId)> GetRegularUserAccessToken()
    {
        var request = UserData.RegularUserRequest;
        var registerResponse = await HttpClient.PostAsJsonAsync("api/v1/users/register", request);
        if (registerResponse.StatusCode != System.Net.HttpStatusCode.Conflict)
            registerResponse.EnsureSuccessStatusCode();

        await WaitUntilUserActiveAsync(request.Email);

        var loginResponse = await HttpClient.PostAsJsonAsync(
            "api/v1/users/login", new LogInUserRequest(request.Email, request.Password));
        loginResponse.EnsureSuccessStatusCode();
        var token = (await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>())!.AccessToken;

        using var connection = SqlConnectionFactory.CreateConnection();
        var userId = await connection.QuerySingleAsync<Guid>(
            "SELECT id FROM users WHERE email = @Email", new { request.Email });
        return (token, userId);
    }

    protected async Task<string> GetAccessToken()
    {
        await WaitUntilUserActiveAsync(UserData.RegisterTestUserRequest.Email);

        var loginResponse = await HttpClient.PostAsJsonAsync(
            "api/v1/users/login",
            new LogInUserRequest(
                UserData.RegisterTestUserRequest.Email,
                UserData.RegisterTestUserRequest.Password));

        loginResponse.EnsureSuccessStatusCode();

        var accessTokenResponse = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        var accessToken = accessTokenResponse!.AccessToken;

        using var connection = SqlConnectionFactory.CreateConnection();
        const string sql = "SELECT id FROM users WHERE email = @Email";
        var userId = await connection.QuerySingleOrDefaultAsync<Guid>(
            sql,
            new { UserData.RegisterTestUserRequest.Email });

        if (userId == Guid.Empty)
            throw new InvalidOperationException("Could not find test user in the database after login.");

        AuthenticatedUserId = userId;

        // Taxonomy writes are administrator-only: promote the shared test user and evict its cached roles.
        const int administratorRoleId = 2;
        await connection.ExecuteAsync(
            "INSERT INTO role_user (roles_id, users_id) VALUES (@RoleId, @UserId) ON CONFLICT DO NOTHING",
            new { RoleId = administratorRoleId, UserId = userId });

        var identityId = await connection.QuerySingleAsync<string>(
            "SELECT identity_id FROM users WHERE id = @UserId", new { UserId = userId });
        using var scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ICacheService>().RemoveAsync($"auth:roles-{identityId}");

        return accessToken;
    }
}