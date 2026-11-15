using System.Linq.Expressions;
using Blogify.Domain.Abstractions;

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
    Task<PagedList<Comment>> GetByPostIdAsync(Guid postId, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetCountsByPostIdsAsync(IReadOnlyCollection<Guid> postIds,
        CancellationToken cancellationToken = default);
}
