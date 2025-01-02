using Asp.Versioning;
using Blogify.Application.Categories.CreateCategory;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Categories.GetCategoryById;
using Blogify.Application.Categories.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogify.Api.Controllers.Categories;

[Authorize]
[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/categories")]
public class CategoriesController(ISender sender) : ControllerBase
{
    // Create a category
    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCategoryCommand(request.Name, request.Description);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Value }, result.Value);
    }

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

    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCategoryByIdQuery(id);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure) return NotFound();

        var mappedResponse = new CategoryResponse(
            result.Value.Id,
            result.Value.Name,
            result.Value.Description,
            result.Value.CreatedAt,
            result.Value.UpdatedAt);

        return Ok(mappedResponse);
    }

    // Update a category
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Name, request.Description);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}