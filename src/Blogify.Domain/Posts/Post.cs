using Blogify.Domain.Abstractions;
using Blogify.Domain.Categories;
using Blogify.Domain.Comments;
using Blogify.Domain.Comments.Events;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags;

namespace Blogify.Domain.Posts;

public sealed class Post : Entity
{
    private readonly List<Category> _categories;
    private readonly List<Comment> _comments;
    private readonly List<Tag> _tags;
    private PostContent _content;
    private PostExcerpt _excerpt;
    private DateTimeOffset? _publishedAt;
    private PostSlug _slug;
    private PublicationStatus _status;
    private PostTitle _title;

    private Post(
        Guid id,
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId,
        PostSlug slug) : base(id)
    {
        _comments = [];
        _tags = [];
        _categories = [];
        _title = title;
        _content = content;
        _excerpt = excerpt;
        _slug = slug;
        _status = PublicationStatus.Draft;
        AuthorId = authorId;
    }

    private Post() : base(Guid.NewGuid())
    {
        _comments = [];
        _tags = [];
        _categories = [];
        _title = null!;
        _content = null!;
        _excerpt = null!;
        _slug = null!;
    }

    public PostTitle Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public PostContent Content
    {
        get => _content;
        private set => SetProperty(ref _content, value);
    }

    public PostExcerpt Excerpt
    {
        get => _excerpt;
        private set => SetProperty(ref _excerpt, value);
    }

    public PostSlug Slug
    {
        get => _slug;
        private set => SetProperty(ref _slug, value);
    }

    public Guid AuthorId { get; }
    public DateTimeOffset? PublishedAt => _publishedAt;

    public PublicationStatus Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public static Result<Post> Create(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId)
    {
        var validationResult = ValidateInvariants(title, content, excerpt, authorId);
        if (validationResult.IsFailure)
            return Result.Failure<Post>(validationResult.Error);

        var slugResult = GenerateSlug(title.Value);
        if (slugResult.IsFailure)
            return Result.Failure<Post>(slugResult.Error);

        var post = new Post(Guid.NewGuid(), title, content, excerpt, authorId, slugResult.Value);
        post.RaiseDomainEvent(new PostCreatedDomainEvent(post.Id, post.Title.Value, post.AuthorId));
        return Result.Success(post);
    }

    public static Result<Post> CreateAndPublish(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId)
    {
        var postResult = Create(title, content, excerpt, authorId);
        if (postResult.IsFailure)
            return Result.Failure<Post>(postResult.Error);

        var publishResult = postResult.Value.Publish();
        if (publishResult.IsFailure)
            return Result.Failure<Post>(publishResult.Error);

        return postResult;
    }

    public Result Update(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt)
    {
        if (!CanBeModified())
            return Result.Failure(PostErrors.CannotUpdateArchived);

        var validationResult = ValidateInvariants(title, content, excerpt, AuthorId);
        if (validationResult.IsFailure)
            return validationResult;

        var slugResult = GenerateSlug(title.Value);
        if (slugResult.IsFailure)
            return Result.Failure(slugResult.Error);

        Title = title;
        Content = content;
        Excerpt = excerpt;
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

        RaiseDomainEvent(new PostPublishedDomainEvent(
            Id,
            Title.Value,
            _publishedAt.Value,
            AuthorId));
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == PublicationStatus.Archived)
            return Result.Success();

        Status = PublicationStatus.Archived;
        RaiseDomainEvent(new PostArchivedDomainEvent(Id, Title.Value, AuthorId));
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
        RaiseDomainEvent(new CommentAddedDomainEvent(
            commentResult.Value.Id,
            Id,
            authorId));

        UpdateModificationTimestamp();
        return Result.Success(commentResult.Value);
    }

    public Result AddTag(Tag? tag)
    {
        if (tag is null)
            return Result.Failure(PostErrors.TagNull);

        if (_tags.Any(t => t.Id == tag.Id))
            return Result.Success();

        _tags.Add(tag);
        RaiseDomainEvent(new PostTaggedDomainEvent(Id, tag.Id));
        UpdateModificationTimestamp();
        return Result.Success();
    }

    public Result RemoveTag(Tag? tag)
    {
        if (tag is null)
            return Result.Failure(PostErrors.TagNull);

        var removed = _tags.RemoveAll(t => t.Id == tag.Id) > 0;
        if (removed)
            RaiseDomainEvent(new PostUntaggedDomainEvent(Id, tag.Id));

        return Result.Success();
    }

    public Result AddCategory(Category category)
    {
        if (category == null)
            return Result.Failure(PostErrors.CategoryNull);

        if (_categories.Any(c => c.Id == category.Id))
            return Result.Success();

        _categories.Add(category);
        RaiseDomainEvent(new PostCategoryAddedDomainEvent(Id, category.Id));
        return Result.Success();
    }

    public Result RemoveCategory(Category category)
    {
        if (category == null)
            return Result.Failure(PostErrors.CategoryNull);

        var removed = _categories.RemoveAll(c => c.Id == category.Id) > 0;
        if (removed)
            RaiseDomainEvent(new PostCategoryRemovedDomainEvent(Id, category.Id));

        return Result.Success();
    }

    private static Result ValidateInvariants(
        PostTitle? title,
        PostContent? content,
        PostExcerpt? excerpt,
        Guid authorId)
    {
        return title is null
            ? Result.Failure(PostErrors.TitleNull)
            : content is null
                ? Result.Failure(PostErrors.ContentNull)
                : excerpt is null
                    ? Result.Failure(PostErrors.ExcerptNull)
                    : authorId == Guid.Empty
                        ? Result.Failure(PostErrors.AuthorIdEmpty)
                        : Result.Success();
    }

    private bool CanBePublished()
    {
        return Status != PublicationStatus.Published && Status != PublicationStatus.Archived;
    }

    private bool CanBeModified()
    {
        return Status != PublicationStatus.Archived;
    }

    private bool CanReceiveComments()
    {
        return Status == PublicationStatus.Published;
    }

    private static Result<PostSlug> GenerateSlug(string title)
    {
        return PostSlug.Create(title);
    }
}