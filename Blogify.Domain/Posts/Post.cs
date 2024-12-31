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
    private Post(Guid id, PostTitle title, PostContent content,PostExcerpt excerpt, Guid authorId,Guid categoryId)
        : base(id)
    {
        Title = title;
        Content = content;
        Excerpt = excerpt;
        AuthorId = authorId;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
        Status = PostStatus.Draft;
        Slug = GenerateSlug(title.Value);
    }

    private Post() { }

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

    public static Post Create(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid authorId,
        Guid categoryId)
    {
        var post = new Post(
            Guid.NewGuid(),
            title,
            content,
            excerpt,
            authorId,
            categoryId);
        post.RaiseDomainEvent(new PostCreatedDomainEvent(post.Id));
        return post;
    }

    public void Update(
        PostTitle title,
        PostContent content,
        PostExcerpt excerpt,
        Guid categoryId)
    {
        Title = title;
        Content = content;
        Excerpt = excerpt;
        CategoryId = categoryId;
        Slug = GenerateSlug(title.Value);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostUpdatedDomainEvent(Id));
    }
    public void Publish()
    {
        if (Status == PostStatus.Published)
        {
            throw new InvalidOperationException("Post is already published.");
        }

        Status = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new PostPublishedDomainEvent(Id));
    }

    public void Archive()
    {
        if (Status == PostStatus.Archived)
        {
            throw new InvalidOperationException("Post is already archived.");
        }

        Status = PostStatus.Archived;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostArchivedDomainEvent(Id));
    }
    public void AddComment(string content, Guid authorId)
    {
        if (Status != PostStatus.Published)
        {
            throw new InvalidOperationException("Cannot add comments to unpublished posts.");
        }

        var comment = Comment.Create(content, authorId, Id);
        _comments.Add(comment.Value);

        RaiseDomainEvent(new CommentAddedDomainEvent(comment.Value.Id));
    }
    
    public void AddTag(Tag tag)
    {
        if (_tags.Any(t => t.Id == tag.Id))
        {
            return;
        }

        _tags.Add(tag);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PostTaggedDomainEvent(Id, tag.Id));
    }

    public void RemoveTag(Tag tag)
    {
        if (_tags.RemoveAll(t => t.Id == tag.Id) > 0)
        {
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new PostUntaggedDomainEvent(Id, tag.Id));
        }
    }
    
    private static PostSlug GenerateSlug(string title)
    {
        return PostSlug.Create(title).Value;
    }
}