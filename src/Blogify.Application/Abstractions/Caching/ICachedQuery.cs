using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Abstractions.Caching;

public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery;

public interface ICachedQuery
{
    string CacheKey { get; }

    TimeSpan? Expiration { get; }

    /// <summary>Tags the cached entry is stored under, enabling tag-based invalidation.</summary>
    IReadOnlyCollection<string> CacheTags => [];
}
