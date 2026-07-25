# Linger.Reflection

`Linger.Reflection` contains the optional runtime metadata and expression-tree features extracted from `Linger.Utils`.

> Upgrading from 1.x? See the [2.0 migration guide](../Linger/MIGRATION.md).

## Installation

```bash
dotnet add package Linger.Reflection
```

The package references `Linger.Utils`; applications normally reference only `Linger.Reflection` when they need these APIs.

## Target Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## Included Features

- Expression composition and predicate helpers
- Dynamic `IQueryable` ordering by property name
- Runtime property inspection and enumeration
- Enum `DescriptionAttribute` and `DisplayAttribute` access
- Reflection-based `IEnumerable<T>.ToDataTable()` conversion
- Reflection-based `DataTable.ToList<T>()` conversion

## Usage

```csharp
using System.Data;
using System.Linq.Expressions;
using Linger.Extensions;
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;

Expression<Func<User, bool>> activeUsers = x => x.IsActive;
Expression<Func<User, bool>> adults = x => x.Age >= 18;
Expression<Func<User, bool>> filter = activeUsers.And(adults);

IQueryable<User> ordered = users.AsQueryable().CreateOrderBy(nameof(User.Name));

string? name = user.GetPropertyValue(nameof(User.Name))?.ToString();
string description = Status.Active.GetDescription();

DataTable table = users.ToDataTable();
List<User>? mappedUsers = table.ToList<User>();
```

## AOT And Trimming

This package discovers members at runtime and is not suitable for Native AOT or trimming-sensitive paths. Use the explicit mapper and factory overloads in `Linger.Utils` for DataTable conversion, and write compile-time projections for other metadata-dependent operations.

## Dependency Direction

`Linger.Reflection` depends on `Linger.Utils`. `Linger.Utils` does not depend on `Linger.Reflection`.
