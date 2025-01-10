using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public interface IPostRepository : IRepository<Post>
{
    Task<IReadOnlyList<Post>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
    
}