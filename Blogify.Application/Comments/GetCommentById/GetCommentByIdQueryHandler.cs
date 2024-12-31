using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.GetCommentById;

public sealed class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, Result<CommentResponse>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentByIdQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (comment is null)
            return Result.Failure<CommentResponse>(Error.NotFound("Comment.NotFound", "Comment not found."));

        var response = new CommentResponse(comment.Id, comment.Content, comment.AuthorId, comment.PostId,
            comment.CreatedAt);
        return Result.Success(response);
    }
}