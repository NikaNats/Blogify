using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Comments;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.GetAllPosts;

internal sealed class GetAllPostsQueryHandler(
    IPostRepository postRepository,
    ITagRepository tagRepository)
    : IQueryHandler<GetAllPostsQuery, List<AllPostResponse>>
{
    public async Task<Result<List<AllPostResponse>>> Handle(GetAllPostsQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await postRepository.GetAllAsync(cancellationToken);
        if (!posts.Any()) return Result.Success(new List<AllPostResponse>());

        // --- NEW: Efficiently batch-load all related tags ---
        // 1. Collect all unique Tag IDs from all posts
        var allTagIds = posts.SelectMany(p => p.TagIds).Distinct().ToList();

        // 2. Fetch all required Tags in a single database call and create a lookup dictionary
        var allTags = new Dictionary<Guid, Tag>();
        if (allTagIds.Any())
        {
            // In a real system with a large number of tags, you would add
            // a `GetByIdsAsync(IEnumerable<Guid> ids)` method to the repository.
            // For now, we'll get all and filter in memory, which is acceptable for a smaller tag set.
            var tagEntities = await tagRepository.GetAllAsync(cancellationToken);
            allTags = tagEntities.Where(t => allTagIds.Contains(t.Id))
                .ToDictionary(t => t.Id);
        }

        // 3. Map each post, looking up its tags from our pre-fetched dictionary
        var response = posts.Select(post =>
        {
            var postTags = post.TagIds
                .Where(id => allTags.ContainsKey(id))
                .Select(id => allTags[id])
                .Select(t => new AllTagResponse(t.Id, t.Name.Value, t.CreatedAt))
                .ToList();

            return new AllPostResponse(
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
                postTags);
        }).ToList();

        return Result.Success(response);
    }
}