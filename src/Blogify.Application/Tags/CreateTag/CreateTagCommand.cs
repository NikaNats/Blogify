using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Tags.CreateTag;

public sealed record CreateTagCommand(string Name) : ICommand<Guid>, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheTagsToInvalidate => [CacheTagNames.AllTags];
}
