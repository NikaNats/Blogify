namespace Blogify.Application.Abstractions.Caching;

/// <summary>
/// Marks a command whose successful execution makes cache entries stale. Entries are evicted by
/// <see cref="Behaviors.CacheInvalidationBehavior{TRequest,TResponse}"/> before the response is returned.
/// </summary>
public interface ICacheInvalidatingCommand
{
    /// <summary>Exact keys to evict.</summary>
    IEnumerable<string> CacheKeysToInvalidate => [];

    /// <summary>Tags whose entries should all be evicted.</summary>
    IEnumerable<string> CacheTagsToInvalidate => [];
}
