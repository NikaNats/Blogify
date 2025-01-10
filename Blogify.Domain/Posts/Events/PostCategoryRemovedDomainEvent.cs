using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed class PostCategoryRemovedDomainEvent(Guid postId, Guid categoryId) : IDomainEvent;