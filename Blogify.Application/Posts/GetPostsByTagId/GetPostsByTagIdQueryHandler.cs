using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts.GetPostById;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetPostsByTagId;

internal sealed class GetPostsByTagIdQueryHandler : IQueryHandler<GetPostsByTagIdQuery, List<PostResponse>>
{
    private readonly IPostRepository _postRepository;

    public GetPostsByTagIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<List<PostResponse>>> Handle(GetPostsByTagIdQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetByTagIdAsync(request.TagId, cancellationToken);
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
            post.Tags.MapToAllTagResponses()
        );
    }
}