using System.Linq.Expressions;
using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class PostRepository(ApplicationDbContext dbContext) : IPostRepository
{
    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // We apply includes here because when you fetch a single Post for a command,
        // you often need its full state, including composed entities like comments.
        return await ApplyIncludes(dbContext.Set<Post>())
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // For GetAll, we typically don't include child collections to keep the query light.
        // The query handler can decide if it needs more data.
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Post entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Post>().Add(entity);
        return Task.CompletedTask;
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
        // This query is now simpler as it doesn't need includes for Category/Tag.
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .Where(post => post.AuthorId == authorId)
            .ToListAsync(cancellationToken);
    }

    // --- FIXED: Filtering logic for Category and Tag ---

    public async Task<IReadOnlyList<Post>> GetByCategoryIdAsync(Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        // CORRECT: We now query the primitive collection of IDs.
        // EF Core is capable of translating `.Contains()` on a collection of primitives
        // into an efficient SQL query (often using `JSON_VALUE` or array functions).
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .Where(post => post.CategoryIds.Contains(categoryId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        // CORRECT: The logic is identical for tags.
        return await dbContext.Set<Post>()
            .AsNoTracking()
            .Where(post => post.TagIds.Contains(tagId))
            .ToListAsync(cancellationToken);
    }

    // A private helper to apply includes consistently.
    // Note: We only include Comments, as Category/Tag are no longer navigation properties.
    private static IQueryable<Post> ApplyIncludes(IQueryable<Post> query)
    {
        return query.Include(p => p.Comments);
    }
}