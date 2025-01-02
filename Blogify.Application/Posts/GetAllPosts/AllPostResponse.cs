using Blogify.Application.Comments.GetCommentById;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetAllPosts;

public sealed record AllPostResponse(
    Guid Id,
    string Title,
    string Content,
    string Excerpt,
    string Slug,
    Guid AuthorId,
    Guid CategoryId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? PublishedAt,
    PostStatus Status,
    List<CommentByIdResponse> Comments,
    List<AllTagResponse> Tags);