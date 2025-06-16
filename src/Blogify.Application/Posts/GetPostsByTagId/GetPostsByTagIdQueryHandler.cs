using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Comments;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetPostsByTagId;

// --- CHANGED: Inject all necessary repositories ---
internal sealed class GetPostsByTagIdQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository)
    : IQueryHandler<GetPostsByTagIdQuery, List<PostResponse>>
{
    public async Task<Result<List<PostResponse>>> Handle(GetPostsByTagIdQuery request,
        CancellationToken cancellationToken)
    {
        var tagExists = await tagRepository.ExistsAsync(t => t.Id == request.TagId, cancellationToken);
        if (!tagExists) return Result.Success(new List<PostResponse>());

        var allPosts = await postRepository.GetAllAsync(cancellationToken);
        var posts = allPosts.Where(p => p.TagIds.Contains(request.TagId)).ToList();

        if (!posts.Any()) return Result.Success(new List<PostResponse>());

        // --- NEW: Efficiently batch-load all related data ---
        var allCategoryIds = posts.SelectMany(p => p.CategoryIds).Distinct().ToList();
        var allTagIds = posts.SelectMany(p => p.TagIds).Distinct().ToList();

        var allCategories = (await categoryRepository.GetAllAsync(cancellationToken))
            .Where(c => allCategoryIds.Contains(c.Id))
            .ToDictionary(c => c.Id);

        var allTags = (await tagRepository.GetAllAsync(cancellationToken))
            .Where(t => allTagIds.Contains(t.Id))
            .ToDictionary(t => t.Id);

        // --- NEW: Reusable mapping logic within the handler ---
        var response = posts.Select(post => MapToPostResponse(post, allCategories, allTags)).ToList();

        return Result.Success(response);
    }

    // Reusing the same mapping helper for consistency.
    private static PostResponse MapToPostResponse(Post post, IReadOnlyDictionary<Guid, Category> categories,
        IReadOnlyDictionary<Guid, Tag> tags)
    {
        var postCategories = post.CategoryIds
            .Select(categories.GetValueOrDefault)
            .Where(c => c is not null)
            .Select(c =>
                new AllCategoryResponse(c!.Id, c.Name.Value, c.Description.Value, c.CreatedAt, c.LastModifiedAt))
            .ToList();

        var postTags = post.TagIds
            .Select(tags.GetValueOrDefault)
            .Where(t => t is not null)
            .Select(t => new AllTagResponse(t!.Id, t.Name.Value, t.CreatedAt))
            .ToList();

        return new PostResponse(
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
            post.Comments.Select(c => new CommentResponse(c.Id, c.Content.Value, c.AuthorId, c.PostId, c.CreatedAt))
                .ToList(),
            postTags,
            postCategories);
    }
}