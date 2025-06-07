using Blogify.Application.Posts.RemoveTagFromPost;
using FluentValidation.TestHelper;

namespace Blogify.Application.UnitTests.Posts.RemoveTag;

public class RemoveTagFromPostCommandValidatorTests
{
    private readonly RemoveTagFromPostCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyPostId_ReturnsValidationError()
    {
        var command = new RemoveTagFromPostCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage("PostId cannot be a default GUID.");
    }

    [Fact]
    public void Validate_EmptyTagId_ReturnsValidationError()
    {
        var command = new RemoveTagFromPostCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TagId)
            .WithErrorMessage("TagId cannot be a default GUID.");
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new RemoveTagFromPostCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}