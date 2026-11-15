using FluentValidation;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Pagination;

namespace Blogify.Application.Categories.GetAllCategories;

// Cached because categories change infrequently compared to read frequency.
// Tagged so any category write evicts every cached page at once.
public sealed record GetAllCategoriesQuery(int Page = 1, int PageSize = Pagination.DefaultPageSize)
    : ICachedQuery<PagedResponse<AllCategoryResponse>>, IPagedQuery
{
    public string CacheKey => CacheKeys.AllCategories(Page, PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

    public IReadOnlyCollection<string> CacheTags => [CacheTagNames.AllCategories];
}

internal sealed class GetAllCategoriesQueryValidator : AbstractValidator<GetAllCategoriesQuery>
{
    public GetAllCategoriesQueryValidator()
    {
        this.AddPagingRules();
    }
}
