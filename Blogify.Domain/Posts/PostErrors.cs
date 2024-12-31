using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public static class PostErrors
{
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
    
    public static readonly Error Overlap = Error.Failure(
        "Post.Overlap",
        "The current Post is overlapping with an existing one");
}