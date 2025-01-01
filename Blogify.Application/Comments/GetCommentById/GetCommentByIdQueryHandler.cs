using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.GetCommentById;

public sealed class GetCommentByIdQueryHandler(ICommentRepository commentRepository)
    : IRequestHandler<GetCommentByIdQuery, Result<CommentResponse>>
{
    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Check for invalid ID
        if (request.Id == Guid.Empty)
            return Result.Failure<CommentResponse>(Error.Validation("Comment.InvalidId",
                "The provided comment ID is invalid."));

        var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (comment is null)
            return Result.Failure<CommentResponse>(Error.NotFound("Comment.NotFound", "Comment not found."));

        var response = new CommentResponse(comment.Id, comment.Content, comment.AuthorId, comment.PostId,
            comment.CreatedAt);
        return Result.Success(response);
    }
}