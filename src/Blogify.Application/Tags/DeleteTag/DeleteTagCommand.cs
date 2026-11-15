using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Tags.DeleteTag;

public sealed record DeleteTagCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheTagsToInvalidate => [CacheTagNames.Tag(Id), CacheTagNames.AllTags];
}
