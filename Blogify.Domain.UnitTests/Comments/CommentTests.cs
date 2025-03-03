using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Comments.Events;

namespace Blogify.Domain.UnitTests.Comments;

public class CommentTests
{
    // Constants for test data
    private const string ValidContent = "This is a test comment.";
    private const int MaxContentLength = 1000;

    #region Creation Tests

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_ValidInputs_ReturnsSuccessResultWithComment()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(ValidContent, authorId, postId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(ValidContent, result.Value.Content.Value);
        Assert.Equal(authorId, result.Value.AuthorId);
        Assert.Equal(postId, result.Value.PostId);
        Assert.NotEqual(default, result.Value.CreatedAt);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_EmptyContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(string.Empty, authorId, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyContent.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyContent.Description, result.Error.Description);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NullContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(null, authorId, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyContent.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyContent.Description, result.Error.Description);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_DefaultAuthorId_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(ValidContent, Guid.Empty, postId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyAuthorId.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyAuthorId.Description, result.Error.Description);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_DefaultPostId_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var authorId = Guid.NewGuid();

        // Act
        var result = Comment.Create(ValidContent, authorId, Guid.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyPostId.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyPostId.Description, result.Error.Description);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_SetsCreatedAtAndLastModifiedAtEqual()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(ValidContent, authorId, postId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.CreatedAt, result.Value.LastModifiedAt);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_ContentExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var content = new string('a', MaxContentLength + 1);
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

    #endregion

    #region Update Tests

    [Fact]
    [Trait("Category", "Update")]
    public void Update_ValidContent_ReturnsSuccessResultAndUpdatesLastModifiedAt()
    {
        // Arrange
        var comment = Comment.Create(ValidContent, Guid.NewGuid(), Guid.NewGuid()).Value;
        var originalLastModifiedAt = comment.LastModifiedAt;
        const string newContent = "Updated Content";

        // Act
        Thread.Sleep(10); // Ensure measurable time difference
        var result = comment.Update(newContent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newContent, comment.Content.Value);
        Assert.True(comment.LastModifiedAt > originalLastModifiedAt);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_EmptyContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var comment = Comment.Create(ValidContent, Guid.NewGuid(), Guid.NewGuid()).Value;

        // Act
        var result = comment.Update(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyContent.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyContent.Description, result.Error.Description);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_NullContent_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var comment = Comment.Create(ValidContent, Guid.NewGuid(), Guid.NewGuid()).Value;

        // Act
        var result = comment.Update(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.EmptyContent.Code, result.Error.Code);
        Assert.Equal(CommentError.EmptyContent.Description, result.Error.Description);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_ContentExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var comment = Comment.Create(ValidContent, Guid.NewGuid(), Guid.NewGuid()).Value;
        var newContent = new string('a', MaxContentLength + 1);

        // Act
        var result = comment.Update(newContent);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CommentError.ContentTooLong.Code, result.Error.Code);
        Assert.Equal(CommentError.ContentTooLong.Description, result.Error.Description);
    }

    #endregion

    #region Removal Tests

    [Fact]
    [Trait("Category", "Removal")]
    public void Remove_ByAuthor_ReturnsSuccessResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var comment = Comment.Create(ValidContent, authorId, Guid.NewGuid()).Value;
        comment.ClearDomainEvents(); // Reset events from creation

        // Act
        var result = comment.Remove(authorId);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    [Trait("Category", "Removal")]
    public void Remove_ByNonAuthor_ReturnsFailureResultWithUnauthorizedError()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var nonAuthorId = Guid.NewGuid();
        var comment = Comment.Create(ValidContent, authorId, Guid.NewGuid()).Value;
        comment.ClearDomainEvents();

        // Act
        var result = comment.Remove(nonAuthorId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(CommentError.UnauthorizedDeletion.Code, result.Error.Code);
        Assert.Equal(CommentError.UnauthorizedDeletion.Description, result.Error.Description);
    }

    #endregion

    #region Domain Event Tests

