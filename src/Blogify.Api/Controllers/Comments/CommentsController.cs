using Asp.Versioning;
using Blogify.Application.Comments.CreateComment;
using Blogify.Application.Comments.DeleteComment;
using Blogify.Application.Comments.GetCommentById;
using Blogify.Application.Comments.GetCommentsByPostId;
using Blogify.Application.Comments.UpdateComment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Comments;

[Authorize]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/comments")]
public sealed class CommentsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCommentById), new { id = result.Value }, result.Value)
            : HandleFailure(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCommentById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCommentByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("by-post/{postId:guid}")]
    public async Task<IActionResult> GetCommentsByPostId(Guid postId, CancellationToken cancellationToken)
    {
        var query = new GetCommentsByPostIdQuery(postId);
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCommentCommand(id);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCommentCommand(id, request.CommentId, request.Content);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }
}