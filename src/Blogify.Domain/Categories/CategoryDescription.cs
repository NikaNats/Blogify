using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories;

public sealed class CategoryDescription : ValueObject
{
    internal const int MaxLength = 500;

    private CategoryDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CategoryDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<CategoryDescription>(CategoryError.DescriptionNullOrEmpty);

        if (value.Length > MaxLength)
            return Result.Failure<CategoryDescription>(CategoryError.DescriptionTooLong);

        return Result.Success(new CategoryDescription(value));
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}