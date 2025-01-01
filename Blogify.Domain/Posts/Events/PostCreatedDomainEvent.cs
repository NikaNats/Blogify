using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed record PostCreatedDomainEvent(Guid PostId) : IDomainEvent;