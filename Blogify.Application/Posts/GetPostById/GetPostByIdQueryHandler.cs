using Blogify.Application.Comments.GetCommentById;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using MediatR;

namespace Blogify.Application.Posts.GetPostById;

public sealed class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, Result<PostResponse>>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);
        if (post is null)
            return Result.Failure<PostResponse>(Error.NotFound("Post.NotFound", "Post not found."));

        var response = new PostResponse(
            post.Id,
            post.Title.Value,
            post.Content.Value,
            post.Excerpt.Value,
            post.Slug.Value,
            post.AuthorId,
            post.CategoryId,
            post.CreatedAt,
            post.LastModifiedAt,
            post.PublishedAt,
            post.Status,
            post.Comments.Select(c => new CommentByIdResponse(c.Id, c.Content.Value, c.AuthorId, c.PostId, c.CreatedAt)).ToList(),
            post.Tags.Select(t => new AllTagResponse(t.Id, t.Name.Value, t.CreatedAt)).ToList());

        return Result.Success(response);
    }
}