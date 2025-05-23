# Linger.EFCore

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A C# Entity Framework Core helper library that provides enhanced query filter capabilities and property conversion extensions for .NET 9.0 and .NET 8.0.

## Introduction

Linger.EFCore extends Entity Framework Core with powerful features including global query filters and property type conversions, making it easier to work with complex data types and filtering scenarios.

## Features

### Global Query Filters
- Apply filters automatically to entities implementing specific interfaces
- Apply filters based on property values
- Type-safe filter expressions
- Supports all Entity Framework Core query scenarios

### Property Conversions
- JSON serialization support for complex types
- Conversion for string collections
- Custom value comparers
- Flexible configuration options

## Usage Examples

### JSON Property Conversion

```csharp
public class User
{ 
    public int Id { get; set; }
    public UserSettings? Settings { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder) 
{ 
    modelBuilder.Entity<User>().Property(x => x.Settings).HasJsonConversion(); 
}
```

### String Collection Conversion
```csharp
public class Product 
{ 
    public int Id { get; set; }
    public ICollection? Tags { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder) 
{
    modelBuilder.Entity<Product>().Property(x => x.Tags).HasStringCollectionConversion(separator: ","); 
}
```

### Interface-based Filtering


```csharp
// Define your interface 
public interface ISoftDelete { bool IsDeleted { get; set; } }

// Implement the interface in your entities 
public class User : ISoftDelete 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; } 
}

// Apply the filter in your DbContext 
protected override void OnModelCreating(ModelBuilder modelBuilder) 
{ 
    // This will automatically filter out soft-deleted entities 
    modelBuilder.ApplyGlobalFilters(x => !x.IsDeleted); 
}
```


### Property-based Filtering

```csharp
// Multi-tenant filtering example 
public class ApplicationDbContext : DbContext 
{
    private readonly int _currentTenantId;
    public ApplicationDbContext(int currentTenantId)
    {
        _currentTenantId = currentTenantId;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This will automatically filter entities by tenant
        modelBuilder.ApplyGlobalFilters("TenantId", _currentTenantId);
    }
}
```

## Install

### From Visual Studio

1. Open the `Solution Explorer`.
2. Right-click on a project within your solution.
3. Click on `Manage NuGet Packages...`.
4. Click on the `Browse` tab and search for "Linger.EFCore".
5. Click on the `Linger.EFCore` package, select the appropriate version and click Install.

### Package Manager Console

```
PM> Install-Package Linger.EFCore
```

### .NET CLI Console

```
> dotnet add package Linger.EFCore
```

