using Blogify.Domain.Abstractions;
using Blogify.Domain.Tags.Events;

namespace Blogify.Domain.Tags;

/// <summary>
///     Represents the Tag aggregate root. A Tag is an independent entity
///     used for labeling posts. It does not own or manage the posts associated with it.
/// </summary>
public sealed class Tag : AuditableEntity
{
    private Tag(Guid id, TagName name) : base(id)
    {
        Name = name;
    }

    // Private constructor for EF Core.
    private Tag()
    {
    }

    public TagName Name { get; private set; }

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

        // Only update if the name is different.
        if (Name.Equals(nameResult.Value)) return Result.Success();

        Name = nameResult.Value;
        // Optionally raise a TagNameUpdatedDomainEvent here if needed.
        return Result.Success();
    }
}