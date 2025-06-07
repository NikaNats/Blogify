using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories.Events;

public sealed record CategoryUpdatedDomainEvent(Guid CategoryId) : DomainEvent;