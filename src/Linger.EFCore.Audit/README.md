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


```csharp
// 1. Add audit trail to your DbContext 
public class AppDbContext : DbContext 
{ 
    public DbSet<AuditTrailEntry> AuditTrails { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        modelBuilder.ApplyAudit(); // Apply audit configurations 
    } 
}

// 2. Register the audit interceptor 
services.AddDbContext(options => { options.AddInterceptors(sp => new AuditEntitiesSaveChangesInterceptor( sp.GetRequiredService() )); });
```

## 📄 Audit Trail Data

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

## 📝 License

This project is licensed under the MIT License.

