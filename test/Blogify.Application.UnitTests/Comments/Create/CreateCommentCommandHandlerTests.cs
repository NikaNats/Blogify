using System.Linq.Expressions;
using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Comments.CreateComment;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Users;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Comments.Create;

public class CreateCommentCommandHandlerTests
{
    #region Setup

    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCommentCommandHandler _handler;

    private static Post CreateTestPost(PublicationStatus status = PublicationStatus.Published)
    {
        var postResult = Post.Create("Test Title", new string('a', 101), "Test Excerpt", Guid.NewGuid());
        if (postResult.IsFailure) throw new InvalidOperationException("Failed to create test post.");

        switch (status)
        {
            case PublicationStatus.Published:
                postResult.Value.Publish();
                break;
            case PublicationStatus.Archived:
                postResult.Value.Archive();
                break;
        }

        return postResult.Value;
    }

    public CreateCommentCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _userContext = Substitute.For<IUserContext>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CreateCommentCommandHandler(
            _userContext,
            _userRepository,
            _postRepository,
            _commentRepository,
            _unitOfWork);
    }

    #endregion
    [Fact]
    public async Task Handle_WithValidRequest_ShouldSucceedAndReturnCommentId()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var testPost = CreateTestPost();
        var command = new CreateCommentCommand(testPost.Id, "This is a valid comment");

        _userContext.UserId.Returns(authorId);

        _postRepository.GetByIdAsync(testPost.Id, Arg.Any<CancellationToken>())
            .Returns(testPost);

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _commentRepository.Received(1).AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.NewGuid(), "Valid content");
        _userContext.UserId.Returns(Guid.NewGuid());

        _postRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Post?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPostIsNotPublished_ShouldReturnFailure()
    {
        // Arrange
        var testPost = CreateTestPost(PublicationStatus.Draft); // Create a real, draft post
        var command = new CreateCommentCommand(testPost.Id, "Valid content");
        _userContext.UserId.Returns(Guid.NewGuid());

        _postRepository.GetByIdAsync(testPost.Id, Arg.Any<CancellationToken>())
            .Returns(testPost);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(PostErrors.CommentToUnpublishedPost);
    }

    [Fact]
    public async Task Handle_WhenAuthorNotFound_ShouldReturnFailure()
    {
        // Arrange
        var testPost = CreateTestPost();
        var command = new CreateCommentCommand(testPost.Id, "Valid content");
        _userContext.UserId.Returns(Guid.NewGuid());

        _postRepository.GetByIdAsync(testPost.Id, Arg.Any<CancellationToken>())
            .Returns(testPost);

        _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.UserNotFound);
    }
}