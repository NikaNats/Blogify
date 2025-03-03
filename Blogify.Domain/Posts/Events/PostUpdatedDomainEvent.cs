using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed record PostUpdatedDomainEvent(
    Guid PostId,
    string PostTitle,
    Guid AuthorId) : DomainEvent;