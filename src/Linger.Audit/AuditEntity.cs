using Linger.Audit.Contracts;

namespace Linger.Audit;

/// <summary>
/// Represents an entity with creation and modification audit information and a strongly typed identifier.
/// </summary>
/// <typeparam name="T">The identifier type.</typeparam>
public abstract class AuditEntity<T> : CreationAuditEntity<T>, IModificationAuditEntity
{
    /// <inheritdoc />
    public string? LastModifierId { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? LastModificationTime { get; set; }
}

/// <summary>
/// Represents an entity with creation and modification audit information.
/// </summary>
public abstract class AuditEntity : CreationAuditEntity, IModificationAuditEntity
{
    /// <inheritdoc />
    public string? LastModifierId { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? LastModificationTime { get; set; }
}
