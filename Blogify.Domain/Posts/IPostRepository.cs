namespace Blogify.Domain.Posts;

public interface IPostRepository
{
    // Single entity retrieval
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    // Bulk retrieval
    Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);

    // Write operations
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdateAsync(Post post, CancellationToken cancellationToken = default);
}