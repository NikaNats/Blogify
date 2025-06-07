using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.ArchivePost;

public sealed record ArchivePostCommand(Guid Id) : ICommand;