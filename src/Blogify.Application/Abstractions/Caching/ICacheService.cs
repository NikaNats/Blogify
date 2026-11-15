namespace Blogify.Application.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value and associates it with <paramref name="tags"/> so it can later be evicted with
    /// <see cref="RemoveByTagAsync"/> without knowing its key.
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Evicts every entry that was stored with the given tag.</summary>
    Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
