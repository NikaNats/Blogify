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
using Blogify.Domain.Abstractions;
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
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPost(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
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
            request.AuthorId);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return HandleFailure(result.Error);

        return CreatedAtAction(nameof(GetPost), new { id = result.Value }, result.Value);
    }

    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> AddCommentToPost(
        Guid postId,
        AddCommentToPostRequest request,
        CancellationToken cancellationToken)
    {
        // Extract and validate the authenticated user's ID.
        var authorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (authorIdClaim is null || !Guid.TryParse(authorIdClaim, out var authorIdGuid))
        {
            return Unauthorized("User not authenticated or invalid user id.");
        }

        // Create the command
        var command = new AddCommentToPostCommand(postId, request.Content, authorIdGuid);
        
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpPost("{id}/tags")]
    public async Task<IActionResult> AddTagToPost(
        Guid id,
        AddTagToPostRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTagToPostCommand(id, request.TagId);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpDelete("{postId}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTagFromPost(
        Guid postId,
        Guid tagId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveTagFromPostCommand(postId, tagId);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpPut("{id}/archive")]
    public async Task<IActionResult> ArchivePost(Guid id, CancellationToken cancellationToken)
    {
        var command = new ArchivePostCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpPut("{id}/publish")]
    public async Task<IActionResult> PublishPost(Guid id, CancellationToken cancellationToken)
    {
        var command = new PublishPostCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
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
            request.Excerpt);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : HandleFailure(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts(CancellationToken cancellationToken)
    {
        var query = new GetAllPostsQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("author/{authorId:guid}")]
    public async Task<IActionResult> GetPostsByAuthorId(Guid authorId, CancellationToken cancellationToken)
    {
        var query = new GetPostsByAuthorIdQuery(authorId);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetPostsByCategoryId(Guid categoryId, CancellationToken cancellationToken)
    {
        var query = new GetPostsByCategoryIdQuery(categoryId);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }

    [HttpGet("tag/{tagId:guid}")]
    public async Task<IActionResult> GetPostsByTagId(Guid tagId, CancellationToken cancellationToken)
    {
        var query = new GetPostsByTagIdQuery(tagId);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePostCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error);
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