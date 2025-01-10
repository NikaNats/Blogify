using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.AddCommentToPost;

public sealed class AddCommentToPostCommandHandler(IPostRepository postRepository)
    : ICommandHandler<AddCommentToPostCommand>
{
    public async Task<Result> Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        var result = post.AddComment(request.Content, request.AuthorId);
        if (result.IsFailure)
            return result;

        await postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}