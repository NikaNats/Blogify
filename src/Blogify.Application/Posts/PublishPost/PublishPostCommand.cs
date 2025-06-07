using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.PublishPost;

public sealed record PublishPostCommand(Guid Id) : ICommand;