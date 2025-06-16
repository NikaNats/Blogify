using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.RemoveTagFromPost;

internal sealed class RemoveTagFromPostCommandHandler(
    IPostRepository postRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveTagFromPostCommand>
{
    public async Task<Result> Handle(RemoveTagFromPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        var tag = await tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null) return Result.Failure(TagErrors.NotFound);

        var removeTagResult = post.RemoveTag(tag);
        if (removeTagResult.IsFailure)
            return removeTagResult;

        if (post.DomainEvents.Any(e => e is PostUntaggedDomainEvent))
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}