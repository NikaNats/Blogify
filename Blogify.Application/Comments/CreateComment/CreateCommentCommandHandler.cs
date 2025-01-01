using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.CreateComment;

public sealed class CreateCommentCommandHandler(ICommentRepository commentRepository)
    : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var commentResult = Comment.Create(request.Content, request.AuthorId, request.PostId);
        if (commentResult.IsFailure)
            return Result.Failure<Guid>(commentResult.Error);

        var comment = commentResult.Value;
        await commentRepository.AddAsync(comment, cancellationToken);

        return Result.Success(comment.Id);
    }
}