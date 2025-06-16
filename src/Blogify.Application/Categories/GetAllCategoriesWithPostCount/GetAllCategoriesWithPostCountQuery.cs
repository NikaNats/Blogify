using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.GetAllCategoriesWithPostCount;

public sealed record GetAllCategoriesWithPostCountQuery
    : IQuery<List<CategoryWithPostCountResponse>>;