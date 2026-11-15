using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Posts;

namespace Blogify.Application.Posts;

/// <summary>A post as shown in a list: no comment bodies, just how many there are.</summary>
public sealed record PostSummaryResponse(
    Guid Id,
    string Title,
    string Content,
    string Excerpt,
    string Slug,
    Guid AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? PublishedAt,
    PublicationStatus Status,
    int CommentCount,
    List<AllTagResponse> Tags,
    List<AllCategoryResponse> Categories);
