using Blogify.Domain.Posts;

namespace Blogify.Domain.UnitTests.Post
{
    public class PostContentTests
    {
        [Fact]
        public void Create_ValidContent_ReturnsSuccess()
        {
            // Arrange
            const string content = "This is a valid content with more than 100 characters. " +
                                   "This is additional text to ensure the content length is greater than 100 characters.";

            // Act
            var result = PostContent.Create(content);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(content, result.Value.Value);
        }

        [Fact]
        public void Create_EmptyContent_ReturnsFailure()
        {
            // Arrange
            var content = "";

            // Act
            var result = PostContent.Create(content);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PostErrors.ContentEmpty, result.Error);
        }

        [Fact]
        public void Create_ContentTooShort_ReturnsFailure()
        {
            // Arrange
            var content = "Short";

            // Act
            var result = PostContent.Create(content);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PostErrors.ContentTooShort, result.Error);
        }
    }
}