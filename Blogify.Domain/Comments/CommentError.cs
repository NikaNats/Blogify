using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments;

public static class CommentError
{
    private const string Prefix = "Comment";

    public static readonly Error EmptyAuthorId = Error.Validation(
        $"{Prefix}.EmptyAuthorId",
        "AuthorId cannot be empty.");

    public static readonly Error EmptyPostId = Error.Validation(
        $"{Prefix}.EmptyPostId",
        "PostId cannot be empty.");

    public static readonly Error InvalidContent = Error.Validation(
        $"{Prefix}.InvalidContent",
        "Content cannot be null or empty.");

    public static readonly Error ContentTooLong = Error.Validation(
        $"{Prefix}.ContentTooLong",
        "Content exceeds the maximum allowed length.");

    public static readonly Error CommentNotFound = Error.NotFound(
        $"{Prefix}.NotFound",
        "The comment was not found.");

    public static readonly Error UnauthorizedUpdate = Error.Conflict(
        $"{Prefix}.UnauthorizedUpdate",
        "You are not authorized to update this comment.");

    public static readonly Error UnauthorizedDeletion = Error.Conflict(
        $"{Prefix}.UnauthorizedDeletion",
        "You are not authorized to delete this comment.");

    public static readonly Error UpdateFailed = Error.Failure(
        $"{Prefix}.UpdateFailed",
        "Failed to update the comment.");

    public static readonly Error DeletionFailed = Error.Failure(
        $"{Prefix}.DeletionFailed",
        "Failed to delete the comment.");
}