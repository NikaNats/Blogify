using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.GetAllCategories;

public sealed record GetAllCategoriesQuery : IQuery<List<AllCategoryResponse>>;