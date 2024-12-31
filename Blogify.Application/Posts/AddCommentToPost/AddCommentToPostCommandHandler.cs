using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.AddCommentToPost;

public sealed class AddCommentToPostCommandHandler : IRequestHandler<AddCommentToPostCommand, Result>
{
    private readonly IPostRepository _postRepository;

    public AddCommentToPostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result> Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        post.AddComment(request.Content, request.AuthorId);
        await _postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}