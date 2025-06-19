namespace Blogify.Api.Controllers.Comments;

public sealed record CreateCommentRequest(Guid PostId, string Content);