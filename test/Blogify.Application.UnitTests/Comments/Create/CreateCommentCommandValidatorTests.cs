using Blogify.Application.Comments.CreateComment;
using Blogify.Domain.Comments;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Comments.Create;

public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.NewGuid(), "This is valid content");

        // Act & Assert
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyContent_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.NewGuid(), "");

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.EmptyContent.Description);
    }

    [Fact]
    public void Validate_ContentExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longContent = new string('a', 1001);
        var command = new CreateCommentCommand(Guid.NewGuid(), longContent);

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.ContentTooLong.Description);
    }

    [Fact]
    public void Validate_EmptyPostId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand(Guid.Empty, "This is valid content");

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage(CommentError.EmptyPostId.Description);
    }
}