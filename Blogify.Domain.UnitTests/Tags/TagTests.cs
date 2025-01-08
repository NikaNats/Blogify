using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using Blogify.Domain.Tags.Events;
using Xunit;

namespace Blogify.Domain.UnitTests.Tags;

public class TagTests
{
    // Test for successful creation of a Tag with a valid name
    [Fact]
    public void Create_ValidName_ReturnsSuccessResultWithTag()
    {
        // Arrange
        const string name = "Unit Testing";

        // Act
        var result = Tag.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.True(DateTime.UtcNow >= result.Value.CreatedAt);
    }

    // Test that creating a Tag with a valid name raises a TagCreatedDomainEvent
    [Fact]
    public void Create_ValidName_RaisesTagCreatedDomainEvent()
    {
        // Arrange
        const string name = "Unit Testing";

        // Act
        var result = Tag.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
        var domainEvent = result.Value.DomainEvents.OfType<TagCreatedDomainEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(result.Value.Id, domainEvent.TagId);
    }

    // Test for creating a Tag with invalid names (null, empty, whitespace)
    [Theory]
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
        Assert.Equal("TagName.Empty", result.Error.Code);
        Assert.Equal("Tag name cannot be empty.", result.Error.Description);
    }

    // Test for updating a Tag's name with a valid name
    [Fact]
    public void UpdateName_ValidName_ReturnsSuccessResult()
    {
        // Arrange
        var tag = Tag.Create("Original Name").Value;
        const string newName = "Updated Name";

        // Act
        var result = tag.UpdateName(newName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, tag.Name.Value);
    }

    // Test for updating a Tag's name with an invalid name
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateName_InvalidName_ReturnsFailureResult(string invalidName)
    {
        // Arrange
        var tag = Tag.Create("Original Name").Value;

        // Act
        var result = tag.UpdateName(invalidName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("TagName.Empty", result.Error.Code);
        Assert.Equal("Tag name cannot be empty.", result.Error.Description);
    }

    // Test for adding a valid post to a Tag
    [Fact]
    public void AddPost_ValidPost_ReturnsSuccessResult()
    {
        // Arrange
        var tag = Tag.Create("Unit Testing").Value;
        var post = CreateValidPost();

        // Act
        var result = tag.AddPost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(post, tag.Posts);
    }

    // Test for adding a null post to a Tag
    [Fact]
    public void AddPost_NullPost_ReturnsFailureResult()
    {
        // Arrange
        var tag = Tag.Create("Unit Testing").Value;

        // Act
        var result = tag.AddPost(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostNull, result.Error);
    }

    // Test for adding a duplicate post to a Tag
    [Fact]
    public void AddPost_DuplicatePost_ReturnsFailureResult()
    {
        // Arrange
        var tag = Tag.Create("Unit Testing").Value;
        var post = CreateValidPost();
        tag.AddPost(post); // Add the post once

        // Act: Try to add the same post again
        var result = tag.AddPost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostDuplicate, result.Error);
    }

    // Test for removing a valid post from a Tag
    [Fact]
    public void RemovePost_ValidPost_ReturnsSuccessResult()
    {
        // Arrange
        var tag = Tag.Create("Unit Testing").Value;
        var post = CreateValidPost();
        tag.AddPost(post); // Add the post first

        // Act
        var result = tag.RemovePost(post);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(post, tag.Posts);
    }

    // Test for removing a null post from a Tag
    [Fact]
    public void RemovePost_NullPost_ReturnsFailureResult()
    {
        // Arrange
        var tag = Tag.Create("Unit Testing").Value;

        // Act
        var result = tag.RemovePost(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostNull, result.Error);
    }

    // Test for removing a post that does not exist in the Tag
    [Fact]
    public void RemovePost_NonExistentPost_ReturnsFailureResult()
    {
        // Arrange
        var tag = Tag.Create("Unit Testing").Value;
        var post = CreateValidPost();

        // Act: Try to remove a post that was never added
        var result = tag.RemovePost(post);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TagErrors.PostNotFound, result.Error);
    }

    // Helper method to create a valid Post
    private static Posts.Post CreateValidPost()
    {
        var titleResult = PostTitle.Create("Valid Post Title");
        var contentResult = PostContent.Create("This is a valid post content that meets the minimum length requirement of 100 characters. It should be long enough to pass validation.");
        var excerptResult = PostExcerpt.Create("This is a valid post excerpt that meets the minimum length requirement. It should be long enough to pass validation.");

        Assert.True(titleResult.IsSuccess, $"Failed to create post title: {titleResult.Error?.Description}");
        Assert.True(contentResult.IsSuccess, $"Failed to create post content: {contentResult.Error?.Description}");
        Assert.True(excerptResult.IsSuccess, $"Failed to create post excerpt: {excerptResult.Error?.Description}");

        var postResult = Posts.Post.Create(
            titleResult.Value,
            contentResult.Value,
            excerptResult.Value,
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.True(postResult.IsSuccess, $"Failed to create post: {postResult.Error?.Description}");
        return postResult.Value;
    }
}