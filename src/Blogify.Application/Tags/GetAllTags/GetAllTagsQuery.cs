using Blogify.Application.Abstractions.Caching;

namespace Blogify.Application.Tags.GetAllTags;

// Cached because tag list changes rarely; short TTL balances freshness.
public sealed record GetAllTagsQuery : ICachedQuery<List<AllTagResponse>>
{
	public string CacheKey => "tags:all";

	public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}