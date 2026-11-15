using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.AssignCategoryToPost;

public sealed record AssignCategoryToPostCommand(Guid PostId, Guid CategoryId) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.Post(PostId)];
}
