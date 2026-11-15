using FluentValidation;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Pagination;

namespace Blogify.Application.Tags.GetAllTags;

// Cached because tag list changes rarely; short TTL balances freshness.
// Tagged so any tag write evicts every cached page at once.
public sealed record GetAllTagsQuery(int Page = 1, int PageSize = Pagination.DefaultPageSize)
    : ICachedQuery<PagedResponse<AllTagResponse>>, IPagedQuery
{
    public string CacheKey => CacheKeys.AllTags(Page, PageSize);

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

    public IReadOnlyCollection<string> CacheTags => [CacheTagNames.AllTags];
}

internal sealed class GetAllTagsQueryValidator : AbstractValidator<GetAllTagsQuery>
{
    public GetAllTagsQueryValidator()
    {
        this.AddPagingRules();
    }
}
