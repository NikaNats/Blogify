using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments;

public class CommentContent : ValueObject
{
    private const int MinLength = 1;
    private const int MaxLength = 1000;

    private CommentContent(string value)
    {
        Value = value.Trim();
    }

    public string Value { get; }

    public static implicit operator string(CommentContent content)
    {
        return content.Value;
    }

    public static Result<CommentContent> Create(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmed))
            return Result.Failure<CommentContent>(CommentError.EmptyContent);

        return trimmed.Length switch
        {
            < MinLength => Result.Failure<CommentContent>(CommentError.ContentTooShort),
            > MaxLength => Result.Failure<CommentContent>(CommentError.ContentTooLong),
            _ => Result.Success(new CommentContent(trimmed))
        };

    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}