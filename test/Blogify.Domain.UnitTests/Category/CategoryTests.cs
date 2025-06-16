using Blogify.Domain.Categories.Events;
using Shouldly;

namespace Blogify.Domain.UnitTests.Category;
// Corrected namespace

public class CategoryTests
{
    private const string ValidName = "Technology";
    private const string ValidDescription = "A category for all things tech.";

    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccessResult()
    {
        // Act
        var result = Categories.Category.Create(ValidName, ValidDescription);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var category = result.Value;
        category.ShouldNotBeNull();
        category.Id.ShouldNotBe(Guid.Empty);
        category.Name.Value.ShouldBe(ValidName);
        category.Description.Value.ShouldBe(ValidDescription);
    }

    [Fact]
    public void Create_WithValidParameters_ShouldRaiseCategoryCreatedDomainEvent()
    {
        // Act
        var category = Categories.Category.Create(ValidName, ValidDescription).Value;

        // Assert
        var domainEvent = category.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CategoryCreatedDomainEvent>();
        domainEvent.CategoryId.ShouldBe(category.Id);
    }

    [Theory]
    [InlineData(null, ValidDescription, "Category.NameNullOrEmpty")]
    [InlineData("", ValidDescription, "Category.NameNullOrEmpty")]
    [InlineData(" ", ValidDescription, "Category.NameNullOrEmpty")]
    [InlineData(ValidName, null, "Category.DescriptionNullOrEmpty")]
    [InlineData(ValidName, "", "Category.DescriptionNullOrEmpty")]
    [InlineData(ValidName, " ", "Category.DescriptionNullOrEmpty")]
    public void Create_WithInvalidParameter_ShouldReturnFailureResult(string name, string description,
        string expectedErrorCode)
    {
        // Act
        var result = Categories.Category.Create(name, description);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(expectedErrorCode);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_WithDifferentValidValues_ShouldChangeProperties()
    {
        // Arrange
        var category = Categories.Category.Create(ValidName, ValidDescription).Value;
        var newName = "Updated Name";
        var newDescription = "Updated Description.";

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.Name.Value.ShouldBe(newName);
        category.Description.Value.ShouldBe(newDescription);
    }

    [Fact]
    public void Update_WithDifferentValidValues_ShouldRaiseCategoryUpdatedDomainEvent()
    {
        // Arrange
        var category = Categories.Category.Create(ValidName, ValidDescription).Value;
        category.ClearDomainEvents(); // Isolate the event from the action

        // Act
        category.Update("New Name", "New Description");

        // Assert
        var domainEvent = category.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CategoryUpdatedDomainEvent>();
        domainEvent.CategoryId.ShouldBe(category.Id);
    }

    [Fact]
    public void Update_WithSameValues_ShouldBeIdempotentAndNotRaiseEvent()
    {
        // Arrange
        var category = Categories.Category.Create(ValidName, ValidDescription).Value;
        category.ClearDomainEvents();

        // Act
        var result = category.Update(ValidName, ValidDescription);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        category.DomainEvents.ShouldBeEmpty();
    }

    #endregion
}