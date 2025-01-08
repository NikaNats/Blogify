using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class PostRepository(ApplicationDbContext dbContext) : Repository<Post>(dbContext), IPostRepository
{
    public async Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Post>()
            .FirstOrDefaultAsync(post => post.Slug.Value == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByAuthorIdAsync(Guid authorId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Post>()
            .Where(post => post.AuthorId == authorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByCategoryIdAsync(Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Post>()
            .Where(post => post.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Post>()
            .Where(post => post.Tags.Any(tag => tag.Id == tagId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<Post>().AddAsync(post, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await DbContext
            .Set<Post>()
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Post post, CancellationToken cancellationToken)
    {
        DbContext.Set<Post>().Update(post);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}