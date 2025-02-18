using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Comments;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts.GetPostById;

public sealed record PostResponse(
    Guid Id,
    string Title,
    string Content,
    string Excerpt,
    string Slug,
    Guid AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? PublishedAt,
    PostStatus Status,
    List<CommentResponse> Comments,
    List<AllTagResponse> Tags,
    List<AllCategoryResponse> Categories);