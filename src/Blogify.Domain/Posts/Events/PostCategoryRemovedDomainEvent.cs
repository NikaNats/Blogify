using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed record PostCategoryRemovedDomainEvent(
    Guid PostId,
    Guid CategoryId) : DomainEvent;