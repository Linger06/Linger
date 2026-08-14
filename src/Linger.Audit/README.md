# Linger.Audit

> Breaking changes and 2.0 migration notes are documented in the [Linger migration guide](../Linger/MIGRATION.md).

A lightweight .NET auditing library that provides base classes and interfaces for entity auditing.

## 📖 Table of Contents

- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [🚀 Quick Start](#-quick-start)
  - [Basic Entities](#basic-entities)
  - [Creation Audit Entities](#creation-audit-entities)
  - [Full Audit Entities](#full-audit-entities)
- [Persistence Integration](#persistence-integration)
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
    // public string CreatorId { get; set; }
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
    // public string CreatorId { get; set; }
    // public DateTimeOffset CreationTime { get; set; }

    // Modification
    // public string? LastModifierId { get; set; }
    // public DateTimeOffset? LastModificationTime { get; set; }

    // Deletion
    // public bool? IsDeleted { get; set; }
    // public string? DeleterId { get; set; }
    // public DateTimeOffset? DeletionTime { get; set; }
}
```

## Persistence Integration

`Linger.Audit` only defines audit contracts and entity base classes. It does not hook into a persistence framework or populate fields automatically.

For EF Core, use [Linger.EFCore.Audit](../Linger.EFCore.Audit/README.md). Its `AuditEntitiesSaveChangesInterceptor` populates creation, modification, deletion, and user fields while converting deletes of `ISoftDelete` entities into updates. Configure the optional soft-delete query filter as shown in the [Linger.EFCore global query filter documentation](../Linger.EFCore/README.md#global-query-filters).

Applications using another persistence technology should populate the same contracts in their own save pipeline.

For legacy database column types and `DateTimeOffset` conversions, see the advanced configuration section in [Linger.EFCore.Audit](../Linger.EFCore.Audit/README.md).

## 📊 Class Diagram Overview

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
    bool? IsDeleted { get; set; }
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
    public string CreatorId { get; set; }
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
public abstract class FullAuditEntity : AuditEntity, ISoftDelete
{
    public bool? IsDeleted { get; set; }
    public string? DeleterId { get; set; }
    public DateTimeOffset? DeletionTime { get; set; }
}
```

## 📄 License

This project is licensed under the MIT License.

