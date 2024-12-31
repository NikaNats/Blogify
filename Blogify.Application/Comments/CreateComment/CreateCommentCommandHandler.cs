using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.CreateComment;

public sealed class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private readonly ICommentRepository _commentRepository;

    public CreateCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var commentResult = Comment.Create(request.Content, request.AuthorId, request.PostId);
        if (commentResult.IsFailure)
            return Result.Failure<Guid>(commentResult.Error);

        var comment = commentResult.Value;
        await _commentRepository.AddAsync(comment, cancellationToken);

        return Result.Success(comment.Id);
    }
}