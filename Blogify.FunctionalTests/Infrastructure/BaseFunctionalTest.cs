using System.Net.Http.Json;
using Blogify.Api.Controllers.Users;
using Blogify.Application.Abstractions.Data;
using Blogify.Application.Users.LogInUser;
using Blogify.FunctionalTests.Users;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace Blogify.FunctionalTests.Infrastructure;

public abstract class BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient = factory.CreateClient();

    protected readonly ISqlConnectionFactory SqlConnectionFactory =
        factory.Services.GetRequiredService<ISqlConnectionFactory>();

    protected Guid AuthenticatedUserId { get; private set; }

    // Get the factory for direct DB access in tests

    protected async Task<string> GetAccessToken()
    {
        var loginResponse = await HttpClient.PostAsJsonAsync(
            "api/v1/users/login",
            new LogInUserRequest(
                UserData.RegisterTestUserRequest.Email,
                UserData.RegisterTestUserRequest.Password));

        loginResponse.EnsureSuccessStatusCode();

        var accessTokenResponse = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        var accessToken = accessTokenResponse!.AccessToken;

        // After logging in, we need our internal application User ID, not Keycloak's ID.
        // We can get this from the database directly.
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