using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.UpdatePost;

public sealed record UpdatePostCommand(
    Guid Id,
    PostTitle Title,
    PostContent Content,
    PostExcerpt Excerpt,
    Guid CategoryId) : IRequest<Result>;