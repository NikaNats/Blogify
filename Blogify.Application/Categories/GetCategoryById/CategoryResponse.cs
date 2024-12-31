namespace Blogify.Application.Categories.GetCategoryById;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);