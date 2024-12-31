using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public sealed record PostExcerpt
{
    private const int MaxLength = 500;
    
    private PostExcerpt(string value) => Value = value;

    public string Value { get; }

    public static Result<PostExcerpt> Create(string? excerpt)
    {
        if (string.IsNullOrWhiteSpace(excerpt))
        {
            return Result.Failure<PostExcerpt>(PostErrors.ExcerptEmpty);
        }

        if (excerpt.Length > MaxLength)
        {
            return Result.Failure<PostExcerpt>(PostErrors.ExcerptTooLong);
        }

        return Result.Success(new PostExcerpt(excerpt));
    }
}