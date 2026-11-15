using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts;

internal static class PostSummaryMapper
{
    /// <summary>Maps a page of posts, batch-loading their categories, tags and comment counts (3 queries total).</summary>
    public static async Task<List<PostSummaryResponse>> MapAsync(
        IReadOnlyList<Post> posts,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        ICommentRepository commentRepository,
        CancellationToken cancellationToken)
    {
        if (posts.Count == 0) return [];

        var categoryIds = posts.SelectMany(p => p.CategoryIds).Distinct().ToList();
        var tagIds = posts.SelectMany(p => p.TagIds).Distinct().ToList();
        var postIds = posts.Select(p => p.Id).ToList();

        var categories = (await categoryRepository.GetByIdsAsync(categoryIds, cancellationToken))
            .ToDictionary(c => c.Id);
        var tags = (await tagRepository.GetByIdsAsync(tagIds, cancellationToken))
            .ToDictionary(t => t.Id);
        var commentCounts = await commentRepository.GetCountsByPostIdsAsync(postIds, cancellationToken);

        return posts.Select(post => new PostSummaryResponse(
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
                .ToList(),
            post.CategoryIds
                .Select(id => categories.GetValueOrDefault(id))
                .OfType<Category>()
                .Select(c => new AllCategoryResponse(c.Id, c.Name.Value, c.Description.Value, c.CreatedAt,
                    c.LastModifiedAt))
                .ToList())).ToList();
    }
}
