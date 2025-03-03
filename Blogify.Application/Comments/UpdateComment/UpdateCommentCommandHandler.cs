using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Comments;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Comments.UpdateComment;

internal sealed class UpdateCommentCommandHandler(ICommentRepository commentRepository)
    : ICommandHandler<UpdateCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Fetch the existing comment
        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
        {
            return Result.Failure<Unit>(CommentError.NotFound);
        }

        // Validate if the author is updating their own comment
        if (comment.AuthorId != request.AuthorId)
        {
            return Result.Failure<Unit>(CommentError.UnauthorizedUpdate);
        }

        // Update the comment content
        var updateResult = comment.Update(request.Content);
        if (updateResult.IsFailure)
        {
            return Result.Failure<Unit>(updateResult.Error);
        }

        // Save the updated comment
        await commentRepository.UpdateAsync(comment, cancellationToken);

        return Result.Success(Unit.Value);
    }
}