using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments;

public sealed class Comment : Entity
{
    private Comment(Guid id, string content, Guid authorId, Guid postId)
        : base(id)
    {
        Content = content;
        AuthorId = authorId;
        PostId = postId;
        CreatedAt = DateTime.UtcNow;
    }

    private Comment()
    {
    }

    public string Content { get; private set; }
    public Guid AuthorId { get; private set; }
    public Guid PostId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Comment> Create(string content, Guid authorId, Guid postId)
    {
        if (string.IsNullOrEmpty(content))
            return Result.Failure<Comment>(Error.Validation("Comment.InvalidContent",
                "Comment content cannot be empty."));

        if (authorId == default)
            return Result.Failure<Comment>(Error.Validation("Comment.AuthorId", "AuthorId cannot be default."));

        if (postId == default)
            return Result.Failure<Comment>(Error.Validation("Comment.PostId", "PostId cannot be default."));

        var comment = new Comment(Guid.NewGuid(), content, authorId, postId);
        return Result.Success(comment);
    }
}