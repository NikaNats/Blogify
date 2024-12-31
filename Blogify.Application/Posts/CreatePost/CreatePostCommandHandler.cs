using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.CreatePost;

public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<Guid>>
{
    private readonly IPostRepository _postRepository;

    public CreatePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var post = Post.Create(
                request.Title,
                request.Content,
                request.Excerpt,
                request.AuthorId,
                request.CategoryId);

            await _postRepository.AddAsync(post.Value, cancellationToken);

            return Result.Success(post.Value.Id);
        }
        catch (ConcurrencyException)
        {
            return Result.Failure<Guid>(PostErrors.Overlap);
        }
    }
}