using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments.Events;

public sealed record CommentRemovedDomainEvent(Guid CommentId) : IDomainEvent;