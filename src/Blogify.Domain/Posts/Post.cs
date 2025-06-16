using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags;

namespace Blogify.Domain.Posts;

/// <summary>
///     Represents the Post aggregate root. A Post is a central entity in the blog,
///     managing its own content, publication state, and associations with categories and tags.
/// </summary>
public sealed class Post : AuditableEntity
{
    // A Post is associated with Categories and Tags, but does not own them.
    // We store only their IDs to maintain loose coupling and respect aggregate boundaries.
    private readonly List<Guid> _categoryIds = [];

    // A Post is the aggregate root for its comments. This is a true composition relationship.
    private readonly List<Comment> _comments = [];
    private readonly List<Guid> _tagIds = [];

    private DateTimeOffset? _publishedAt;

    private Post(
        Guid id,
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId,
        PostSlug slug) : base(id)
    {
        Title = title;
        Content = content;
        Excerpt = excerpt;
        Slug = slug;
        Status = PublicationStatus.Draft;
        AuthorId = authorId;
    }

    // Private constructor for EF Core.
    private Post()
    {
    }

    public PostTitle Title { get; private set; }
    public PostContent Content { get; private set; }
    public PostExcerpt Excerpt { get; private set; }
    public PostSlug Slug { get; private set; }
    public PublicationStatus Status { get; private set; }
    public Guid AuthorId { get; }
    public DateTimeOffset? PublishedAt => _publishedAt;

    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();
    public IReadOnlyCollection<Guid> TagIds => _tagIds.AsReadOnly();

    public static Result<Post> Create(
        string title,
        string content,
        string excerpt,
        Guid authorId)
    {
        // First, create the Value Objects. The Result pattern allows us to chain the validation.
        // If any of these fail, we'll capture the error and return immediately.
        var titleResult = PostTitle.Create(title);
        if (titleResult.IsFailure)
            return Result.Failure<Post>(titleResult.Error);

        var contentResult = PostContent.Create(content);
        if (contentResult.IsFailure)
            return Result.Failure<Post>(contentResult.Error);

        var excerptResult = PostExcerpt.Create(excerpt);
        if (excerptResult.IsFailure)
            return Result.Failure<Post>(excerptResult.Error);

        // Slug generation depends on a valid title.
        var slugResult = PostSlug.Create(titleResult.Value.Value);
        if (slugResult.IsFailure)
            return Result.Failure<Post>(slugResult.Error);

        // Basic primitive validation.
        if (authorId == Guid.Empty)
            return Result.Failure<Post>(PostErrors.AuthorIdEmpty);

        // All inputs are valid, so we can create the aggregate.
        var post = new Post(
            Guid.NewGuid(),
            titleResult.Value,
            contentResult.Value,
            excerptResult.Value,
            authorId,
            slugResult.Value);

        post.RaiseDomainEvent(new PostCreatedDomainEvent(post.Id, post.Title.Value, post.AuthorId));

        return Result.Success(post);
    }

    public Result Update(
        string title,
        string content,
        string excerpt)
    {
        if (!CanBeModified())
            return Result.Failure(PostErrors.CannotUpdateArchived);

        // Create Value Objects from primitives, just like in the Create method.
        var titleResult = PostTitle.Create(title);
        if (titleResult.IsFailure) return Result.Failure(titleResult.Error);

        var contentResult = PostContent.Create(content);
        if (contentResult.IsFailure) return Result.Failure(contentResult.Error);

        var excerptResult = PostExcerpt.Create(excerpt);
        if (excerptResult.IsFailure) return Result.Failure(excerptResult.Error);

        var slugResult = PostSlug.Create(titleResult.Value.Value);
        if (slugResult.IsFailure) return Result.Failure(slugResult.Error);

        var hasChanged =
            !Title.Equals(titleResult.Value) ||
            !Content.Equals(contentResult.Value) ||
            !Excerpt.Equals(excerptResult.Value);

        if (!hasChanged) return Result.Success();

        Title = titleResult.Value;
        Content = contentResult.Value;
        Excerpt = excerptResult.Value;
        Slug = slugResult.Value;

        RaiseDomainEvent(new PostUpdatedDomainEvent(Id, Title.Value, AuthorId));

        return Result.Success();
    }

    public Result Publish()
    {
        if (Status == PublicationStatus.Published)
            return Result.Failure(PostErrors.AlreadyPublished);

        if (Status == PublicationStatus.Archived)
            return Result.Failure(PostErrors.CannotPublishArchived);

        Status = PublicationStatus.Published;
        _publishedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PostPublishedDomainEvent(Id, Title.Value, _publishedAt.Value, AuthorId));
        return Result.Success();
    }

    /// <summary>
    ///     Archives the post, moving it to an archived state. This prevents further modifications or publishing.
    ///     This action is idempotent; archiving an already-archived post will succeed without change.
    /// </summary>
    /// <returns>A success result.</returns>
    public Result Archive()
    {
        if (Status == PublicationStatus.Archived)
            // The post is already in the desired state. Succeed without doing anything.
            return Result.Success();

        Status = PublicationStatus.Archived;
        RaiseDomainEvent(new PostArchivedDomainEvent(Id, Title.Value, AuthorId));
        return Result.Success();
    }

    public Result AssignToCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (_categoryIds.Contains(category.Id)) return Result.Success(); // Idempotent: No change, no Error.

        _categoryIds.Add(category.Id);
        RaiseDomainEvent(new PostCategoryAddedDomainEvent(Id, category.Id));
        return Result.Success();
    }

    public Result RemoveFromCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (_categoryIds.Remove(category.Id)) RaiseDomainEvent(new PostCategoryRemovedDomainEvent(Id, category.Id));

        return Result.Success(); // Idempotent: If it's not there, the state is already correct.
    }

    public Result AddTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_tagIds.Contains(tag.Id)) return Result.Success();

        _tagIds.Add(tag.Id);
        RaiseDomainEvent(new PostTaggedDomainEvent(Id, tag.Id));
        return Result.Success();
    }

    public Result RemoveTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_tagIds.Remove(tag.Id)) RaiseDomainEvent(new PostUntaggedDomainEvent(Id, tag.Id));

        return Result.Success();
    }

    public Result<Comment> AddComment(string content, Guid authorId)
    {
        if (!CanReceiveComments())
            return Result.Failure<Comment>(PostErrors.CommentToUnpublishedPost);

        var commentResult = Comment.Create(content, authorId, Id);
        if (commentResult.IsFailure)
            return Result.Failure<Comment>(commentResult.Error);

        _comments.Add(commentResult.Value);

        return Result.Success(commentResult.Value);
    }

    private bool CanBeModified()
    {
        return Status != PublicationStatus.Archived;
    }

    private bool CanReceiveComments()
    {
        return Status == PublicationStatus.Published;
    }
}