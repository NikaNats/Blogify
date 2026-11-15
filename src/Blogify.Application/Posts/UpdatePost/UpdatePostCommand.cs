using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Posts.UpdatePost;

public sealed record UpdatePostCommand(
    Guid Id,
    string Title,
    string Content,
    string Excerpt) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.Post(Id)];
}
