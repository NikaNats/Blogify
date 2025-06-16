namespace Blogify.Domain.Abstractions;

public sealed record Error(string Code, string Description, ErrorType Type)
{
    private const string GeneralPrefix = "General";
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new($"{GeneralPrefix}.Null", "Null value was provided", ErrorType.Failure);

    public static Error Create(string code, string description, ErrorType type)
    {
        return new Error(GuardEmptyString(code), GuardEmptyString(description), type);
    }

    public static Error Failure(string code, string description)
    {
        return Create(code, description, ErrorType.Failure);
    }

    public static Error NotFound(string code, string description)
    {
        return Create(code, description, ErrorType.NotFound);
    }

    public static Error Problem(string code, string description)
    {
        return Create(code, description, ErrorType.Problem);
    }

    public static Error Conflict(string code, string description)
    {
        return Create(code, description, ErrorType.Conflict);
    }

    public static Error Validation(string code, string description)
    {
        return Create(code, description, ErrorType.Validation);
    }

    public static Error Unexpected(string code, string description)
    {
        return Create(code, description, ErrorType.Unexpected);
    }

    private static string GuardEmptyString(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null or whitespace", nameof(value))
            : value;
    }
}