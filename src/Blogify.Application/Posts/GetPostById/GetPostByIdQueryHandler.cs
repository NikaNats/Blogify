using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Comments;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetPostById;

internal sealed class GetPostByIdQueryHandler(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository)
    : IQueryHandler<GetPostByIdQuery, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        // Query 1: Get the main Post aggregate.
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null) return Result.Failure<PostResponse>(PostErrors.NotFound);

        // --- BATCH FETCH CATEGORIES ---
        // Query 2: Get all required categories in a single database call.
        var categories = new List<Category>();
        if (post.CategoryIds.Any())
        {
            // This assumes an efficient `GetByIdsAsync` method exists.
            // If not, fetching all and filtering is a fallback.
            var allCategories = await categoryRepository.GetAllAsync(cancellationToken);
            categories = allCategories.Where(c => post.CategoryIds.Contains(c.Id)).ToList();
        }

        // --- BATCH FETCH TAGS ---
        // Query 3: Get all required tags in a single database call.
        var tags = new List<Tag>();
        if (post.TagIds.Any())
        {
            var allTags = await tagRepository.GetAllAsync(cancellationToken);
            tags = allTags.Where(t => post.TagIds.Contains(t.Id)).ToList();
        }

        // Assemble the response from the efficiently-loaded entities. Total queries: 3.
        var response = new PostResponse(
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
            tags.Select(t => new AllTagResponse(t.Id, t.Name.Value, t.CreatedAt)).ToList(),
            categories.Select(c =>
                    new AllCategoryResponse(c.Id, c.Name.Value, c.Description.Value, c.CreatedAt, c.LastModifiedAt))
                .ToList());

        return Result.Success(response);
    }
}