using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.DeletePost;

public sealed record DeletePostCommand(Guid PostId) : ICommand;