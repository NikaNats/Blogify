using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using FluentValidation;
using MediatR;
using ValidationException = Blogify.Application.Exceptions.ValidationException;

namespace Blogify.Application.Abstractions.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next();

        var validationContext = new ValidationContext<TRequest>(request);

        var validationErrors = validators
            .Select(validator => validator.Validate(validationContext))
            .Where(validationResult => validationResult.Errors.Count > 0)
            .SelectMany(validationResult => validationResult.Errors)
            .Select(failure => new ValidationError(
                failure.PropertyName,
                failure.ErrorMessage))
            .ToList();

        if (validationErrors.Count != 0) throw new ValidationException(validationErrors);

        return await next();
    }
}