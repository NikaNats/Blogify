using Blogify.Application.Abstractions.Behaviors;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestRequest, Result>> _logger;
    private readonly LoggingBehavior<TestRequest, Result> _loggingBehavior;
    private readonly RequestHandlerDelegate<Result> _next;

    public LoggingBehaviorTests()
    {
        _logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, Result>>>();
        _loggingBehavior = new LoggingBehavior<TestRequest, Result>(_logger);
        _next = Substitute.For<RequestHandlerDelegate<Result>>();
    }

    [Fact]
    public async Task Handle_WhenRequestIsSuccessful_Should_LogInformationMessages()
    {
        // Arrange
        var request = new TestRequest();
        _next.Invoke().Returns(Result.Success());

        // Act
        var result = await _loggingBehavior.Handle(request, _next, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        _logger.Received(2).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null,
            Arg.Any<Func<object, Exception, string>>());
        _logger.DidNotReceive().Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Handle_WhenRequestFails_Should_LogInformationAndErrorMessage()
    {
        // Arrange
        var request = new TestRequest();
        var error = new Error("Test.Error", "A test error occurred", ErrorType.Failure);
        _next.Invoke().Returns(Result.Failure(error));

        // Act
        var result = await _loggingBehavior.Handle(request, _next, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();

        _logger.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null,
            Arg.Any<Func<object, Exception, string>>());
        _logger.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<object>(), null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Handle_WhenHandlerThrowsException_Should_LogErrorAndRethrow()
    {
        // Arrange
        var request = new TestRequest();
        var exception = new InvalidOperationException("Handler failed");
        _next.Invoke().Throws(exception);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _loggingBehavior.Handle(request, _next, CancellationToken.None));

        _logger.Received(1).Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null,
            Arg.Any<Func<object, Exception, string>>());
        _logger.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Any<object>(), exception,
            Arg.Any<Func<object, Exception, string>>());
    }
}