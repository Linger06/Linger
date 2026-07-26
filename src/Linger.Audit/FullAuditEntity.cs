using Linger.Audit.Contracts;

namespace Linger.Audit;

/// <summary>
/// Represents a fully audited entity with a strongly typed identifier.
/// </summary>
/// <typeparam name="T">The identifier type.</typeparam>
public abstract class FullAuditEntity<T> : AuditEntity<T>, ISoftDelete
{
    /// <inheritdoc />
    public DateTimeOffset? DeletionTime { get; set; }

    /// <inheritdoc />
    public string? DeleterId { get; set; }

    /// <inheritdoc />
    public bool? IsDeleted { get; set; }
}

/// <summary>
/// Represents an entity with creation, modification, and deletion audit information.
/// </summary>
public abstract class FullAuditEntity : AuditEntity, ISoftDelete
{
    /// <inheritdoc />
    public DateTimeOffset? DeletionTime { get; set; }

    /// <inheritdoc />
    public string? DeleterId { get; set; }

    /// <inheritdoc />
    public bool? IsDeleted { get; set; }
}
