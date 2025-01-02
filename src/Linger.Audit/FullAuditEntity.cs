using Linger.Audit.Contracts;

namespace Linger.Audit;
public abstract class FullAuditEntity<T> : FullAuditEntity, IEntity<T>
{
    public T Id { get; set; } = default!;
}

public abstract class FullAuditEntity : AuditEntity, ISoftDelete
{
    public DateTimeOffset? DeletionTime { get; set; }
    public string? DeleterId { get; set; }
    public bool? IsDeleted { get; set; }
}