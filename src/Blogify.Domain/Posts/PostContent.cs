using System.Text.Json.Serialization;
using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public sealed class PostContent : ValueObject
{
    public const int MinLength = 100;
    public const int MaxLength = 5000;

    [JsonConstructor]
    private PostContent(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PostContent> Create(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Result.Failure<PostContent>(PostErrors.ContentEmpty);

        if (content.Length < MinLength) return Result.Failure<PostContent>(PostErrors.ContentTooShort);

        if (content.Length > MaxLength) return Result.Failure<PostContent>(PostErrors.ContentTooLong);

        return Result.Success(new PostContent(content));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}