using System.Text.Json;
using Blogify.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Blogify.Infrastructure.Caching;

/// <summary>
/// Distributed cache wrapper. The cache is an optimization, never a hard dependency: connectivity
/// and serialization problems are logged and treated as a cache miss instead of failing the request.
/// </summary>
internal sealed class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    internal static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new ResultJsonConverterFactory() }
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        byte[]? bytes;
        try
        {
            bytes = await cache.GetAsync(key, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Cache read failed for {CacheKey}; treating as a miss", key);
            return default;
        }

        if (bytes is null)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // Outdated schema or corrupt entry: drop it so the next request repopulates it.
            logger.LogWarning(ex, "Cache entry {CacheKey} could not be deserialized as {Type}; evicting", key, typeof(T).Name);
            await RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
            await cache.SetAsync(key, bytes, CacheOptions.Create(expiration), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Cache write failed for {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The entry still expires via its TTL.
            logger.LogError(ex, "Cache eviction failed for {CacheKey}; entry may be stale until it expires", key);
        }
    }
}
