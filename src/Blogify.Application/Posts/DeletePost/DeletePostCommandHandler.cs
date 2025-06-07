using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.DeletePost;

internal sealed class DeletePostCommandHandler(
    IPostRepository postRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePostCommand>
{
    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        await postRepository.DeleteAsync(post, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            return Result.Failure(PostErrors.Overlap);
        }

        return Result.Success();
    }
}