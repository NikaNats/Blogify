using Blogify.Application.Posts.RemoveTagFromPost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.RemoveTag;

public class RemoveTagFromPostCommandHandlerTests
{
    private readonly RemoveTagFromPostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly ITagRepository _tagRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public RemoveTagFromPostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _tagRepositoryMock = Substitute.For<ITagRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _handler = new RemoveTagFromPostCommandHandler(
            _postRepositoryMock,
            _tagRepositoryMock,
            _unitOfWorkMock);
    }

    // Helper to create a valid Post for tests.
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

    // Helper to create a valid Tag for tests.
    private static Tag CreateTestTag()
    {
        var tagResult = Tag.Create("TestTag");
        if (tagResult.IsFailure) throw new InvalidOperationException("Test setup failed: could not create tag.");
        return tagResult.Value;
    }

    [Fact]
    public async Task Handle_WhenPostAndTagExist_ShouldRemoveTagAndSaveChanges()
    {
        // Arrange
        var post = CreateTestPost();
        var tag = CreateTestTag();
        post.AddTag(tag); // Ensure the tag is on the post initially

        var command = new RemoveTagFromPostCommand(post.Id, tag.Id);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>())
            .Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify the tag was removed from the in-memory collection.
        post.Tags.ShouldNotContain(t => t.Id == tag.Id);

        // Verify the transaction was committed to persist this change.
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveTagFromPostCommand(Guid.NewGuid(), Guid.NewGuid());

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
    public async Task Handle_WhenTagNotFound_ShouldReturnFailure()
    {
        // Arrange
        var post = CreateTestPost();
        var command = new RemoveTagFromPostCommand(post.Id, Guid.NewGuid());

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagIsNotOnPost_ShouldReturnSuccessAndNotSaveChanges()
    {
        // Arrange
        var post = CreateTestPost(); // Post has no tags
        var tag = CreateTestTag();
        var command = new RemoveTagFromPostCommand(post.Id, tag.Id);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>())
            .Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // The domain logic is idempotent. Removing a non-existent tag is a success,
        // but it doesn't change state, so we should not commit a transaction.
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}