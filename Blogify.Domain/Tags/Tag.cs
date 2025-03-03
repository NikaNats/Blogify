using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts;
using Blogify.Domain.Tags.Events;

namespace Blogify.Domain.Tags;

public sealed class Tag : Entity
{
    private readonly List<Post> _posts = new();
    private TagName _name;

    private Tag(Guid id, TagName name)
        : base(id)
    {
        _name = name;
    }

    private Tag()
    {
    }

    public TagName Name
    {
        get => _name;
        private set => SetProperty(ref _name, value);
    }

    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    public static Result<Tag> Create(string name)
    {
        var nameResult = TagName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Tag>(nameResult.Error);

        var tag = new Tag(Guid.NewGuid(), nameResult.Value);
        tag.RaiseDomainEvent(new TagCreatedDomainEvent(tag.Id));
        return Result.Success(tag);
    }

    public Result UpdateName(string name)
    {
        var nameResult = TagName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;
        return Result.Success();
    }

    public Result AddPost(Post post)
    {
        if (post == null)
            return Result.Failure(TagErrors.PostNull);
        if (_posts.Contains(post))
            return Result.Failure(TagErrors.PostDuplicate);

        _posts.Add(post);
        UpdateModificationTimestamp();
        return Result.Success();
    }

    public Result RemovePost(Post post)
    {
        if (post == null)
            return Result.Failure(TagErrors.PostNull);

        if (!_posts.Remove(post))
            return Result.Failure(TagErrors.PostNotFound);

        UpdateModificationTimestamp();
        return Result.Success();
    }
}