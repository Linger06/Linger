# Linger.Audit

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

## 🏗️ Architecture

- `IAuditUserProvider` - Interface for getting current user
- `FullAuditEntity<T>` - Base class for entities with full audit trail
- `AuditableEntity` - Base class with creation/modification audit
- `ISoftDelete` - Interface for soft delete support

## 📝 License

This project is licensed under the MIT License.

