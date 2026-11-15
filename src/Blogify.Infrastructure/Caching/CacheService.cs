using System.Text.Json;
using Blogify.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Blogify.Infrastructure.Caching;

/// <summary>
/// Distributed cache wrapper. The cache is an optimization, never a hard dependency: connectivity
/// and serialization problems are logged and treated as a cache miss instead of failing the request.
/// Tag membership is tracked in Redis sets so entries can be evicted by tag.
/// </summary>
internal sealed class CacheService(
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    ILogger<CacheService> logger) : ICacheService
{
    private static readonly TimeSpan TagSetLifetime = TimeSpan.FromHours(1);

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

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default)
    {
        // Register tag membership first: a tag that outlives its entry is harmless, while an entry
        // that is not registered under its tag could never be evicted by it.
        try
        {
            var database = redis.GetDatabase();
            foreach (var tag in tags)
            {
                var tagKey = TagKey(tag);
                await database.SetAddAsync(tagKey, key);
                await database.KeyExpireAsync(tagKey, TagSetLifetime);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Without tag registration the entry could go stale, so do not cache it at all.
            logger.LogWarning(ex, "Cache tag registration failed for {CacheKey}; skipping cache write", key);
            return;
        }

        await SetAsync(key, value, expiration, cancellationToken);
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

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = redis.GetDatabase();
            var tagKey = TagKey(tag);
            var members = await database.SetMembersAsync(tagKey);

            foreach (var member in members)
                await cache.RemoveAsync(member.ToString(), cancellationToken);

            await database.KeyDeleteAsync(tagKey);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Cache eviction by tag {CacheTag} failed; entries may be stale until they expire", tag);
        }
    }

    private static RedisKey TagKey(string tag) => $"cache-tag:{tag}";
}
