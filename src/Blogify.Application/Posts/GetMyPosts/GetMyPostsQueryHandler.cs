using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using Blogify.Domain.Users;

namespace Blogify.Application.Posts.GetMyPosts;

internal sealed class GetMyPostsQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    ICommentRepository commentRepository,
    IUserContext userContext)
    : IQueryHandler<GetMyPostsQuery, PagedResponse<PostSummaryResponse>>
{
    public async Task<Result<PagedResponse<PostSummaryResponse>>> Handle(GetMyPostsQuery request,
        CancellationToken cancellationToken)
    {
        if (userContext.UserId is not { } userId)
            return Result.Failure<PagedResponse<PostSummaryResponse>>(UserErrors.Unauthenticated);

        var page = await postRepository.GetByAuthorIdAsync(
            userId, request.Page, request.PageSize, publishedOnly: false, cancellationToken);

        var items = await PostSummaryMapper.MapAsync(
            page.Items, categoryRepository, tagRepository, commentRepository, cancellationToken);

        return Result.Success(new PagedResponse<PostSummaryResponse>(items, request.Page, request.PageSize,
            page.TotalCount));
    }
}
