using System.Net;
using System.Net.Http.Json;
using Blogify.Api.Controllers.Users;
using Blogify.FunctionalTests.Infrastructure;
using Shouldly;

namespace Blogify.FunctionalTests.Users;

public class RegisterUserTests(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory)
{
    [Theory]
    [InlineData("", "first", "last", "12345")]
    [InlineData("test.com", "first", "last", "12345")]
    [InlineData("@test.com", "first", "last", "12345")]
    [InlineData("test@", "first", "last", "12345")]
    [InlineData("test@test.com", "", "last", "12345")]
    [InlineData("test@test.com", "first", "", "12345")]
    [InlineData("test@test.com", "first", "last", "")]
    [InlineData("test@test.com", "first", "last", "1")]
    [InlineData("test@test.com", "first", "last", "12")]
    [InlineData("test@test.com", "first", "last", "123")]
    [InlineData("test@test.com", "first", "last", "1234")]
    public async Task Register_ShouldReturnBadRequest_WhenRequestIsInvalid(
        string email,
        string firstName,
        string lastName,
        string password)
    {
        // Arrange
        var request = new RegisterUserRequest(email, firstName, lastName, password);

        // Act
        var response = await HttpClient.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new RegisterUserRequest("create@test.com", "first", "last", "12345");

        // Act
        var response = await HttpClient.PostAsJsonAsync("api/v1/users/register", request);

        // Log the response content for debugging
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}