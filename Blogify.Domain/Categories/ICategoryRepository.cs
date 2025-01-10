using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAllWithPostsCountAsync(CancellationToken cancellationToken = default);
    // Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // Task<IReadOnlyCollection<Category?>> GetAllAsync(CancellationToken cancellationToken = default);
    // Task AddAsync(Category category, CancellationToken cancellationToken = default);
    // Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    // Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
}