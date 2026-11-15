using Blogify.Application.Comments.GetCommentById;
using Blogify.Domain.Comments;
using NSubstitute;
using Shouldly;

namespace Blogify.Application.UnitTests.Comments.GetById;

public class GetCommentByIdQueryHandlerTests
{
    private readonly ICommentRepository _commentRepositoryMock;
    private readonly GetCommentByIdQueryHandler _handler;

    public GetCommentByIdQueryHandlerTests()
    {
        _commentRepositoryMock = Substitute.For<ICommentRepository>();
        _handler = new GetCommentByIdQueryHandler(_commentRepositoryMock, PublishedPostRepository(), Substitute.For<Blogify.Application.Abstractions.Authentication.IUserContext>());
    }

    private static Blogify.Domain.Posts.IPostRepository PublishedPostRepository()
    {
        var posts = Substitute.For<Blogify.Domain.Posts.IPostRepository>();
        posts.GetVisibilityAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(
            new Blogify.Domain.Posts.PostVisibility(Blogify.Domain.Posts.PublicationStatus.Published, Guid.NewGuid()));
        return posts;
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
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(comment.Id);
        result.Value.Content.ShouldBe(comment.Content.Value);
        result.Value.AuthorId.ShouldBe(comment.AuthorId);
        result.Value.PostId.ShouldBe(comment.PostId);
        result.Value.CreatedAt.ShouldBe(comment.CreatedAt);
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
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.NotFound);
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
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.NotFound);
        await _commentRepositoryMock.Received().GetByIdAsync(Guid.Empty, Arg.Any<CancellationToken>());
    }
}