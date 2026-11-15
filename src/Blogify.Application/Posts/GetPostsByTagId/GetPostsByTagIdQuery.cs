using FluentValidation;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;

namespace Blogify.Application.Posts.GetPostsByTagId;

public sealed record GetPostsByTagIdQuery(Guid TagId, int Page = 1, int PageSize = Pagination.DefaultPageSize)
    : IQuery<PagedResponse<PostSummaryResponse>>, IPagedQuery;

internal sealed class GetPostsByTagIdQueryValidator : AbstractValidator<GetPostsByTagIdQuery>
{
    public GetPostsByTagIdQueryValidator()
    {
        this.AddPagingRules();
    }
}
