using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed record PostArchivedDomainEvent(
    Guid PostId,
    string PostTitle,
    Guid AuthorId) : DomainEvent;