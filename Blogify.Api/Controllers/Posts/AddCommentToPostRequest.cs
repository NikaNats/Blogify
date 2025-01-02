namespace Blogify.Api.Controllers.Posts;

public sealed record AddCommentToPostRequest(string Content, Guid AuthorId);