using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.AddCommentToPost;

internal sealed class AddCommentToPostCommandHandler(
    IPostRepository postRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<AddCommentToPostCommand>
{
    public async Task<Result> Handle(AddCommentToPostCommand request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        var authorId = userContext.UserId;

        var commentResult = post.AddComment(request.Content, authorId);
        if (commentResult.IsFailure) return Result.Failure(commentResult.Error);

        await commentRepository.AddAsync(commentResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}