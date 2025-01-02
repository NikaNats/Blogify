namespace Blogify.Application.Categories.GetCategoryById;

public sealed record CategoryByIdResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);