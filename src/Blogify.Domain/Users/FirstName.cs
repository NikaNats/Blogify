using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public sealed class FirstName : ValueObject
{
    private const int MaxLength = 50;

    private FirstName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<FirstName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<FirstName>(UserErrors.InvalidFirstName);

        if (value.Length > MaxLength)
            return Result.Failure<FirstName>(UserErrors.FirstNameTooLong);

        var trimmedValue = value.Trim();
        var firstName = new FirstName(trimmedValue);

        return Result.Success(firstName);
    }

    public static implicit operator string(FirstName firstName)
    {
        return firstName.Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}