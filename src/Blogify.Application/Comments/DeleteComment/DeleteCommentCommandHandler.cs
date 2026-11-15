using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Caching;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Users;
using MediatR;

namespace Blogify.Application.Comments.DeleteComment;

internal sealed class DeleteCommentCommandHandler(
    ICommentRepository commentRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ICacheService cacheService)
    : ICommandHandler<DeleteCommentCommand, Unit>
{
    public async Task<Result<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null) return Result.Failure<Unit>(CommentError.NotFound);

        if (userContext.UserId is not { } userId) return Result.Failure<Unit>(UserErrors.Unauthenticated);

        // The aggregate enforces authorship (or administrator moderation) and raises CommentDeletedDomainEvent.
        var removeResult = comment.Remove(userId, userContext.IsInRole(Role.Administrator.Name));
        if (removeResult.IsFailure) return Result.Failure<Unit>(removeResult.Error);

        await commentRepository.DeleteAsync(comment, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The command only carries the comment id, so the owning post is evicted here.
        await cacheService.RemoveAsync(CacheKeys.Post(comment.PostId), cancellationToken);

        return Result.Success(Unit.Value);
    }
}