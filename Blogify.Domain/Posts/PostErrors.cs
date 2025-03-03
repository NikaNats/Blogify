using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public static class PostErrors
{
    public static readonly Error Overlap = Error.Failure(
        "Post.Overlap",
        "The current Post is overlapping with an existing one.");

    public static readonly Error NotFound = Error.NotFound(
        "Post.NotFound",
        "The post was not found.");

    public static readonly Error CommentToUnpublishedPost = Error.Validation(
        "Post.AddComment",
        "Cannot add comments to unpublished posts.");

    public static Error TitleEmpty => Error.Validation(
        "Post.Title.Empty",
        "The post title cannot be empty.");

    public static Error TitleTooLong => Error.Validation(
        "Post.Title.TooLong",
        "The post title cannot be longer than 200 characters.");

    public static Error ContentEmpty => Error.Validation(
        "Post.Content.Empty",
        "The post content cannot be empty.");

    public static Error ContentTooShort => Error.Validation(
        "Post.Content.TooShort",
        "The post content must be at least 100 characters long.");

    public static Error ContentTooLong => Error.Validation(
        "Post.Content.TooLong",
        "The post content cannot exceed 500 characters.");

    public static Error ExcerptEmpty => Error.Validation(
        "Post.Excerpt.Empty",
        "The post excerpt cannot be empty.");

    public static Error ExcerptTooLong => Error.Validation(
        "Post.Excerpt.TooLong",
        "The post excerpt cannot be longer than 500 characters.");

    public static Error SlugEmpty => Error.Validation(
        "Post.Slug.Empty",
        "The post slug cannot be empty.");

    public static Error SlugTooLong => Error.Validation(
        "Post.Slug.TooLong",
        "The post slug cannot be longer than 200 characters.");

    public static Error AuthorIdEmpty => Error.Validation(
        "Post.AuthorId.Empty",
        "AuthorId cannot be empty.");

    public static Error CategoryIdEmpty => Error.Validation(
        "Post.CategoryId.Empty",
        "CategoryId cannot be empty.");

    public static Error AuthorNotFound => Error.NotFound(
        "Post.Author.NotFound",
        "The author of the post was not found.");

    public static Error CategoryNotFound => Error.NotFound(
        "Post.Category.NotFound",
        "The category of the post was not found.");

    public static Error TagNotFound => Error.NotFound(
        "Post.Tag.NotFound",
        "The tag was not found.");

    public static Error AlreadyPublished => Error.Conflict(
        "Post.AlreadyPublished",
        "The post is already published.");

    public static Error NotPublished => Error.Validation(
        "Post.NotPublished",
        "The post is not published and cannot be archived.");

    public static Error TitleNull => Error.Validation(
        "Post.Title.Null",
        "The post title cannot be null.");

    public static Error ContentNull => Error.Validation(
        "Post.Content.Null",
        "The post content cannot be null.");

    public static Error ExcerptNull => Error.Validation(
        "Post.Excerpt.Null",
        "The post excerpt cannot be null.");

    public static Error CannotUpdateArchived => Error.Validation(
        "Post.Update.Archived",
        "Cannot update an archived post.");

    public static Error CannotPublishArchived => Error.Validation(
        "Post.Publish.Archived",
        "Cannot publish an archived post.");

    public static Error TagNull => Error.Validation(
        "Post.Tag.Null",
        "The tag cannot be null.");

    public static Error CategoryNull => Error.Validation(
        "Post.Category.Null",
        "The category cannot be null.");
}