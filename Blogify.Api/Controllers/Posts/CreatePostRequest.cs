using Blogify.Domain.Posts;

namespace Blogify.Api.Controllers.Posts;

public sealed record CreatePostRequest(
    PostTitle Title,
    PostContent Content,
    PostExcerpt Excerpt,
    Guid AuthorId,
    Guid CategoryId);