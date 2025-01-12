using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.GetCommentsByPostId;

internal sealed class
    GetCommentsByPostIdQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentsByPostIdQuery, List<CommentResponse>>
{
    public async Task<Result<List<CommentResponse>>> Handle(GetCommentsByPostIdQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var comments = await commentRepository.GetByPostIdAsync(request.PostId, cancellationToken);
        if (comments.Count == 0)
            return Result.Failure<List<CommentResponse>>(CommentError.NoCommentsFound);
        
        var response = comments.
            Select(comment => new CommentResponse(
                comment.Id,
                comment.Content.Value,
                comment.AuthorId,
                comment.PostId,
                comment.CreatedAt))
            .ToList();
        return Result.Success(response);
    }
}