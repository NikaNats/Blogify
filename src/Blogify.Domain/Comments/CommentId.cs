using Blogify.Domain.Abstractions;

namespace Blogify.Domain.Comments;

public sealed class CommentId : ValueObject
{
    internal CommentId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static CommentId New()
    {
        return new CommentId(Guid.NewGuid());
    }

    public static CommentId From(Guid id)
    {
        return new CommentId(id);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}