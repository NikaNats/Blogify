using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Comments;
using Blogify.Application.Posts.GetPostById;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetPostsByCategoryId;

// --- CHANGED: Inject all necessary repositories ---
internal sealed class GetPostsByCategoryIdQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository)
    : IQueryHandler<GetPostsByCategoryIdQuery, List<PostResponse>>
{
    public async Task<Result<List<PostResponse>>> Handle(GetPostsByCategoryIdQuery request,
        CancellationToken cancellationToken)
    {
        // First, ensure the category itself exists.
        var categoryExists = await categoryRepository.ExistsAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            // Primary resource (Category) does not exist => propagate NotFound failure for consistent API semantics.
            return Result.Failure<List<PostResponse>>(CategoryError.NotFound);

        // NOTE: A more efficient repository method would be `GetPostsByCategoryIdAsync`.
        // We are simulating this with a Where clause on GetAllAsync for this example.
        var allPosts = await postRepository.GetAllAsync(cancellationToken);
        var posts = allPosts.Where(p => p.CategoryIds.Contains(request.CategoryId)).ToList();

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

    // A private helper makes the mapping logic reusable and clean.
    private static PostResponse MapToPostResponse(Post post, IReadOnlyDictionary<Guid, Category> categories,
        IReadOnlyDictionary<Guid, Tag> tags)
    {
        var postCategories = post.CategoryIds
            .Select(id => categories.GetValueOrDefault(id))
            .Where(c => c is not null)
            .Select(c =>
                new AllCategoryResponse(c!.Id, c.Name.Value, c.Description.Value, c.CreatedAt, c.LastModifiedAt))
            .ToList();

        var postTags = post.TagIds
            .Select(id => tags.GetValueOrDefault(id))
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