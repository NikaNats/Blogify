using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts.Events;

public sealed class PostCategoryAddedDomainEvent(Guid postId, Guid categoryId) : IDomainEvent;