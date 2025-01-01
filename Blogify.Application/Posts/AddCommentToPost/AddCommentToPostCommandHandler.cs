using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.AddCommentToPost;

public sealed class AddCommentToPostCommandHandler(IPostRepository postRepository)
    : IRequestHandler<AddCommentToPostCommand, Result>
{
    public async Task<Result> Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        var result = post.AddComment(request.Content, request.AuthorId);
        if (result.IsFailure)
            return result;
        await postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}