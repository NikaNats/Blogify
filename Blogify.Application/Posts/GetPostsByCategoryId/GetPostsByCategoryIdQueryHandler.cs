using Blogify.Application.Abstractions.Messaging;
using Blogify.Application.Posts.GetPostById;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetPostsByCategoryId;

internal sealed class GetPostsByCategoryIdQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetPostsByCategoryIdQuery, List<PostResponse>>
{
    public async Task<Result<List<PostResponse>>> Handle(GetPostsByCategoryIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var posts = await postRepository.GetByCategoryIdAsync(
                request.CategoryId, 
                cancellationToken
            );
        
            var response = posts.Select(MapPostToResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<PostResponse>>(Error.Create(
                "Posts.RetrievalFailed",
                $"Failed to retrieve posts for category {request.CategoryId}",
                ErrorType.Failure
            ));
        }
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
            post.Categories.MapToCategoryResponses()
        );
    }
}