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
        var validContent = new string('a', 100); // Meets minimum length
        var command = new CreatePostCommand(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(validContent).Value,
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid());

        // Act & Assert
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullTitle_ShouldHaveValidationError()
    {
        // Arrange
        var validContent = new string('a', 100);
        var command = new CreatePostCommand(
            null!,
            PostContent.Create(validContent).Value,
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.NewGuid());

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Post title cannot be null.");
    }

    [Fact]
    public void Validate_EmptyAuthorId_ShouldHaveValidationError()
    {
        // Arrange
        var validContent = new string('a', 100);
        var command = new CreatePostCommand(
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(validContent).Value,
            PostExcerpt.Create("Valid Excerpt").Value,
            Guid.Empty);

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage("AuthorId cannot be empty.");
    }

    // [Fact]
    // public void Validate_EmptyCategoryId_ShouldHaveValidationError()
    // {
    //     // Arrange
    //     var titleResult = PostTitle.Create("Valid Title");
    //     var contentResult = PostContent.Create("Valid Content");
    //     var excerptResult = PostExcerpt.Create("Valid Excerpt");
    //
    //     // Ensure the creation was successful
    //     titleResult.IsSuccess.Should().BeTrue("Title creation should succeed with valid input.");
    //     contentResult.IsSuccess.Should().BeTrue("Content creation should succeed with valid input.");
    //     excerptResult.IsSuccess.Should().BeTrue("Excerpt creation should succeed with valid input.");
    //
    //     var command = new CreatePostCommand(
    //         titleResult.Value,
    //         contentResult.Value,
    //         excerptResult.Value,
    //         Guid.NewGuid());
    //
    //     // Act
    //     var result = _validator.TestValidate(command);
    //
    //     // Assert
    //     result.ShouldHaveValidationErrorFor(x => x.CategoryId)
    //         .WithErrorMessage("CategoryId cannot be empty.");
    // }
}