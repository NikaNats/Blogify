using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.GetCommentsByPostId;

public sealed class
    GetCommentsByPostIdQueryHandler : IRequestHandler<GetCommentsByPostIdQuery, Result<List<CommentResponse>>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsByPostIdQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<List<CommentResponse>>> Handle(GetCommentsByPostIdQuery request,
        CancellationToken cancellationToken)
    {
        var comments = await _commentRepository.GetByPostIdAsync(request.PostId, cancellationToken);
        var response = comments.Select(comment =>
                new CommentResponse(comment.Id, comment.Content, comment.AuthorId, comment.PostId, comment.CreatedAt))
            .ToList();
        return Result.Success(response);
    }
}