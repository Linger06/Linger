using Linger.Audit.Contracts;

namespace Linger.Audit;

public abstract class BaseEntity<T> : BaseEntity, IEntity<T>
{
    public T Id { get; set; } = default!;
}

public abstract class BaseEntity : IEntity
{
}