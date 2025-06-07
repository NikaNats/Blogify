using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.UpdateComment;

internal sealed class UpdateCommentCommandHandler(
    ICommentRepository commentRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null) return Result.Failure<Unit>(CommentError.NotFound);

        if (comment.AuthorId != userContext.UserId) return Result.Failure<Unit>(CommentError.UnauthorizedUpdate);

        var updateResult = comment.Update(request.Content);
        if (updateResult.IsFailure) return Result.Failure<Unit>(updateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success(Unit.Value);
    }
}