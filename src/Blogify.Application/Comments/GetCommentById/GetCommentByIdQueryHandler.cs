using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts;

namespace Blogify.Application.Comments.GetCommentById;

internal sealed class GetCommentByIdQueryHandler(
    ICommentRepository commentRepository,
    IPostRepository postRepository,
    IUserContext userContext)
    : IQueryHandler<GetCommentByIdQuery, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (comment is null) return Result.Failure<CommentResponse>(CommentError.NotFound);

        // This endpoint is public, so a comment on a draft/archived post must not be readable by id either.
        var visibility = await postRepository.GetVisibilityAsync(comment.PostId, cancellationToken);
        if (visibility is null || !PostAccess.CanView(visibility.Status, visibility.AuthorId, userContext))
            return Result.Failure<CommentResponse>(CommentError.NotFound);

        return Result.Success(new CommentResponse(
            comment.Id,
            comment.Content.Value,
            comment.AuthorId,
            comment.PostId,
            comment.CreatedAt));
    }
}
