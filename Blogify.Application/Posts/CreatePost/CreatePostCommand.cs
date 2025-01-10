using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.CreatePost;

public sealed record CreatePostCommand(
    PostTitle Title,
    PostContent Content,
    PostExcerpt Excerpt,
    Guid AuthorId,
    Guid CategoryId) : ICommand<Guid>;