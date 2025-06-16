using Blogify.Application.Posts.ArchivePost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.ArchivePost;

public class ArchivePostCommandHandlerTests
{
    private readonly ArchivePostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public ArchivePostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new ArchivePostCommandHandler(_postRepositoryMock, _unitOfWorkMock);
    }

    // A robust helper that creates a valid Post or throws if setup fails.
    private static Post CreateTestPost(PublicationStatus initialStatus = PublicationStatus.Draft)
    {
        var postResult = Post.Create(
            "Valid Title",
            new string('a', 100),
            "Valid excerpt.",
            Guid.NewGuid());

        // A setup helper should ensure success, not assert.
        if (postResult.IsFailure) throw new InvalidOperationException("Test setup failed: could not create post.");

        var post = postResult.Value;

        // Set the initial status for the test scenario
        if (initialStatus == PublicationStatus.Published) post.Publish();
        if (initialStatus == PublicationStatus.Archived) post.Archive();

        return post;
    }

    [Fact]
    public async Task Handle_WhenPostExistsAsDraft_ShouldArchivePostAndSaveChanges()
    {
        // Arrange
        var post = CreateTestPost();
        var command = new ArchivePostCommand(post.Id);

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        post.Status.ShouldBe(PublicationStatus.Archived);

        // Verify the change was committed.
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new ArchivePostCommand(Guid.NewGuid());

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

// ... inside ArchivePostCommandHandlerTests class

    [Fact]
    public async Task Handle_WhenPostIsAlreadyArchived_ShouldReturnSuccessAndNotSaveChanges()
    {
        // Arrange
        // Helper creates the post and archives it.
        var post = CreateTestPost(PublicationStatus.Archived);
        // Clear any events raised during setup.
        post.ClearDomainEvents();

        var command = new ArchivePostCommand(post.Id);

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // This assertion will now pass because the fixed handler only saves when
        // a PostArchivedDomainEvent is raised. In this case, it won't be.
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}