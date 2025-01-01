using Blogify.Domain.Comments;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken)
    {
        await DbContext.Set<Comment>().AddAsync(comment, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext
            .Set<Comment>()
            .FirstOrDefaultAsync(comment => comment.Id == id, cancellationToken);
    }

    public async Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken)
    {
        return await DbContext
            .Set<Comment>()
            .Where(comment => comment.PostId == postId)
            .ToListAsync(cancellationToken);
    }
}