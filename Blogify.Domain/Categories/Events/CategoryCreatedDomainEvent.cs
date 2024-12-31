using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories.Events;

public sealed record CategoryCreatedDomainEvent(Guid CategoryId) : IDomainEvent;
