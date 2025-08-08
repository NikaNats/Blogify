using Blogify.Application.Abstractions.Caching;

namespace Blogify.Application.Posts.GetPostById;

// Cached because individual post content is read often; invalidated by updates via TTL.
public sealed record GetPostByIdQuery(Guid Id) : ICachedQuery<PostResponse>
{
	public string CacheKey => $"posts:{Id}";

	// Slightly longer cache for individual posts; adjust if edits are frequent.
	public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}