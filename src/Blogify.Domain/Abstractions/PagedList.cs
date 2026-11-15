namespace Blogify.Domain.Abstractions;

public sealed record PagedList<T>(IReadOnlyList<T> Items, int TotalCount)
{
    public static PagedList<T> Empty { get; } = new([], 0);
}
