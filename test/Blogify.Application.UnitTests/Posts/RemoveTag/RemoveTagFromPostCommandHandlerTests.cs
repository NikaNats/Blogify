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

    [Fact]
    public async Task Handle_WhenPostAndTagExist_ShouldRemoveTagAndSaveChanges()
    {
        // Arrange
        var post = TestFactory.CreatePost();
        var tag = TestFactory.CreateTag();
        post.AddTag(tag); // Ensure the tag is on the post initially
        post.ClearDomainEvents(); // Isolate the 'remove' action

        var command = new RemoveTagFromPostCommand(post.Id, tag.Id);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // --- FIXED: Assert on the correct ID-based collection ---
        post.TagIds.ShouldNotContain(tag.Id);

        // Verify the transaction was committed because a domain event was raised.
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveTagFromPostCommand(Guid.NewGuid(), Guid.NewGuid());
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
        var command = new RemoveTagFromPostCommand(post.Id, Guid.NewGuid());
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
    public async Task Handle_WhenTagIsNotOnPost_ShouldSucceedAndNotSaveChanges()
    {
        // Arrange
        var post = TestFactory.CreatePost(); // Post has no tags
        var tag = TestFactory.CreateTag();
        var command = new RemoveTagFromPostCommand(post.Id, tag.Id);

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>()).Returns(post);
        _tagRepositoryMock.GetByIdAsync(command.TagId, Arg.Any<CancellationToken>()).Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // The domain logic is idempotent. Removing a non-existent tag is a success,
        // but it doesn't raise a domain event, so the handler should not commit a transaction.
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #region TestFactory

    private static class TestFactory
    {
        internal static Post CreatePost()
        {
            var postResult = Post.Create(
                "Test Post",
                new string('a', 101),
                "An excerpt.",
                Guid.NewGuid());
            if (postResult.IsFailure) throw new InvalidOperationException("Test setup failed: could not create post.");
            return postResult.Value;
        }

        internal static Tag CreateTag()
        {
            var tagResult = Tag.Create("TestTag");
            if (tagResult.IsFailure) throw new InvalidOperationException("Test setup failed: could not create tag.");
            return tagResult.Value;
        }
    }

    #endregion
}