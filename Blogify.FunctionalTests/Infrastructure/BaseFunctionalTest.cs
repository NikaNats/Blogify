using System.Net.Http.Json;
using Blogify.Api.Controllers.Users;
using Blogify.Application.Users.LogInUser;
using Blogify.FunctionalTests.Users;

namespace Blogify.FunctionalTests.Infrastructure;

public abstract class BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient HttpClient = factory.CreateClient();

    protected async Task<string> GetAccessToken()
    {
        var loginResponse = await HttpClient.PostAsJsonAsync(
            "api/v1/users/login",
            new LogInUserRequest(
                UserData.RegisterTestUserRequest.Email,
                UserData.RegisterTestUserRequest.Password));

        var accessTokenResponse = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

        return accessTokenResponse!.AccessToken;
    }
}