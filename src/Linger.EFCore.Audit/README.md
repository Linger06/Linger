# Linger.EFCore.Audit

> Breaking changes and 2.0 migration notes are documented in the [Linger migration guide](../Linger/MIGRATION.md).

An Entity Framework Core audit trail library for automatically tracking data changes.

## ✨ Features

- Automatic audit logging for Entity Framework Core operations
- Tracks entity creation, modification, and deletion
- Captures old and new values for changed properties
- Records user information for each change
- Supports soft delete
- Built-in JSON serialization for audit data
- Compatible with EF Core 9.0 and 8.0

## 📦 Installation

### From Visual Studio

1. Open the `Solution Explorer`.
2. Right-click on a project within your solution.
3. Click on `Manage NuGet Packages...`.
4. Click on the `Browse` tab and search for "Linger.EFCore.Audit".
5. Click on the `Linger.EFCore.Audit` package, select the appropriate version and click Install.

### Package Manager Console

```
PM> Install-Package Linger.EFCore.Audit
```

### .NET CLI Console

```
> dotnet add package Linger.EFCore.Audit
```

## 🚀 Quick Start

### Configuration

Add the audit functionality to your EF Core DbContext:

```csharp
// 1. Add audit trail to your DbContext 
public class AppDbContext : DbContext 
{ 
    public DbSet<AuditTrailEntry> AuditTrails { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        base.OnModelCreating(modelBuilder);
        
        // Apply audit configurations
        modelBuilder.ApplyAudit();
    } 
}

// 2. Implement audit user provider
public class CurrentUserProvider : IAuditUserProvider 
{ 
    // Can get user info from your authentication system
    public string? GetUser() => "john.doe";
}

// 3. Register services and interceptor
services.AddScoped<IAuditUserProvider, CurrentUserProvider>();

services.AddDbContext<AppDbContext>((serviceProvider, options) => 
{
    options.UseSqlServer(connectionString);
    
    // Without logging
    options.AddInterceptors(
        new AuditEntitiesSaveChangesInterceptor(
            serviceProvider.GetRequiredService<IAuditUserProvider>()
        )
    );
    
    // Or with logging (Recommended - for monitoring and debugging)
    options.AddInterceptors(
        new AuditEntitiesSaveChangesInterceptor(
            serviceProvider.GetRequiredService<IAuditUserProvider>(),
            serviceProvider.GetRequiredService<ILogger<AuditEntitiesSaveChangesInterceptor>>()
        )
    );
});
```

### Audit time

The interceptor uses UTC by default. Applications that need a specific time zone can register `IAuditTimeProvider`; the interceptor obtains a fresh timestamp for each save operation:

```csharp
public sealed class ChinaStandardTimeAuditTimeProvider : IAuditTimeProvider
{
    public DateTimeOffset GetNow() =>
        DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));
}

services.AddSingleton<IAuditTimeProvider, ChinaStandardTimeAuditTimeProvider>();
```

The existing two-parameter constructor remains supported. If no time provider is registered, UTC behavior is preserved.

### Retrying a failed save and generated keys

**A failed save raises an exception; retrying is optional.** The audit interceptor neither suppresses exceptions from `SaveChanges` / `SaveChangesAsync` nor calls save again. The caller can report the error, end the unit of work, and dispose of the `DbContext`. No second save is needed to clean up audit rows.

Pending audit rows matter if the caller knows the first save did not commit, corrects the cause, and explicitly saves again on the **same `DbContext`**. For entities that do not implement `ISoftDelete`, the interceptor regenerates its own pending audit rows from the current changes and leaves application-added audit rows alone. Entities implementing `ISoftDelete` are excluded even for ordinary updates. There is no cross-context deduplication; if the commit outcome is unknown, check the database before retrying.

`EntityId` is captured **before** the database write. If any part of the primary key is still temporary (for example, an identity column generated on insert), the audit row stores `null`; it is not updated after the insert. Keys already available at save time are recorded, including keys assigned in advance by the InMemory provider.

### Example Usage

Automatic tracking of all changes to entities:

```csharp
// Create a new entity
var user = new User
{
    Name = "John Doe",
    Email = "john.doe@example.com"
};
dbContext.Users.Add(user);
await dbContext.SaveChangesAsync();  // Automatically generates "Created" audit record with user info

// Modify entity
user.Email = "new.email@example.com";
await dbContext.SaveChangesAsync();  // Automatically generates "Modified" audit record

// Delete entity
dbContext.Users.Remove(user);
await dbContext.SaveChangesAsync();  // Soft delete: generates a Modified audit record
```

### Soft-delete limitations

When `Remove` receives a key-only entity, the interceptor does not write unloaded business columns back to the database. To change business data as well, load the entity and edit it explicitly. `CurrentValuesSnapshot` reflects the tracked object; no database read is performed to fill in a stub. Load the entity first if a full audit snapshot is required. If the model uses a concurrency token, its correct original value is still required.

