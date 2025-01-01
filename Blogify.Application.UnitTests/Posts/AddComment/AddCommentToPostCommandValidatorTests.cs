using Blogify.Application.Posts.AddCommentToPost;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Posts.AddComment;

public class AddCommentToPostCommandValidatorTests
{
    private readonly AddCommentToPostCommandValidator _validator = new();

    [Fact]
    public void Validate_ContentEmpty_ReturnsValidationError()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), string.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Comment content cannot be empty.");
    }

    [Fact]
    public void Validate_ContentExceedsMaxLength_ReturnsValidationError()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), new string('a', 501), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Comment content cannot exceed 500 characters.");
    }

    [Fact]
    public void Validate_AuthorIdEmpty_ReturnsValidationError()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "Test comment", Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage("AuthorId cannot be empty.");
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsNoValidationErrors()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "Test comment", Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}