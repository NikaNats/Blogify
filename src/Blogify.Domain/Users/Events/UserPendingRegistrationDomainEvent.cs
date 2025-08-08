using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users.Events;

public sealed record UserPendingRegistrationDomainEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Password) : DomainEvent;
