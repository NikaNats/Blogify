using Blogify.Application.Abstractions.Authentication;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts;
using Blogify.Domain.Posts;
using Blogify.Application.Abstractions.Pagination;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;

namespace Blogify.Application.Comments.GetCommentsByPostId;

internal sealed class
    GetCommentsByPostIdQueryHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserContext userContext)
    : IQueryHandler<GetCommentsByPostIdQuery, PagedResponse<CommentResponse>>
{
    public async Task<Result<PagedResponse<CommentResponse>>> Handle(GetCommentsByPostIdQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.PostId == Guid.Empty)
            return Result.Failure<PagedResponse<CommentResponse>>(CommentError.EmptyPostId);

        // Comments of a missing post, or of a draft/archived post the caller cannot see, are reported as NotFound.
        var visibility = await postRepository.GetVisibilityAsync(request.PostId, cancellationToken);
        if (visibility is null || !PostAccess.CanView(visibility.Status, visibility.AuthorId, userContext))
            return Result.Failure<PagedResponse<CommentResponse>>(PostErrors.NotFound);

        var page = await commentRepository.GetByPostIdAsync(
            request.PostId, request.Page, request.PageSize, cancellationToken);

        return Result.Success(PagedResponse<CommentResponse>.From(page, request.Page, request.PageSize,
            comment => new CommentResponse(
                comment.Id,
                comment.Content.Value,
                comment.AuthorId,
                comment.PostId,
                comment.CreatedAt)));
    }
}
