using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users.Events;

public sealed record UserNameChangedDomainEvent(
    Guid Id,
    string OldFirstName,
    string OldLastName,
    string FirstNameValue,
    string LastNameValue
) : DomainEvent;