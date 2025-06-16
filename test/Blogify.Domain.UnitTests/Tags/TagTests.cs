using Blogify.Domain.Tags;
using Blogify.Domain.Tags.Events;
using Shouldly;

namespace Blogify.Domain.UnitTests.Tags;

public class TagTests
{
    private const string ValidName = "C# Programming";

    #region UpdateName

    [Fact]
    public void UpdateName_WithValidNewName_ShouldChangeName()
    {
        // Arrange
        var tag = Tag.Create(ValidName).Value;
        var newName = ".NET Core";

        // Act
        var result = tag.UpdateName(newName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        tag.Name.Value.ShouldBe(newName);
    }

    #endregion

    #region Create

    [Fact]
    public void Create_WithValidName_ShouldReturnSuccessResult()
    {
        // Act
        var result = Tag.Create(ValidName);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var tag = result.Value;
        tag.ShouldNotBeNull();
        tag.Name.Value.ShouldBe(ValidName);
    }

    [Fact]
    public void Create_WithValidName_ShouldRaiseTagCreatedEvent()
    {
        // Act
        var tag = Tag.Create(ValidName).Value;

        // Assert
        var domainEvent = tag.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<TagCreatedDomainEvent>();
        domainEvent.TagId.ShouldBe(tag.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_ShouldReturnFailure(string invalidName)
    {
        // Act
        var result = Tag.Create(invalidName);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TagErrors.NameEmpty);
    }

    #endregion
}