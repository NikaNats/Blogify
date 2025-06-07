using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Tags.UpdateTag;

public sealed record UpdateTagCommand(Guid Id, string Name) : ICommand;