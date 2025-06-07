using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blogify.Application.Users.GetLoggedInUser;
using Blogify.FunctionalTests.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Shouldly;

namespace Blogify.FunctionalTests.Users;

public class GetLoggedInUserTests(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Get_ShouldReturnUnauthorized_WhenAccessTokenIsMissing()
    {
        // Act
        var response = await HttpClient.GetAsync("api/v1/users/me");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ShouldReturnUser_WhenAccessTokenIsNotMissing()
    {
        // Arrange
        var accessToken = await GetAccessToken();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            JwtBearerDefaults.AuthenticationScheme,
            accessToken);

        // Act
        var user = await HttpClient.GetFromJsonAsync<UserResponse>("api/v1/users/me");

        // Assert
        user.ShouldNotBeNull();
    }
}