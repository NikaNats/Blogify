using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories;

public sealed class CategoryName : ValueObject
{
    internal const int MaxLength = 100;

    private CategoryName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CategoryName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<CategoryName>(CategoryError.NameNullOrEmpty);

        if (value.Length > MaxLength)
            return Result.Failure<CategoryName>(CategoryError.NameTooLong);

        return Result.Success(new CategoryName(value));
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