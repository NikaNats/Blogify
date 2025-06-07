using Blogify.Application.Categories.GetAllCategories;
using Blogify.Application.Comments;
using Blogify.Application.Tags.GetAllTags;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Tags;

namespace Blogify.Application.Posts;

public static class MappingExtensions
{
    public static List<CommentResponse> MapToCommentResponses(this IEnumerable<Comment> comments)
    {
        if (comments == null)
            throw new ArgumentNullException(nameof(comments));

        return comments.Select(c => c.MapToCommentResponse()).ToList();
    }

    public static List<AllTagResponse> MapToAllTagResponses(this IEnumerable<Tag> tags)
    {
        if (tags == null)
            throw new ArgumentNullException(nameof(tags));

        return tags.Select(t => new AllTagResponse(t.Id, t.Name.Value, t.CreatedAt)).ToList();
    }

    public static CommentResponse MapToCommentResponse(this Comment comment)
    {
        if (comment == null)
            throw new ArgumentNullException(nameof(comment));

        return new CommentResponse(
            comment.Id,
            comment.Content.Value,
            comment.AuthorId,
            comment.PostId,
            comment.CreatedAt);
    }

    public static AllTagResponse MapToAllTagResponse(this Tag tag)
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));

        return new AllTagResponse(
            tag.Id,
            tag.Name.Value,
            tag.CreatedAt);
    }

    public static List<AllCategoryResponse> MapToCategoryResponses(this IEnumerable<Category> categories)
    {
        if (categories == null)
            throw new ArgumentNullException(nameof(categories));

        return categories.Select(c => c.MapToCategoryResponse()).ToList();
    }

    public static AllCategoryResponse MapToCategoryResponse(this Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        return new AllCategoryResponse(
            category.Id,
            category.Name.Value,
            category.Description.Value,
            category.CreatedAt,
            category.LastModifiedAt);
    }
}