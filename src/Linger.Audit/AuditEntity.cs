using Linger.Audit.Contracts;

namespace Linger.Audit;

public abstract class AuditEntity<T> : AuditEntity, IEntity<T>
{
    public T Id { get; set; } = default!;
}

public abstract class AuditEntity : CreationAuditEntity, IModificationAuditEntity
{
    public string? LastModifierId { get; set; }

    public DateTimeOffset? LastModificationTime { get; set; }
}