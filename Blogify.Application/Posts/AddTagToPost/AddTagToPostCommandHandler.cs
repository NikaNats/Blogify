using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using MediatR;

namespace Blogify.Application.Posts.AddTagToPost;

public sealed class AddTagToPostCommandHandler(IPostRepository postRepository, ITagRepository tagRepository)
    : IRequestHandler<AddTagToPostCommand, Result>
{
    public async Task<Result> Handle(AddTagToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        var tag = await tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return Result.Failure(Error.NotFound("Tag.NotFound", "Tag not found."));

        // Add the tag to the post
        var addTagResult = post.AddTag(tag);
        if (addTagResult.IsFailure)
            return addTagResult;

        await postRepository.UpdateAsync(post, cancellationToken);
        
        return Result.Success();
    }
}