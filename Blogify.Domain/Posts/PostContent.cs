using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public sealed class PostContent : ValueObject
{
    private const int MinLength = 100;

    private PostContent(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PostContent> Create(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Result.Failure<PostContent>(PostErrors.ContentEmpty);

        if (content.Length < MinLength) return Result.Failure<PostContent>(PostErrors.ContentTooShort);

        return Result.Success(new PostContent(content));
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}