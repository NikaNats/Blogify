using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments.Events;

public sealed record CommentAddedDomainEvent(Guid CommentId) : IDomainEvent;