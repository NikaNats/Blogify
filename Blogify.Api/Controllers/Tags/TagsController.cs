using Asp.Versioning;
using Blogify.Application.Tags.CreateTag;
using Blogify.Application.Tags.DeleteTag;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Application.Tags.GetTagById;
using Blogify.Application.Tags.UpdateTag;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Tags;

[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/tags")]
public sealed class TagsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetTagById), new { id = result.Value }, result.Value)
            : HandleFailure(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTags(CancellationToken cancellationToken)
    {
        var query = new GetAllTagsQuery();
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTagById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTagByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteTagCommand(id);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }
}