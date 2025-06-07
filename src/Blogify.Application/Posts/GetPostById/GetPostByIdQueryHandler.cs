using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetPostById;

internal sealed class GetPostByIdQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetPostByIdQuery, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure<PostResponse>(PostErrors.NotFound);

        var response = new PostResponse(
            post.Id,
            post.Title.Value,
            post.Content.Value,
            post.Excerpt.Value,
            post.Slug.Value,
            post.AuthorId,
            post.CreatedAt,
            post.LastModifiedAt,
            post.PublishedAt,
            post.Status,
            post.Comments.MapToCommentResponses(),
            post.Tags.MapToAllTagResponses(),
            post.Categories.MapToCategoryResponses());

        return Result.Success(response);
    }
}