namespace Blogify.Api.Controllers.Posts;

public sealed record CreatePostRequest(
    string Title,
    string Content,
    string Excerpt);