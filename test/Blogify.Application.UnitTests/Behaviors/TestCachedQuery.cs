using Blogify.Application.Abstractions.Caching;
using Blogify.Domain.Abstractions;

namespace Blogify.Application.UnitTests.Behaviors;

public sealed record TestCachedQuery(string CacheKey, TimeSpan? Expiration)
    : ICachedQuery<Result<string>>;