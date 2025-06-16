using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Comments;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetPostsByAuthorId;

// --- CHANGED: Inject all necessary repositories ---
internal sealed class GetPostsByAuthorIdQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository)
    : IQueryHandler<GetPostsByAuthorIdQuery, List<PostResponse>>
{
    public async Task<Result<List<PostResponse>>> Handle(GetPostsByAuthorIdQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await postRepository.GetByAuthorIdAsync(request.AuthorId, cancellationToken);
        if (!posts.Any()) return Result.Success(new List<PostResponse>());

        // --- NEW: Efficiently batch-load all related data ---

        // 1. Collect all unique Category and Tag IDs from all posts
        var allCategoryIds = posts.SelectMany(p => p.CategoryIds).Distinct().ToList();
        var allTagIds = posts.SelectMany(p => p.TagIds).Distinct().ToList();

        // 2. Fetch all required Categories and Tags in two single database calls
        var allCategories = new Dictionary<Guid, Category>();
        if (allCategoryIds.Any())
        {
            // NOTE: This assumes an ICategoryRepository.GetByIdsAsync method exists.
            // If not, this would be a loop, which is less efficient but still works.
            // For a real system, we'd add `GetByIdsAsync`.
            var categoryEntities = await categoryRepository.GetAllAsync(cancellationToken); // Simplified for example
            allCategories = categoryEntities.Where(c => allCategoryIds.Contains(c.Id))
                .ToDictionary(c => c.Id);
        }

        var allTags = new Dictionary<Guid, Tag>();
        if (allTagIds.Any())
        {
            var tagEntities = await tagRepository.GetAllAsync(cancellationToken); // Simplified for example
            allTags = tagEntities.Where(t => allTagIds.Contains(t.Id))
                .ToDictionary(t => t.Id);
        }

        // 3. Map each post, looking up its categories and tags from our pre-fetched dictionaries
        var response = posts.Select(post =>
        {
            var postCategories = post.CategoryIds
                .Where(id => allCategories.ContainsKey(id))
                .Select(id => allCategories[id])
                .Select(c =>
                    new AllCategoryResponse(c.Id, c.Name.Value, c.Description.Value, c.CreatedAt, c.LastModifiedAt))
                .ToList();

            var postTags = post.TagIds
                .Where(id => allTags.ContainsKey(id))
                .Select(id => allTags[id])
                .Select(t => new AllTagResponse(t.Id, t.Name.Value, t.CreatedAt))
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
        }).ToList();

        return Result.Success(response);
    }
}