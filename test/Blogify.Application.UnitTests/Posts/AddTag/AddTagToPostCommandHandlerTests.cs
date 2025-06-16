using Blogify.Application.Posts.AddTagToPost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.AddTag;

public class AddTagToPostCommandHandlerTests
{
    private readonly AddTagToPostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly ITagRepository _tagRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public AddTagToPostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _tagRepositoryMock = Substitute.For<ITagRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _handler = new AddTagToPostCommandHandler(
            _postRepositoryMock,
            _tagRepositoryMock,
            _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_WhenPostAndTagExist_ShouldAddTagAndSaveChanges()
    {
        // Arrange
        var post = TestFactory.CreatePost();
        var tag = TestFactory.CreateTag();
        var command = new AddTagToPostCommand(post.Id, tag.Id);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // --- FIXED: Assert on the collection of IDs, not entities. ---
        post.TagIds.ShouldContain(tag.Id);

        // Verify the entire transaction was committed. This is the crucial check,
        // and relies on the handler correctly detecting that a domain event was raised.
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddTagToPostCommand(Guid.NewGuid(), Guid.NewGuid());
        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns((Post?)null);

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
        var post = TestFactory.CreatePost();
        var command = new AddTagToPostCommand(post.Id, Guid.NewGuid());
        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NotFound);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTagAlreadyOnPost_ShouldSucceedAndNotSaveChanges()
    {
        // Arrange
        var post = TestFactory.CreatePost();
        var tag = TestFactory.CreateTag();
        post.AddTag(tag); // Pre-add the tag to the post.
        post.ClearDomainEvents(); // Clear event from the initial setup add.

        var command = new AddTagToPostCommand(post.Id, tag.Id);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // The domain logic is idempotent. If the tag is already there,
        // no change should occur, and thus the handler should not commit the transaction.
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #region TestFactory

    private static class TestFactory
    {
        // Helper to create a valid Post for tests.
        internal static Post CreatePost()
        {
            var postResult = Post.Create(
                "Test Post",
                new string('a', 101),
                "An excerpt.",
                Guid.NewGuid());
            postResult.IsSuccess.ShouldBeTrue("Test setup failed: could not create post.");
            return postResult.Value;
        }

        // Helper to create a valid Tag for tests.
        internal static Tag CreateTag()
        {
            var tagResult = Tag.Create("TestTag");
            tagResult.IsSuccess.ShouldBeTrue("Test setup failed: could not create tag.");
            return tagResult.Value;
        }
    }

    #endregion
}