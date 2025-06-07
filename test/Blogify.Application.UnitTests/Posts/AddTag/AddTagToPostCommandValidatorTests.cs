using Blogify.Application.Posts.AddTagToPost;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Posts.AddTag;

public class AddTagToPostCommandValidatorTests
{
    private readonly AddTagToPostCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyPostId_ReturnsValidationError()
    {
        // Arrange
        var command = new AddTagToPostCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage("PostId cannot be a default GUID.");
    }

    [Fact]
    public void Validate_EmptyTagId_ReturnsValidationError()
    {
        // Arrange
        var command = new AddTagToPostCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TagId)
            .WithErrorMessage("TagId cannot be a default GUID.");
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new AddTagToPostCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}