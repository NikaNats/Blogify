using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Comments.Events;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags;

namespace Blogify.Domain.Posts;

public sealed class Post : Entity
{
    // Private fields with consistent naming
    private readonly List<Comment> _comments;
    private readonly List<Tag> _tags;
    private Guid _categoryId;
    private PostContent _content;
    private PostExcerpt _excerpt;
    private DateTimeOffset? _publishedAt;
    private PostSlug _slug;
    private PostStatus _status;
    private PostTitle _title;

    // Private constructor for encapsulation
    private Post(
        Guid id,
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId,
        Guid categoryId,
        PostSlug slug) : base(id)
    {
        _comments = new List<Comment>();
        _tags = new List<Tag>();

        // Initialize all properties using SetProperty for consistent change tracking
        SetProperty(ref _title, title);
        SetProperty(ref _content, content);
        SetProperty(ref _excerpt, excerpt);
        SetProperty(ref _categoryId, categoryId);
        AuthorId = authorId; // Immutable property
        SetProperty(ref _status, PostStatus.Draft);
        SetProperty(ref _slug, slug);
    }

    // Required by EF Core
    private Post() : base(Guid.NewGuid())
    {
        _comments = new List<Comment>();
        _tags = new List<Tag>();
    }

    // Properties with consistent encapsulation
    public PostTitle Title => _title;
    public PostContent Content => _content;
    public PostExcerpt Excerpt => _excerpt;
    public PostSlug Slug => _slug;
    public Guid AuthorId { get; }
    public DateTimeOffset? PublishedAt => _publishedAt;
    public Guid CategoryId => _categoryId;
    public PostStatus Status => _status;
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    // Factory method with proper validation
    public static Result<Post> Create(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId,
        Guid categoryId)
    {
        // Validate all inputs before object creation
        var validationResult = ValidateInvariants(title, content, excerpt, authorId, categoryId);
        if (validationResult.IsFailure)
            return Result.Failure<Post>(validationResult.Error);

        var slugResult = GenerateSlug(title.Value);
        if (slugResult.IsFailure)
            return Result.Failure<Post>(slugResult.Error);

        var post = new Post(Guid.NewGuid(), title, content, excerpt, authorId, categoryId, slugResult.Value);

        post.RaiseDomainEvent(new PostCreatedDomainEvent(post.Id));
        return Result.Success(post);
    }

    public Result Update(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid categoryId)
    {
        // Business rule validation
        if (_status == PostStatus.Archived)
            return Result.Failure(Error.Validation("Post.Update", "Cannot update archived post."));

        // Input validation
        var validationResult = ValidateInvariants(title, content, excerpt, AuthorId, categoryId);
        if (validationResult.IsFailure)
            return validationResult;

        // Slug generation validation
        var slugResult = GenerateSlug(title.Value);
        if (slugResult.IsFailure)
            return Result.Failure(slugResult.Error);

        // Update all properties using SetProperty for change tracking
        SetProperty(ref _title, title);
        SetProperty(ref _content, content);
        SetProperty(ref _excerpt, excerpt);
        SetProperty(ref _categoryId, categoryId);
        SetProperty(ref _slug, slugResult.Value);

        RaiseDomainEvent(new PostUpdatedDomainEvent(Id));
        return Result.Success();
    }

    public Result Publish()
    {
        // Business rules validation
        if (_status == PostStatus.Published)
            return Result.Failure(Error.Validation("Post.Publish", "Post is already published."));

        if (_status == PostStatus.Archived)
            return Result.Failure(Error.Validation("Post.Publish", "Cannot publish archived post."));

        // Update state using SetProperty
        SetProperty(ref _status, PostStatus.Published);
        SetProperty(ref _publishedAt, DateTimeOffset.UtcNow);

        RaiseDomainEvent(new PostPublishedDomainEvent(Id));
        return Result.Success();
    }

    public Result Archive()
    {
        // Skip if already archived
        if (_status == PostStatus.Archived)
            return Result.Success();

        SetProperty(ref _status, PostStatus.Archived);
        RaiseDomainEvent(new PostArchivedDomainEvent(Id));
        return Result.Success();
    }

    public Result AddComment(string content, Guid authorId)
    {
        // Business rule validation
        if (_status != PostStatus.Published)
            return Result.Failure(Error.Validation(
                "Post.AddComment",
                "Cannot add comments to unpublished posts."));

        // Create comment with validation
        var commentResult = Comment.Create(content, authorId, Id);
        if (commentResult.IsFailure)
            return Result.Failure(commentResult.Error);

        _comments.Add(commentResult.Value);
        RaiseDomainEvent(new CommentAddedDomainEvent(commentResult.Value.Id));
        return Result.Success();
    }

    public Result AddTag(Tag? tag)
    {
        if (tag is null)
            return Result.Failure(Error.Validation("Post.AddTag", "Tag cannot be null."));

        if (_tags.Any(t => t.Id == tag.Id))
            return Result.Success();

        _tags.Add(tag);
        RaiseDomainEvent(new PostTaggedDomainEvent(Id, tag.Id));
        return Result.Success();
    }

    public Result RemoveTag(Tag? tag)
    {
        if (tag is null)
            return Result.Failure(Error.Validation("Post.RemoveTag", "Tag cannot be null."));

        var removed = _tags.RemoveAll(t => t.Id == tag.Id) > 0;
        if (removed) RaiseDomainEvent(new PostUntaggedDomainEvent(Id, tag.Id));

        return Result.Success();
    }

    // Private helper methods
    private static Result ValidateInvariants(
        PostTitle? title,
        PostContent? content,
        PostExcerpt? excerpt,
        Guid authorId,
        Guid categoryId)
    {
        return title is null
            ? Result.Failure(Error.Validation("Post.Title", "Title cannot be null."))
            : content is null
                ? Result.Failure(Error.Validation("Post.Content", "Content cannot be null."))
                : excerpt is null
                    ? Result.Failure(Error.Validation("Post.Excerpt", "Excerpt cannot be null."))
                    : authorId == Guid.Empty
                        ? Result.Failure(Error.Validation("Post.AuthorId", "AuthorId cannot be empty."))
                        : categoryId == Guid.Empty
                            ? Result.Failure(Error.Validation("Post.CategoryId", "CategoryId cannot be empty."))
                            : Result.Success();
    }

    private static Result<PostSlug> GenerateSlug(string title)
    {
        return PostSlug.Create(title);
    }
}