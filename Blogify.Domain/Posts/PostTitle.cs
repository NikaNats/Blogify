using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Posts;

public sealed record PostTitle
{
    private const int MaxLength = 200;

    private PostTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PostTitle> Create(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return Result.Failure<PostTitle>(PostErrors.TitleEmpty);

        if (title.Length > MaxLength) return Result.Failure<PostTitle>(PostErrors.TitleTooLong);

        return Result.Success(new PostTitle(title));
    }
}