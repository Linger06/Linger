using Linger.Audit.Contracts;
using Linger.Extensions.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Linger.EFCore.Audit.Interceptors;

/// <summary>
/// Intercepts save changes operations to automatically populate audit fields
/// </summary>
public class AuditEntitiesSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IAuditUserProvider _auditUserProvider;
    private readonly ILogger<AuditEntitiesSaveChangesInterceptor>? _logger;

    public AuditEntitiesSaveChangesInterceptor(IAuditUserProvider auditUserProvider, ILogger<AuditEntitiesSaveChangesInterceptor>? logger = null)
    {
        _auditUserProvider = auditUserProvider;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AuditEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AuditEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditEntities(DbContext? context)
    {
        if (context == null)
        {
            _logger?.LogWarning("DbContext is null, audit skipped");
            return;
        }

        var userId = _auditUserProvider.UserName == null ? "Unknown" : _auditUserProvider.GetUser();
        var auditedEntities = new HashSet<object>();

        try
        {
            foreach (EntityEntry<ICreationAuditEntity> entry in context.ChangeTracker.Entries<ICreationAuditEntity>().ToList())
            {
                if (entry.State == EntityState.Added)
                {
                    ArgumentException.ThrowIfNullOrEmpty(userId);
                    entry.Entity.CreatorId ??= userId;
                    if (entry.Entity.CreationTime.IsNull() || entry.Entity.CreationTime == DateTimeOffset.MinValue)
                    {
                        entry.Entity.CreationTime = DateTimeOffset.Now;
                    }
                    auditedEntities.Add(entry.Entity);
                }
            }

            foreach (EntityEntry<ISoftDelete> entry in context.ChangeTracker.Entries<ISoftDelete>().ToList())
            {
                if (entry is { State: EntityState.Deleted, Entity: { } softDelete })
                {
                    softDelete.IsDeleted = true;
                    softDelete.DeleterId ??= userId;
                    softDelete.DeletionTime = DateTimeOffset.Now;
                    entry.State = EntityState.Modified;
                    auditedEntities.Add(entry.Entity);
                }
            }

            foreach (EntityEntry<IModificationAuditEntity> entry in context.ChangeTracker.Entries<IModificationAuditEntity>().ToList())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifierId = userId;
                    entry.Entity.LastModificationTime = DateTimeOffset.Now;
                    auditedEntities.Add(entry.Entity);
                }
            }

            if (auditedEntities.Count > 0)
            {
                _logger?.LogDebug("Populated audit fields for {Count} entities (user: {UserId})", auditedEntities.Count, userId);
            }

            // Check if the Audit Entry exists in the current DbContext
            if (context.Model.FindEntityType(typeof(AuditTrailEntry)) == null)
            {
                return;
            }

            context.ChangeTracker.DetectChanges();
            var entries = new List<AuditTrailEntry>();

            foreach (EntityEntry entry in context.ChangeTracker.Entries())
            {
                // Do not audit entities that are not tracked, not changed, or not of type IAuditable
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || entry.Entity is not IBaseAuditEntity)
                {
                    continue;
                }

                var auditEntry = new AuditTrailEntry
                {
                    AuditType =
                        entry.State switch
                        {
                            EntityState.Added => AuditType.Added,
                            EntityState.Deleted => AuditType.Deleted,
                            EntityState.Modified => AuditType.Modified,
                            _ => AuditType.Unknown
                        },
                    EntityId = entry.Properties.Single(p => p.Metadata.IsPrimaryKey()).CurrentValue?.ToString(),
                    EntityName = entry.Metadata.ClrType.Name,
                    Username = userId,
                    TimeStamp = DateTimeOffset.Now,
                    Changes = entry.Properties.Select(p => new { p.Metadata.Name, p.CurrentValue }).ToDictionary(i => i.Name, i => i.CurrentValue),

                    // TempProperties are properties that are only generated on save, e.g. IDs
                    // These properties will be set correctly after the audited entity has been saved
                    TempProperties = entry.Properties.Where(p => p.IsTemporary),
                    NewValues = [],
                    OldValues = [],
                    AffectedColumns = []
                };

                foreach (PropertyEntry prop in entry.Properties)
                {
                    var propertyName = prop.Metadata.Name;
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.EntityId = prop.CurrentValue?.ToString();
                    }
                    else
                    {
                        switch (auditEntry.AuditType)
                        {
                            case AuditType.Added:
                                auditEntry.NewValues[propertyName] = prop.CurrentValue;
                                break;
                            case AuditType.Deleted:
                                auditEntry.OldValues[propertyName] = prop.OriginalValue;
                                break;
                            case AuditType.Modified:
                                if (prop.IsModified)
                                {
                                    if (prop.OriginalValue.IsNull() && prop.CurrentValue.IsNotNull())
                                    {
                                        auditEntry.AffectedColumns.Add(propertyName);
                                        auditEntry.OldValues[propertyName] = prop.OriginalValue;
                                        auditEntry.NewValues[propertyName] = prop.CurrentValue;
                                    }
                                    else if (prop.OriginalValue.IsNotNull() && prop.OriginalValue.Equals(prop.CurrentValue) == false)
                                    {
                                        auditEntry.AffectedColumns.Add(propertyName);
                                        auditEntry.OldValues[propertyName] = prop.OriginalValue;
                                        auditEntry.NewValues[propertyName] = prop.CurrentValue;
                                    }
                                }
                                break;
                        }
                    }
                }

                auditEntry.NewValues = auditEntry.NewValues.Count == 0 ? null : auditEntry.NewValues;
                auditEntry.OldValues = auditEntry.OldValues.Count == 0 ? null : auditEntry.OldValues;
                auditEntry.AffectedColumns = auditEntry.AffectedColumns.Count == 0 ? null : auditEntry.AffectedColumns;

                entries.Add(auditEntry);
            }

            if (entries.Count != 0)
            {
                context.Set<AuditTrailEntry>().AddRange(entries);
                _logger?.LogDebug("Created {Count} audit trail entries for user: {UserId}", entries.Count, userId);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred during entity auditing for user: {UserId}", userId);
            throw;
        }
    }
}