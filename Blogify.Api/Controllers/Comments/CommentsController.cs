using Asp.Versioning;
using Blogify.Application.Comments.CreateComment;
using Blogify.Application.Comments.GetCommentById;
using Blogify.Application.Comments.GetCommentsByPostId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Comments;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/comments")]
public sealed class CommentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetCommentById), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCommentById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCommentByIdQuery(id);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return NotFound(result.Error);

        return Ok(result.Value);
    }
    
    [HttpGet("by-post/{postId:guid}")]
    public async Task<IActionResult> GetCommentsByPostId(Guid postId, CancellationToken cancellationToken)
    {
        var query = new GetCommentsByPostIdQuery(postId);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return NotFound(result.Error);

        return Ok(result.Value);
    }
}