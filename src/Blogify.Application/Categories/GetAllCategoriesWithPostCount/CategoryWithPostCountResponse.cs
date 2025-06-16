namespace Blogify.Application.Categories.GetAllCategoriesWithPostCount;

public sealed record CategoryWithPostCountResponse(
    Guid Id,
    string Name,
    string Description,
    int PostCount);