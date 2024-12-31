using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.ArchivePost;

public sealed class ArchivePostCommandHandler : IRequestHandler<ArchivePostCommand, Result>
{
    private readonly IPostRepository _postRepository;

    public ArchivePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result> Handle(ArchivePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        post.Archive();
        await _postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}