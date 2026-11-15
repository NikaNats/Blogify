using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.AllCategories];
}
