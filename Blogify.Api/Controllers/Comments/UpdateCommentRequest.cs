namespace Blogify.Api.Controllers.Comments;

public sealed record UpdateCommentRequest(Guid CommentId, string Content);