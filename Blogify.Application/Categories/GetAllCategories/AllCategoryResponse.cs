namespace Blogify.Application.Categories.GetAllCategories;

public sealed record AllCategoryResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);