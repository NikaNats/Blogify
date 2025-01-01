using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.CreatePost;

public sealed class CreatePostCommandHandler(IPostRepository postRepository)
    : IRequestHandler<CreatePostCommand, Result<Guid>>
{
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

            if (post.IsFailure) return Result.Failure<Guid>(post.Error);

            await postRepository.AddAsync(post.Value, cancellationToken);

            return Result.Success(post.Value.Id);
        }
        catch (ConcurrencyException)
        {
            return Result.Failure<Guid>(PostErrors.Overlap);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure("Post.Create.Failed",
                "An error occurred while creating the post."));
        }
    }
}