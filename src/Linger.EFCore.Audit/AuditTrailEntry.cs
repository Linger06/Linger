using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linger.EFCore.Audit;

/// <summary>
/// Represents a persisted audit record for an entity change.
/// </summary>
[Table(nameof(AuditTrailEntry))]
public class AuditTrailEntry
{
    /// <summary>
    /// Gets or sets the audit record identifier.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the audited entity type name.
    /// </summary>
    public required string EntityName { get; set; }

    /// <summary>
    /// Gets or sets the audit operation type.
    /// </summary>
    public required AuditType AuditType { get; set; }

    /// <summary>
    /// Gets or sets the user identifier associated with the operation.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the operation timestamp.
    /// </summary>
    public DateTimeOffset TimeStamp { get; set; }

    /// <summary>
    /// Gets or sets the entity identifier. Composite identifiers are stored as JSON.
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the original property values.
    /// </summary>
    public Dictionary<string, object?>? OldValues { get; set; } = [];

    /// <summary>
    /// Gets or sets the current property values.
    /// </summary>
    public Dictionary<string, object?>? NewValues { get; set; } = [];

    /// <summary>
    /// Gets or sets a complete snapshot of the current non-key property values.
    /// </summary>
    public Dictionary<string, object?>? CurrentValuesSnapshot { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of modified properties.
    /// </summary>
    public ICollection<string>? AffectedColumns { get; set; } = [];
}
