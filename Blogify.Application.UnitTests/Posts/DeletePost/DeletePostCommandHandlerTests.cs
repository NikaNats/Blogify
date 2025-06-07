using Blogify.Application.Exceptions;
using Blogify.Application.Posts.DeletePost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.DeletePost;

public class DeletePostCommandHandlerTests
{
    private readonly DeletePostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public DeletePostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new DeletePostCommandHandler(_postRepositoryMock, _unitOfWorkMock);
    }

    private static Post CreateTestPost()
    {
        var postResult = Post.Create(
            PostTitle.Create("Test Post").Value,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("An excerpt.").Value,
            Guid.NewGuid());
        if (postResult.IsFailure) throw new InvalidOperationException("Test setup failed: could not create post.");
        return postResult.Value;
    }

    [Fact]
    public async Task Handle_WhenPostExists_ShouldDeletePostAndSaveChanges()
    {
        // Arrange
        var post = CreateTestPost();
        var command = new DeletePostCommand(post.Id);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _postRepositoryMock.Received(1).DeleteAsync(post, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailureAndNotSaveChanges()
    {
        // Arrange
        var command = new DeletePostCommand(Guid.NewGuid());

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDbThrowsConcurrencyException_ShouldReturnOverlapError()
    {
        // Arrange
        var post = CreateTestPost();
        var command = new DeletePostCommand(post.Id);
        var concurrencyException = new ConcurrencyException("Concurrency conflict.", new Exception());

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        _unitOfWorkMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(concurrencyException);

        // Act
        // FIX: We no longer expect an unhandled exception. We expect the handler
        // to catch it and return a proper failed Result.
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.Overlap);
    }
}