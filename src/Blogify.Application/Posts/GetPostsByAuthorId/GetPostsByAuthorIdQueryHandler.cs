using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts.GetPostById;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetPostsByAuthorId;

internal sealed class GetPostsByAuthorIdQueryHandler : IQueryHandler<GetPostsByAuthorIdQuery, List<PostResponse>>
{
    private readonly IPostRepository _postRepository;

    public GetPostsByAuthorIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<List<PostResponse>>> Handle(GetPostsByAuthorIdQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetByAuthorIdAsync(request.AuthorId, cancellationToken);
        var response = posts.Select(MapPostToResponse).ToList();
        return Result.Success(response);
    }

    private static PostResponse MapPostToResponse(Post post)
    {
        return new PostResponse(
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
    }
}