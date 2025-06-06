using System.Net;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Users;
using Blogify.FunctionalTests.Infrastructure;
using Shouldly;

namespace Blogify.FunctionalTests.Users;

public class LoginUserTests(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory)
{
    private const string Email = "login@test.com";
    private const string Password = "12345";

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LogInUserRequest(Email, Password);

        // Act
        var response = await HttpClient.PostAsJsonAsync("api/v1/users/login", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var registerRequest = new RegisterUserRequest(Email, "first", "last", Password);
        await HttpClient.PostAsJsonAsync("api/v1/users/register", registerRequest);

        var request = new LogInUserRequest(Email, Password);

        // Act
        var response = await HttpClient.PostAsJsonAsync("api/v1/users/login", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}