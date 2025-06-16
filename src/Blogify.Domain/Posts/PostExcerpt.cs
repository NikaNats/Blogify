using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public sealed class PostExcerpt : ValueObject
{
    private const int MaxLength = 500;

    private PostExcerpt(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PostExcerpt> Create(string excerpt)
    {
        if (string.IsNullOrWhiteSpace(excerpt)) return Result.Failure<PostExcerpt>(PostErrors.ExcerptEmpty);
        if (excerpt.Length > MaxLength) return Result.Failure<PostExcerpt>(PostErrors.ExcerptTooLong);
        return Result.Success(new PostExcerpt(excerpt));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}