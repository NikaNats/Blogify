namespace Blogify.Application.Categories.GetAllCategories;

public sealed record AllCategoryResponse(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);