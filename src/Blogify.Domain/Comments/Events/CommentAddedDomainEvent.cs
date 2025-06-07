using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments.Events;

public sealed record CommentAddedDomainEvent(
    Guid commentId,
    Guid postId,
    Guid authorId)
    : DomainEvent;