using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.AddTagToPost;

public sealed record AddTagToPostCommand(Guid PostId, Guid TagId) : ICommand;