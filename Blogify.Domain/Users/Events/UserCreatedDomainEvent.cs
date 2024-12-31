using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users.Events;

public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;