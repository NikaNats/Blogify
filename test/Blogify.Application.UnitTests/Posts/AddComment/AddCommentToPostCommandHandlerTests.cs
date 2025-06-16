using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Posts.AddCommentToPost;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using NSubstitute;
using Shouldly;
// Add this using

namespace Blogify.Application.UnitTests.Posts.AddComment;

public class AddCommentToPostCommandHandlerTests
{
    private readonly ICommentRepository _commentRepositoryMock;
    private readonly AddCommentToPostCommandHandler _handler;
    private readonly IPostRepository _postRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IUserContext _userContextMock; // --- FIX: Add mock for IUserContext ---

    public AddCommentToPostCommandHandlerTests()
    {
        _postRepositoryMock = Substitute.For<IPostRepository>();
        _commentRepositoryMock = Substitute.For<ICommentRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userContextMock = Substitute.For<IUserContext>(); // --- FIX: Instantiate the mock ---

        _handler = new AddCommentToPostCommandHandler(
            _postRepositoryMock,
            _commentRepositoryMock,
            _unitOfWorkMock,
            _userContextMock); // --- FIX: Inject the new dependency ---
    }

    // Helper to create a valid Post entity for tests, simplifying Arrange blocks.
    private static Post CreateTestPost(bool isPublished = false)
    {
        var postResult = Post.Create(
            "Test Post",
            new string('a', 100),
            "An excerpt.",
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
        var authenticatedUserId = Guid.NewGuid();

        // --- FIX: Create the command without the authorId ---
        var command = new AddCommentToPostCommand(post.Id, "This is a valid comment.");

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        // --- FIX: Simulate an authenticated user ---
        _userContextMock.UserId.Returns(authenticatedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify a new comment was added to the repository with the correct author ID from the context
        await _commentRepositoryMock.Received(1).AddAsync(
            Arg.Is<Comment>(c => c.AuthorId == authenticatedUserId),
            Arg.Any<CancellationToken>());

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostIsNotPublished_ShouldReturnFailure()
    {
        // Arrange
        var post = CreateTestPost(); // Post is a draft

        // --- FIX: Create the command without the authorId ---
        var command = new AddCommentToPostCommand(post.Id, "This comment should be rejected.");

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.CommentToUnpublishedPost);
        await _commentRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        // --- FIX: Create the command without the authorId ---
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "This comment will fail.");

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
        // --- FIX: Create the command without the authorId ---
        var command = new AddCommentToPostCommand(post.Id, ""); // Invalid empty content

        _postRepositoryMock.GetByIdAsync(command.PostId, Arg.Any<CancellationToken>())
            .Returns(post);

        // Simulate a user context, although it won't be used if the domain logic fails first
        _userContextMock.UserId.Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.EmptyContent);
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}