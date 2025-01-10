using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public static class PostErrors
{
    // General Errors
    public static readonly Error Overlap = Error.Failure(
        "Post.Overlap",
        "The current Post is overlapping with an existing one.");

    public static readonly Error NotFound = Error.NotFound(
        "Post.NotFound",
        "The post was not found.");

    // Validation Errors
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

    // Not Found Errors
    public static Error AuthorNotFound => Error.NotFound(
        "Post.Author.NotFound",
        "The author of the post was not found.");

    public static Error CategoryNotFound => Error.NotFound(
        "Post.Category.NotFound",
        "The category of the post was not found.");

    public static Error TagNotFound => Error.NotFound(
        "Post.Tag.NotFound",
        "The tag was not found.");

    // Conflict Errors
    public static Error AlreadyPublished => Error.Conflict(
        "Post.AlreadyPublished",
        "The post is already published.");

    public static Error NotPublished => Error.Validation(
        "Post.NotPublished",
        "The post is not published and cannot be archived.");
}