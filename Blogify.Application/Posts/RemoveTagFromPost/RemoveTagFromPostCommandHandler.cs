using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts.RemoveTagFromPost;

public sealed class RemoveTagFromPostCommandHandler : ICommandHandler<RemoveTagFromPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;

    public RemoveTagFromPostCommandHandler(IPostRepository postRepository, ITagRepository tagRepository)
    {
        _postRepository = postRepository;
        _tagRepository = tagRepository;
    }

    public async Task<Result> Handle(RemoveTagFromPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        var tag = await _tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return Result.Failure(TagErrors.NotFound);

        post.RemoveTag(tag);
        await _postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}