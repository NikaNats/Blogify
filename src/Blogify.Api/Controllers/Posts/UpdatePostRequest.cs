namespace Blogify.Api.Controllers.Posts;

public sealed record UpdatePostRequest(
    string Title,
    string Content,
    string Excerpt);