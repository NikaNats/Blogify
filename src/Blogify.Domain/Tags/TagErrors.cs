using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Tags;

public static class TagErrors
{
    public static readonly Error PostNull = Error.Validation(
        "Tag.Post.Null",
        "Post cannot be null."
    );

    public static readonly Error DuplicateName = Error.Validation(
        "Tag.DuplicateName",
        "A tag with the same name already exists.");

    public static readonly Error PostDuplicate = Error.Validation(
        "Tag.Post.Duplicate",
        "Post already exists in the tag."
    );

    public static readonly Error PostNotFound = Error.Validation(
        "Tag.Post.NotFound",
        "Post does not exist in the tag."
    );

    public static Error NotFound => Error.NotFound(
        "Tag.NotFound",
        "The tag was not found.");

    // Validation Errors
    public static Error NameEmpty => Error.Validation(
        "Tag.Name.Empty",
        "The tag name cannot be empty.");

    public static Error NameTooLong => Error.Validation(
        "Tag.Name.TooLong",
        "The tag name cannot exceed 50 characters.");

    public static Error AlreadyExists => Error.Conflict(
        "Tag.AlreadyExists",
        "A tag with the same name already exists.");
}