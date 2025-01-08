namespace Blogify.Domain.Abstractions;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        ValidateState(isSuccess, error);
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, Error.None);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result<TValue> Failure<TValue>(Error error)
    {
        return new Result<TValue>(default, false, error);
    }

    private static void ValidateState(bool isSuccess, Error error)
    {
        if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            throw new ArgumentException(
                "Invalid error state for result. Success must have no error, failure must have error.",
                nameof(error));
    }
}

/// <summary>
///     Represents a result that contains a value when successful.
/// </summary>
/// <typeparam name="TValue">The type of the value in case of success</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    ///     Gets the value of a successful result or throws if the result represents a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value ?? throw new InvalidOperationException("Successful result contains null value.")
        : throw new InvalidOperationException("Cannot access value of failed result.");

    public static implicit operator Result<TValue>(TValue? value)
    {
        return value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
    }

    public static Result<TValue> From(TValue? value, Error error)
    {
        return value is not null ? Success(value) : Failure<TValue>(error);
    }
}