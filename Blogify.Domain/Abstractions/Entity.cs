using System.Runtime.CompilerServices;

namespace Blogify.Domain.Abstractions;

public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly HashSet<string> _modifiedProperties = new();

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty.", nameof(id));

        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = CreatedAt;
    }

    protected Entity() : this(Guid.NewGuid())
    {
    }

    public Guid Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset LastModifiedAt { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public IReadOnlyCollection<string> ModifiedProperties => _modifiedProperties.ToList().AsReadOnly();

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetType() == other.GetType() && Id == other.Id;
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;
        _modifiedProperties.Add(propertyName);
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void ResetChangeTracking()
    {
        _modifiedProperties.Clear();
    }

    public void UpdateModificationTimestamp()
    {
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}, CreatedAt={CreatedAt}, LastModifiedAt={LastModifiedAt}]";
    }
}