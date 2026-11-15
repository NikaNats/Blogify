using Blogify.Domain.Abstractions;

namespace Blogify.Application.Abstractions.Pagination;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static PagedResponse<T> From<TSource>(PagedList<TSource> source, int page, int pageSize,
        Func<TSource, T> map)
    {
        return new PagedResponse<T>(source.Items.Select(map).ToList(), page, pageSize, source.TotalCount);
    }
}
