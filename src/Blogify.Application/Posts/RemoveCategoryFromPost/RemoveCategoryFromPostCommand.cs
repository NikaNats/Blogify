using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.RemoveCategoryFromPost;

public sealed record RemoveCategoryFromPostCommand(Guid PostId, Guid CategoryId) : ICommand;