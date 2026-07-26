using System.Globalization;
using System.Text.Json;
using Linger.Audit.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Linger.EFCore.Audit.Interceptors;

/// <summary>
/// Intercepts save operations to populate audit fields and create audit trail records.
/// </summary>
public class AuditEntitiesSaveChangesInterceptor : SaveChangesInterceptor
{
    private const string UnknownUser = "Unknown";
    private readonly IAuditUserProvider _auditUserProvider;
    private readonly ILogger<AuditEntitiesSaveChangesInterceptor>? _logger;

    /// <summary>
    /// Initializes a new interceptor.
    /// </summary>
    /// <param name="auditUserProvider">The current audit user provider.</param>
    /// <param name="logger">An optional logger.</param>
    public AuditEntitiesSaveChangesInterceptor(
        IAuditUserProvider auditUserProvider,
        ILogger<AuditEntitiesSaveChangesInterceptor>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(auditUserProvider);

        _auditUserProvider = auditUserProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AuditEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AuditEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AuditEntities(DbContext? context)
    {
        if (context is null)
        {
            _logger?.LogWarning("DbContext is null, audit skipped");
            return;
        }

        var userId = _auditUserProvider.GetUser();
        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = UnknownUser;
        }

        var timestamp = DateTimeOffset.UtcNow;
        var auditedEntities = new HashSet<object>();

        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries<ICreationAuditEntity>().ToList())
        {
            if (entry.State != EntityState.Added)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Entity.CreatorId))
            {
                entry.Entity.CreatorId = userId;
            }

            if (entry.Entity.CreationTime == default)
            {
                entry.Entity.CreationTime = timestamp;
            }

            auditedEntities.Add(entry.Entity);
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>().ToList())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.Entity.IsDeleted = true;
            entry.Entity.DeleterId ??= userId;
            entry.Entity.DeletionTime = timestamp;
            entry.State = EntityState.Modified;
            auditedEntities.Add(entry.Entity);
        }

        foreach (var entry in context.ChangeTracker.Entries<IModificationAuditEntity>().ToList())
        {
            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            entry.Entity.LastModifierId = userId;
            entry.Entity.LastModificationTime = timestamp;
            auditedEntities.Add(entry.Entity);
        }

        if (auditedEntities.Count > 0)
        {
            _logger?.LogDebug(
                "Populated audit fields for {Count} entities (user: {UserId})",
                auditedEntities.Count,
                userId);
        }

        if (context.Model.FindEntityType(typeof(AuditTrailEntry)) is null)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        var auditEntries = CreateAuditEntries(context.ChangeTracker.Entries(), userId, timestamp);
        if (auditEntries.Count == 0)
        {
            return;
        }

        context.Set<AuditTrailEntry>().AddRange(auditEntries);
        _logger?.LogDebug(
            "Created {Count} audit trail entries for user: {UserId}",
            auditEntries.Count,
            userId);
    }

    private static List<AuditTrailEntry> CreateAuditEntries(
        IEnumerable<EntityEntry> trackedEntries,
        string userId,
        DateTimeOffset timestamp)
    {
        var auditEntries = new List<AuditTrailEntry>();

        foreach (var entry in trackedEntries)
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged ||
                entry.Entity is not IBaseAuditEntity)
            {
                continue;
            }

            var keyProperties = entry.Properties
                .Where(property => property.Metadata.IsPrimaryKey())
                .ToArray();
            var auditEntry = new AuditTrailEntry
            {
                AuditType = entry.State switch
                {
                    EntityState.Added => AuditType.Added,
                    EntityState.Deleted => AuditType.Deleted,
                    EntityState.Modified => AuditType.Modified,
                    _ => AuditType.Unknown
                },
                EntityId = keyProperties.Any(property => property.IsTemporary)
                    ? null
                    : FormatEntityId(keyProperties),
                EntityName = entry.Metadata.ClrType.Name,
                Username = userId,
                TimeStamp = timestamp,
                CurrentValuesSnapshot = CreateCurrentValuesSnapshot(entry),
                NewValues = [],
                OldValues = [],
                AffectedColumns = []
            };

            PopulateChangedValues(entry, auditEntry);

            auditEntry.NewValues = auditEntry.NewValues.Count == 0 ? null : auditEntry.NewValues;
            auditEntry.OldValues = auditEntry.OldValues.Count == 0 ? null : auditEntry.OldValues;
            auditEntry.AffectedColumns = auditEntry.AffectedColumns.Count == 0 ? null : auditEntry.AffectedColumns;

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    private static Dictionary<string, object?> CreateCurrentValuesSnapshot(EntityEntry entry)
    {
        return entry.Properties
            .Where(property => !property.Metadata.IsPrimaryKey())
            .ToDictionary(property => property.Metadata.Name, property => property.CurrentValue);
    }

    private static void PopulateChangedValues(EntityEntry entry, AuditTrailEntry auditEntry)
    {
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            var propertyName = property.Metadata.Name;
            switch (auditEntry.AuditType)
            {
                case AuditType.Added:
                    auditEntry.NewValues![propertyName] = property.CurrentValue;
                    break;
                case AuditType.Deleted:
                    auditEntry.OldValues![propertyName] = property.OriginalValue;
                    break;
                case AuditType.Modified when property.IsModified &&
                                             !Equals(property.OriginalValue, property.CurrentValue):
                    auditEntry.AffectedColumns!.Add(propertyName);
                    auditEntry.OldValues![propertyName] = property.OriginalValue;
                    auditEntry.NewValues![propertyName] = property.CurrentValue;
                    break;
            }
        }
    }

    private static string? FormatEntityId(IReadOnlyList<PropertyEntry> keyProperties)
    {
        if (keyProperties.Count == 0)
        {
            return null;
        }

        if (keyProperties.Count == 1)
        {
            return FormatKeyValue(keyProperties[0].CurrentValue);
        }

        var keyValues = new Dictionary<string, object?>(keyProperties.Count, StringComparer.Ordinal);
        foreach (var keyProperty in keyProperties)
        {
            keyValues[keyProperty.Metadata.Name] = keyProperty.CurrentValue;
        }

        return JsonSerializer.Serialize(keyValues);
    }

    private static string? FormatKeyValue(object? value)
    {
        return value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value?.ToString();
    }
}
