namespace Blogify.Application.Comments;

public sealed record CommentResponse(Guid Id, string Content, Guid AuthorId, Guid PostId, DateTimeOffset CreatedAt);