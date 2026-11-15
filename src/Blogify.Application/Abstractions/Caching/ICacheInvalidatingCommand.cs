namespace Blogify.Application.Abstractions.Caching;

/// <summary>
/// Marks a command whose successful execution makes the listed cache entries stale.
/// Entries are evicted synchronously by <see cref="Behaviors.CacheInvalidationBehavior{TRequest,TResponse}"/>.
/// </summary>
public interface ICacheInvalidatingCommand
{
    IEnumerable<string> CacheKeysToInvalidate { get; }
}
