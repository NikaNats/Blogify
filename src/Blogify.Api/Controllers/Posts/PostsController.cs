using Asp.Versioning;
using Blogify.Application.Posts.AddTagToPost;
using Blogify.Application.Posts.AssignCategoriesToPost;
using Blogify.Application.Posts.ArchivePost;
using Blogify.Application.Posts.CreatePost;
using Blogify.Application.Posts.DeletePost;
using Blogify.Application.Posts.GetAllPosts;
using Blogify.Application.Posts.GetMyPosts;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Posts.GetPostsByAuthorId;
using Blogify.Application.Posts.GetPostsByCategoryId;
using Blogify.Application.Posts.GetPostsByTagId;
using Blogify.Application.Posts.PublishPost;
using Blogify.Application.Posts.RemoveCategoryFromPost;
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
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery(id);
        var result = await Sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    // In: Blogify.Api.Controllers.Posts/PostsController.cs
    [HttpPost]
    public async Task<IActionResult> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePostCommand(
            request.Title,
            request.Content,
            request.Excerpt);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPost), new { id = result.Value }, result.Value)
            : HandleFailure(result.Error);
    }

    [HttpPost("{id:guid}/tags")]
    public async Task<IActionResult> AddTagToPost(Guid id, [FromBody] AddTagToPostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTagToPostCommand(id, request.TagId);
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpDelete("{postId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> RemoveTagFromPost(Guid postId, Guid tagId, CancellationToken cancellationToken)
    {
        var command = new RemoveTagFromPostCommand(postId, tagId);
        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpPost("{id:guid}/categories")]
    public async Task<IActionResult> AssignCategoriesToPost(Guid id, [FromBody] AssignCategoriesToPostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new AssignCategoriesToPostCommand(id, request.CategoryIds), cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpDelete("{postId:guid}/categories/{categoryId:guid}")]
    public async Task<IActionResult> RemoveCategoryFromPost(Guid postId, Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new RemoveCategoryFromPostCommand(postId, categoryId), cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpPut("{id:guid}/archive")]
    public async Task<IActionResult> ArchivePost(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ArchivePostCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpPut("{id:guid}/publish")]
    public async Task<IActionResult> PublishPost(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new PublishPostCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePostCommand(
            id,
            request.Title,
            request.Content,
            request.Excerpt);

        var result = await Sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetAllPostsQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    /// <summary>The caller's own posts in every status. Requires authentication.</summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetMyPostsQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [AllowAnonymous]
    [HttpGet("author/{authorId:guid}")]
    public async Task<IActionResult> GetPostsByAuthorId(Guid authorId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPostsByAuthorIdQuery(authorId, page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [AllowAnonymous]
    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetPostsByCategoryId(Guid categoryId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPostsByCategoryIdQuery(categoryId, page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [AllowAnonymous]
    [HttpGet("tag/{tagId:guid}")]
    public async Task<IActionResult> GetPostsByTagId(Guid tagId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetPostsByTagIdQuery(tagId, page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeletePostCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
    }
}