using Linger.Audit.Contracts;

namespace Linger.Audit;

/// <summary>
/// Represents an entity with a strongly typed identifier.
/// </summary>
/// <typeparam name="T">The identifier type.</typeparam>
public abstract class BaseEntity<T> : BaseEntity, IEntity<T>
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public T Id { get; set; } = default!;
}

/// <summary>
/// Represents the non-generic base type for entities.
/// </summary>
public abstract class BaseEntity : IEntity
{
}
