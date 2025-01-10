namespace Blogify.Api.Controllers.Categories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);