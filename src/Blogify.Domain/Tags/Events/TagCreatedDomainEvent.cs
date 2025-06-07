using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Tags.Events;

public sealed record TagCreatedDomainEvent(Guid TagId) : DomainEvent;