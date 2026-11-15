using FluentValidation;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;

namespace Blogify.Application.Posts.GetAllPosts;

public sealed record GetAllPostsQuery(int Page = 1, int PageSize = Pagination.DefaultPageSize)
    : IQuery<PagedResponse<AllPostResponse>>, IPagedQuery;

internal sealed class GetAllPostsQueryValidator : AbstractValidator<GetAllPostsQuery>
{
    public GetAllPostsQueryValidator()
    {
        this.AddPagingRules();
    }
}
