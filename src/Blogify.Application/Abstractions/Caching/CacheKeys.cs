namespace Blogify.Application.Abstractions.Caching;

/// <summary>
/// Single source of truth for cache keys, shared by cached queries and the commands that invalidate them.
/// </summary>
public static class CacheKeys
{
    public const string AllCategories = "categories:all";

    public const string AllTags = "tags:all";

    public static string Post(Guid postId) => $"posts:{postId}";
}
