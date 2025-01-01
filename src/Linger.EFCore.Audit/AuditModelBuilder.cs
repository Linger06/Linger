using Microsoft.EntityFrameworkCore;

namespace Linger.EFCore.Audit;
public static class AuditModelBuilder
{
    public static void ApplyAudit(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.AuditType).HasConversion<string>();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.Changes).HasJsonConversion();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.NewValues).HasJsonConversion();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.OldValues).HasJsonConversion();
        modelBuilder.Entity<AuditTrailEntry>().Property(t => t.AffectedColumns).HasArrayConversion();
    }
}