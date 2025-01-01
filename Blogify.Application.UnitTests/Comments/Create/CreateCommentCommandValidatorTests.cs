using Blogify.Application.Comments.CreateComment;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Comments.Create;

public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateCommentCommand("Valid content", Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyContent_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand("", Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Comment content cannot be empty.");
    }

    [Fact]
    public void Validate_ContentExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longContent = new string('a', 501); // 501 characters exceeds the 500-character limit
        var command = new CreateCommentCommand(longContent, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Comment content cannot exceed 500 characters.");
    }

    [Fact]
    public void Validate_EmptyAuthorId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand("Valid content", Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage("AuthorId cannot be empty.");
    }

    [Fact]
    public void Validate_EmptyPostId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand("Valid content", Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage("PostId cannot be empty.");
    }
}