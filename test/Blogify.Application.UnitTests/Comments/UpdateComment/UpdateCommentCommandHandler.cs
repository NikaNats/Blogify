using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Comments.UpdateComment;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Comments.UpdateComment;

public class UpdateCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly UpdateCommentCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public UpdateCommentCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _userContext = Substitute.For<IUserContext>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new UpdateCommentCommandHandler(_commentRepository, _userContext, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenCommentExistsAndUserIsAuthor_Should_SucceedAndUpdateComment()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new UpdateCommentCommand(Guid.NewGuid(), "This is the new, valid content.");

        // Create a comment with a known author
        var comment = Comment.Create("Original content", authorId, Guid.NewGuid()).Value;

        _commentRepository.GetByIdAsync(command.CommentId, Arg.Any<CancellationToken>())
            .Returns(comment);

        _userContext.UserId.Returns(authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Unit.Value);

        // Verify that the domain entity's content was updated
        comment.Content.Value.ShouldBe(command.Content);

        // Verify that the changes were saved
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommentDoesNotExist_Should_ReturnNotFoundError()
    {
        // Arrange
        var command = new UpdateCommentCommand(Guid.NewGuid(), "New content");

        _commentRepository.GetByIdAsync(command.CommentId, Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.NotFound);

        // Verify no changes were saved
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthor_Should_ReturnUnauthorizedError()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        var command = new UpdateCommentCommand(Guid.NewGuid(), "New content");

        var comment = Comment.Create("Original content", authorId, Guid.NewGuid()).Value;

        _commentRepository.GetByIdAsync(command.CommentId, Arg.Any<CancellationToken>())
            .Returns(comment);

        _userContext.UserId.Returns(unauthorizedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.UnauthorizedUpdate);

        // Verify no changes were saved
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_Should_ReturnDomainError()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        // Command with invalid content (empty string)
        var command = new UpdateCommentCommand(Guid.NewGuid(), "");

        var comment = Comment.Create("Original content", authorId, Guid.NewGuid()).Value;
        var originalContent = comment.Content;

        _commentRepository.GetByIdAsync(command.CommentId, Arg.Any<CancellationToken>())
            .Returns(comment);

        _userContext.UserId.Returns(authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        // The error should be the specific one from the domain logic
        result.Error.ShouldBe(CommentError.EmptyContent);

        // Verify the comment's content was not actually changed
        comment.Content.ShouldBe(originalContent);

        // Verify no changes were saved
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}