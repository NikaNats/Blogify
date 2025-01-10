using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Tags;

public interface ITagRepository : IRepository<Tag>
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken);

}