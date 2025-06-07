using Blogify.Application.Posts.AddCommentToPost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.AddComment;

public class AddCommentToPostCommandHandlerTests
{
    private readonly ICommentRepository _commentRepositoryMock;
    private readonly AddCommentToPostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public AddCommentToPostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _commentRepositoryMock = Substitute.For<ICommentRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _handler = new AddCommentToPostCommandHandler(
            _postRepositoryMock,
            _commentRepositoryMock,
            _unitOfWorkMock);
    }

    // Helper to create a valid Post entity for tests, simplifying Arrange blocks.
    private static Post CreateTestPost(bool isPublished = false)
    {
        var postResult = Post.Create(
            PostTitle.Create("Test Post").Value,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("An excerpt.").Value,
            Guid.NewGuid()
        );
        postResult.IsSuccess.ShouldBeTrue("Test setup failed: could not create post.");

        var post = postResult.Value;
        if (isPublished) post.Publish();

        return post;
    }

    [Fact]
    public async Task Handle_WhenPostIsPublished_ShouldAddCommentAndSaveChanges()
    {
        // Arrange
        var post = CreateTestPost(true);
        var command = new AddCommentToPostCommand(post.Id, "This is a valid comment.", Guid.NewGuid());

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify a new comment was added to the repository.
        await _commentRepositoryMock.Received(1).AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());

        // Verify the entire transaction was committed.
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostIsNotPublished_ShouldReturnFailure()
    {
        // Arrange
        var post = CreateTestPost(false); // Post is a draft
        var command = new AddCommentToPostCommand(post.Id, "This comment should be rejected.", Guid.NewGuid());

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.CommentToUnpublishedPost);

        // Verify no new comment was added and no changes were saved.
        await _commentRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "This comment will fail.", Guid.NewGuid());

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
    public async Task Handle_WhenCommentContentIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var post = CreateTestPost(true);
        var command = new AddCommentToPostCommand(post.Id, "", Guid.NewGuid()); // Invalid empty content

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.EmptyContent);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}