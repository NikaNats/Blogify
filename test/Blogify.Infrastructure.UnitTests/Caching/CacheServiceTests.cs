using System.Text;
using Blogify.Domain.Abstractions;
using Blogify.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Infrastructure.UnitTests.Caching;

public class CacheServiceTests
{
    private readonly MemoryDistributedCache _cache = new(Options.Create(new MemoryDistributedCacheOptions()));
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _sut = new CacheService(_cache, NullLogger<CacheService>.Instance);
    }

    public sealed record SampleResponse(Guid Id, string Title, List<string> Tags);

    [Fact]
    public async Task Successful_result_round_trips_with_its_value()
    {
        var ct = TestContext.Current.CancellationToken;
        var value = new SampleResponse(Guid.NewGuid(), "Hello", ["a", "b"]);

        await _sut.SetAsync("k", Result.Success(value), cancellationToken: ct);
        var cached = await _sut.GetAsync<Result<SampleResponse>>("k", ct);

        cached.ShouldNotBeNull();
        cached.IsSuccess.ShouldBeTrue();
        cached.Value.ShouldBe(value with { Tags = cached.Value.Tags });
        cached.Value.Tags.ShouldBe(["a", "b"]);
    }

    [Fact]
    public async Task Failed_result_round_trips_with_its_error()
    {
        var ct = TestContext.Current.CancellationToken;
        var error = Error.NotFound("Post.NotFound", "Post was not found");

        await _sut.SetAsync("k", Result.Failure<SampleResponse>(error), cancellationToken: ct);
        var cached = await _sut.GetAsync<Result<SampleResponse>>("k", ct);

        cached.ShouldNotBeNull();
        cached.IsFailure.ShouldBeTrue();
        cached.Error.ShouldBe(error);
    }

    [Fact]
    public async Task Non_generic_result_round_trips()
    {
        var ct = TestContext.Current.CancellationToken;

        await _sut.SetAsync("k", Result.Success(), cancellationToken: ct);
        var cached = await _sut.GetAsync<Result>("k", ct);

        cached.ShouldNotBeNull().IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Corrupt_entry_is_treated_as_miss_and_evicted()
    {
        var ct = TestContext.Current.CancellationToken;
        await _cache.SetAsync("k", Encoding.UTF8.GetBytes("{not json"), ct);

        var cached = await _sut.GetAsync<Result<SampleResponse>>("k", ct);

        cached.ShouldBeNull();
        (await _cache.GetAsync("k", ct)).ShouldBeNull();
    }

    [Fact]
    public async Task Cache_outage_is_treated_as_miss()
    {
        var ct = TestContext.Current.CancellationToken;
        var failingCache = Substitute.For<IDistributedCache>();
        failingCache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("redis down"));
        var sut = new CacheService(failingCache, NullLogger<CacheService>.Instance);

        var cached = await sut.GetAsync<Result<SampleResponse>>("k", ct);

        cached.ShouldBeNull();
    }
}
