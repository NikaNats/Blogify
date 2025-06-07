using Blogify.Domain.Posts;

namespace Blogify.Api.Controllers.Posts;

public sealed record UpdatePostRequest(
    PostTitle Title,
    PostContent Content,
    PostExcerpt Excerpt);