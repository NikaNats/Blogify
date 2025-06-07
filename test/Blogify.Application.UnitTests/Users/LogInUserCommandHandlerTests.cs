using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Users.LogInUser;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using NSubstitute;

namespace Blogify.Application.UnitTests.Users;

public class LogInUserCommandHandlerTests
{
    private const string Email = "test@example.com";
    private const string Password = "password123";
    private readonly LogInUserCommandHandler _handler;
    private readonly IJwtService _jwtService;

    public LogInUserCommandHandlerTests()
    {
        // Arrange - Setup
        _jwtService = Substitute.For<IJwtService>();
        _handler = new LogInUserCommandHandler(_jwtService);
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ReturnsAccessToken()
    {
        // Arrange
        var command = new LogInUserCommand(Email, Password);
        const string token = "valid-jwt-token";

        _jwtService.GetAccessTokenAsync(
                Email,
                Password,
                Arg.Any<CancellationToken>())
            .Returns(Result.Success(token));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(token, result.Value.AccessToken);

        // Verify JWT service was called with correct parameters
        await _jwtService.Received(1).GetAccessTokenAsync(
            Email,
            Password,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCredentialsInvalid_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var command = new LogInUserCommand(Email, Password);
        var authError = Error.Failure("Auth.Failed", "Authentication failed");

        _jwtService.GetAccessTokenAsync(
                Email,
                Password,
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>(authError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
        Assert.Equal(UserErrors.InvalidCredentials.Description, result.Error.Description);

        // Verify JWT service was called with correct parameters
        await _jwtService.Received(1).GetAccessTokenAsync(
            Email,
            Password,
            Arg.Any<CancellationToken>());
    }
}