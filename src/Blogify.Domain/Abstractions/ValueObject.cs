namespace Blogify.Domain.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    private readonly Lazy<int> _cachedHashCode;

    protected ValueObject()
    {
        _cachedHashCode = new Lazy<int>(ComputeHashCode);
    }

    public bool Equals(ValueObject? other)
    {
        return other is not null && GetType() == other.GetType() &&
               GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    protected abstract IEnumerable<object> GetAtomicValues();

    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    public override int GetHashCode()
    {
        return _cachedHashCode.Value;
    }

    private int ComputeHashCode()
    {
        return GetAtomicValues().Aggregate(0, HashCode.Combine);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true, (null, _) => false, (_, null) => false, _ => left.Equals(right)
        };
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}