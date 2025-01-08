using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public sealed class FirstName : ValueObject
{
    private FirstName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FirstName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FirstName>(UserErrors.InvalidFirstName);

        var trimmedValue = value.Trim();

        var firstName = new FirstName(trimmedValue);

        return Result.Success(firstName);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}