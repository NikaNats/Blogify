using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed record PostCategoryAddedDomainEvent(
    Guid PostId,
    Guid CategoryId,
    string CategoryName) : DomainEvent;