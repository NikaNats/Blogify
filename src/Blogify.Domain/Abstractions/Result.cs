using Blogify.Domain.Abstractions;

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
    public static Result Success() => new(true, Error.None);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
    public Result<TNewValue> Map<TNewValue>(Func<TNewValue> mapper) => IsSuccess ? Success(mapper()) : Failure<TNewValue>(Error);
    private static void ValidateState(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None: throw new ArgumentException("A successful result cannot have an error.", nameof(error));
            case false when error == Error.None: throw new ArgumentException("A failed result must have an error.", nameof(error));
        }
    }
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;
    internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error) { _value = value; }
    public TValue Value => IsSuccess ? _value ?? throw new InvalidOperationException("Successful result contains null value.") : throw new InvalidOperationException("Cannot access value of failed result.");
    public static implicit operator Result<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
    public static Result<TValue> From(TValue? value, Error error) => value is not null ? Success(value) : Failure<TValue>(error);
    public Result<TNewValue> Map<TNewValue>(Func<TValue, TNewValue> mapper) => IsSuccess ? Success(mapper(Value)) : Failure<TNewValue>(Error);
}