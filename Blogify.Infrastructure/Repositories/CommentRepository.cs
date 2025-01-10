using Blogify.Domain.Comments;
using Microsoft.EntityFrameworkCore;

namespace Blogify.Infrastructure.Repositories;

internal sealed class CommentRepository(ApplicationDbContext dbContext)
    : Repository<Comment>(dbContext), ICommentRepository
{
    
    public async Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Comment>()
            .AsNoTracking()
            .Where(comment => comment.PostId == postId)
            .ToListAsync(cancellationToken);
    }
    
}