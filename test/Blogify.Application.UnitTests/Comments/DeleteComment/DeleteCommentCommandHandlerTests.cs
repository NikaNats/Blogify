using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Comments.DeleteComment;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Comments.DeleteComment;

public class DeleteCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly DeleteCommentCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public DeleteCommentCommandHandlerTests()
    {
        // Mock all dependencies of the handler
        _commentRepository = Substitute.For<ICommentRepository>();
        _userContext = Substitute.For<IUserContext>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new DeleteCommentCommandHandler(_commentRepository, _userContext, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenCommentExistsAndUserIsAuthor_Should_SucceedAndSaveChanges()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new DeleteCommentCommand(Guid.NewGuid());

        // Create a test comment that will be "found" by the repository
        var comment = Comment.Create("A valid comment", authorId, Guid.NewGuid()).Value;

        // Mock the repository to return the comment
        _commentRepository.GetByIdAsync(command.CommentId, Arg.Any<CancellationToken>())
            .Returns(comment);

        // Mock the user context to simulate the author making the request
        _userContext.UserId.Returns(authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Unit.Value);

        // Verify that the DeleteAsync and SaveChangesAsync methods were called
        await _commentRepository.Received(1).DeleteAsync(comment, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_Should_ReturnNotFoundError()
    {
        // Arrange
        var command = new DeleteCommentCommand(Guid.NewGuid());

        // Mock the repository to return null, simulating a "not found" scenario
        _commentRepository.GetByIdAsync(command.CommentId, Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.NotFound);

        // Verify that no write operations were attempted
        await _commentRepository.DidNotReceive().DeleteAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthor_Should_ReturnUnauthorizedError()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        var command = new DeleteCommentCommand(Guid.NewGuid());

        var comment = Comment.Create("A valid comment", authorId, Guid.NewGuid()).Value;

        _commentRepository.GetByIdAsync(command.CommentId, Arg.Any<CancellationToken>())
            .Returns(comment);

        // Mock the user context to simulate a different user making the request
        _userContext.UserId.Returns(unauthorizedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.UnauthorizedDeletion);

        // Verify that no write operations were attempted
        await _commentRepository.DidNotReceive().DeleteAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}