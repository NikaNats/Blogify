using Blogify.Application.Users.RegisterUser;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Blogify.Application.UnitTests.Users;

public class RegisterUserCommandHandlerTests
{
    private const string Email = "test@example.com";
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string Password = "password123";
    private readonly RegisterUserCommandHandler _handler;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();

        _handler = new RegisterUserCommandHandler(
            _userRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidInput_QueuesPendingUser()
    {
        var command = new RegisterUserCommand(Email, FirstName, LastName, Password);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidFirstName_ReturnsError()
    {
        var command = new RegisterUserCommand(Email, string.Empty, LastName, Password);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidLastName_ReturnsError()
    {
        var command = new RegisterUserCommand(Email, FirstName, string.Empty, Password);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsError()
    {
        var command = new RegisterUserCommand("invalid-email", FirstName, LastName, Password);
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        var command = new RegisterUserCommand(Email, FirstName, LastName, Password);
        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(ci => throw new Exception("repo"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}