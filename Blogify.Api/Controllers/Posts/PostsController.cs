using Asp.Versioning;
using Blogify.Application.Posts.AddCommentToPost;
using Blogify.Application.Posts.AddTagToPost;
using Blogify.Application.Posts.ArchivePost;
using Blogify.Application.Posts.CreatePost;
using Blogify.Application.Posts.GetAllPosts;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Posts.PublishPost;
using Blogify.Application.Posts.RemoveTagFromPost;
using Blogify.Application.Posts.UpdatePost;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Posts;

[Authorize]
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/posts")]
public class PostsController(ISender sender) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery(id);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost(
        CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePostCommand(
            request.Title,
            request.Content,
            request.Excerpt,
            request.AuthorId,
            request.CategoryId);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetPost), new { id = result.Value }, result.Value);
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddCommentToPost(
        Guid id,
        AddCommentToPostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddCommentToPostCommand(id, request.Content, request.AuthorId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("{id}/tags")]
    public async Task<IActionResult> AddTagToPost(
        Guid id,
        AddTagToPostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTagToPostCommand(id, request.TagId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpDelete("{postId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromPost(
        Guid postId,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveTagFromPostCommand(postId, tagId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPut("{id}/archive")]
    public async Task<IActionResult> ArchivePost(Guid id, CancellationToken cancellationToken)
    {
        var command = new ArchivePostCommand(id);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPut("{id}/publish")]
    public async Task<IActionResult> PublishPost(Guid id, CancellationToken cancellationToken)
    {
        var command = new PublishPostCommand(id);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(
        Guid id,
        UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePostCommand(
            id,
            request.Title,
            request.Content,
            request.Excerpt,
            request.CategoryId);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts(CancellationToken cancellationToken)
    {
        var query = new GetAllPostsQuery();

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}