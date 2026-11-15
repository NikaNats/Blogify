using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;

namespace Blogify.Application.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string Description) : ICommand<Guid>, ICacheInvalidatingCommand
{
    public IEnumerable<string> CacheKeysToInvalidate => [CacheKeys.AllCategories];
}
