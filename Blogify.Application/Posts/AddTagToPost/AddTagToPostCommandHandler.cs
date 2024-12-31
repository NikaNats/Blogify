using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;
using MediatR;

namespace Blogify.Application.Posts.AddTagToPost;

public sealed class AddTagToPostCommandHandler : IRequestHandler<AddTagToPostCommand, Result>
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;

    public AddTagToPostCommandHandler(IPostRepository postRepository, ITagRepository tagRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
    }

    public async Task<Result> Handle(AddTagToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        var tag = await _tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return Result.Failure(Error.NotFound("Tag.NotFound", "Tag not found."));

        post.AddTag(tag);
        await _postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}