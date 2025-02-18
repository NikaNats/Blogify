using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.UpdatePost;

public sealed record UpdatePostCommand(
    Guid Id,
    PostTitle Title,
    PostContent Content,
    PostExcerpt Excerpt) : ICommand;