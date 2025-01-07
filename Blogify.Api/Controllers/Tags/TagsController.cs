using Blogify.Application.Tags.CreateTag;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Application.Tags.GetTagById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Tags;

[ApiController]
[Route("api/tags")]
public sealed class TagsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetTagById), new { id = result.Value }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTags(CancellationToken cancellationToken)
    {
        var query = new GetAllTagsQuery();

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTagById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTagByIdQuery(id);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return NotFound(result.Error);

        return Ok(result.Value);
    }
}