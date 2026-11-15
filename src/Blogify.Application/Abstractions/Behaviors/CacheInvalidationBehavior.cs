using Blogify.Application.Abstractions.Caching;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Blogify.Application.Abstractions.Behaviors;

/// <summary>
/// Evicts cache entries declared by <see cref="ICacheInvalidatingCommand"/> once the command succeeds,
/// so subsequent reads never observe data older than the write that just completed.
/// </summary>
public sealed class CacheInvalidationBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheInvalidatingCommand
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (response.IsFailure)
            return response;

        foreach (var key in request.CacheKeysToInvalidate.Distinct())
        {
            await cacheService.RemoveAsync(key, cancellationToken);
            logger.LogDebug("Invalidated cache key {CacheKey} after {Command}", key, typeof(TRequest).Name);
        }

        foreach (var tag in request.CacheTagsToInvalidate.Distinct())
        {
            await cacheService.RemoveByTagAsync(tag, cancellationToken);
            logger.LogDebug("Invalidated cache tag {CacheTag} after {Command}", tag, typeof(TRequest).Name);
        }

        return response;
    }
}
