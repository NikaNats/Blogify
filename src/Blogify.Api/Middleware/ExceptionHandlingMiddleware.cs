using Blogify.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

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