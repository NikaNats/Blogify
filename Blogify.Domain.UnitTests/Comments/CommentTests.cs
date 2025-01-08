using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Comments.Events;
using Xunit;

namespace Blogify.Domain.UnitTests.Comments;

public class CommentTests
{
    // Test for successful creation of a Comment with valid inputs
    [Fact]
    public void Create_ValidInputs_ReturnsSuccessResultWithComment()
    {
        // Arrange
        const string content = "This is a test comment.";
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(content, result.Value.Content.Value);
        Assert.Equal(authorId, result.Value.AuthorId);
        Assert.Equal(postId, result.Value.PostId);
        Assert.NotEqual(default, result.Value.CreatedAt);
    }

    // Test for empty content
    [Fact]
    public void Create_EmptyContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var content = string.Empty;
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.InvalidContent.Code, result.Error.Code);
        Assert.Equal(CommentError.InvalidContent.Description, result.Error.Description);
    }

    // Test for null content
    [Fact]
    public void Create_NullContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        string content = null;
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.InvalidContent.Code, result.Error.Code);
        Assert.Equal(CommentError.InvalidContent.Description, result.Error.Description);
    }

    // Test for default authorId
    [Fact]
    public void Create_DefaultAuthorId_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        const string content = "This is a test comment.";
        var authorId = Guid.Empty;
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyAuthorId.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyAuthorId.Description, result.Error.Description);
    }

    // Test for default postId
    [Fact]
    public void Create_DefaultPostId_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        const string content = "This is a test comment.";
        var authorId = Guid.NewGuid();
        var postId = Guid.Empty;

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyPostId.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyPostId.Description, result.Error.Description);
    }

    // Test for CreatedAt being set to current UTC time
    [Fact]
    public void Create_CommentCreatedAtSetToUtcNow()
    {
        // Arrange
        const string content = "This is a test comment.";
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var expectedTime = DateTime.UtcNow;

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.CreatedAt, expectedTime.AddSeconds(-1), expectedTime.AddSeconds(1));
    }

    // Test for updating a Comment's content with valid content
    [Fact]
    public void Update_ValidContent_ReturnsSuccessResult()
    {
        // Arrange
        var comment = Comment.Create("Original Content", Guid.NewGuid(), Guid.NewGuid()).Value;
        const string newContent = "Updated Content";

        // Act
        var result = comment.Update(newContent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newContent, comment.Content.Value);
    }

    // Test for updating a Comment's content with invalid content (empty)
    [Fact]
    public void Update_EmptyContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var comment = Comment.Create("Original Content", Guid.NewGuid(), Guid.NewGuid()).Value;
        var newContent = string.Empty;

        // Act
        var result = comment.Update(newContent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.InvalidContent.Code, result.Error.Code);
        Assert.Equal(CommentError.InvalidContent.Description, result.Error.Description);
    }

    // Test for updating a Comment's content with invalid content (null)
    [Fact]
    public void Update_NullContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var comment = Comment.Create("Original Content", Guid.NewGuid(), Guid.NewGuid()).Value;
        string newContent = null;

        // Act
        var result = comment.Update(newContent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.InvalidContent.Code, result.Error.Code);
        Assert.Equal(CommentError.InvalidContent.Description, result.Error.Description);
    }

    // Test for removing a Comment by the author
    [Fact]
    public void Remove_ByAuthor_ReturnsSuccessResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = Comment.Create("Original Content", authorId, Guid.NewGuid()).Value;

        // Act
        var result = comment.Remove(authorId);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // Test for removing a Comment by a non-author
    [Fact]
    public void Remove_ByNonAuthor_ReturnsFailureResultWithUnauthorizedError()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var nonAuthorId = Guid.NewGuid();
        var comment = Comment.Create("Original Content", authorId, Guid.NewGuid()).Value;

        // Act
        var result = comment.Remove(nonAuthorId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(CommentError.UnauthorizedDeletion.Code, result.Error.Code);
        Assert.Equal(CommentError.UnauthorizedDeletion.Description, result.Error.Description);
    }

    // Test for domain event being raised on Comment creation
    [Fact]
    public void Create_RaisesCommentAddedDomainEvent()
    {
        // Arrange
        const string content = "This is a test comment.";
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.DomainEvents);
        Assert.IsType<CommentAddedDomainEvent>(result.Value.DomainEvents.First());
    }

    // Test for domain event being raised on Comment removal
    [Fact]
    public void Remove_RaisesCommentRemovedDomainEvent()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = Comment.Create("Original Content", authorId, Guid.NewGuid()).Value;

        // Clear domain events raised during creation
        comment.ClearDomainEvents();

        // Act
        var result = comment.Remove(authorId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(comment.DomainEvents);
        Assert.IsType<CommentRemovedDomainEvent>(comment.DomainEvents.First());
    }

    // Test for content exceeding maximum length
    [Fact]
    public void Create_ContentExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var content = new string('a', 1001); // Assume MaxLength is 1000
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(content, authorId, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.ContentTooLong.Code, result.Error.Code);
        Assert.Equal(CommentError.ContentTooLong.Description, result.Error.Description);
    }

    // Test for updating content exceeding maximum length
    [Fact]
    public void Update_ContentExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var comment = Comment.Create("Original Content", Guid.NewGuid(), Guid.NewGuid()).Value;
        var newContent = new string('a', 1001); // Assume MaxLength is 1000

        // Act
        var result = comment.Update(newContent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.ContentTooLong.Code, result.Error.Code);
        Assert.Equal(CommentError.ContentTooLong.Description, result.Error.Description);
    }
}