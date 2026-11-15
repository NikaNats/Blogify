using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.PublishPost;

public sealed record PublishPostCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.Post(Id)];
}
