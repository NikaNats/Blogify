using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Users;

public sealed class LastName : ValueObject
{
    private LastName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LastName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LastName>(UserErrors.InvalidLastName);

        var trimmedValue = value.Trim();
        var lastName = new LastName(trimmedValue);

        return Result.Success(lastName);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}