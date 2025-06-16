namespace Blogify.Domain.Abstractions;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id) : base(id)
    {
    }

    protected AuditableEntity()
    {
    }

    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? LastModifiedAt { get; private set; }
    public Guid? LastModifiedBy { get; private set; }

    internal void UpdateCreationProperties(Guid? userId, DateTimeOffset createdAt)
    {
        CreatedBy = userId;
        CreatedAt = createdAt;
    }

    internal void UpdateModificationProperties(Guid? userId, DateTimeOffset modifiedAt)
    {
        LastModifiedBy = userId;
        LastModifiedAt = modifiedAt;
    }
}