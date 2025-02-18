using Asp.Versioning;
using Blogify.Application.Comments.CreateComment;
using Blogify.Application.Comments.DeleteComment;
using Blogify.Application.Comments.GetCommentById;
using Blogify.Application.Comments.GetCommentsByPostId;
using Blogify.Application.Comments.UpdateComment;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Comments;

[Authorize]
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/comments")]
public sealed class CommentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return HandleFailure(result.Error);

        return CreatedAtAction(nameof(GetCommentById), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCommentById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCommentByIdQuery(id);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return HandleFailure(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("by-post/{postId:guid}")]
    public async Task<IActionResult> GetCommentsByPostId(Guid postId, CancellationToken cancellationToken)
    {
        var query = new GetCommentsByPostIdQuery(postId);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);

    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id, [FromBody] Guid authorId, CancellationToken cancellationToken)
    {
        var command = new DeleteCommentCommand(id, authorId);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return HandleFailure(result.Error);

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.CommentId)
        {
            return HandleFailure(new Error("InvalidRequest", "Comment ID in the route does not match the Comment ID in the body.", ErrorType.Validation));
        }

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return HandleFailure(result.Error);

        return Ok();
    }

    private ActionResult HandleFailure(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.AuthenticationFailed => StatusCodes.Status401Unauthorized,
            ErrorType.Unexpected or ErrorType.Problem => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Title = error.Type.ToString(),
            Detail = error.Description,
            Status = statusCode,
            Instance = HttpContext.Request.Path.Value,
            Type = $"https://api.blogify.com/errors/{error.Type.ToString().ToLower()}",
            Extensions =
            {
                ["traceId"] = HttpContext.TraceIdentifier,
                ["errorCode"] = error.Code
            }
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}