using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetPostsByCategoryId;

internal sealed class GetPostsByCategoryIdQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    ICommentRepository commentRepository)
    : IQueryHandler<GetPostsByCategoryIdQuery, PagedResponse<PostSummaryResponse>>
{
    public async Task<Result<PagedResponse<PostSummaryResponse>>> Handle(GetPostsByCategoryIdQuery request,
        CancellationToken cancellationToken)
    {
        var categoryExists = await categoryRepository.ExistsAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result.Failure<PagedResponse<PostSummaryResponse>>(CategoryError.NotFound);

        var page = await postRepository.GetByCategoryIdAsync(
            request.CategoryId, request.Page, request.PageSize, publishedOnly: true, cancellationToken);

        var items = await PostSummaryMapper.MapAsync(
            page.Items, categoryRepository, tagRepository, commentRepository, cancellationToken);

        return Result.Success(new PagedResponse<PostSummaryResponse>(items, request.Page, request.PageSize,
            page.TotalCount));
    }
}
