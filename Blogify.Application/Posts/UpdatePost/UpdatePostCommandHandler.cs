using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.UpdatePost;

public sealed class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, Result>
{
    private readonly IPostRepository _postRepository;

    public UpdatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure(Error.NotFound("Post.NotFound", "Post not found."));

        post.Update(
            request.Title,
            request.Content,
            request.Excerpt,
            request.CategoryId);

        await _postRepository.UpdateAsync(post, cancellationToken);
        return Result.Success();
    }
}