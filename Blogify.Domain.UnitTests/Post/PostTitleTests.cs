using Blogify.Domain.Posts;

namespace Blogify.Domain.UnitTests.Post
{
    public class PostTitleTests
    {
        [Fact]
        public void Create_ValidTitle_ReturnsSuccess()
        {
            // Arrange
            var title = "Valid Title";

            // Act
            var result = PostTitle.Create(title);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(title, result.Value.Value);
        }

        [Fact]
        public void Create_EmptyTitle_ReturnsFailure()
        {
            // Arrange
            var title = "";

            // Act
            var result = PostTitle.Create(title);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PostErrors.TitleEmpty, result.Error);
        }

        [Fact]
        public void Create_TitleTooLong_ReturnsFailure()
        {
            // Arrange
            var title = new string('a', 201);

            // Act
            var result = PostTitle.Create(title);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(PostErrors.TitleTooLong, result.Error);
        }
    }
}