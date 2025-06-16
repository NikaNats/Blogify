using Blogify.Application.Abstractions.Behaviors;
using Blogify.Application.Abstractions.Caching;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Behaviors;

public class QueryCachingBehaviorTests
{
    private readonly ICacheService _cacheService;
    private readonly QueryCachingBehavior<TestCachedQuery, Result<string>> _cachingBehavior;
    private readonly ILogger<QueryCachingBehavior<TestCachedQuery, Result<string>>> _logger;
    private readonly RequestHandlerDelegate<Result<string>> _next;

    public QueryCachingBehaviorTests()
    {
        _cacheService = Substitute.For<ICacheService>();
        _logger = Substitute.For<ILogger<QueryCachingBehavior<TestCachedQuery, Result<string>>>>();
        _cachingBehavior = new QueryCachingBehavior<TestCachedQuery, Result<string>>(_cacheService, _logger);
        _next = Substitute.For<RequestHandlerDelegate<Result<string>>>();
    }

    [Fact]
    public async Task Handle_WhenCacheHit_Should_ReturnCachedResultAndNotCallNext()
    {
        // Arrange
        var query = new TestCachedQuery("test-key", TimeSpan.FromMinutes(5));
        var cachedResponse = Result.Success("cached data");

        _cacheService.GetAsync<Result<string>>(query.CacheKey, Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        // Act
        var result = await _cachingBehavior.Handle(query, _next, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("cached data");

        // Verify that the 'next' delegate (the actual handler) was never called
        await _next.DidNotReceive().Invoke();

        // Verify that the cache was read from but not written to
        await _cacheService.Received(1).GetAsync<Result<string>>(query.CacheKey, Arg.Any<CancellationToken>());
        await _cacheService.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<Result<string>>(), Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCacheMissAndHandlerSucceeds_Should_CallNextAndSetCache()
    {
        // Arrange
        var query = new TestCachedQuery("test-key", TimeSpan.FromMinutes(5));
        var handlerResponse = Result.Success("fresh data from handler");

        _cacheService.GetAsync<Result<string>>(query.CacheKey, Arg.Any<CancellationToken>())
            .Returns((Result<string>?)null); // Simulate cache miss
        _next.Invoke().Returns(handlerResponse);

        // Act
        var result = await _cachingBehavior.Handle(query, _next, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("fresh data from handler");

        // Verify that the 'next' delegate was called exactly once
        await _next.Received(1).Invoke();

        // Verify that the cache was set with the correct key, value, and expiration
        await _cacheService.Received(1)
            .SetAsync(query.CacheKey, handlerResponse, query.Expiration, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCacheMissAndHandlerFails_Should_CallNextAndNotSetCache()
    {
        // Arrange
        var query = new TestCachedQuery("test-key", TimeSpan.FromMinutes(5));
        var error = new Error("Handler.Error", "Handler failed", ErrorType.Failure);
        var handlerResponse = Result.Failure<string>(error);

        _cacheService.GetAsync<Result<string>>(query.CacheKey, Arg.Any<CancellationToken>())
            .Returns((Result<string>?)null); // Simulate cache miss
        _next.Invoke().Returns(handlerResponse);

        // Act
        var result = await _cachingBehavior.Handle(query, _next, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(error);

        // Verify that 'next' was called
        await _next.Received(1).Invoke();

        // Verify that SetAsync was NOT called because the result was a failure
        await _cacheService.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<Result<string>>(), Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }
}