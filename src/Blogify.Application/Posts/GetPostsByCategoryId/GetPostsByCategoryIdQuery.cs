using FluentValidation;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;

namespace Blogify.Application.Posts.GetPostsByCategoryId;

public sealed record GetPostsByCategoryIdQuery(
    Guid CategoryId,
    int Page = 1,
    int PageSize = Pagination.DefaultPageSize)
    : IQuery<PagedResponse<PostSummaryResponse>>, IPagedQuery;

internal sealed class GetPostsByCategoryIdQueryValidator : AbstractValidator<GetPostsByCategoryIdQuery>
{
    public GetPostsByCategoryIdQueryValidator()
    {
        this.AddPagingRules();
    }
}
