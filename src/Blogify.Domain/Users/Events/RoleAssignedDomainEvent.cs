using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users.Events;

public sealed record RoleAssignedDomainEvent(
    Guid UserId,
    int RoleId,
    string RoleName) : DomainEvent;