using Blogify.Domain.Abstractions;
using Blogify.Domain.Comments;
using Blogify.Domain.Comments.Events;
using Blogify.Domain.Posts.Events;
using Blogify.Domain.Tags;

namespace Blogify.Domain.Posts;

public sealed class Post : Entity
{
    private readonly List<Comment> _comments = new();
    private readonly List<Tag> _tags = new();

    private Post(Guid id, PostTitle title, PostContent content, PostExcerpt excerpt, Guid authorId, Guid categoryId)
        : base(id)
    {
        Title = title;
        Content = content;
        Excerpt = excerpt;
        AuthorId = authorId;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
        Status = PostStatus.Draft;
        Slug = GenerateSlug(title.Value).Value;
    }

    private Post()
    {
    }

    public PostTitle Title { get; private set; }
    public PostContent Content { get; private set; }
    public PostExcerpt Excerpt { get; private set; }
    public PostSlug Slug { get; private set; }
    public Guid AuthorId { get; private set; }
    public Guid CategoryId { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public PostStatus Status { get; private set; }
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public static Result<Post> Create(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId,
        Guid categoryId)
    {
        if (title == null)
            return Result.Failure<Post>(Error.Validation("Post.Title", "Title cannot be null."));
        if (content == null)
            return Result.Failure<Post>(Error.Validation("Post.Content", "Content cannot be null."));
        if (excerpt == null)
            return Result.Failure<Post>(Error.Validation("Post.Excerpt", "Excerpt cannot be null."));
        if (authorId == Guid.Empty)
            return Result.Failure<Post>(Error.Validation("Post.AuthorId", "AuthorId cannot be empty."));
        if (categoryId == Guid.Empty)
            return Result.Failure<Post>(Error.Validation("Post.CategoryId", "CategoryId cannot be empty."));

        var post = new Post(
            Guid.NewGuid(),
            title,
            content,
            excerpt,
            authorId,
            categoryId);

        post.RaiseDomainEvent(new PostCreatedDomainEvent(post.Id));
        return Result.Success(post);
    }

    public void Update(PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid categoryId)
    {
        Title = title;
        Content = content;
        Excerpt = excerpt;
        CategoryId = categoryId;
        Slug = GenerateSlug(title.Value).Value;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostUpdatedDomainEvent(Id));
        Result.Success();
    }

    public void Publish()
    {
        if (Status == PostStatus.Published)
        {
            Result.Failure(Error.Validation("Post.Status", "Post is already published."));
            return;
        }

        Status = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostPublishedDomainEvent(Id));
        Result.Success();
    }

    public bool Archive()
    {
        if (Status == PostStatus.Archived) return false; // Post is already archived, no update needed

        Status = PostStatus.Archived;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostArchivedDomainEvent(Id));
        return true; // Post was archived
    }

    public Result AddComment(string content, Guid authorId)
    {
        if (Status != PostStatus.Published)
            return Result.Failure(Error.Validation("Post.Status", "Cannot add comments to unpublished posts."));

        var commentResult = Comment.Create(content, authorId, Id);
        if (commentResult.IsFailure) return Result.Failure(commentResult.Error);

        _comments.Add(commentResult.Value);
        RaiseDomainEvent(new CommentAddedDomainEvent(commentResult.Value.Id));
        return Result.Success();
    }

    public bool AddTag(Tag tag)
    {
        if (_tags.Any(t => t.Id == tag.Id)) return false; // Tag already exists, no update needed

        _tags.Add(tag);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostTaggedDomainEvent(Id, tag.Id));
        return true; // Tag was added
    }

    public void RemoveTag(Tag tag)
    {
        if (_tags.RemoveAll(t => t.Id == tag.Id) > 0)
        {
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new PostUntaggedDomainEvent(Id, tag.Id));
        }
    }

    private static Result<PostSlug> GenerateSlug(string title)
    {
        var slugResult = PostSlug.Create(title);
        return slugResult.IsFailure
            ? Result.Failure<PostSlug>(slugResult.Error)
            : Result.Success(slugResult.Value);
    }
}