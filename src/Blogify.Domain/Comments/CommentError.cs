using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments;

public static class CommentError
{
    private const string Prefix = "Comment";

    public static readonly Error EmptyCommentId = Error.Validation(
        $"{Prefix}.EmptyCommentId",
        "A valid comment identifier is required.");

    public static readonly Error EmptyAuthorId = Error.Validation(
        $"{Prefix}.EmptyAuthorId",
        "A valid author identifier is required.");

    public static readonly Error EmptyPostId = Error.Validation(
        $"{Prefix}.EmptyPostId",
        "A valid post identifier is required.");

    public static readonly Error EmptyContent = Error.Validation(
        $"{Prefix}.EmptyContent",
        "Comment content cannot be empty.");

    public static readonly Error ContentTooShort = Error.Validation(
        $"{Prefix}.ContentTooShort",
        "Comment content is too short.");

    public static readonly Error ContentTooLong = Error.Validation(
        $"{Prefix}.ContentTooLong",
        "Comment content exceeds the maximum allowed length of 1000 characters.");

    public static readonly Error DisallowedContent = Error.Validation(
        $"{Prefix}.DisallowedContent",
        "Comment contains disallowed content. Please review our community guidelines.");

    // Domain rule errors
    public static readonly Error UnauthorizedUpdate = Error.Conflict(
        $"{Prefix}.UnauthorizedUpdate",
        "Only the author can update this comment.");

    public static readonly Error UnauthorizedDeletion = Error.Conflict(
        $"{Prefix}.UnauthorizedDeletion",
        "Only the author can delete this comment.");

    public static readonly Error AlreadyDeleted = Error.Conflict(
        $"{Prefix}.AlreadyDeleted",
        "This comment has already been deleted.");

    public static readonly Error CannotModifyDeletedComment = Error.Conflict(
        $"{Prefix}.CannotModifyDeletedComment",
        "Deleted comments cannot be modified.");

    public static readonly Error CannotFlagDeletedComment = Error.Conflict(
        $"{Prefix}.CannotFlagDeletedComment",
        "Deleted comments cannot be flagged.");

    public static readonly Error CannotFlagOwnComment = Error.Conflict(
        $"{Prefix}.CannotFlagOwnComment",
        "You cannot flag your own comment.");

    // Infrastructure errors
    public static readonly Error NotFound = Error.NotFound(
        $"{Prefix}.NotFound",
        "The requested comment could not be found.");

    public static readonly Error OperationFailed = Error.Failure(
        $"{Prefix}.OperationFailed",
        "The comment operation failed due to a technical Error.");
}