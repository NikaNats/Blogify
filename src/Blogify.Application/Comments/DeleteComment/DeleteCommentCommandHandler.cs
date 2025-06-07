using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.DeleteComment;

internal sealed class DeleteCommentCommandHandler(
    ICommentRepository commentRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null) return Result.Failure<Unit>(CommentError.NotFound);

        if (comment.AuthorId != userContext.UserId) return Result.Failure<Unit>(CommentError.UnauthorizedDeletion);

        await commentRepository.DeleteAsync(comment, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}