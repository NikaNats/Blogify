namespace Blogify.Application.Comments.GetCommentById;

public sealed record CommentByIdResponse(Guid Id, string Content, Guid AuthorId, Guid PostId, DateTime CreatedAt);