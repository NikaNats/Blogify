using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags;
using Blogify.Domain.Tags.Events;

namespace Blogify.Domain.UnitTests.Tags;

public class TagTests
{
    [Fact]
    public void Create_ValidName_ReturnsSuccessResultWithTag()
    {
        // Arrange
        var name = "Unit Testing";

        // Act
        var result = Tag.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(name, result.Value.Name);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.True(DateTime.UtcNow >= result.Value.CreatedAt);
    }

    [Fact]
    public void Create_ValidName_RaisesTagCreatedDomainEvent()
    {
        // Arrange
        var name = "Unit Testing";

        // Act
        var result = Tag.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
        var domainEvent = result.Value.GetDomainEvents().OfType<TagCreatedDomainEvent>().SingleOrDefault();
        Assert.NotNull(domainEvent);
        Assert.Equal(result.Value.Id, domainEvent.TagId);
    }

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
        Assert.Equal("Tag.Name.Empty", result.Error.Code);
        Assert.Equal("Tag name cannot be empty.", result.Error.Description);
    }
}