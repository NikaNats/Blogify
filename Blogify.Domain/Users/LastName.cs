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
        if (value is null)
            return Result.Failure<LastName>(UserErrors.InvalidLastName);

        var trimmedValue = value.Trim();
        if (string.IsNullOrEmpty(trimmedValue))
            return Result.Failure<LastName>(UserErrors.InvalidLastName);

        return new LastName(value.Trim());
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}