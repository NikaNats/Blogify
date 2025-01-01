using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags.Events;

namespace Blogify.Domain.Tags;

public sealed class Tag : Entity
{
    private readonly List<Post> _posts = new();

    private Tag(Guid id, string name)
        : base(id)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    private Tag()
    {
    }

    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    public static Result<Tag> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Tag>(Error.Validation("Tag.Name.Empty", "Tag name cannot be empty."));

        var tag = new Tag(Guid.NewGuid(), name);
        tag.RaiseDomainEvent(new TagCreatedDomainEvent(tag.Id));
        return Result.Success(tag);
    }
}