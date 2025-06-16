using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Tags;

public sealed class TagName : ValueObject
{
    private const int MaxLength = 50;

    private TagName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<TagName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return Result.Failure<TagName>(TagErrors.NameEmpty);
        if (value.Length > MaxLength) return Result.Failure<TagName>(TagErrors.NameTooLong);
        return Result.Success(new TagName(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}