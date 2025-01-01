using Linger.Audit.Contracts;

namespace Linger.Audit;

public abstract class AuditEntity<T> : AuditableEntity, IEntity<T>
{
    public T Id { get; set; } = default!;
}

public abstract class AuditableEntity : CreationAuditEntity, IModificationAuditEntity
{
    public string? LastModifierId { get; set; }

    public DateTimeOffset? LastModificationTime { get; set; }
}