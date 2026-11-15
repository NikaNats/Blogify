using System.Text.Json.Serialization;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Comments;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetPostById;

public sealed record PostResponse(
    Guid Id,
    string Title,
    string Content,
    string Excerpt,
    string Slug,
    Guid AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? PublishedAt,
    PublicationStatus Status,
    List<CommentResponse> Comments,
    List<AllTagResponse> Tags,
    List<AllCategoryResponse> Categories) : ICacheTagged, ICacheEligible
{
    /// <summary>
    /// Only published posts are cached, so a cache entry is always safe to serve to anonymous callers. A draft
    /// shown to its author is read fresh every time and never stored.
    /// </summary>
    [JsonIgnore]
    public bool IsCacheable => Status == PublicationStatus.Published;

    /// <summary>Lets tag/category edits evict this post from the cache (see <see cref="CacheTagNames"/>).</summary>
    [JsonIgnore]
    public IReadOnlyCollection<string> CacheTags =>
    [
        "posts",
        CacheTagNames.Post(Id),
        .. Categories.Select(c => CacheTagNames.Category(c.Id)),
        .. Tags.Select(t => CacheTagNames.Tag(t.Id))
    ];
}
