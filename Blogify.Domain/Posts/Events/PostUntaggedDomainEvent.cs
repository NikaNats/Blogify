using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed record PostUntaggedDomainEvent(
    Guid PostId,
    Guid TagId,
    string TagName) : DomainEvent;