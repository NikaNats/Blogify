using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed record PostTaggedDomainEvent(
    Guid PostId,
    Guid TagId,
    string TagName) : DomainEvent;