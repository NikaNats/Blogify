namespace Blogify.Application.Categories.GetAllCategories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);