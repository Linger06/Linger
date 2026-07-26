using Microsoft.EntityFrameworkCore;

namespace Linger.EFCore.Audit;

/// <summary>
/// Provides audit model configuration extensions.
/// </summary>
public static class AuditModelBuilder
{
    /// <summary>
    /// Configures the audit trail entity and its value conversions.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ApplyAudit(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.AuditType).HasConversion<string>();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.CurrentValuesSnapshot).HasJsonConversion();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.NewValues).HasJsonConversion();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.OldValues).HasJsonConversion();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.AffectedColumns).HasStringCollectionConversion();
    }
}
