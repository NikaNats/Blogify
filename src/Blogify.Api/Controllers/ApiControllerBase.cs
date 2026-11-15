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
        var statusCode = MapStatusCode(error.Type);
        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected server error occurred."
            : error.Description;

        return StatusCode(statusCode, CreateProblemDetails(error, statusCode, detail));
    }

    private static int MapStatusCode(ErrorType type)
    {
        return type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.AuthenticationFailed => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private ProblemDetails CreateProblemDetails(Error error, int statusCode, string detail)
    {
        return new ProblemDetails
        {
            Title = error.Type.ToString(),
            Detail = detail,
            Status = statusCode,
            Instance = HttpContext.Request.Path.Value,
            Type = $"https://api.blogify.com/errors/{error.Type.ToString().ToLower()}",
            Extensions =
            {
                ["traceId"] = HttpContext.TraceIdentifier,
                ["errorCode"] = error.Code
            }
        };
    }
}
