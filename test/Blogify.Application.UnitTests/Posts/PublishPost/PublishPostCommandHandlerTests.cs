using Blogify.Application.Posts.PublishPost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.PublishPost;

public class PublishPostCommandHandlerTests
{
    private readonly PublishPostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public PublishPostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new PublishPostCommandHandler(_postRepositoryMock, _unitOfWorkMock);
    }

    // A robust helper that creates a valid Post or throws if setup fails.
    private static Post CreateTestPost(PublicationStatus initialStatus = PublicationStatus.Draft)
    {
        var postResult = Post.Create(
            "Test Post",
            new string('a', 100),
            "An excerpt.",
            Guid.NewGuid());

        if (postResult.IsFailure) throw new InvalidOperationException("Test setup failed: could not create post.");

        var post = postResult.Value;
        if (initialStatus == PublicationStatus.Published) post.Publish();

        return post;
    }

    [Fact]
    public async Task Handle_WhenPostIsDraft_ShouldPublishPostAndSaveChanges()
    {
        // Arrange
        var post = CreateTestPost();
        var command = new PublishPostCommand(post.Id);

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.Status.ShouldBe(PublicationStatus.Published);

        // Verify that the status change was committed to the database.
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new PublishPostCommand(Guid.NewGuid());

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);

        // Verify no attempt was made to save changes.
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostIsAlreadyPublished_ShouldReturnFailureAndNotSaveChanges()
    {
        // Arrange
        var post = CreateTestPost(PublicationStatus.Published);
        var command = new PublishPostCommand(post.Id);

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.AlreadyPublished);

        // Verify no attempt was made to save changes.
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}