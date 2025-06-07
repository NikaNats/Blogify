using System.Security.Claims;
using Asp.Versioning;
using Blogify.Application.Posts.AddCommentToPost;
using Blogify.Application.Posts.AddTagToPost;
using Blogify.Application.Posts.ArchivePost;
using Blogify.Application.Posts.CreatePost;
using Blogify.Application.Posts.DeletePost;
using Blogify.Application.Posts.GetAllPosts;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Posts.GetPostsByAuthorId;
using Blogify.Application.Posts.GetPostsByCategoryId;
using Blogify.Application.Posts.GetPostsByTagId;
using Blogify.Application.Posts.PublishPost;
using Blogify.Application.Posts.RemoveTagFromPost;
using Blogify.Application.Posts.UpdatePost;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Posts;

[Authorize]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/posts")]
public class PostsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost(CreatePostRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePostCommand(request.Title, request.Content, request.Excerpt, request.AuthorId);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPost), new { id = result.Value }, result.Value)
            : HandleFailure(result.Error);
    }

    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> AddCommentToPost(Guid postId, AddCommentToPostRequest request,
        CancellationToken cancellationToken)
    {
        var authorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (authorIdClaim is null || !Guid.TryParse(authorIdClaim, out var authorIdGuid))
            return Unauthorized("User not authenticated or invalid user id.");

        var command = new AddCommentToPostCommand(postId, request.Content, authorIdGuid);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpPost("{id}/tags")]
    public async Task<IActionResult> AddTagToPost(Guid id, AddTagToPostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTagToPostCommand(id, request.TagId);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpDelete("{postId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromPost(Guid postId, Guid tagId, CancellationToken cancellationToken)
    {
        var command = new RemoveTagFromPostCommand(postId, tagId);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    // Additional endpoints refactored for brevity...

    [HttpPut("{id}/archive")]
    public async Task<IActionResult> ArchivePost(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ArchivePostCommand(id), cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpPut("{id}/publish")]
    public async Task<IActionResult> PublishPost(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new PublishPostCommand(id), cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(Guid id, UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePostCommand(id, request.Title, request.Content, request.Excerpt);
        var result = await Sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAllPostsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("author/{authorId:guid}")]
    public async Task<IActionResult> GetPostsByAuthorId(Guid authorId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPostsByAuthorIdQuery(authorId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetPostsByCategoryId(Guid categoryId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPostsByCategoryIdQuery(categoryId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("tag/{tagId:guid}")]
    public async Task<IActionResult> GetPostsByTagId(Guid tagId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetPostsByTagIdQuery(tagId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeletePostCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }
}