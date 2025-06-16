using Blogify.Application.Posts.AddCommentToPost;
using Blogify.Domain.Comments;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Posts.AddComment;

public class AddCommentToPostCommandValidatorTests
{
    private readonly AddCommentToPostCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyContent_ReturnsValidationError()
    {
        // Arrange
        // --- FIX: Use the new constructor which does not include AuthorId. ---
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.EmptyContent.Description);
    }

    [Fact]
    public void Validate_ContentExceedsMaxLength_ReturnsValidationError()
    {
        // Arrange
        // --- FIX: Use the new constructor. ---
        var command = new AddCommentToPostCommand(Guid.NewGuid(), new string('a', 501));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.ContentTooLong.Description);
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        // --- FIX: Use the new constructor. ---
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "This is a valid comment.");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}