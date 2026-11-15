using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using Blogify.Domain.Users;

namespace Blogify.Application.Posts.GetPostsByAuthorId;

internal sealed class GetPostsByAuthorIdQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    ICommentRepository commentRepository,
    IUserContext userContext)
    : IQueryHandler<GetPostsByAuthorIdQuery, PagedResponse<PostSummaryResponse>>
{
    public async Task<Result<PagedResponse<PostSummaryResponse>>> Handle(GetPostsByAuthorIdQuery request,
        CancellationToken cancellationToken)
    {
        // Drafts and archived posts are only visible to their author and to administrators.
        var canSeeAllStatuses = userContext.UserId == request.AuthorId
                                || userContext.IsInRole(Role.Administrator.Name);

        var page = await postRepository.GetByAuthorIdAsync(
            request.AuthorId, request.Page, request.PageSize, publishedOnly: !canSeeAllStatuses, cancellationToken);

        var items = await PostSummaryMapper.MapAsync(
            page.Items, categoryRepository, tagRepository, commentRepository, cancellationToken);

        return Result.Success(new PagedResponse<PostSummaryResponse>(items, request.Page, request.PageSize,
            page.TotalCount));
    }
}
