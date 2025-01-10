using Blogify.Domain.Categories;
using Blogify.Domain.Posts;

namespace Blogify.Domain.UnitTests.Category;

public class CategoryTests
{
    // Helper method to create a valid category
    private static Categories.Category CreateCategory(string name = "Technology",
        string description = "All about technology.")
    {
        var result = Categories.Category.Create(name, description);
        Assert.True(result.IsSuccess, $"Failed to create category: {result.Error.Description}");
        return result.Value;
    }

    // Helper method to create a valid post
    private static Posts.Post CreatePost(Guid categoryId)
    {
        var postTitleResult = PostTitle.Create("Valid Post Title");
        var postContentResult =
            PostContent.Create(
                "This is a valid post content that meets the minimum length requirement of 100 characters. It should be long enough to pass validation.");
        var postExcerptResult =
            PostExcerpt.Create(
                "This is a valid post excerpt that meets the minimum length requirement. It should be long enough to pass validation.");

        Assert.True(postTitleResult.IsSuccess, $"Failed to create post title: {postTitleResult.Error.Description}");
        Assert.True(postContentResult.IsSuccess,
            $"Failed to create post content: {postContentResult.Error.Description}");
        Assert.True(postExcerptResult.IsSuccess,
            $"Failed to create post excerpt: {postExcerptResult.Error.Description}");

        var postResult = Posts.Post.Create(
            postTitleResult.Value,
            postContentResult.Value,
            postExcerptResult.Value,
            Guid.NewGuid());

        Assert.True(postResult.IsSuccess, $"Failed to create post: {postResult.Error.Description}");
        return postResult.Value;
    }

    // Test for successful creation of a Category
    [Fact]
    public void Create_ValidInputs_ReturnsSuccessResultWithCategory()
    {
        // Arrange
        const string name = "Technology";
        const string description = "All about technology.";

        // Act
        var result = Categories.Category.Create(name, description);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name.Value);
        Assert.Equal(description, result.Value.Description.Value);
        Assert.NotEqual(default, result.Value.CreatedAt);
    }

    // Test for empty name
    [Fact]
    public void Create_EmptyName_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var name = string.Empty;
        const string description = "All about technology.";

        // Act
        var result = Categories.Category.Create(name, description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    // Test for null name
    [Fact]
    public void Create_NullName_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        string name = null;
        const string description = "All about technology.";

        // Act
        var result = Categories.Category.Create(name, description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    // Test for empty description
    [Fact]
    public void Create_EmptyDescription_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        const string name = "Technology";
        var description = string.Empty;

        // Act
        var result = Categories.Category.Create(name, description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    // Test for null description
    [Fact]
    public void Create_NullDescription_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        const string name = "Technology";
        string description = null;

        // Act
        var result = Categories.Category.Create(name, description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    // Test for successful update of a Category
    [Fact]
    public void Update_ValidInputs_ReturnsSuccessResult()
    {
        // Arrange
        var category = CreateCategory();
        var newName = "Tech";
        var newDescription = "Latest in tech.";

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, category.Name.Value);
        Assert.Equal(newDescription, category.Description.Value);
    }

    // Test for empty name during update
    [Fact]
    public void Update_EmptyName_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();
        var newName = string.Empty;
        var newDescription = "Latest in tech.";

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    // Test for null name during update
    [Fact]
    public void Update_NullName_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();
        string newName = null;
        var newDescription = "Latest in tech.";

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    // Test for empty description during update
    [Fact]
    public void Update_EmptyDescription_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();
        const string newName = "Tech";
        var newDescription = string.Empty;

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    // Test for null description during update
    [Fact]
    public void Update_NullDescription_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();
        const string newName = "Tech";
        string newDescription = null;

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    // Test for adding a valid post to a category
    [Fact]
    public void AddPost_ValidPost_ReturnsSuccessResult()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);

        // Act
        var result = category.AddPost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(post, category.Posts);
    }

    // Test for adding a null post to a category
    [Fact]
    public void AddPost_NullPost_ReturnsFailureResult()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var result = category.AddPost(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.PostNull, result.Error);
    }

    // Test for adding a duplicate post to a category
    [Fact]
    public void AddPost_DuplicatePost_ReturnsFailureResult()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);

        // Add the post to the category
        var firstAddResult = category.AddPost(post);
        Assert.True(firstAddResult.IsSuccess, "Failed to add post to category for the first time.");

        // Act: Try to add the same post again
        var result = category.AddPost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.PostAlreadyExists, result.Error);
    }

    // Test for removing a valid post from a category
    [Fact]
    public void RemovePost_ValidPost_ReturnsSuccessResult()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);

        // Add the post to the category
        var addPostResult = category.AddPost(post);
        Assert.True(addPostResult.IsSuccess, "Failed to add post to category.");

        // Act
        var result = category.RemovePost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(post, category.Posts);
    }

    // Test for removing a null post from a category
    [Fact]
    public void RemovePost_NullPost_ReturnsFailureResult()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var result = category.RemovePost(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.PostNull, result.Error);
    }

    // Test for removing a post that does not exist in the category
    [Fact]
    public void RemovePost_NonExistentPost_ReturnsFailureResult()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);

        // Act: Try to remove a post that was never added to the category
        var result = category.RemovePost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.PostNotFound, result.Error);
    }
}