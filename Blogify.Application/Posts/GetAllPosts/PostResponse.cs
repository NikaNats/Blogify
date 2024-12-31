using Blogify.Application.Comments.GetCommentById;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetAllPosts;

public sealed record PostResponse(
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
    List<CommentResponse> Comments,
    List<TagResponse> Tags);