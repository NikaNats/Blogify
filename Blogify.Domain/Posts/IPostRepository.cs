namespace Blogify.Domain.Posts;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Post>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<List<Post>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<List<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    Task<List<Post>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Post post, CancellationToken cancellationToken);
    void Update(Post post);
}