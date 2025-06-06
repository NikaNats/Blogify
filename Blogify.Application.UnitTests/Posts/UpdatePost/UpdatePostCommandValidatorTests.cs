using Blogify.Application.Posts.UpdatePost;
using Blogify.Domain.Posts;
using Shouldly;

namespace Blogify.Application.UnitTests.Posts.UpdatePost;

public class UpdatePostCommandValidatorTests
{
    private readonly UpdatePostCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsFullyValid_ShouldSucceed()
    {
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeTrue("a valid command should pass validation");
        result.Errors.ShouldBeEmpty("no validation errors should be present for a valid command");
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldFailWithTitleEmptyError()
    {
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            null,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse("a command with a null title should fail validation");
        result.Errors.ShouldHaveSingleItem()
            .ErrorMessage.ShouldBe(PostErrors.TitleEmpty.Description, "the error should indicate the title cannot be empty");
    }

    [Fact]
    public void Validate_WhenContentIsNull_ShouldFailWithContentEmptyError()
    {
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Valid Title").Value,
            null,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse("a command with null content should fail validation");
        result.Errors.ShouldHaveSingleItem()
            .ErrorMessage.ShouldBe(PostErrors.ContentEmpty.Description, "the error should indicate the content cannot be empty");
    }

    [Fact]
    public void Validate_WhenExcerptIsNull_ShouldFailWithExcerptEmptyError()
    {
        var command = new UpdatePostCommand(
            Guid.NewGuid(),
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            null
        );

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse("a command with a null excerpt should fail validation");
        result.Errors.ShouldHaveSingleItem()
            .ErrorMessage.ShouldBe(PostErrors.ExcerptEmpty.Description, "the error should indicate the excerpt cannot be empty");
    }

    [Fact]
    public void Validate_WhenAllPropertiesAreNull_ShouldFailWithMultipleErrors()
    {
        var command = new UpdatePostCommand(Guid.NewGuid(), null, null, null);

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse("a command with all properties null should fail validation");
        result.Errors.Count.ShouldBe(3, "three validation errors should be reported");
        result.Errors.Select(e => e.ErrorMessage).ShouldBe(
            new[]
            {
                PostErrors.TitleEmpty.Description,
                PostErrors.ContentEmpty.Description,
                PostErrors.ExcerptEmpty.Description
            }, "errors should match the expected messages for null title, content, and excerpt"
        );
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldFailWithInvalidIdError()
    {
        var command = new UpdatePostCommand(
            Guid.Empty,
            PostTitle.Create("Valid Title").Value,
            PostContent.Create(new string('a', 100)).Value,
            PostExcerpt.Create("Valid excerpt.").Value
        );

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse("a command with an empty Id should fail validation");
        result.Errors.Select(e => e.ErrorMessage)
            .ShouldContain(error => error.Contains("Id"), "the error should relate to the invalid Id");
    }

}
