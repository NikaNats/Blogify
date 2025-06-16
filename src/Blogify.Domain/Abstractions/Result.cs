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

    public Result<TNewValue> Map<TNewValue>(Func<TNewValue> mapper)
    {
        return IsSuccess ? Success(mapper()) : Failure<TNewValue>(Error);
    }

    private static void ValidateState(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new ArgumentException("A successful result cannot have an error.", nameof(error));
            case false when error == Error.None:
                throw new ArgumentException("A failed result must have an error.", nameof(error));
        }
    }
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

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

    public Result<TNewValue> Map<TNewValue>(Func<TValue, TNewValue> mapper)
    {
        return IsSuccess ? Success(mapper(Value)) : Failure<TNewValue>(Error);
    }
}