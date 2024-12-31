using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.CreatePost;

public sealed record CreatePostCommand(
    PostTitle Title,
    PostContent Content,
    PostExcerpt Excerpt,
    Guid AuthorId,
    Guid CategoryId) : IRequest<Result<Guid>>;