using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Tags;

public static class TagErrors
{
    public static readonly Error PostNull = Error.Validation(
        "Tag.Post.Null",
        "Post cannot be null."
    );

    public static readonly Error PostDuplicate = Error.Validation(
        "Tag.Post.Duplicate",
        "Post already exists in the tag."
    );

    public static readonly Error PostNotFound = Error.Validation(
        "Tag.Post.NotFound",
        "Post does not exist in the tag."
    );
}