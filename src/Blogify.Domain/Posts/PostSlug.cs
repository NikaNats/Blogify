using System.Text.RegularExpressions;
using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public sealed class PostSlug : ValueObject
{
    private const int MaxLength = 200;

    private PostSlug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PostSlug> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<PostSlug>(PostErrors.SlugEmpty);

        // A more robust slug generation algorithm
        var slug = GenerateSlug(title);

        if (slug.Length > MaxLength)
            // Optionally truncate or return an Error
            slug = slug[..MaxLength];

        return Result.Success(new PostSlug(slug));
    }

    private static string GenerateSlug(string phrase)
    {
        var str = phrase.ToLowerInvariant();
        // invalid chars
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        // convert multiple spaces into one space
        str = Regex.Replace(str, @"\s+", " ").Trim();
        // cut and trim
        str = str[..(str.Length <= MaxLength ? str.Length : MaxLength)].Trim();
        str = Regex.Replace(str, @"\s", "-"); // hyphens
        return str;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}