using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public sealed class LastName : ValueObject
{
    private const int MaxLength = 50;

    private LastName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LastName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LastName>(UserErrors.InvalidLastName);

        if (value.Length > MaxLength)
            return Result.Failure<LastName>(UserErrors.LastNameTooLong);

        var trimmedValue = value.Trim();
        var lastName = new LastName(trimmedValue);

        return Result.Success(lastName);
    }

    public static implicit operator string(LastName lastName)
    {
        return lastName.Value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}