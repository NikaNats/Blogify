using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Tags;

public interface ITagRepository : IRepository<Tag>
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedList<Tag>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken);
}