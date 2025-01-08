using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments;

public class CommentContent : ValueObject
{
    private const int MaxLength = 1000;

    private CommentContent(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CommentContent> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<CommentContent>(CommentError.InvalidContent); // Use CommentError.InvalidContent


        if (value.Length > MaxLength)
            return Result.Failure<CommentContent>(CommentError.ContentTooLong); // Use CommentError.ContentTooLong


        return Result.Success(new CommentContent(value));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}