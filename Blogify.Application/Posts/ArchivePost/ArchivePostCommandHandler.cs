using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.ArchivePost;

public sealed class ArchivePostCommandHandler(IPostRepository postRepository)
    : IRequestHandler<ArchivePostCommand, Result>
{
    public async Task<Result> Handle(ArchivePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        // Archive the post
        var archiveResult = post.Archive();
        if (archiveResult.IsFailure)
            return archiveResult;

        await postRepository.UpdateAsync(post, cancellationToken);
        
        return Result.Success();
    }
}