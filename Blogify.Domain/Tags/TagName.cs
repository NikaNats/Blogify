using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Tags;

public sealed class TagName : ValueObject
{
    private TagName(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     Gets the value of the tag name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Creates a new tag name.
    /// </summary>
    /// <param name="value">The value of the tag name.</param>
    /// <returns>A result containing the tag name or an error.</returns>
    public static Result<TagName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<TagName>(Error.Validation("TagName.Empty", "Tag name cannot be empty."));

        return Result.Success(new TagName(value));
    }

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}