# Linger.EFCore.Audit

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

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

// 2. Register the audit interceptor 
services.AddDbContext<AppDbContext>(options => 
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp => 
        new AuditEntitiesSaveChangesInterceptor(
            sp.GetRequiredService<IAuditUserProvider>()
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
await dbContext.SaveChangesAsync();  // This will generate a "Created" audit record

// Modify entity
user.Email = "new.email@example.com";
await dbContext.SaveChangesAsync();  // This will generate a "Modified" audit record

// Delete entity
dbContext.Users.Remove(user);
await dbContext.SaveChangesAsync();  // This will generate a "Deleted" audit record
```

### Setting Current User Information

Audit records can include user information by implementing the `IAuditUserProvider` interface:

```csharp
// 1. Implement audit user provider
public class CurrentUserProvider : IAuditUserProvider 
{ 
    // Can get user info from your authentication system
    public string? UserName => "john.doe"; 
    
    public string GetUser() => UserName ?? "anonymous"; 
}

// 2. Register in your dependency injection container
services.AddScoped<IAuditUserProvider, CurrentUserProvider>();

// 3. Use with the interceptor
services.AddDbContext<AppDbContext>(options => 
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp => 
        new AuditEntitiesSaveChangesInterceptor(
            sp.GetRequiredService<IAuditUserProvider>()
        )
    );
});

// Now all operations will automatically include user information
var product = new Product { Name = "Sample Product", Price = 100.00m };
dbContext.Products.Add(product);
await dbContext.SaveChangesAsync();  // Audit record includes user ID and username
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

