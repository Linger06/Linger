using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Linger.EFCore.Audit;

[Table(nameof(AuditTrailEntry))]
public class AuditTrailEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public required string EntityName { get; set; }
    public required AuditType AuditType { get; set; }
    public required string Username { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public string? EntityId { get; set; }
    public Dictionary<string, object?>? Changes { get; set; }

    public Dictionary<string, object?>? OldValues { get; set; } = [];
    public Dictionary<string, object?>? NewValues { get; set; } = [];
    public ICollection<string>? AffectedColumns { get; set; } = [];

    [NotMapped]
    // TempProperties are used for properties that are only generated on save, e.g. ID's
    public List<PropertyEntry> TempProperties { get; set; } = [];
}