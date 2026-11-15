using System.Linq.Expressions;
using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Post entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Post entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Status and author of a post (null when it does not exist), without loading the aggregate.</summary>
    Task<PostVisibility?> GetVisibilityAsync(Guid id, CancellationToken cancellationToken = default);

    // Paged read methods (newest first). Comments are not loaded; use ICommentRepository for counts.
    // publishedOnly defaults to true so a new caller can never leak Draft/Archived posts by forgetting the flag.
    Task<PagedList<Post>> GetPagedAsync(int page, int pageSize, bool publishedOnly = true,
        CancellationToken cancellationToken = default);

    Task<PagedList<Post>> GetByAuthorIdAsync(Guid authorId, int page, int pageSize, bool publishedOnly = true,
        CancellationToken cancellationToken = default);

    Task<PagedList<Post>> GetByCategoryIdAsync(Guid categoryId, int page, int pageSize, bool publishedOnly = true,
        CancellationToken cancellationToken = default);

    Task<PagedList<Post>> GetByTagIdAsync(Guid tagId, int page, int pageSize, bool publishedOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a deleted category's id from every post that references it.</summary>
    Task RemoveCategoryReferencesAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>Removes a deleted tag's id from every post that references it.</summary>
    Task RemoveTagReferencesAsync(Guid tagId, CancellationToken cancellationToken = default);
}
