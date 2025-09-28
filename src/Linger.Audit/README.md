# Linger.Audit

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

[![NuGet](https://img.shields.io/nuget/v/Linger.Audit.svg)](https://www.nuget.org/packages/Linger.Audit/)
[![Downloads](https://img.shields.io/nuget/dt/Linger.Audit.svg)](https://www.nuget.org/packages/Linger.Audit/)
[![License](https://img.shields.io/github/license/lingershub/linger.audit)](LICENSE)

A lightweight .NET auditing library that provides base classes and interfaces for entity auditing.

## 📖 Table of Contents

- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [🚀 Quick Start](#-quick-start)
  - [Basic Entities](#basic-entities)
  - [Creation Audit Entities](#creation-audit-entities)
  - [Full Audit Entities](#full-audit-entities)
- [💡 Usage Examples](#-usage-examples)
  - [Setting Up Current User Context](#setting-up-current-user-context)
  - [EF Core Integration](#ef-core-integration)
  - [Soft Delete Filtering](#soft-delete-filtering)
- [🔧 Advanced Configuration](#-advanced-configuration)
  - [Handling Legacy Database DateTime Types](#handling-legacy-database-datetime-types)
- [🧩 Class Diagram Overview](#-class-diagram-overview)
- [📋 Interface and Base Class Reference](#-interface-and-base-class-reference)
- [📜 License](#-license)

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
1. Open the `Solution Explorer`
2. Right-click on your project
3. Select `Manage NuGet Packages...`
4. Click the `Browse` tab and search for "Linger.Audit"
5. Click `Install`

### Package Manager Console
```powershell
Install-Package Linger.Audit
```

### .NET CLI
```bash
dotnet add package Linger.Audit
```

## 🚀 Quick Start

### Basic Entities

Inherit from base entity classes to get ID properties:

```csharp
// Simple entity with Guid ID type
public class Product : BaseEntity<Guid>
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string Description { get; set; } = null!;
}

// Entity with int ID type
public class Category : BaseEntity<int>
{
    public string Name { get; set; } = null!;
}

// Entity with string ID type
public class Tag : BaseEntity<string>
{
    public string Value { get; set; } = null!;
}
```

### Creation Audit Entities

Track creation time and creator:

```csharp
// Record when and who created a comment
public class Comment : CreationAuditEntity<Guid>
{
    public string Text { get; set; } = null!;
    public Guid ProductId { get; set; }
    
    // Inherited properties:
    // public string? CreatorId { get; set; }
    // public DateTimeOffset CreationTime { get; set; }
}
```

### Full Audit Entities

Track creation, modification, and deletion information:

```csharp
// User entity with full audit tracking
public class User : FullAuditEntity<Guid>
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    // Inherited properties:
    // Creation
    // public string? CreatorId { get; set; }
    // public DateTimeOffset CreationTime { get; set; }
    
    // Modification
    // public string? LastModifierId { get; set; }
    // public DateTimeOffset? LastModificationTime { get; set; }
    
    // Deletion
    // public bool IsDeleted { get; set; }
    // public string? DeleterId { get; set; }
    // public DateTimeOffset? DeletionTime { get; set; }
}
```

## 💡 Usage Examples

### Setting Up Current User Context

Use audit entities in your application services, and the system will automatically populate audit fields:

```csharp
// In your application service
public class ProductService : IProductService
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IAuditUserProvider _auditUserProvider;
    
    public ProductService(IRepository<Product, Guid> productRepository, IAuditUserProvider auditUserProvider)
    {
        _productRepository = productRepository;
        _auditUserProvider = auditUserProvider;
    }
    
    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            // ID, CreatorId and CreationTime will be set automatically when saving
        };
        
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();
        
        return product;
    }
}
```

### EF Core Integration

Configure EF Core DbContext to automatically handle audit fields:

```csharp
// Example of handling audit fields in EF Core
public class AppDbContext : DbContext
{
    private readonly IAuditUserProvider _auditUserProvider;
    
    public AppDbContext(DbContextOptions options, IAuditUserProvider auditUserProvider) 
        : base(options)
    {
        _auditUserProvider = auditUserProvider;
    }
    
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateAuditFields()
    {
        var userId = _auditUserProvider.GetUser();
        var now = DateTimeOffset.UtcNow;
        
        foreach (var entry in ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is ICreationAuditEntity creationAuditEntity)
                {
                    creationAuditEntity.CreationTime = now;
                    creationAuditEntity.CreatorId = userId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is IModificationAuditEntity modificationAuditEntity)
                {
                    modificationAuditEntity.LastModificationTime = now;
                    modificationAuditEntity.LastModifierId = userId;
                }
            }
            else if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDeleteEntity)
            {
                // Convert to soft delete
                entry.State = EntityState.Modified;
                softDeleteEntity.IsDeleted = true;
                
                if (entry.Entity is IDeletionAuditEntity deletionAuditEntity)
                {
                    deletionAuditEntity.DeletionTime = now;
                    deletionAuditEntity.DeleterId = userId;
                }
            }
        }
    }
}
```

### Soft Delete Filtering

Use global query filters to automatically filter soft-deleted entities:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply soft delete filter for all entities implementing ISoftDelete
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.PropertyOrField(parameter, nameof(ISoftDelete.IsDeleted));
            var condition = Expression.Not(property);
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
```

## 🔧 Advanced Configuration

### Handling Legacy Database DateTime Types

In real-world projects, you may need to integrate with existing databases that use `datetime` types instead of `datetimeoffset`. When you cannot modify the database table structure, you need to configure data type conversion in EF Core.

**Use Cases**:
- Database tables already exist using `datetime` type
- Cannot modify existing table structure
- Need to use `DateTimeOffset` type for auditing in your application

**Solution**:

```csharp
public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        // Configure CreationTime field
        entity.Property(e => e.CreationTime)
            .HasColumnType("datetime")
            .HasConversion(
                // When saving to database: DateTimeOffset -> DateTime
                v => v.ToDateTime(), 
                // When reading from database: DateTime -> DateTimeOffset
                v => new DateTimeOffset(v)
            );
        
        // Configure LastModificationTime field (nullable type)
        entity.Property(e => e.LastModificationTime)
            .HasColumnType("datetime")
            .HasConversion(
                // When saving to database: DateTimeOffset? -> DateTime?
                v => v.HasValue ? v.Value.ToDateTime() : (DateTime?)null,
                // When reading from database: DateTime? -> DateTimeOffset?
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null
            );
        
        // Configure DeletionTime field (if using FullAuditEntity)
        entity.Property(e => e.DeletionTime)
            .HasColumnType("datetime")
            .HasConversion(
                v => v.HasValue ? v.Value.ToDateTime() : (DateTime?)null,
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null
            );
        
        // Configure audit user fields
        entity.Property(e => e.CreatorId)
            .HasMaxLength(30)
            .IsUnicode(false);
        
        entity.Property(e => e.LastModifierId)
            .HasMaxLength(30)
            .IsUnicode(false);
        
        entity.Property(e => e.DeleterId)
            .HasMaxLength(30)
            .IsUnicode(false);

        OnConfigurePartial(entity);
    }
    
    partial void OnConfigurePartial(EntityTypeBuilder<User> entity);
}
```

**Important Notes**:
- Timezone information will be lost during conversion, recommend using UTC time consistently in your application
- `TimeSpan.Zero` represents UTC timezone offset
- Ensure all times stored in the database are in UTC to avoid timezone confusion

## � Class Diagram Overview

Relationships between main classes and interfaces:

```
BaseEntity<T>
    |
    ├─ CreationAuditEntity<T>
    |      |
    |      ├─ AuditEntity<T>
    |      |      |
    |      |      └─ FullAuditEntity<T>
    |      |
    |      └─ [Custom Entity]
    |
    └─ [Custom Entity]
```

## 📋 Interface and Base Class Reference

### IEntity\<T\> Interface

Defines an entity with typed ID:

```csharp
public interface IEntity<T> : IEntity
{
    T Id { get; set; }
}
```

### ISoftDelete Interface

Enables soft delete functionality:

```csharp
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
```

### BaseEntity Class

Base class for entities:

```csharp
public abstract class BaseEntity<T> : IEntity<T>
{
    public T Id { get; set; } = default!;
}
```

### CreationAuditEntity Class

Base class that tracks creation information:

```csharp
public abstract class CreationAuditEntity : ICreationAuditEntity
{
    public string? CreatorId { get; set; }
    public DateTimeOffset CreationTime { get; set; }
}
```

### AuditEntity Class

Base class that tracks creation and modification information:

```csharp
public abstract class AuditEntity : CreationAuditEntity, IModificationAuditEntity
{
    public string? LastModifierId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
}
```

### FullAuditEntity Class

Base class that tracks creation, modification, and deletion information:

```csharp
public abstract class FullAuditEntity : AuditEntity, IDeletionAuditEntity, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public string? DeleterId { get; set; }
    public DateTimeOffset? DeletionTime { get; set; }
}
```

## � License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

