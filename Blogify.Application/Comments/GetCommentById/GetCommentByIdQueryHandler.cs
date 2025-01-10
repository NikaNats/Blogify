using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.GetCommentById;

public sealed class GetCommentByIdQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentByIdQuery, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (comment is null)
            return Result.Failure<CommentResponse>(CommentError.CommentNotFound);

        var response = new CommentResponse(comment.Id, comment.Content.Value, comment.AuthorId, comment.PostId,
            comment.CreatedAt);
        return Result.Success(response);
    }
}