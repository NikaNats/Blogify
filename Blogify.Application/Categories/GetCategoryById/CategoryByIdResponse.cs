namespace Blogify.Application.Categories.GetCategoryById;

public sealed record CategoryByIdResponse(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);