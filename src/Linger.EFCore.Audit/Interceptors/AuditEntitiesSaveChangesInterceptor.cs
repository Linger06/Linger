using System.Globalization;
using System.Runtime.CompilerServices;
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
    private readonly IAuditTimeProvider _timeProvider;
    private readonly ConditionalWeakTable<DbContext, List<AuditTrailEntry>> _pendingAudits = new();

    /// <summary>
    /// Initializes a new interceptor.
    /// </summary>
    /// <param name="auditUserProvider">The current audit user provider.</param>
    /// <param name="logger">An optional logger.</param>
    public AuditEntitiesSaveChangesInterceptor(
        IAuditUserProvider auditUserProvider,
        ILogger<AuditEntitiesSaveChangesInterceptor>? logger = null)
        : this(auditUserProvider, logger, null)
    {
    }

    /// <summary>
    /// Initializes a new interceptor with a configurable audit time provider.
    /// </summary>
    /// <param name="auditUserProvider">The current audit user provider.</param>
    /// <param name="logger">An optional logger.</param>
    /// <param name="timeProvider">The provider used to obtain audit timestamps.</param>
    public AuditEntitiesSaveChangesInterceptor(
        IAuditUserProvider auditUserProvider,
        ILogger<AuditEntitiesSaveChangesInterceptor>? logger,
        IAuditTimeProvider? timeProvider)
    {
        ArgumentNullException.ThrowIfNull(auditUserProvider);

        _auditUserProvider = auditUserProvider;
        _logger = logger;
        _timeProvider = timeProvider ?? new UtcAuditTimeProvider();
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

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is { } context)
        {
            _pendingAudits.Remove(context);
        }

        return base.SavedChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            _pendingAudits.Remove(context);
        }

        return base.SavedChangesAsync(eventData, result, cancellationToken);
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

        var timestamp = _timeProvider.GetNow();
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

            // 只传主键时，未加载的业务字段是默认值，不能把整行标为 Modified。
            entry.State = EntityState.Unchanged;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeleterId ??= userId;
            entry.Entity.DeletionTime = timestamp;
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.Name is nameof(ISoftDelete.IsDeleted) or nameof(ISoftDelete.DeleterId) or nameof(ISoftDelete.DeletionTime))
                {
                    property.IsModified = true;
                }
            }
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

        context.ChangeTracker.DetectChanges();
        if (context.Model.FindEntityType(typeof(AuditTrailEntry)) is null)
        {
            return;
        }

        // 未成功完成的保存可能留下 Added 审计行；只重建本拦截器生成的非软删除审计。
        if (_pendingAudits.TryGetValue(context, out var pending))
        {
            foreach (var audit in pending)
            {
                var entry = context.Entry(audit);
                if (entry.State == EntityState.Added)
                {
                    entry.State = EntityState.Detached;
                }
            }

            _pendingAudits.Remove(context);
        }

        var auditEntries = CreateAuditEntries(context, userId, timestamp);
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

    private List<AuditTrailEntry> CreateAuditEntries(
        DbContext context,
        string userId,
        DateTimeOffset timestamp)
    {
        var auditEntries = new List<AuditTrailEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
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

            if (entry.Entity is not ISoftDelete)
            {
                _pendingAudits.GetOrCreateValue(context).Add(auditEntry);
            }

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

/// <summary>
/// Supplies the timestamp used by the audit interceptor.
/// </summary>
public interface IAuditTimeProvider
{
    /// <summary>
    /// Gets the current timestamp for audit fields and audit trail records.
    /// </summary>
    /// <returns>The current timestamp.</returns>
    DateTimeOffset GetNow();
}

internal sealed class UtcAuditTimeProvider : IAuditTimeProvider
{
    public DateTimeOffset GetNow()
    {
        return DateTimeOffset.UtcNow;
    }
}
