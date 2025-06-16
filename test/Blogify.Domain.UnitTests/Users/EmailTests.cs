using Blogify.Domain.Users;
using Shouldly;

namespace Blogify.Domain.UnitTests.Users;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceedAndCanonicalize()
    {
        // Act
        var result = Email.Create("  Test.User@Example.COM  ");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Address.ShouldBe("test.user@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public void Create_WithInvalidFormat_ShouldReturnFailure(string invalidEmail)
    {
        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_WithTooLongEmail_ShouldReturnFailure()
    {
        // Arrange
        var longEmail = new string('a', 245) + "@example.com"; // 257 characters

        // Act
        var result = Email.Create(longEmail);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.EmailTooLong);
    }

    [Fact]
    public void Equality_ForEmailsWithDifferentCasing_ShouldBeTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("Test@Example.COM").Value;

        // Assert
        email1.ShouldBe(email2);
        (email1 == email2).ShouldBeTrue();
    }
}