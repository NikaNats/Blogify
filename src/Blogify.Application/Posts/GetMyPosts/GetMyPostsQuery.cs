using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using FluentValidation;

namespace Blogify.Application.Posts.GetMyPosts;

/// <summary>The caller's own posts in every status (Draft, Published, Archived).</summary>
public sealed record GetMyPostsQuery(int Page = 1, int PageSize = Pagination.DefaultPageSize)
    : IQuery<PagedResponse<PostSummaryResponse>>, IPagedQuery;

internal sealed class GetMyPostsQueryValidator : AbstractValidator<GetMyPostsQuery>
{
    public GetMyPostsQueryValidator()
    {
        this.AddPagingRules();
    }
}
