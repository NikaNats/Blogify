using Blogify.Domain.Comments;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class CommentRepository(ApplicationDbContext dbContext)
    : Repository<Comment>(dbContext), ICommentRepository
{
    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<Comment>().AddAsync(comment, cancellationToken);
    }

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Comment>()
            .FirstOrDefaultAsync(comment => comment.Id == id, cancellationToken);
    }

    public async Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Comment>()
            .Where(comment => comment.PostId == postId)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        DbContext.Set<Comment>().Update(comment);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        DbContext.Set<Comment>().Remove(comment);
        return Task.CompletedTask;
    }
}