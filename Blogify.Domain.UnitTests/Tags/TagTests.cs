using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using Blogify.Domain.Tags.Events;

namespace Blogify.Domain.UnitTests.Tags;

public class TagTests
{
    private const string ValidName = "Unit Testing";
    private const int MaxNameLength = 50;

    #region Domain Event Tests

    [Fact]
    [Trait("Category", "Events")]
    public void Create_RaisesTagCreatedDomainEvent_WithCorrectProperties()
    {
        // Act
        var result = Tag.Create(ValidName);

        // Assert
        Assert.True(result.IsSuccess);
        var domainEvent = result.Value.DomainEvents.OfType<TagCreatedDomainEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(result.Value.Id, domainEvent.TagId);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Creates a Tag with the specified name, asserting success.
    /// </summary>
    private static Tag CreateTag(string name = ValidName)
    {
        var result = Tag.Create(name);
        Assert.True(result.IsSuccess, $"Failed to create tag: {result.Error.Description}");
        return result.Value;
    }

    /// <summary>
    ///     Creates a valid Post for testing.
    /// </summary>
    private static Posts.Post CreateValidPost()
    {
        var titleResult = PostTitle.Create("Valid Post Title");
        var contentResult =
            PostContent.Create(
                "This is a valid post content that meets the minimum length requirement of 100 characters. It should be long enough to pass validation.");
        var excerptResult =
            PostExcerpt.Create(
                "This is a valid post excerpt that meets the minimum length requirement. It should be long enough to pass validation.");

        Assert.True(titleResult.IsSuccess, $"Failed to create post title: {titleResult.Error?.Description}");
        Assert.True(contentResult.IsSuccess, $"Failed to create post content: {contentResult.Error?.Description}");
        Assert.True(excerptResult.IsSuccess, $"Failed to create post excerpt: {excerptResult.Error?.Description}");

        var postResult = Posts.Post.Create(
            titleResult.Value,
            contentResult.Value,
            excerptResult.Value,
            Guid.NewGuid());

        Assert.True(postResult.IsSuccess, $"Failed to create post: {postResult.Error?.Description}");
        return postResult.Value;
    }

    #endregion

    #region Creation Tests

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_ValidName_ReturnsSuccessResultWithTag()
    {
        // Act
        var result = Tag.Create(ValidName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(ValidName, result.Value.Name.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.True(DateTime.UtcNow >= result.Value.CreatedAt);
    }

    [Theory]
    [Trait("Category", "Creation")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_InvalidName_ReturnsFailureResult(string invalidName)
    {
        // Act
        var result = Tag.Create(invalidName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Tag.Name.Empty", result.Error.Code);
        Assert.Equal("The tag name cannot be empty.", result.Error.Description);
    }

    [Fact]
    public void Create_EmptyName_ReturnsFailureResult()
    {
        var result = TagName.Create("");
        Assert.False(result.IsSuccess);
        Assert.Equal("Tag.Name.Empty", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Create_ValidName_ReturnsSuccessResult()
    {
        var result = TagName.Create("ValidTag");
        Assert.True(result.IsSuccess);
        Assert.Equal("ValidTag", result.Value.Value);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_NameExceedsMaxLength_ReturnsFailureResult()
    {
        // Arrange
        var name = new string('a', MaxNameLength + 1);

        // Act
        var result = Tag.Create(name);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Tag.Name.TooLong", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "Creation")]
    public void Create_SetsCreatedAtAndLastModifiedAtEqual()
    {
        // Act
        var result = Tag.Create(ValidName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.CreatedAt, result.Value.LastModifiedAt);
    }

    #endregion

    #region Update Tests

    [Fact]
    [Trait("Category", "Update")]
    public void UpdateName_ValidName_ReturnsSuccessResultAndUpdatesLastModifiedAt()
    {
        // Arrange
        var tag = CreateTag();
        var originalLastModifiedAt = tag.LastModifiedAt;
        const string newName = "Updated Name";

        // Act
        Thread.Sleep(10); // Ensure measurable time difference
        var result = tag.UpdateName(newName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, tag.Name.Value);
        Assert.True(tag.LastModifiedAt > originalLastModifiedAt);
    }

    [Theory]
    [Trait("Category", "Update")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateName_InvalidName_ReturnsFailureResult(string invalidName)
    {
        // Arrange
        var tag = CreateTag();

        // Act
        var result = tag.UpdateName(invalidName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Tag.Name.Empty", result.Error.Code);
        Assert.Equal("The tag name cannot be empty.", result.Error.Description);
    }

    #endregion

    #region Post Management Tests

    [Fact]
    [Trait("Category", "PostManagement")]
    public void AddPost_ValidPost_ReturnsSuccessResultAndUpdatesLastModifiedAt()
    {
        // Arrange
        var tag = CreateTag();
        var originalLastModifiedAt = tag.LastModifiedAt;
        var post = CreateValidPost();

        // Act
        Thread.Sleep(10); // Ensure measurable time difference
        var result = tag.AddPost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(post, tag.Posts);
        Assert.True(tag.LastModifiedAt > originalLastModifiedAt);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
    public void AddPost_NullPost_ReturnsFailureResult()
    {
        // Arrange
        var tag = CreateTag();

        // Act
        var result = tag.AddPost(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostNull, result.Error);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
    public void AddPost_DuplicatePost_ReturnsFailureResult()
    {
        // Arrange
        var tag = CreateTag();
        var post = CreateValidPost();
        tag.AddPost(post);

        // Act
        var result = tag.AddPost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostDuplicate, result.Error);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
    public void RemovePost_ValidPost_ReturnsSuccessResultAndUpdatesLastModifiedAt()
    {
        // Arrange
        var tag = CreateTag();
        var post = CreateValidPost();
        tag.AddPost(post);
        var originalLastModifiedAt = tag.LastModifiedAt;

        // Act
        Thread.Sleep(10); // Ensure measurable time difference
        var result = tag.RemovePost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(post, tag.Posts);
        Assert.True(tag.LastModifiedAt > originalLastModifiedAt);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
    public void RemovePost_NullPost_ReturnsFailureResult()
    {
        // Arrange
        var tag = CreateTag();

        // Act
        var result = tag.RemovePost(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostNull, result.Error);
    }

    [Fact]
    [Trait("Category", "PostManagement")]
    public void RemovePost_NonExistentPost_ReturnsFailureResult()
    {
        // Arrange
        var tag = CreateTag();
        var post = CreateValidPost();

        // Act
        var result = tag.RemovePost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostNotFound, result.Error);
    }

    #endregion

    #region Equality Tests

    [Fact]
    [Trait("Category", "Equality")]
    public void Tag_IsEqualToItself()
    {
        // Arrange
        var tag = CreateTag();

        // Assert
        Assert.Equal(tag, tag);
        Assert.True(tag.Equals(tag));
    }

    [Fact]
    [Trait("Category", "Equality")]
    public void Tags_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var tag1 = CreateTag();
        var tag2 = CreateTag();

        // Assert
        Assert.NotEqual(tag1, tag2);
        Assert.False(tag1.Equals(tag2));
    }

    #endregion

    #region Value Object Tests

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void TagName_Create_ValidName_Succeeds()
    {
        // Act
        var result = TagName.Create(ValidName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ValidName, result.Value.Value);
    }

    [Theory]
    [Trait("Category", "ValueObjects")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void TagName_Create_InvalidName_Fails(string invalidName)
    {
        // Act
        var result = TagName.Create(invalidName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Tag.Name.Empty", result.Error.Code);
    }

    [Fact]
    [Trait("Category", "ValueObjects")]
    public void TagName_Equality_SameValue_AreEqual()
    {
        // Arrange
        var name1 = TagName.Create(ValidName).Value;
        var name2 = TagName.Create(ValidName).Value;

        // Assert
        Assert.Equal(name1, name2);
    }

    #endregion
}