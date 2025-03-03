using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories.Events;

public sealed record PostAddedToCategoryDomainEvent(Guid IdValue, Guid PostIdValue) : DomainEvent;