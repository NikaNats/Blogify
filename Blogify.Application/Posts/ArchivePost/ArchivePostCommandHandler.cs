using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.ArchivePost;

internal sealed class ArchivePostCommandHandler(IPostRepository postRepository)
    : ICommandHandler<ArchivePostCommand>
{
    public async Task<Result> Handle(ArchivePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        // Check current status before archiving
        if (post.Status == PublicationStatus.Archived)
            return Result.Success();

        var archiveResult = post.Archive();
        if (archiveResult.IsFailure)
            return archiveResult;

        await postRepository.UpdateAsync(post, cancellationToken);

        return Result.Success();
    }
}