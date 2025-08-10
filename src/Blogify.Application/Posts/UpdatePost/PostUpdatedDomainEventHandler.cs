using Blogify.Application.Abstractions.Caching;
using Blogify.Domain.Posts.Events;
using MediatR;

namespace Blogify.Application.Posts.UpdatePost;

// Invalidates cached individual post when it's updated.
internal sealed class PostUpdatedDomainEventHandler(ICacheService cacheService)
    : INotificationHandler<PostUpdatedDomainEvent>
{
    public async Task Handle(PostUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var cacheKey = $"posts:{notification.PostId}";
        await cacheService.RemoveAsync(cacheKey, cancellationToken);
    }
}
