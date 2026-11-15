using System.Net.Http.Json;
using Blogify.Api.Controllers.Users;
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

        return accessToken;
    }
}