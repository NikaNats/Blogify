using System.Linq.Expressions;
using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class PostRepository(ApplicationDbContext dbContext) : IPostRepository
{
    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>()
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Post entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Post>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Post entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Post>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Post entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Post>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>().AnyAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByAuthorIdAsync(Guid authorId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Where(post => post.AuthorId == authorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByCategoryIdAsync(Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .Include(p => p.Tags)
            .Include(p => p.Categories)
            .Where(post => post.Categories.Any(c => c.Id == categoryId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .Include(p => p.Categories)
            .Include(p => p.Tags)
            .Where(post => post.Tags.Any(tag => tag.Id == tagId))
            .ToListAsync(cancellationToken);
    }
}