using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;
using Blogify.Domain.Users;

namespace Blogify.Application.Comments.CreateComment;

internal sealed class CreateCommentCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository,
    IPostRepository postRepository,
    ICommentRepository commentRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var authorId = userContext.UserId;
        if (authorId == Guid.Empty) return Result.Failure<Guid>(UserErrors.UserNotFound);

        var post = await postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null) return Result.Failure<Guid>(PostErrors.NotFound);

        if (!post.CanReceiveComments()) return Result.Failure<Guid>(PostErrors.CommentToUnpublishedPost);

        var authorExists = await userRepository.ExistsAsync(u => u.Id == authorId, cancellationToken);
        if (!authorExists) return Result.Failure<Guid>(UserErrors.UserNotFound);

        var commentResult = Comment.Create(
            request.Content,
            authorId,
            request.PostId);

        if (commentResult.IsFailure) return Result.Failure<Guid>(commentResult.Error);

        await commentRepository.AddAsync(commentResult.Value, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(commentResult.Value.Id);
    }
}