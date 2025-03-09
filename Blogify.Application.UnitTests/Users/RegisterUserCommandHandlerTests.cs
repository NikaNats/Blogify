using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Users.RegisterUser;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Users
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        private readonly RegisterUserCommandHandler _handler;

        private const string Email = "test@example.com";
        private const string FirstName = "John";
        private const string LastName = "Doe";
        private const string Password = "password123";
        private const string IdentityId = "test-identity-id";

        public RegisterUserCommandHandlerTests()
        {
            // Arrange - Setup
            _authenticationService = Substitute.For<IAuthenticationService>();
            _userRepository = Substitute.For<IUserRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();
            
            _handler = new RegisterUserCommandHandler(
                _authenticationService,
                _userRepository,
                _unitOfWork,
                _logger);
        }

        [Fact]
        public async Task Handle_WithValidInput_RegistersUserSuccessfully()
        {
            // Arrange
            var command = new RegisterUserCommand(Email, FirstName, LastName, Password);
            
            _authenticationService.RegisterAsync(
                Arg.Any<User>(),
                Arg.Is(Password),
                Arg.Any<CancellationToken>())
                .Returns(IdentityId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<Guid>(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value);
            
            // Verify services were called with correct parameters
            await _authenticationService.Received(1).RegisterAsync(
                Arg.Is<User>(u => 
                    u.Email.Address == Email.ToLowerInvariant() && 
                    u.FirstName.Value == FirstName && 
                    u.LastName.Value == LastName),
                Arg.Is(Password),
                Arg.Any<CancellationToken>());
            
            await _userRepository.Received(1).AddAsync(
                Arg.Is<User>(u => u.IdentityId == IdentityId),
                Arg.Any<CancellationToken>());
            
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithInvalidFirstName_ReturnsFirstNameError()
        {
            // Arrange
            var command = new RegisterUserCommand(Email, string.Empty, LastName, Password);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(UserErrors.InvalidFirstName.Code, result.Error.Code);
            
            // Verify no services were called
            await _authenticationService.DidNotReceive().RegisterAsync(
                Arg.Any<User>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
            
            await _userRepository.DidNotReceive().AddAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>());
            
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithInvalidLastName_ReturnsLastNameError()
        {
            // Arrange
            var command = new RegisterUserCommand(Email, FirstName, string.Empty, Password);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(UserErrors.InvalidLastName.Code, result.Error.Code);
        }

        [Fact]
        public async Task Handle_WithInvalidEmail_ReturnsEmailError()
        {
            // Arrange
            var command = new RegisterUserCommand("invalid-email", FirstName, LastName, Password);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(UserErrors.InvalidEmail.Code, result.Error.Code);
        }

        [Fact]
        public async Task Handle_WhenAuthenticationFails_RollsBackAndReturnsError()
        {
            // Arrange
            var command = new RegisterUserCommand(Email, FirstName, LastName, Password);
            var exception = new Exception("Authentication service failed");
            
            _authenticationService.RegisterAsync(
                Arg.Any<User>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(IdentityId);
            
            _userRepository.AddAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>())
                .Throws(exception);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User.Registration.Failed", result.Error.Code);
            
            // Verify rollback was attempted
            await _authenticationService.Received(1).DeleteIdentityAsync(
                Arg.Is(IdentityId),
                Arg.Any<CancellationToken>());
            
            // Verify the error was logged
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Is(exception),
                Arg.Any<Func<object, Exception, string>>()!);
        }

        [Fact]
        public async Task Handle_WhenRollbackFails_LogsRollbackError()
        {
            // Arrange
            var command = new RegisterUserCommand(Email, FirstName, LastName, Password);
            var persistenceException = new Exception("Database error");
            var rollbackException = new Exception("Rollback failed");
            
            _authenticationService.RegisterAsync(
                Arg.Any<User>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
                .Returns(IdentityId);
            
            _userRepository.AddAsync(
                Arg.Any<User>(),
                Arg.Any<CancellationToken>())
                .Throws(persistenceException);
            
            _authenticationService
                .DeleteIdentityAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Throws(rollbackException);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            
            // Verify both errors were logged
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Is(persistenceException),
                Arg.Any<Func<object, Exception, string>>()!);
            
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Is(rollbackException),
                Arg.Any<Func<object, Exception, string>>()!);
        }
    }
}