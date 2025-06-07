using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    protected readonly ISender Sender = sender;

    protected ActionResult HandleFailure(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation => BadRequest(CreateProblemDetails(error)),
            ErrorType.NotFound => NotFound(CreateProblemDetails(error)),
            ErrorType.Conflict => Conflict(CreateProblemDetails(error)),
            ErrorType.AuthenticationFailed => Unauthorized(CreateProblemDetails(error)),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                CreateProblemDetails(error, "An unexpected server error occurred."))
        };
    }

    private ProblemDetails CreateProblemDetails(Error error, string? defaultDetail = null)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.AuthenticationFailed => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Title = error.Type.ToString(),
            Detail = defaultDetail ?? error.Description,
            Status = statusCode,
            Instance = HttpContext.Request.Path.Value,
            Type = $"https://api.blogify.com/errors/{error.Type.ToString().ToLower()}",
            Extensions =
            {
                ["traceId"] = HttpContext.TraceIdentifier,
                ["errorCode"] = error.Code
            }
        };

        return problemDetails;
    }
}