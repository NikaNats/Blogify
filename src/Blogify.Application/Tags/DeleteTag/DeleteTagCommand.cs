using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Tags.DeleteTag;

public sealed record DeleteTagCommand(Guid Id) : ICommand;