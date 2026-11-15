using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetAllPosts;

internal sealed class GetAllPostsQueryHandler(
    IPostRepository postRepository,
    ITagRepository tagRepository,
    ICommentRepository commentRepository)
    : IQueryHandler<GetAllPostsQuery, PagedResponse<AllPostResponse>>
{
    public async Task<Result<PagedResponse<AllPostResponse>>> Handle(GetAllPostsQuery request,
        CancellationToken cancellationToken)
    {
        var page = await postRepository.GetPagedAsync(
            request.Page, request.PageSize, publishedOnly: true, cancellationToken);
        var posts = page.Items;

        if (posts.Count == 0)
            return Result.Success(new PagedResponse<AllPostResponse>([], request.Page, request.PageSize,
                page.TotalCount));

        var tagIds = posts.SelectMany(p => p.TagIds).Distinct().ToList();
        var tags = (await tagRepository.GetByIdsAsync(tagIds, cancellationToken)).ToDictionary(t => t.Id);
        var commentCounts = await commentRepository.GetCountsByPostIdsAsync(
            posts.Select(p => p.Id).ToList(), cancellationToken);

        var response = PagedResponse<AllPostResponse>.From(page, request.Page, request.PageSize, post =>
            new AllPostResponse(
                post.Id,
                post.Title.Value,
                post.Content.Value,
                post.Excerpt.Value,
                post.Slug.Value,
                post.AuthorId,
                post.CreatedAt,
                post.LastModifiedAt,
                post.PublishedAt,
                post.Status,
                commentCounts.GetValueOrDefault(post.Id),
                post.TagIds
                    .Select(id => tags.GetValueOrDefault(id))
                    .OfType<Tag>()
                    .Select(t => new AllTagResponse(t.Id, t.Name.Value, t.CreatedAt))
                    .ToList()));

        return Result.Success(response);
    }
}
