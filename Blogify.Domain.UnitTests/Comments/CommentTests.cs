using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;

namespace Blogify.Domain.UnitTests.Comments
{
    public class CommentTests
    {
        // Test for successful creation of a Comment
        [Fact]
        public void Create_ValidInputs_ReturnsSuccessResultWithComment()
        {
            // Arrange
            var content = "This is a test comment.";
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();

            // Act
            var result = Comment.Create(content, authorId, postId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(content, result.Value.Content);
            Assert.Equal(authorId, result.Value.AuthorId);
            Assert.Equal(postId, result.Value.PostId);
            Assert.NotEqual(default, result.Value.CreatedAt);
        }

        // Test for empty content
        [Fact]
        public void Create_EmptyContent_ReturnsFailureResultWithNullValueError()
        {
            // Arrange
            var content = string.Empty;
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();

            // Act
            var result = Comment.Create(content, authorId, postId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(Error.NullValue, result.Error);
        }

        // Test for null content
        [Fact]
        public void Create_NullContent_ReturnsFailureResultWithNullValueError()
        {
            // Arrange
            string content = null;
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();

            // Act
            var result = Comment.Create(content, authorId, postId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(Error.NullValue, result.Error);
        }

        // Test for default authorId
        [Fact]
        public void Create_DefaultAuthorId_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            var content = "This is a test comment.";
            var authorId = default(Guid);
            var postId = Guid.NewGuid();

            // Act
            var result = Comment.Create(content, authorId, postId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Comment.AuthorId", result.Error.Code);
            Assert.Equal("AuthorId cannot be default.", result.Error.Description);
        }

        // Test for default postId
        [Fact]
        public void Create_DefaultPostId_ReturnsFailureResultWithValidationError()
        {
            // Arrange
            var content = "This is a test comment.";
            var authorId = Guid.NewGuid();
            var postId = Guid.Empty;

            // Act
            var result = Comment.Create(content, authorId, postId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Comment.PostId", result.Error.Code);
            Assert.Equal("PostId cannot be default.", result.Error.Description);
        }

        // Test for CreatedAt being set to current UTC time
        [Fact]
        public void Create_CommentCreatedAtSetToUtcNow()
        {
            // Arrange
            var content = "This is a test comment.";
            var authorId = Guid.NewGuid();
            var postId = Guid.NewGuid();
            var expectedTime = DateTime.UtcNow;

            // Act
            var result = Comment.Create(content, authorId, postId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.InRange(result.Value.CreatedAt, expectedTime.AddSeconds(-1), expectedTime.AddSeconds(1));
        }
    }
}