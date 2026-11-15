using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetPostsByTagId;

internal sealed class GetPostsByTagIdQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    ICommentRepository commentRepository)
    : IQueryHandler<GetPostsByTagIdQuery, PagedResponse<PostSummaryResponse>>
{
    public async Task<Result<PagedResponse<PostSummaryResponse>>> Handle(GetPostsByTagIdQuery request,
        CancellationToken cancellationToken)
    {
        var tagExists = await tagRepository.ExistsAsync(t => t.Id == request.TagId, cancellationToken);
        if (!tagExists)
            return Result.Success(new PagedResponse<PostSummaryResponse>([], request.Page, request.PageSize, 0));

        var page = await postRepository.GetByTagIdAsync(
            request.TagId, request.Page, request.PageSize, publishedOnly: true, cancellationToken);

        var items = await PostSummaryMapper.MapAsync(
            page.Items, categoryRepository, tagRepository, commentRepository, cancellationToken);

        return Result.Success(new PagedResponse<PostSummaryResponse>(items, request.Page, request.PageSize,
            page.TotalCount));
    }
}
