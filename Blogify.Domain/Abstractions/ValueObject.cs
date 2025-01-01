namespace Blogify.Domain.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    private int? _cachedHashCode;

    public bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    protected abstract IEnumerable<object> GetAtomicValues();

    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    public override int GetHashCode()
    {
        if (!_cachedHashCode.HasValue)
            _cachedHashCode = GetAtomicValues()
                .Aggregate(default(int), HashCode.Combine);

        return _cachedHashCode.Value;
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    protected static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        if (left is null ^ right is null)
            return false;

        return left?.Equals(right) ?? true;
    }
}