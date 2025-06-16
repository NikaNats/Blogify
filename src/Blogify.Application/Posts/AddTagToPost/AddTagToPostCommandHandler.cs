using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.AddTagToPost;

internal sealed class AddTagToPostCommandHandler(
    IPostRepository postRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddTagToPostCommand>
{
    public async Task<Result> Handle(AddTagToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        var tag = await tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null) return Result.Failure(TagErrors.NotFound);

        var eventCountBefore = post.DomainEvents.Count;

        var addTagResult = post.AddTag(tag);
        if (addTagResult.IsFailure) return addTagResult;

        var eventCountAfter = post.DomainEvents.Count;

        if (eventCountAfter > eventCountBefore)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}