For entities implementing `ISoftDelete`, use `Remove` + `SaveChanges` so the interceptor can convert deletion to an update and record deletion metadata. **Do not call `ExecuteDelete` / `ExecuteDeleteAsync` for these entities**: they delete directly in the database and bypass the ChangeTracker and `SaveChanges` interceptors, physically removing rows. For bulk soft deletion, explicitly update `IsDeleted` and related fields, and handle audit metadata and audit rows yourself.

Hiding soft-deleted rows depends on an EF Core global query filter. Explicitly call `ApplyGlobalFilters<ISoftDelete>(e => e.IsDeleted != true)` in `OnModelCreating`; `ApplyAudit()` does not enable this filter automatically.
### Querying Audit Records

Audit records are stored in the `AuditTrails` DbSet and can be queried in various ways:

```csharp
// Find all audit records related to a specific entity
var entityAudits = await dbContext.AuditTrails
    .Where(a => a.EntityId == "123" && a.EntityName == "User")
    .OrderBy(a => a.TimeStamp)
    .ToListAsync();

// Display audit history
foreach (var audit in entityAudits)
{
    Console.WriteLine($"Action: {audit.AuditType}, Time: {audit.TimeStamp}, User: {audit.Username}");
    
    // Display all changed properties
    if (audit.AffectedColumns != null)
    {
        Console.WriteLine("Changes:");
        foreach (var column in audit.AffectedColumns)
        {
            var oldValue = audit.OldValues?[column];
            var newValue = audit.NewValues?[column];
            Console.WriteLine($"  {column}: Old = {oldValue}, New = {newValue}");
        }
    }
}

// Query audit records by user
var userAudits = await dbContext.AuditTrails
    .Where(a => a.Username == "john.doe")
    .OrderByDescending(a => a.TimeStamp)
    .Take(10)
    .ToListAsync();

// Query audit records within a specific date range
var startDate = DateTimeOffset.Now.AddDays(-7);
var endDate = DateTimeOffset.Now;

var recentAudits = await dbContext.AuditTrails
    .Where(a => a.TimeStamp >= startDate && a.TimeStamp <= endDate)
    .OrderBy(a => a.TimeStamp)
    .ToListAsync();
```

## 📄 Audit Trail Data

The `AuditTrailEntry` class is the main class that represents a single audit record:

```csharp
public class AuditTrailEntry
{
    public long Id { get; set; }
    public string? Username { get; set; }
    public AuditType AuditType { get; set; }  // Added, Modified or Deleted
    public string EntityName { get; set; }
    public string? EntityId { get; set; }
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
    public Dictionary<string, object>? CurrentValuesSnapshot { get; set; }
    public List<string>? AffectedColumns { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}
```

The `AuditTrailEntry` captures:
- Entity name and ID
- Type of change (Added/Modified/Deleted)
- Username performing the change
- Timestamp
- Old and new property values
- Complete current non-key property value snapshot
- List of modified columns

## 🔍 Automatic Tracking

- Creation audit: CreatorId, CreationTime
- Modification audit: LastModifierId, LastModificationTime
- Soft delete: IsDeleted, DeleterId, DeletionTime

## 🔧 Advanced Configuration

### Handling Legacy Database DateTime Types

When an existing database uses `datetime` instead of `datetimeoffset`, configure the audit properties in the EF Core entity mapping:

```csharp
public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.Property(e => e.CreationTime)
            .HasColumnType("datetime")
            .HasConversion(
                value => value.ToDateTime(),
                value => new DateTimeOffset(value));

        entity.Property(e => e.LastModificationTime)
            .HasColumnType("datetime")
            .HasConversion(
                value => value.HasValue ? value.Value.ToDateTime() : (DateTime?)null,
                value => value.HasValue
                    ? new DateTimeOffset(value.Value, TimeSpan.Zero)
                    : (DateTimeOffset?)null);

        entity.Property(e => e.DeletionTime)
            .HasColumnType("datetime")
            .HasConversion(
                value => value.HasValue ? value.Value.ToDateTime() : (DateTime?)null,
                value => value.HasValue
                    ? new DateTimeOffset(value.Value, TimeSpan.Zero)
                    : (DateTimeOffset?)null);

        entity.Property(e => e.CreatorId).HasMaxLength(30).IsUnicode(false);
        entity.Property(e => e.LastModifierId).HasMaxLength(30).IsUnicode(false);
        entity.Property(e => e.DeleterId).HasMaxLength(30).IsUnicode(false);
    }
}
```

The conversion loses the original timezone offset. Store and read these values consistently as UTC; `TimeSpan.Zero` represents the UTC offset.

## 📊 Logging

The interceptor supports optional logging to help monitor audit operations and troubleshoot issues.

### Configure Log Level

In `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Linger.EFCore.Audit.Interceptors": "Debug"
    }
  }
}
```
