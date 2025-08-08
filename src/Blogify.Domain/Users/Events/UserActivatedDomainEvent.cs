using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users.Events;

public sealed record UserActivatedDomainEvent(Guid UserId) : DomainEvent;
