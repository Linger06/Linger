# Linger.Audit

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

[![NuGet](https://img.shields.io/nuget/v/Linger.Audit.svg)](https://www.nuget.org/packages/Linger.Audit/)
[![Downloads](https://img.shields.io/nuget/dt/Linger.Audit.svg)](https://www.nuget.org/packages/Linger.Audit/)
[![License](https://img.shields.io/github/license/lingershub/linger.audit)](LICENSE)

A lightweight .NET auditing library that provides base classes and interfaces for entity auditing.

## ✨ Features

- Multi-target framework support (.NET 9.0/.NET 8.0/.NET 6.0/NetStandard 2.0)
- Full audit trail tracking (creation, modification, deletion)
- Generic entity support with type-safe IDs
- Soft delete capability
- Built-in audit timestamps and user tracking
- Nullable reference types enabled
- MIT licensed

## 📦 Installation

### From Visual Studio

1. Open the `Solution Explorer`.
2. Right-click on a project within your solution.
3. Click on `Manage NuGet Packages...`.
4. Click on the `Browse` tab and search for "Linger.Audit".
5. Click on the `Linger.Audit` package, select the appropriate version and click Install.

### Package Manager Console

```
PM> Install-Package Linger.Audit
```

### .NET CLI Console

```
> dotnet add package Linger.Audit
```

## 🚀 Quick Start

### Basic Setup

```csharp
// 1. Implement audit user provider 
public class CurrentUserProvider : IAuditUserProvider 
{ 
    public string? UserName => "current-user"; 
    public string GetUser() => UserName ?? "anonymous"; 
}

// 2. Create auditable entity 
public class Product : FullAuditEntity 
{ 
    public string Name { get; set; } = default!; 
    public decimal Price { get; set; }
}

// The entity will track: 
// - Creation time and creator 
// - Last modification time and modifier
// - Deletion time and deleter 
// - Soft delete status
```

### Using with Dependency Injection

```csharp
// Register in your DI container
services.AddSingleton<IAuditUserProvider, CurrentUserProvider>();

// Usage with Entity Framework Core
public class ApplicationDbContext : DbContext
{
    private readonly IAuditUserProvider _auditUserProvider;
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IAuditUserProvider auditUserProvider) : base(options)
    {
        _auditUserProvider = auditUserProvider;
    }
    
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }
    
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries().Where(e => 
            e.State == EntityState.Added || 
            e.State == EntityState.Modified ||
            e.State == EntityState.Deleted);
            
        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.CreationTime = DateTime.UtcNow;
                    auditableEntity.CreatorUserId = _auditUserProvider.GetUser();
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableEntity.LastModificationTime = DateTime.UtcNow;
                    auditableEntity.LastModifierUserId = _auditUserProvider.GetUser();
                }
            }
            
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDelete)
            {
                entry.State = EntityState.Modified;
                softDelete.IsDeleted = true;
                softDelete.DeletionTime = DateTime.UtcNow;
                softDelete.DeleterUserId = _auditUserProvider.GetUser();
            }
        }
    }
}
```

## 🏗️ Architecture

Linger.Audit provides the following key components:

- `IAuditUserProvider` - Interface for getting current user
- `FullAuditEntity<T>` - Base class for entities with full audit trail and generic ID type
- `FullAuditEntity` - Base class for entities with full audit trail using `string` ID type
- `AuditableEntity<T>` - Base class with creation/modification audit and generic ID type
- `AuditableEntity` - Base class with creation/modification audit using `string` ID type
- `ISoftDelete` - Interface for soft delete support

## ❓ Frequently Asked Questions

### How to use custom ID types?

```csharp
// Using Guid as ID type
public class Customer : FullAuditEntity<Guid> 
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}

// Using int as ID type
public class Order : FullAuditEntity<int>
{
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
}
```

### How to use only partial auditing features?

```csharp
// Only need creation and modification auditing
public class SimpleProduct : AuditableEntity
{
    public string Name { get; set; } = default!;
}

// Only need soft delete functionality
public class Document : Entity, ISoftDelete
{
    public string Title { get; set; } = default!;
    public bool IsDeleted { get; set; }
    public DateTime? DeletionTime { get; set; }
    public string? DeleterUserId { get; set; }
}
```

## 🤝 Contributing

Contributions are welcome! If you have feature requests, bug reports, or pull requests, please feel free to submit them on our GitHub repository.

## 📝 License

This project is licensed under the MIT License.

