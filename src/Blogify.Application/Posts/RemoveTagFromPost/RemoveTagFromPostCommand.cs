using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.RemoveTagFromPost;

public sealed record RemoveTagFromPostCommand(Guid PostId, Guid TagId) : ICommand;