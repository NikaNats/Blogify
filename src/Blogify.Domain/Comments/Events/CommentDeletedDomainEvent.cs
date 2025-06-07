using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments.Events;

public sealed record CommentDeletedDomainEvent(
    Guid CommentId,
    Guid PostId) : DomainEvent;