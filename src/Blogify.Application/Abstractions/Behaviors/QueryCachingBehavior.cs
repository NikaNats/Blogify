using Blogify.Application.Abstractions.Caching;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blogify.Application.Abstractions.Behaviors;

public sealed class QueryCachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cachedResult = await cacheService.GetAsync<TResponse>(
            request.CacheKey,
            cancellationToken);

        var name = typeof(TRequest).Name;
        if (cachedResult is not null)
        {
            logger.LogInformation("Cache hit for {Query}", name);

            return cachedResult;
        }

        logger.LogInformation("Cache miss for {Query}", name);

        var result = await next();

        if (result.IsSuccess && IsCacheable(result))
        {
            var tags = ResolveTags(request, result);
            if (tags.Count > 0)
                await cacheService.SetAsync(request.CacheKey, result, request.Expiration, tags, cancellationToken);
            else
                await cacheService.SetAsync(request.CacheKey, result, request.Expiration, cancellationToken);
        }

        return result;
    }

    // A response that depends on who is asking (e.g. a draft post) must never enter the shared cache.
    private static bool IsCacheable(TResponse result)
    {
        var value = result.GetType().GetProperty("Value")?.GetValue(result);
        return value is not ICacheEligible { IsCacheable: false };
    }

    // Tags come from the query itself and, when the response value declares them, from the value too
    // (e.g. a post is tagged with the tags and categories it embeds).
    private static IReadOnlyCollection<string> ResolveTags(TRequest request, TResponse result)
    {
        var tags = new HashSet<string>(request.CacheTags);

        var value = result.GetType().GetProperty("Value")?.GetValue(result);
        if (value is ICacheTagged tagged)
            tags.UnionWith(tagged.CacheTags);

        return tags;
    }
}
