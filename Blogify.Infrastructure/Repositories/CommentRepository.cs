using System.Linq.Expressions;
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

    public async Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<Comment>()
            .AsNoTracking()
            .Where(comment => comment.PostId == postId)
            .ToListAsync(cancellationToken);
    }
}