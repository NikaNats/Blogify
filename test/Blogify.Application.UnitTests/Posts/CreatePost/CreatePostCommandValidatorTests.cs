using Blogify.Application.Posts.CreatePost;
using Blogify.Domain.Posts;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Posts.CreatePost;

public class CreatePostCommandValidatorTests
{
    private readonly CreatePostCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        // --- FIX: Use the new constructor without AuthorId. ---
        var command = new CreatePostCommand(
            "Valid Title",
            new string('a', 100), // Meets minimum length
            "Valid Excerpt");

        // Act & Assert
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidTitle_ShouldHaveValidationError(string invalidTitle)
    {
        // Arrange
        // --- FIX: Use the new constructor. ---
        var command = new CreatePostCommand(
            invalidTitle,
            new string('a', 100),
            "Valid Excerpt");

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage(PostErrors.TitleEmpty.Description);
    }

    // --- FIX: This entire test is removed as AuthorId is no longer on the command. ---
    // [Fact]
    // public void Validate_EmptyAuthorId_ShouldHaveValidationError()
    // {
    // }

    [Fact]
    public void Validate_ShortContent_ShouldHaveValidationError()
    {
        // Arrange
        // --- FIX: Use the new constructor. ---
        var command = new CreatePostCommand(
            "Valid Title",
            "Too short", // Content does not meet minimum length
            "Valid Excerpt");

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(PostErrors.ContentTooShort.Description);
    }
}