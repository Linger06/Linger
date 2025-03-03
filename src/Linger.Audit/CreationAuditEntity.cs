using Linger.Audit.Contracts;

namespace Linger.Audit;
public abstract class CreationAuditEntity<T> : CreationAuditEntity, IEntity<T>
{
    public T Id { get; set; } = default!;
}

public abstract class CreationAuditEntity : BaseEntity, ICreationAuditEntity
{
    public string CreatorId { get; set; } = null!;

    public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;
}