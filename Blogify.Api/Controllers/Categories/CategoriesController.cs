using Asp.Versioning;
using Blogify.Application.Categories.AddPostToCategoryCommand;
using Blogify.Application.Categories.CreateCategory;
using Blogify.Application.Categories.DeleteCategory;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Categories.GetCategoryById;
using Blogify.Application.Categories.UpdateCategory;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Categories;

[Authorize]
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request.Name, request.Description);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Value }, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        var query = new GetAllCategoriesQuery();

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return BadRequest(result.Error);

        var mappedResponse = result.Value.Select(category => new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.CreatedAt,
            category.UpdatedAt)).ToList();

        return Ok(mappedResponse);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        var mappedResponse = new CategoryResponse(
            result.Value.Id,
            result.Value.Name,
            result.Value.Description,
            result.Value.CreatedAt,
            result.Value.UpdatedAt);

        return Ok(mappedResponse);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Name, request.Description);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }
    
    [HttpPost("{categoryId:guid}/posts/{postId:guid}")]
    public async Task<IActionResult> AddPostToCategory(
        Guid categoryId,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var command = new AddPostToCategoryCommand(categoryId, postId);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

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