using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Exceptions;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.CreatePost;

internal sealed class CreatePostCommandHandler(
    IPostRepository postRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<CreatePostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var authorId = userContext.UserId;

        var postResult = Post.Create(
            request.Title,
            request.Content,
            request.Excerpt,
            authorId);

        if (postResult.IsFailure) return Result.Failure<Guid>(postResult.Error);

        var post = postResult.Value;
        await postRepository.AddAsync(post, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            return Result.Failure<Guid>(PostErrors.Overlap);
        }

        return Result.Success(post.Id);
    }
}