    [Fact]
    [Trait("Category", "Events")]
    public void Create_RaisesCommentAddedDomainEvent_WithCorrectProperties()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        // Act
        var result = Comment.Create(ValidContent, authorId, postId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.DomainEvents);
        var domainEvent = Assert.IsType<CommentAddedDomainEvent>(result.Value.DomainEvents.First());
        Assert.Equal(result.Value.Id, domainEvent.commentId);
        Assert.Equal(postId, domainEvent.postId);
        Assert.Equal(authorId, domainEvent.authorId);
    }

    [Fact]
    [Trait("Category", "Events")]
    public void Remove_RaisesCommentRemovedDomainEvent_WithCorrectProperties()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var comment = Comment.Create(ValidContent, authorId, postId).Value;
        comment.ClearDomainEvents();

        // Act
        var result = comment.Remove(authorId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(comment.DomainEvents);
        var domainEvent = Assert.IsType<CommentDeletedDomainEvent>(comment.DomainEvents.First());
        Assert.Equal(comment.Id, domainEvent.CommentId);
        Assert.Equal(postId, domainEvent.PostId);
    }

    #endregion

    #region Equality Tests

    [Fact]
    [Trait("Category", "Equality")]
    public void Comment_IsEqualToItself()
    {
        // Arrange
        var comment = Comment.Create(ValidContent, Guid.NewGuid(), Guid.NewGuid()).Value;

        // Act & Assert
        Assert.Equal(comment, comment);
        Assert.True(comment.Equals(comment));
    }

    [Fact]
    [Trait("Category", "Equality")]
    public void Comments_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var comment1 = Comment.Create(ValidContent, Guid.NewGuid(), Guid.NewGuid()).Value;
        var comment2 = Comment.Create(ValidContent, Guid.NewGuid(), Guid.NewGuid()).Value;

        // Act & Assert
        Assert.NotEqual(comment1, comment2);
        Assert.False(comment1.Equals(comment2));
    }

    #endregion

    #region CommentContent Tests

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_ValidContent_Succeeds()
    {
        // Arrange
        const string content = ValidContent;

        // Act
        var result = CommentContent.Create(content);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(content, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_EmptyContent_Fails()
    {
        // Act
        var result = CommentContent.Create(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CommentError.EmptyContent.Code, result.Error.Code);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_NullContent_Fails()
    {
        // Act
        var result = CommentContent.Create(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CommentError.EmptyContent.Code, result.Error.Code);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_ContentTooLong_Fails()
    {
        // Arrange
        var content = new string('a', MaxContentLength + 1);

        // Act
        var result = CommentContent.Create(content);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CommentError.ContentTooLong.Code, result.Error.Code);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Equality_SameValue_AreEqual()
    {
        // Arrange
        var content1 = CommentContent.Create(ValidContent).Value;
        var content2 = CommentContent.Create(ValidContent).Value;

        // Act & Assert
        Assert.Equal(content1, content2);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Equality_DifferentValue_AreNotEqual()
    {
        // Arrange
        var content1 = CommentContent.Create("Content1").Value;
        var content2 = CommentContent.Create("Content2").Value;

        // Act & Assert
        Assert.NotEqual(content1, content2);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_ImplicitConversion_ToString()
    {
        // Arrange
        var content = CommentContent.Create(ValidContent).Value;

        // Act
        string stringValue = content;

        // Assert
        Assert.Equal(ValidContent, stringValue);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_MaxLengthContent_Succeeds()
    {
        // Arrange
        var content = new string('a', MaxContentLength);

        // Act
        var result = CommentContent.Create(content);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(content, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_SingleCharacter_Succeeds()
    {
        // Arrange
        const string content = "a";

        // Act
        var result = CommentContent.Create(content);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(content, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_OnlyWhitespace_Fails()
    {
        // Arrange
        const string content = "   ";

        // Act
        var result = CommentContent.Create(content);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CommentError.EmptyContent.Code, result.Error.Code);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Create_WhitespaceTrimmed()
    {
        // Arrange
        const string untrimmed = "  hello  ";
        const string expected = "hello";

        // Act
        var result = CommentContent.Create(untrimmed);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "CommentContent")]
    public void CommentContent_Equality_DifferentUntrimmedSameTrimmed_AreEqual()
    {
        // Arrange
        var content1 = CommentContent.Create("  hello  ").Value;
        var content2 = CommentContent.Create("hello").Value;

        // Act & Assert
        Assert.Equal(content1, content2);
    }

    #endregion
}