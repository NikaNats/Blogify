using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.AddPostToCategoryCommand;

public sealed record AddPostToCategoryCommand(Guid CategoryId, Guid PostId) : ICommand;