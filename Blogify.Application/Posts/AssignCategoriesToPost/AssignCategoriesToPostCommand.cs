using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.AssignCategoriesToPost;

public sealed record AssignCategoriesToPostCommand(Guid PostId, List<Guid> CategoryIds) : ICommand;
