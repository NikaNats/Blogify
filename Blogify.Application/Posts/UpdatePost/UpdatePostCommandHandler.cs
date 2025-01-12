using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.UpdatePost;

internal sealed class UpdatePostCommandHandler(IPostRepository postRepository) : ICommandHandler<UpdatePostCommand>
{
    public async Task<Result> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        post.Update(
            request.Title,
            request.Content,
            request.Excerpt
            );

        await postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}