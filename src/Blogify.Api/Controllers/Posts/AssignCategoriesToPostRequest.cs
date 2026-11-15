namespace Blogify.Api.Controllers.Posts;

public sealed record AssignCategoriesToPostRequest(List<Guid> CategoryIds);
