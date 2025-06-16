using Blogify.Application.Abstractions.Behaviors;
using Blogify.Domain.Abstractions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Shouldly;
using ValidationException = Blogify.Application.Exceptions.ValidationException;

namespace Blogify.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly RequestHandlerDelegate<Result> _next;

    public ValidationBehaviorTests()
    {
        _next = Substitute.For<RequestHandlerDelegate<Result>>();
    }

    [Fact]
    public async Task Handle_WhenNoValidatorsExist_Should_CallNextAndSucceed()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>([]);
        var request = new TestRequest();

        // Act
        await behavior.Handle(request, _next, CancellationToken.None);

        // Assert
        await _next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WhenValidationIsSuccessful_Should_CallNextAndSucceed()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.Validate(Arg.Any<IValidationContext>()).Returns(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, Result>([validator]);
        var request = new TestRequest();

        // Act
        await behavior.Handle(request, _next, CancellationToken.None);

        // Assert
        await _next.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_Should_ThrowValidationExceptionAndNotCallNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        var validationFailures = new List<ValidationFailure>
        {
            new("Property1", "Error message 1"),
            new("Property2", "Error message 2")
        };
        validator.Validate(Arg.Any<IValidationContext>()).Returns(new ValidationResult(validationFailures));

        var behavior = new ValidationBehavior<TestRequest, Result>([validator]);
        var request = new TestRequest();

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(async () =>
            await behavior.Handle(request, _next, CancellationToken.None));

        exception.Errors.Count().ShouldBe(2);

        await _next.DidNotReceive().Invoke();
    }
}