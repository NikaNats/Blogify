using System.Linq.Expressions;
using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Category entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Category entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<Category, bool>> predicate, CancellationToken cancellationToken = default);

    // Specific methods for Category
    Task<PagedList<Category>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken);
}