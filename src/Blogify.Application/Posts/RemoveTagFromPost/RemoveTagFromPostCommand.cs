using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.RemoveTagFromPost;

public sealed record RemoveTagFromPostCommand(Guid PostId, Guid TagId) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.Post(PostId)];
}
