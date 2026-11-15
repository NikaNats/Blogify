using System.Linq.Expressions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class CommentRepository(ApplicationDbContext dbContext) : ICommentRepository
{
    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Comment>().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Comment>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Comment entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Comment>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Comment entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Comment>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Comment entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<Comment>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Comment, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Comment>().AnyAsync(predicate, cancellationToken);
    }

    public async Task<PagedList<Comment>> GetByPostIdAsync(Guid postId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Comment>().AsNoTracking().Where(comment => comment.PostId == postId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Comment>(items, total);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetCountsByPostIdsAsync(IReadOnlyCollection<Guid> postIds,
        CancellationToken cancellationToken = default)
    {
        if (postIds.Count == 0) return new Dictionary<Guid, int>();

        return await dbContext.Set<Comment>()
            .AsNoTracking()
            .Where(c => postIds.Contains(c.PostId))
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);
    }
}
