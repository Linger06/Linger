using Linger.Audit.Contracts;

namespace Linger.Audit;

/// <summary>
/// Represents an entity with creation audit information and a strongly typed identifier.
/// </summary>
/// <typeparam name="T">The identifier type.</typeparam>
public abstract class CreationAuditEntity<T> : BaseEntity<T>, ICreationAuditEntity
{
    /// <inheritdoc />
    public string CreatorId { get; set; } = string.Empty;

    /// <inheritdoc />
    public DateTimeOffset CreationTime { get; set; }
}

/// <summary>
/// Represents an entity with creation audit information.
/// </summary>
public abstract class CreationAuditEntity : BaseEntity, ICreationAuditEntity
{
    /// <inheritdoc />
    public string CreatorId { get; set; } = string.Empty;

    /// <inheritdoc />
    public DateTimeOffset CreationTime { get; set; }
}
