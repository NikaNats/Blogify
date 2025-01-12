using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetAllPosts;

internal sealed class GetAllPostsQueryHandler : IQueryHandler<GetAllPostsQuery, List<AllPostResponse>>
{
    private readonly IPostRepository _postRepository;

    public GetAllPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<List<AllPostResponse>>> Handle(GetAllPostsQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetAllAsync(cancellationToken);
        var response = posts.Select(MapPostToResponse).ToList();
        return Result.Success(response);
    }

    private static AllPostResponse MapPostToResponse(Post post)
    {
        return new AllPostResponse(
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
            post.Tags.MapToAllTagResponses());
    }
}