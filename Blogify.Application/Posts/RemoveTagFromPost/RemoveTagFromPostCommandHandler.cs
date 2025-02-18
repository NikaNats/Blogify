using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.RemoveTagFromPost;

internal sealed class RemoveTagFromPostCommandHandler(IPostRepository postRepository, ITagRepository tagRepository)
    : ICommandHandler<RemoveTagFromPostCommand>
{
    public async Task<Result> Handle(RemoveTagFromPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        var tag = await tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return Result.Failure(TagErrors.NotFound);

        // Remove the tag from the post
        var removeTagResult = post.RemoveTag(tag);
        if (removeTagResult.IsFailure)
        {
            return removeTagResult;
        }
        
        await postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}