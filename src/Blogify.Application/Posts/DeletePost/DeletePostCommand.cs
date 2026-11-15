using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.DeletePost;

public sealed record DeletePostCommand(Guid PostId) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.Post(PostId)];
}
