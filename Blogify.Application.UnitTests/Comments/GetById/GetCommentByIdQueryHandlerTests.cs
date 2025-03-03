using Blogify.Application.Comments.GetCommentById;
using Blogify.Domain.Comments;
using FluentAssertions;
using NSubstitute;

namespace Blogify.Application.UnitTests.Comments.GetById;

public class GetCommentByIdQueryHandlerTests
{
    private readonly ICommentRepository _commentRepositoryMock;
    private readonly GetCommentByIdQueryHandler _handler;

    public GetCommentByIdQueryHandlerTests()
    {
        _commentRepositoryMock = Substitute.For<ICommentRepository>();
        _handler = new GetCommentByIdQueryHandler(_commentRepositoryMock);
    }

    [Fact]
    public async Task Handle_ValidCommentId_ReturnsCommentResponse()
    {
        // Arrange
        var comment = Comment.Create("Valid content", Guid.NewGuid(), Guid.NewGuid()).Value;
        _commentRepositoryMock.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var query = new GetCommentByIdQuery(comment.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(comment.Id);
        result.Value.Content.Should().Be(comment.Content.Value);
        result.Value.AuthorId.Should().Be(comment.AuthorId);
        result.Value.PostId.Should().Be(comment.PostId);
        result.Value.CreatedAt.Should().Be(comment.CreatedAt);
        await _commentRepositoryMock.Received(1).GetByIdAsync(comment.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CommentNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _commentRepositoryMock.GetByIdAsync(commentId, Arg.Any<CancellationToken>()).Returns((Comment)null);

        var query = new GetCommentByIdQuery(commentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentError.NotFound);
        await _commentRepositoryMock.Received(1).GetByIdAsync(commentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyCommentId_ReturnsValidationError()
    {
        // Arrange
        var query = new GetCommentByIdQuery(Guid.Empty);
        _commentRepositoryMock.GetByIdAsync(Guid.Empty, Arg.Any<CancellationToken>()).Returns((Comment)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentError.NotFound);
        await _commentRepositoryMock.Received().GetByIdAsync(Guid.Empty, Arg.Any<CancellationToken>());
    }
}