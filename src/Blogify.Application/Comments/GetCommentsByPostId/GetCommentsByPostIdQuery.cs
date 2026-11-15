using FluentValidation;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;

namespace Blogify.Application.Comments.GetCommentsByPostId;

public sealed record GetCommentsByPostIdQuery(Guid PostId, int Page = 1, int PageSize = Pagination.DefaultPageSize)
    : IQuery<PagedResponse<CommentResponse>>, IPagedQuery;

internal sealed class GetCommentsByPostIdQueryValidator : AbstractValidator<GetCommentsByPostIdQuery>
{
    public GetCommentsByPostIdQueryValidator()
    {
        this.AddPagingRules();
    }
}
