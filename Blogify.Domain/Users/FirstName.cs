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
        if (value is null)
            return Result.Failure<FirstName>(UserErrors.InvalidFirstName);

        var trimmedValue = value.Trim();
        if (string.IsNullOrEmpty(trimmedValue))
            return Result.Failure<FirstName>(UserErrors.InvalidFirstName);

        return new FirstName(trimmedValue);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}