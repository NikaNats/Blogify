using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories;

public class CategoryDescription : ValueObject
{
    private CategoryDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CategoryDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<CategoryDescription>(CategoryError.DescriptionNullOrEmpty);

        return Result.Success(new CategoryDescription(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}