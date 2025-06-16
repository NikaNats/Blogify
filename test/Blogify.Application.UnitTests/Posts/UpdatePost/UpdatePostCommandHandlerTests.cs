using Blogify.Application.Posts.UpdatePost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.UpdatePost;

public class UpdatePostCommandHandlerTests
{
    private readonly UpdatePostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public UpdatePostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new UpdatePostCommandHandler(_postRepositoryMock, _unitOfWorkMock);
    }

    private static Post CreateTestPost(PublicationStatus initialStatus = PublicationStatus.Draft)
    {
        var postResult = Post.Create(
            "Initial Title",
            new string('a', 100),
            "Initial excerpt.",
            Guid.NewGuid());

        postResult.IsSuccess.ShouldBeTrue($"Test setup failed: {postResult.Error.Description}");

        var post = postResult.Value;
        if (initialStatus == PublicationStatus.Archived)
        {
            var archiveResult = post.Archive();
            archiveResult.IsSuccess.ShouldBeTrue("Test setup failed: could not archive post.");
        }

        post.ClearDomainEvents();
        return post;
    }

    // --- FIX: This helper now creates the command with primitive strings. ---
    private static UpdatePostCommand CreateValidCommand(Guid postId)
    {
        const string validContent =
            "This is a piece of updated content that is definitely, positively, and absolutely longer than the one hundred character minimum requirement for a post.";

        return new UpdatePostCommand(
            postId,
            "Updated Title",
            validContent,
            "Updated excerpt."
        );
    }

    [Fact]
    public async Task Handle_WhenPostExistsAndDataIsValid_ShouldUpdatePostAndSaveChanges()
    {
        // Arrange
        var post = CreateTestPost();
        var command = CreateValidCommand(post.Id);

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // --- FIX: The assertion now compares the primitive string values. ---
        post.Title.Value.ShouldBe(command.Title);
        post.Content.Value.ShouldBe(command.Content);

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateValidCommand(Guid.NewGuid());

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostIsArchived_ShouldReturnFailureAndNotSaveChanges()
    {
        // Arrange
        var post = CreateTestPost(PublicationStatus.Archived);
        var command = CreateValidCommand(post.Id);

        _postRepositoryMock.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.CannotUpdateArchived);

        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}