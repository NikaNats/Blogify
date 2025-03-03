using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Comments;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Comments.DeleteComment;

internal sealed class DeleteCommentCommandHandler(ICommentRepository commentRepository)
    : ICommandHandler<DeleteCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure<Unit>(CommentError.NotFound);
        }

        // Validate if the author is deleting their own comment
        if (comment.AuthorId != request.AuthorId)
        {
            return Result.Failure<Unit>(CommentError.UnauthorizedDeletion);
        }

        await commentRepository.DeleteAsync(comment, cancellationToken);

        return Result.Success(Unit.Value);
    }
}