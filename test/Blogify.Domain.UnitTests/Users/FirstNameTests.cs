using Blogify.Domain.Users;
using Shouldly;

namespace Blogify.Domain.UnitTests.Users;

public class FirstNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldSucceed()
    {
        // Act
        var result = FirstName.Create("John");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("John");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string invalidName)
    {
        // Act
        var result = FirstName.Create(invalidName);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.InvalidFirstName);
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('a', 51);

        // Act
        var result = FirstName.Create(longName);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.FirstNameTooLong);
    }
}