using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Posts.Events;

namespace Blogify.Application.Posts.ArchivePost;

internal sealed class ArchivePostCommandHandler(
    IPostRepository postRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<ArchivePostCommand>
{
    public async Task<Result> Handle(ArchivePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        if (!PostAccess.CanModify(post, userContext)) return Result.Failure(PostErrors.Forbidden);

        var archiveResult = post.Archive();
        if (archiveResult.IsFailure)
            return archiveResult;

        if (post.DomainEvents.Any(e => e is PostArchivedDomainEvent))
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}