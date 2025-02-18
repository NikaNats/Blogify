using Blogify.Application.Exceptions;
using Blogify.Application.Posts.CreatePost;
using Blogify.Domain.Posts;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Blogify.Application.UnitTests.Posts.CreatePost;

public class CreatePostCommandHandlerTests
{
    private readonly CreatePostCommandHandler _handler;
    private readonly IPostRepository _postRepository;

    public CreatePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new CreatePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithPostId()
    {
        // Arrange
        var command = new CreatePostCommand(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value, // Exactly 100 characters
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid());

        _postRepository.AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _postRepository.Received(1).AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConcurrencyException_ReturnsFailure()
    {
        // Arrange
        var command = new CreatePostCommand(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value, // Valid content
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid());

        _postRepository
            .AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>())
            .Throws(new ConcurrencyException("Concurrency conflict", new Exception()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.Overlap);
        result.Invoking(r => r.Value).Should().Throw<InvalidOperationException>();
    }
}