using Blogify.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Blogify.Api.Middleware;

internal sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = CreateProblemDetails(exception, context);

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private static ProblemDetails CreateProblemDetails(Exception exception, HttpContext context)
    {
        return exception switch
        {
            ValidationException validationException => new ProblemDetails
            {
                Title = "Validation",
                Type = "https://api.blogify.com/errors/validation",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Instance = context.Request.Path,
                Extensions = { ["errors"] = validationException.Errors }
            },

            DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
                new ProblemDetails
                {
                    Title = "Conflict",
                    Type = "https://api.blogify.com/errors/conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "A resource with the same unique value already exists.",
                    Instance = context.Request.Path.Value
                },

            ConcurrencyException => new ProblemDetails
            {
                Title = "Conflict",
                Type = "https://api.blogify.com/errors/conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = "The resource was modified by another request. Please retry.",
                Instance = context.Request.Path.Value
            },

            _ => new ProblemDetails
            {
                Title = "ServerError",
                Type = "https://api.blogify.com/errors/servererror",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected internal server error has occurred.",
                Instance = context.Request.Path.Value
            }
        };
    }
}