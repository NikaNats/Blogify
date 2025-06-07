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
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "", Guid.NewGuid());

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
        var command = new AddCommentToPostCommand(Guid.NewGuid(), new string('a', 501), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.ContentTooLong.Description);
    }

    [Fact]
    public void Validate_EmptyAuthorId_ReturnsValidationError()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "Valid content", Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage(CommentError.EmptyAuthorId.Description);
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "Valid content", Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}