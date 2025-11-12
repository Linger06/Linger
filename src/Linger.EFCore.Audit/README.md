# Linger.EFCore.Audit

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
    public string? UserName => "john.doe"; 
    
    public string GetUser() => UserName ?? "anonymous"; 
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
await dbContext.SaveChangesAsync();  // Automatically generates "Deleted" audit record (soft delete)
```

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
    public List<string>? AffectedColumns { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public Dictionary<string, object>? Changes { get; set; }
    public IEnumerable<PropertyEntry>? TempProperties { get; set; }
}
```

The `AuditTrailEntry` captures:
- Entity name and ID
- Type of change (Added/Modified/Deleted)
- Username performing the change
- Timestamp
- Old and new property values
- List of modified columns

## 🔍 Automatic Tracking

- Creation audit: CreatorId, CreationTime
- Modification audit: LastModifierId, LastModificationTime
- Soft delete: IsDeleted, DeleterId, DeletionTime

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