using Blogify.Application.Posts.UpdatePost;
using Blogify.Domain.Posts;
using FluentAssertions;

namespace Blogify.Application.UnitTests.Posts.UpdatePost;

public class UpdatePostCommandValidatorTests
{
    private readonly UpdatePostCommandValidator _validator = new();

    /// <summary>
    ///     Verifies that a fully valid command passes validation without errors.
    /// </summary>
    [Fact]
    public void Validate_WhenCommandIsFullyValid_ShouldSucceed()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue("a valid command should pass validation");
        result.Errors.Should().BeEmpty("no validation errors should be present for a valid command");
    }

    /// <summary>
    ///     Ensures that a null Title property fails validation with the expected error.
    /// </summary>
    [Fact]
    public void Validate_WhenTitleIsNull_ShouldFailWithTitleEmptyError()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            null,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse("a command with a null title should fail validation");
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(
                PostErrors.TitleEmpty.Description,
                "the error should indicate the title cannot be empty"
            );
    }

    /// <summary>
    ///     Ensures that a null Content property fails validation with the expected error.
    /// </summary>
    [Fact]
    public void Validate_WhenContentIsNull_ShouldFailWithContentEmptyError()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Valid Title").Value,
            null,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse("a command with null content should fail validation");
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(
                PostErrors.ContentEmpty.Description,
                "the error should indicate the content cannot be empty"
            );
    }

    /// <summary>
    ///     Ensures that a null Excerpt property fails validation with the expected error.
    /// </summary>
    [Fact]
    public void Validate_WhenExcerptIsNull_ShouldFailWithExcerptEmptyError()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse("a command with a null excerpt should fail validation");
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(
                PostErrors.ExcerptEmpty.Description,
                "the error should indicate the excerpt cannot be empty"
            );
    }

    /// <summary>
    ///     Verifies that a command with all properties null fails with all expected errors.
    /// </summary>
    [Fact]
    public void Validate_WhenAllPropertiesAreNull_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            null,
            null,
            null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse("a command with all properties null should fail validation");
        result.Errors.Should().HaveCount(3, "three validation errors should be reported");
        result.Errors.Select(e => e.ErrorMessage).Should().Contain(
            new[]
            {
                PostErrors.TitleEmpty.Description,
                PostErrors.ContentEmpty.Description,
                PostErrors.ExcerptEmpty.Description
            },
            "errors should match the expected messages for null title, content, and excerpt"
        );
    }

    /// <summary>
    ///     Ensures that an empty Id fails validation, assuming the validator checks it.
    /// </summary>
    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldFailWithInvalidIdError()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.Empty,
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse("a command with an empty Id should fail validation");
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain(
                "Id",
                "the error should relate to the invalid Id"
            );
    }


    [Fact]
    public void Validate_ValidCommand_Succeeds()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Initial Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Initial excerpt.").Value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TitleIsNull_FailsWithTitleEmptyError()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            null,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Initial excerpt.").Value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(PostErrors.TitleEmpty.Description);
    }


    [Fact]
    public void Validate_ContentIsNull_FailsWithContentEmptyError()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Initial Title").Value,
            null,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(PostErrors.ContentEmpty.Description);
    }


    [Fact]
    public void Validate_ExcerptIsNull_FailsWithExcerptEmptyError()
    {
        // Arrange
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(PostErrors.ExcerptEmpty.Description);
    }
}