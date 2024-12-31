using Blogify.Domain.Posts;

namespace Blogify.Domain.UnitTests.Post
{
    public class PostSlugTests
    {
        [Fact]
        public void Create_ValidTitle_ReturnsSuccess()
        {
            // Arrange
            var title = "Valid Title";

            // Act
            var result = PostSlug.Create(title);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("valid-title", result.Value.Value);
        }

        [Fact]
        public void Create_EmptyTitle_ReturnsFailure()
        {
            // Arrange
            var title = "";

            // Act
            var result = PostSlug.Create(title);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PostErrors.SlugEmpty, result.Error);
        }

        [Fact]
        public void Create_TitleTooLong_ReturnsFailure()
        {
            // Arrange
            var title = new string('a', 201);

            // Act
            var result = PostSlug.Create(title);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PostErrors.SlugTooLong, result.Error);
        }
    }
}