using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.AddCommentToPost;

public sealed record AddCommentToPostCommand(Guid PostId, string Content) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.Post(PostId)];
}
