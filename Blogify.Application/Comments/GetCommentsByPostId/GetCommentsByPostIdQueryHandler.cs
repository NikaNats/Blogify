using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using MediatR;

namespace Blogify.Application.Comments.GetCommentsByPostId;

public sealed class
    GetCommentsByPostIdQueryHandler(ICommentRepository commentRepository)
    : IRequestHandler<GetCommentsByPostIdQuery, Result<List<CommentByPostIdResponse>>>
{
    public async Task<Result<List<CommentByPostIdResponse>>> Handle(GetCommentsByPostIdQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var comments = await commentRepository.GetByPostIdAsync(request.PostId, cancellationToken);
        var response = comments.Select(comment =>
                new CommentByPostIdResponse(comment.Id, comment.Content.Value, comment.AuthorId, comment.PostId, comment.CreatedAt))
            .ToList();
        return Result.Success(response);
    }
}