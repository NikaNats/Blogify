using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users.Events;

public sealed record EmailChangedDomainEvent(
    Guid UserId,
    string OldEmail,
    string NewEmail) : DomainEvent;