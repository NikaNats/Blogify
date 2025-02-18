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
        var command = new CreateCommentCommand("Valid content", Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyContent_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand("", Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.InvalidContent.Description);
    }

    [Fact]
    public void Validate_ContentExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longContent = new string('a', 501);
        var command = new CreateCommentCommand(longContent, Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.ContentTooLong.Description);
    }

    [Fact]
    public void Validate_EmptyAuthorId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand("Valid content", Guid.Empty, Guid.NewGuid());

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage(CommentError.EmptyAuthorId.Description);
    }

    [Fact]
    public void Validate_EmptyPostId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCommentCommand("Valid content", Guid.NewGuid(), Guid.Empty);

        // Act & Assert
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage(CommentError.EmptyPostId.Description);
    }
}