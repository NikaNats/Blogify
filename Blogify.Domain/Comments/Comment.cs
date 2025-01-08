using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments.Events;

namespace Blogify.Domain.Comments;

public sealed class Comment : Entity
{
    private Comment(Guid id, CommentContent content, Guid authorId, Guid postId)
        : base(id)
    {
        Content = content;
        AuthorId = authorId;
        PostId = postId;

        RaiseDomainEvent(new CommentAddedDomainEvent(id));
    }

    private Comment()
    {
    }

    public CommentContent Content { get; private set; }
    public Guid AuthorId { get; private set; }
    public Guid PostId { get; private set; }

    public static Result<Comment> Create(string content, Guid authorId, Guid postId)
    {
        var validationResult = Validate(authorId, postId);
        if (validationResult.IsFailure)
            return Result.Failure<Comment>(validationResult.Error);

        var contentResult = CommentContent.Create(content);
        if (contentResult.IsFailure)
            return Result.Failure<Comment>(contentResult.Error);

        var comment = new Comment(Guid.NewGuid(), contentResult.Value, authorId, postId);

        return Result.Success(comment);
    }

    public Result Update(string content)
    {
        var contentResult = CommentContent.Create(content);
        if (contentResult.IsFailure)
            return Result.Failure(contentResult.Error);

        Content = contentResult.Value;
        UpdateModificationTimestamp();

        return Result.Success();
    }
    
    public Result Remove(Guid userId)
    {
        if (AuthorId != userId)
            return Result.Failure(CommentError.UnauthorizedDeletion);

        RaiseDomainEvent(new CommentRemovedDomainEvent(Id));

        return Result.Success();
    }

    private static Result Validate(Guid authorId, Guid postId)
    {
        if (authorId == Guid.Empty)
            return Result.Failure(CommentError.EmptyAuthorId);

        if (postId == Guid.Empty)
            return Result.Failure(CommentError.EmptyPostId);

        return Result.Success();
    }
}