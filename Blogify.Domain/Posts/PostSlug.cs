using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public sealed class PostSlug: ValueObject
{
    private const int MaxLength = 200;

    private PostSlug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PostSlug> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return Result.Failure<PostSlug>(PostErrors.SlugEmpty);

        var slug = title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "and");

        if (slug.Length > MaxLength) return Result.Failure<PostSlug>(PostErrors.SlugTooLong);

        return Result.Success(new PostSlug(slug));
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}