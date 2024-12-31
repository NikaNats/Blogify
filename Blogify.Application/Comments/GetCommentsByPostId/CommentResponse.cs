namespace Blogify.Application.Comments.GetCommentsByPostId;

public sealed record CommentResponse(Guid Id, string Content, Guid AuthorId, Guid PostId, DateTime CreatedAt);