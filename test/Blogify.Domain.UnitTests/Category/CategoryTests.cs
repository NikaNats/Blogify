using Blogify.Domain.Categories;
using Blogify.Domain.Categories.Events;
using Blogify.Domain.Posts;

namespace Blogify.Domain.UnitTests.Category;

public class CategoryTests
{
    // Constants for test data
    private const string ValidName = "Technology";
    private const string ValidDescription = "All about technology.";
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    #region Helper Methods

    private static Categories.Category CreateCategory(string name = ValidName, string description = ValidDescription)
    {
        var result = Categories.Category.Create(name, description);
        Assert.True(result.IsSuccess, $"Failed to create category: {result.Error.Description}");
        return result.Value;
    }

    private static Posts.Post CreatePost(Guid categoryId)
    {
        var postTitleResult = PostTitle.Create("Valid Post Title");
        var postContentResult = PostContent.Create(
            "This is a valid post content that meets the minimum length requirement of 100 characters. It should be long enough to pass validation.");
        var postExcerptResult = PostExcerpt.Create(
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

    #endregion

    #region Creation Tests

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_ValidInputs_ReturnsSuccessResultWithCategory()
    {
        // Act
        var result = Categories.Category.Create(ValidName, ValidDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(ValidName, result.Value.Name.Value);
        Assert.Equal(ValidDescription, result.Value.Description.Value);
        Assert.NotEqual(default, result.Value.CreatedAt);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_EmptyName_ReturnsFailureResultWithValidationError()
    {
        // Act
        var result = Categories.Category.Create(string.Empty, ValidDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NullName_ReturnsFailureResultWithValidationError()
    {
        // Act
        var result = Categories.Category.Create(null, ValidDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_EmptyDescription_ReturnsFailureResultWithValidationError()
    {
        // Act
        var result = Categories.Category.Create(ValidName, string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NullDescription_ReturnsFailureResultWithValidationError()
    {
        // Act
        var result = Categories.Category.Create(ValidName, null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NameExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var name = new string('a', MaxNameLength + 1);

        // Act
        var result = Categories.Category.Create(name, ValidDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameTooLong, result.Error);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_DescriptionExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var description = new string('a', MaxDescriptionLength + 1);

        // Act
        var result = Categories.Category.Create(ValidName, description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionTooLong, result.Error);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_SetsCreatedAtAndLastModifiedAtEqual()
    {
        // Act
        var result = Categories.Category.Create(ValidName, ValidDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.CreatedAt, result.Value.LastModifiedAt);
    }

    #endregion

    #region Update Tests

    [Fact]
    [Trait("Category", "Update")]
    public void Update_ValidInputs_ReturnsSuccessResultAndUpdatesLastModifiedAt()
    {
        // Arrange
        var category = CreateCategory();
        var originalLastModifiedAt = category.LastModifiedAt;
        var newName = "Tech";
        var newDescription = "Latest in tech.";

        // Act
        Thread.Sleep(10); // Ensure measurable time difference
        var result = category.Update(newName, newDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, category.Name.Value);
        Assert.Equal(newDescription, category.Description.Value);
        Assert.True(category.LastModifiedAt > originalLastModifiedAt);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_EmptyName_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var result = category.Update(string.Empty, ValidDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_NullName_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var result = category.Update(null, ValidDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_EmptyDescription_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var result = category.Update(ValidName, string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_NullDescription_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var result = category.Update(ValidName, null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_NameExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();
        var newName = new string('a', MaxNameLength + 1);

        // Act
        var result = category.Update(newName, ValidDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameTooLong, result.Error);
    }

    [Fact]
    [Trait("Category", "Update")]
    public void Update_DescriptionExceedsMaxLength_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var category = CreateCategory();
        var newDescription = new string('a', MaxDescriptionLength + 1);

        // Act
        var result = category.Update(ValidName, newDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionTooLong, result.Error);
    }

    #endregion

    #region Post Management Tests

    [Fact]
    [Trait("Category", "PostManagement")]
    public void AddPost_ValidPost_ReturnsSuccessResultAndUpdatesLastModifiedAt()
    {
        // Arrange
        var category = CreateCategory();
        var originalLastModifiedAt = category.LastModifiedAt;
        var post = CreatePost(category.Id);

        // Act
        Thread.Sleep(10); // Ensure measurable time difference
        var result = category.AddPost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(post, category.Posts);
        Assert.True(category.LastModifiedAt > originalLastModifiedAt);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
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

    [Fact]
    [Trait("Category", "PostManagement")]
    public void AddPost_DuplicatePost_ReturnsFailureResult()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);
        var firstAddResult = category.AddPost(post);
        Assert.True(firstAddResult.IsSuccess, "Failed to add post to category for the first time.");

        // Act
        var result = category.AddPost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.PostAlreadyExists, result.Error);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
    public void RemovePost_ValidPost_ReturnsSuccessResultAndUpdatesLastModifiedAt()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);
        var addPostResult = category.AddPost(post);
        Assert.True(addPostResult.IsSuccess, "Failed to add post to category.");
        var originalLastModifiedAt = category.LastModifiedAt;

        // Act
        Thread.Sleep(10); // Ensure measurable time difference
        var result = category.RemovePost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(post, category.Posts);
        Assert.True(category.LastModifiedAt > originalLastModifiedAt);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
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

    [Fact]
    [Trait("Category", "PostManagement")]
    public void RemovePost_NonExistentPost_ReturnsFailureResult()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);

        // Act
        var result = category.RemovePost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.PostNotFound, result.Error);
    }

    #endregion

    #region Domain Event Tests

    [Fact]
    [Trait("Category", "Events")]
    public void Create_RaisesCategoryCreatedDomainEvent_WithCorrectProperties()
    {
        // Act
        var result = Categories.Category.Create(ValidName, ValidDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.DomainEvents);
        var domainEvent = Assert.IsType<CategoryCreatedDomainEvent>(result.Value.DomainEvents.First());
        Assert.Equal(result.Value.Id, domainEvent.CategoryId);
    }

    [Fact]
    [Trait("Category", "Events")]
    public void Update_RaisesCategoryUpdatedDomainEvent_WithCorrectProperties()
    {
        // Arrange
        var category = CreateCategory();
        category.ClearDomainEvents(); // Clear events from creation
        var newName = "Tech";
        var newDescription = "Latest in tech.";

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(category.DomainEvents);
        var domainEvent = Assert.IsType<CategoryUpdatedDomainEvent>(category.DomainEvents.First());
        Assert.Equal(category.Id, domainEvent.CategoryId);
    }

    [Fact]
    [Trait("Category", "Events")]
    public void AddPost_RaisesPostAddedToCategoryDomainEvent_WithCorrectProperties()
    {
        // Arrange
        var category = CreateCategory();
        category.ClearDomainEvents(); // Clear events from creation
        var post = CreatePost(category.Id);

        // Act
        var result = category.AddPost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(category.DomainEvents);
        var domainEvent = Assert.IsType<PostAddedToCategoryDomainEvent>(category.DomainEvents.First());
        Assert.Equal(category.Id, domainEvent.IdValue);
        Assert.Equal(post.Id, domainEvent.PostIdValue);
    }

    [Fact]
    [Trait("Category", "Events")]
    public void RemovePost_RaisesPostRemovedFromCategoryDomainEvent_WithCorrectProperties()
    {
        // Arrange
        var category = CreateCategory();
        var post = CreatePost(category.Id);
        var addPostResult = category.AddPost(post);
        Assert.True(addPostResult.IsSuccess, "Failed to add post to category.");
        category.ClearDomainEvents(); // Clear events from addition

        // Act
        var result = category.RemovePost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(category.DomainEvents);
        var domainEvent = Assert.IsType<PostRemovedFromCategoryDomainEvent>(category.DomainEvents.First());
        Assert.Equal(category.Id, domainEvent.IdValue);
        Assert.Equal(post.Id, domainEvent.PostIdValue);
    }

    #endregion

    #region Equality Tests

    [Fact]
    [Trait("Category", "Equality")]
    public void Category_IsEqualToItself()
    {
        // Arrange
        var category = CreateCategory();

        // Assert
        Assert.Equal(category, category);
        Assert.True(category.Equals(category));
    }

    [Fact]
    [Trait("Category", "Equality")]
    public void Categories_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var category1 = CreateCategory();
        var category2 = CreateCategory();

        // Assert
        Assert.NotEqual(category1, category2);
        Assert.False(category1.Equals(category2));
    }

    #endregion

    #region Value Object Tests

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryName_Create_ValidName_Succeeds()
    {
        // Act
        var result = CategoryName.Create(ValidName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ValidName, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryName_Create_EmptyName_Fails()
    {
        // Act
        var result = CategoryName.Create(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryName_Create_NameTooLong_Fails()
    {
        // Arrange
        var name = new string('a', MaxNameLength + 1);

        // Act
        var result = CategoryName.Create(name);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.NameTooLong, result.Error);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryDescription_Create_ValidDescription_Succeeds()
    {
        // Act
        var result = CategoryDescription.Create(ValidDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ValidDescription, result.Value.Value);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryDescription_Create_EmptyDescription_Fails()
    {
        // Act
        var result = CategoryDescription.Create(string.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionNullOrEmpty, result.Error);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryDescription_Create_DescriptionTooLong_Fails()
    {
        // Arrange
        var description = new string('a', MaxDescriptionLength + 1);

        // Act
        var result = CategoryDescription.Create(description);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CategoryError.DescriptionTooLong, result.Error);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryName_Equality_SameValue_AreEqual()
    {
        // Arrange
        var name1 = CategoryName.Create(ValidName).Value;
        var name2 = CategoryName.Create(ValidName).Value;

        // Assert
        Assert.Equal(name1, name2);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void CategoryDescription_Equality_SameValue_AreEqual()
    {
        // Arrange
        var description1 = CategoryDescription.Create(ValidDescription).Value;
        var description2 = CategoryDescription.Create(ValidDescription).Value;

        // Assert
        Assert.Equal(description1, description2);
    }

    #endregion
}