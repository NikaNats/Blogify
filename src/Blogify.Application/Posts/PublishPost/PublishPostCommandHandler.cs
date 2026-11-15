using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.PublishPost;

internal sealed class PublishPostCommandHandler(
    IPostRepository postRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<PublishPostCommand>
{
    public async Task<Result> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        if (!PostAccess.CanModify(post, userContext)) return Result.Failure(PostErrors.Forbidden);

        var publishResult = post.Publish();
        if (publishResult.IsFailure) return publishResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}