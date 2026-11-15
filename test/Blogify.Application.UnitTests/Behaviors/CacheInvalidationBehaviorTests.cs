using Blogify.Application.Abstractions.Behaviors;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts.DeletePost;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Behaviors;

public class CacheInvalidationBehaviorTests
{
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly CacheInvalidationBehavior<TestInvalidatingCommand, Result> _behavior;

    public CacheInvalidationBehaviorTests()
    {
        _behavior = new CacheInvalidationBehavior<TestInvalidatingCommand, Result>(
            _cacheService,
            NullLogger<CacheInvalidationBehavior<TestInvalidatingCommand, Result>>.Instance);
    }

    [Fact]
    public async Task Handle_WhenCommandSucceeds_Should_EvictEveryDistinctKey()
    {
        var command = new TestInvalidatingCommand(["a", "b", "a"]);

        var result = await _behavior.Handle(command, _ => Task.FromResult(Result.Success()), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _cacheService.Received(1).RemoveAsync("a", Arg.Any<CancellationToken>());
        await _cacheService.Received(1).RemoveAsync("b", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommandFails_Should_NotEvict()
    {
        var command = new TestInvalidatingCommand(["a"]);
        var failure = Result.Failure(Error.Failure("Test.Failed", "failed"));

        var result = await _behavior.Handle(command, _ => Task.FromResult(failure), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        await _cacheService.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Post_commands_Should_InvalidateTheCachedPost()
    {
        var postId = Guid.NewGuid();

        new DeletePostCommand(postId).CacheKeysToInvalidate.ShouldBe([CacheKeys.Post(postId)]);
    }

    public sealed record TestInvalidatingCommand(IReadOnlyList<string> Keys) : ICommand, ICacheInvalidatingCommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => Keys;
    }
}
