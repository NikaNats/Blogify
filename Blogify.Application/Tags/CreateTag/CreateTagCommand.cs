using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Tags.CreateTag;

public sealed record CreateTagCommand(string Name) : ICommand<Guid>;