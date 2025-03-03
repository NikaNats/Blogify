using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.GetCommentById;

internal sealed class GetCommentByIdQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentByIdQuery, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken);
        
        return comment is null 
            ? Result.Failure<CommentResponse>(CommentError.NotFound) 
            : Result.Success(new CommentResponse(
                comment.Id,
                comment.Content.Value,
                comment.AuthorId,
                comment.PostId,
                comment.CreatedAt));
    }
}