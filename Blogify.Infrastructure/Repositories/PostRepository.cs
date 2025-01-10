using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class PostRepository(ApplicationDbContext dbContext) : Repository<Post>(dbContext), IPostRepository
{
    public async Task<IReadOnlyList<Post>> GetByAuthorIdAsync(Guid authorId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Post>()
            .AsNoTracking()
            .Include(p => p.Categories)  // Include related entities
            .Include(p => p.Tags)
            .Where(post => post.AuthorId == authorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByCategoryIdAsync(Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Post>()
            .AsNoTracking()
            .Include(p => p.Tags) // Include related Tags
            .Include(p => p.Categories) // Include related Categories
            .Where(post => post.Categories.Any(c => c.Id == categoryId)) // Filter by CategoryId
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Post>()
            .AsNoTracking()
            .Include(p => p.Categories)  // Include related entities
            .Include(p => p.Tags)
            .Where(post => post.Tags.Any(tag => tag.Id == tagId))
            .ToListAsync(cancellationToken);
    }
    
}