using Blogify.Application.Comments.GetCommentById;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.GetAllPosts;

public sealed class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQuery, Result<List<AllPostResponse>>>
{
    private readonly IPostRepository _postRepository;

    public GetAllPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<List<AllPostResponse>>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetAllAsync(cancellationToken);
        var response = posts.Select(post => new AllPostResponse(
            post.Id,
            post.Title.Value,
            post.Content.Value,
            post.Excerpt.Value,
            post.Slug.Value,
            post.AuthorId,
            post.CategoryId,
            post.CreatedAt,
            post.UpdatedAt,
            post.PublishedAt,
            post.Status,
            post.Comments.Select(c => new CommentByIdResponse(c.Id, c.Content, c.AuthorId, c.PostId, c.CreatedAt)).ToList(),
            post.Tags.Select(t => new AllTagResponse(t.Id, t.Name, t.CreatedAt)).ToList())).ToList();

        return Result.Success(response);
    }
}