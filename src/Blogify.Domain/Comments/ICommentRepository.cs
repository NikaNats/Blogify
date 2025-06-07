using System.Linq.Expressions;

namespace Blogify.Domain.Comments;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Comment entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Comment entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Comment entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<Comment, bool>> predicate, CancellationToken cancellationToken = default);

    // Specific methods for Comment
    Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken);
}