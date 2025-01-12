using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.CreateComment;

internal sealed class CreateCommentCommandHandler(ICommentRepository commentRepository)
    : ICommandHandler<CreateCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var commentResult = Comment.Create(request.Content, request.AuthorId, request.PostId);
        if (commentResult.IsFailure)
            return Result.Failure<Guid>(commentResult.Error);

        await commentRepository.AddAsync(commentResult.Value, cancellationToken);
        return Result.Success(commentResult.Value.Id);
    }
}