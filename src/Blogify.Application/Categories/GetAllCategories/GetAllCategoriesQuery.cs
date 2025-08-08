using Blogify.Application.Abstractions.Caching;

namespace Blogify.Application.Categories.GetAllCategories;

// Cached because categories change infrequently compared to read frequency.
public sealed record GetAllCategoriesQuery : ICachedQuery<List<AllCategoryResponse>>
{
	public string CacheKey => "categories:all";

	// Cache for 5 minutes; adjust if needed based on update patterns.
	public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}