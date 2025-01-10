using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments;

public interface ICommentRepository: IRepository<Comment>
{ 
    Task<List<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken);
}