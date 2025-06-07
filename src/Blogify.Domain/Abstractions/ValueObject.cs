namespace Blogify.Domain.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    private readonly Lazy<int> _cachedHashCode;
    protected ValueObject() => _cachedHashCode = new Lazy<int>(ComputeHashCode);
    public bool Equals(ValueObject? other) => other is not null && GetType() == other.GetType() && GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    protected abstract IEnumerable<object> GetAtomicValues();
    public override bool Equals(object? obj) => Equals(obj as ValueObject);
    public override int GetHashCode() => _cachedHashCode.Value;
    private int ComputeHashCode() => GetAtomicValues().Aggregate(0, HashCode.Combine);
    public static bool operator ==(ValueObject? left, ValueObject? right) => (left, right) switch { (null, null) => true, (null, _) => false, (_, null) => false, _ => left.Equals(right) };
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}