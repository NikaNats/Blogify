using Blogify.Domain.Comments;
using Blogify.Domain.Comments.Events;
using Shouldly;

namespace Blogify.Domain.UnitTests.Comments;

public class CommentTests
{
    private const string ValidContent = "This is a valid test comment.";

    #region Update

    [Fact]
    public void Update_WithValidContent_ShouldChangeContent()
    {
        // Arrange
        var comment = TestFactory.CreateValidComment();
        const string newContent = "This is the updated content.";

        // Act
        var result = comment.Update(newContent);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        comment.Content.Value.ShouldBe(newContent);
    }

    #endregion

    private static class TestFactory
    {
        internal static Comment CreateValidComment(Guid? authorId = null)
        {
            return Comment.Create(
                ValidContent,
                authorId ?? Guid.NewGuid(),
                Guid.NewGuid()
            ).Value;
        }
    }

    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccessResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(ValidContent, authorId, postId);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var comment = result.Value;
        comment.ShouldNotBeNull();
        comment.Content.Value.ShouldBe(ValidContent);
        comment.AuthorId.ShouldBe(authorId);
        comment.PostId.ShouldBe(postId);
    }

    [Fact]
    public void Create_WithValidParameters_ShouldRaiseCommentAddedDomainEvent()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var comment = Comment.Create(ValidContent, authorId, postId).Value;

        // Assert
        var domainEvent = comment.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CommentAddedDomainEvent>();
        domainEvent.commentId.ShouldBe(comment.Id);
        domainEvent.postId.ShouldBe(postId);
        domainEvent.authorId.ShouldBe(authorId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidContent_ShouldReturnFailure(string invalidContent)
    {
        // Act
        var result = Comment.Create(invalidContent, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.EmptyContent);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111")]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000")]
    public void Create_WithEmptyGuids_ShouldReturnFailure(string authorIdStr, string postIdStr)
    {
        // Arrange
        var authorId = Guid.Parse(authorIdStr);
        var postId = Guid.Parse(postIdStr);

        // Act
        var result = Comment.Create(ValidContent, authorId, postId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOneOf(CommentError.EmptyAuthorId, CommentError.EmptyPostId);
    }

    #endregion

    #region Remove

    [Fact]
    public void Remove_WhenCalledByAuthor_ShouldSucceedAndRaiseEvent()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = TestFactory.CreateValidComment(authorId);
        comment.ClearDomainEvents();

        // Act
        var result = comment.Remove(authorId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var domainEvent = comment.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CommentDeletedDomainEvent>();
        domainEvent.CommentId.ShouldBe(comment.Id);
    }

    [Fact]
    public void Remove_WhenCalledByNonAuthor_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var nonAuthorId = Guid.NewGuid();
        var comment = TestFactory.CreateValidComment(authorId);
        comment.ClearDomainEvents();

        // Act
        var result = comment.Remove(nonAuthorId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CommentError.UnauthorizedDeletion);
        comment.DomainEvents.ShouldBeEmpty();
    }

    #endregion
}