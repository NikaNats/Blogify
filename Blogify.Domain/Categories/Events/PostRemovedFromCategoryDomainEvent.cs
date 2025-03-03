using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories.Events;

public sealed record PostRemovedFromCategoryDomainEvent(Guid IdValue, Guid PostIdValue) : DomainEvent;