using Blogify.Application.Posts.AddCommentToPost;
using Blogify.Domain.Comments;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Posts.AddComment;

public class AddCommentToPostCommandValidatorTests
{
    private readonly AddCommentToPostCommandValidator _validator = new();

    [Fact]
    public void Validate_ContentEmpty_ReturnsValidationError()
    {
        var command = new AddCommentToPostCommand(Guid.NewGuid(), string.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.EmptyContent.Description); // "Content cannot be empty"
    }

    [Fact]
    public void Validate_ContentExceedsMaxLength_ReturnsValidationError()
    {
        var command = new AddCommentToPostCommand(Guid.NewGuid(), new string('a', 501), Guid.NewGuid());
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage(CommentError.ContentTooLong.Description); // "Content exceeds maximum allowed length"
    }

    [Fact]
    public void Validate_AuthorIdEmpty_ReturnsValidationError()
    {
        var command = new AddCommentToPostCommand(Guid.NewGuid(), "Valid content", Guid.Empty);
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage(CommentError.EmptyAuthorId.Description); // "Author ID cannot be empty"
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