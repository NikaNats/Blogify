using Blogify.Domain.Abstractions;

namespace Blogify.Application.UnitTests;

internal static class TestPaging
{
    public static PagedList<T> Of<T>(IReadOnlyList<T> items)
    {
        return new PagedList<T>(items, items.Count);
    }
}
