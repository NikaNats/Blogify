using Blogify.Application.Posts.UpdatePost;
using Blogify.Domain.Posts;
using FluentValidation.TestHelper;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.UpdatePost;

public class UpdatePostCommandValidatorTests
{
    private readonly UpdatePostCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsFullyValid_ShouldSucceed()
    {
        // --- FIX: Create the command with primitive types ---
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            "Valid Title",
            new string('a', 100), // Meets minimum length
            "Valid excerpt."
        );

        var result = _validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenTitleIsInvalid_ShouldFailWithTitleEmptyError(string invalidTitle)
    {
        // --- FIX: Create the command with an invalid primitive title ---
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            invalidTitle,
            new string('a', 100),
            "Valid excerpt."
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage(PostErrors.TitleEmpty.Description);
    }

    [Fact]
    public void Validate_WhenContentIsTooShort_ShouldFailWithContentTooShortError()
    {
        // --- FIX: Create the command with invalid primitive content ---
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            "Valid Title",
            "Too short",
            "Valid excerpt."
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(PostErrors.ContentTooShort.Description);
    }

    [Fact]
    public void Validate_WhenExcerptIsEmpty_ShouldFailWithExcerptEmptyError()
    {
        // --- FIX: Create the command with invalid primitive excerpt ---
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            "Valid Title",
            new string('a', 100),
            ""
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Excerpt)
            .WithErrorMessage(PostErrors.ExcerptEmpty.Description);
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldFailWithInvalidIdError()
    {
        // --- FIX: Create the command with an empty Guid ---
        var command = new UpdatePostCommand(
            Guid.Empty,
            "Valid Title",
            new string('a', 100),
            "Valid excerpt."
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(PostErrors.PostIdEmpty.Description);
    }
}