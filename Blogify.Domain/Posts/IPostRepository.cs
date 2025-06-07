using System.Linq.Expressions;

namespace Blogify.Domain.Posts;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Post entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default);

    // Specific methods for Post
    Task<IReadOnlyList<Post>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
}