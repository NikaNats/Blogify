using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.DeletePost;

internal sealed class DeletePostCommandHandler(IPostRepository postRepository)
    : ICommandHandler<DeletePostCommand>
{
    public async Task<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Retrieve the post from the repository
            var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
            if (post is null)
                return Result.Failure(PostErrors.NotFound);

            // // Business rule validation: Prevent removal of published posts
            // if (post.Status == PostStatus.Published)
            //     return Result.Failure(PostErrors.CannotRemovePublishedPost);

            // Remove the post from the repository
            await postRepository.DeleteAsync(post, cancellationToken);

            return Result.Success();
        }
        catch (ConcurrencyException)
        {
            return Result.Failure(PostErrors.Overlap);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure("Post.Remove.Failed",
                "An error occurred while removing the post."));
        }
    }
}