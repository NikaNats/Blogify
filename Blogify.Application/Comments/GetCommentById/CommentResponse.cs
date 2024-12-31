namespace Blogify.Application.Comments.GetCommentById;

public sealed record CommentResponse(Guid Id, string Content, Guid AuthorId, Guid PostId, DateTime CreatedAt);