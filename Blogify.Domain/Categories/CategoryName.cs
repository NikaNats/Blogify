using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Categories;

public class CategoryName : ValueObject
{
    private CategoryName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CategoryName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<CategoryName>(CategoryError.NameNullOrEmpty);


        return Result.Success(new CategoryName(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}