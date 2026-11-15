namespace Blogify.Application.Abstractions.Caching;

/// <summary>
/// Single source of truth for cache keys and tags, shared by cached queries and the commands that invalidate them.
/// </summary>
public static class CacheKeys
{
    public static string Post(Guid postId) => $"posts:{postId}";

    public static string AllCategories(int page, int pageSize) => $"{CacheTagNames.AllCategories}:{page}:{pageSize}";

    public static string AllTags(int page, int pageSize) => $"{CacheTagNames.AllTags}:{page}:{pageSize}";
}

/// <summary>Tags group cache entries so related entries can be evicted together.</summary>
public static class CacheTagNames
{
    public const string AllCategories = "categories:all";

    public const string AllTags = "tags:all";

    public static string Post(Guid postId) => $"post:{postId}";

    public static string Category(Guid categoryId) => $"category:{categoryId}";

    public static string Tag(Guid tagId) => $"tag:{tagId}";
}

/// <summary>
/// Implemented by cached query responses that may only be stored when they are safe to serve to every caller.
/// A response that is not cacheable is returned to its caller but never written to the shared cache.
/// </summary>
public interface ICacheEligible
{
    bool IsCacheable { get; }
}

/// <summary>Implemented by cached query responses that know which tags they depend on.</summary>
public interface ICacheTagged
{
    IReadOnlyCollection<string> CacheTags { get; }
}
