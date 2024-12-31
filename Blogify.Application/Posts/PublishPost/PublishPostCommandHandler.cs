using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.PublishPost;

public sealed class PublishPostCommandHandler : IRequestHandler<PublishPostCommand, Result>
{
    private readonly IPostRepository _postRepository;

    public PublishPostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result> Handle(PublishPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        post.Publish();
        await _postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}