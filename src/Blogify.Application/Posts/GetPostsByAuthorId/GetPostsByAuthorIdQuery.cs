using FluentValidation;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;

namespace Blogify.Application.Posts.GetPostsByAuthorId;

public sealed record GetPostsByAuthorIdQuery(Guid AuthorId, int Page = 1, int PageSize = Pagination.DefaultPageSize)
    : IQuery<PagedResponse<PostSummaryResponse>>, IPagedQuery;

internal sealed class GetPostsByAuthorIdQueryValidator : AbstractValidator<GetPostsByAuthorIdQuery>
{
    public GetPostsByAuthorIdQueryValidator()
    {
        this.AddPagingRules();
    }
}
