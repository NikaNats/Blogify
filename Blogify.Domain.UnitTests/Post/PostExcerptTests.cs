using Blogify.Domain.Posts;

namespace Blogify.Domain.UnitTests.Post;

public class PostExcerptTests
{
    [Fact]
    public void Create_ValidExcerpt_ReturnsSuccess()
    {
        // Arrange
        var excerpt = "This is a valid excerpt.";

        // Act
        var result = PostExcerpt.Create(excerpt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(excerpt, result.Value.Value);
    }

    [Fact]
    public void Create_EmptyExcerpt_ReturnsFailure()
    {
        // Arrange
        var excerpt = "";

        // Act
        var result = PostExcerpt.Create(excerpt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.ExcerptEmpty, result.Error);
    }

    [Fact]
    public void Create_ExcerptTooLong_ReturnsFailure()
    {
        // Arrange
        var excerpt = new string('a', 501);

        // Act
        var result = PostExcerpt.Create(excerpt);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PostErrors.ExcerptTooLong, result.Error);
    }
}