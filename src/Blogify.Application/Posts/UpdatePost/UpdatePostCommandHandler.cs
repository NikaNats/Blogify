using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Posts.Events;
using System.Linq;

namespace Blogify.Application.Posts.UpdatePost;

internal sealed class UpdatePostCommandHandler(
    IPostRepository postRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePostCommand>
{
    public async Task<Result> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        var updateResult = post.Update(
            request.Title,
            request.Content,
            request.Excerpt
        );

        if (updateResult.IsFailure) return updateResult;

        // Only persist if an update domain event was raised (i.e., something actually changed)
        if (post.DomainEvents.Any(e => e is PostUpdatedDomainEvent))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}