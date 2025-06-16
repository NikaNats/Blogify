using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.CreatePost;

public sealed record CreatePostCommand(
    string Title,
    string Content,
    string Excerpt) : ICommand<Guid>;