namespace Blogify.Domain.Abstractions;

/// <summary>
///     Base class for Value Objects in the domain model.
///     Implements value semantics and equality comparison.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    // Using readonly for thread safety
    private readonly Lazy<int> _cachedHashCode;

    protected ValueObject()
    {
        _cachedHashCode = new Lazy<int>(ComputeHashCode);
    }

    /// <summary>
    ///     Implements equality comparison between two ValueObjects
    /// </summary>
    /// <param name="other">The ValueObject to compare with</param>
    /// <returns>True if the objects are equal, false otherwise</returns>
    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return GetType() == other.GetType() &&
               GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    /// <summary>
    ///     Gets the atomic values that define the value object's identity
    /// </summary>
    /// <returns>Collection of object values that comprise this value object's identity</returns>
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
        return GetAtomicValues()
            .Aggregate(0, HashCode.Combine);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return EqualOperator(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Handles equality comparison taking into account null values
    /// </summary>
    private static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
}