# Linger.Utils

[Migration guide for 2.0.0-preview.4](https://github.com/Linger06/Linger/blob/v2.0.0-preview.4/src/Linger/MIGRATION.md)

[![NuGet](https://img.shields.io/nuget/v/Linger.Utils.svg)](https://www.nuget.org/packages/Linger.Utils/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-11.0%20%7C%2010.0%20%7C%209.0%20%7C%208.0%20%7C%20Standard%202.0-512BD4)](https://dotnet.microsoft.com/)

`Linger.Utils` is a .NET utility library for everyday development. It provides extension methods and helper classes for strings, numbers, dates, collections, files, DataTables, encryption, and argument validation. The core API stays lightweight and supports multiple target frameworks; runtime reflection and expression-tree features are provided by the optional `Linger.Reflection` package.

For complete examples, behavior rules, and compatibility notes, see the [detailed usage guide](docs/usage.md).

## Feature overview

- **Strings and type conversion**: Validation, splitting, truncation, formatting, and predictable conversion of numbers and common types
- **Date and time**: Age calculation, range checks, date boundaries, and type conversion
- **Collections and data**: Paging, iteration, joins, DataTable mapping, and delimited strings
- **Files and paths**: File and directory operations, copying, moving, deletion, and cross-platform path handling
- **Security and reliability**: Authenticated encryption, retry policies, GUID utilities, and argument validation
- **Compatibility**: Selected BCL APIs for older target frameworks and explicit mapping APIs for AOT-friendly code

## Installation

```bash
dotnet add package Linger.Utils --version 2.0.0-preview.4
```

Supported target frameworks:

- .NET 11.0
- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## Quick start

### Strings and type conversion

```csharp
using Linger.Extensions.Core;

string value = "123";
int number = value.ToIntOrDefault();
int? nullableNumber = value.ToIntOrNull();
bool isEmail = "user@example.com".IsEmail();

string[] columns = "id,name,email".SplitToArray(',');
string shortened = "a long piece of text".Truncate(10);
```

`ToIntOrDefault`, `ToIntOrNull`, and `TryToInt` use a conservative decimal-to-integer policy: conversion succeeds only when the fractional part is zero and the value is within the target type's range. Use `ToSplitList(string)` for regular-expression delimiters; prefer `SplitToArray` or `SplitToList` for literal delimiters.

### Dates, GUIDs, and enums

```csharp
using Linger.Extensions.Core;

DateTime birthDate = new(1990, 5, 15);
int age = birthDate.CalculateAge();
bool inRange = DateTime.Now.InRange(DateTime.Today, DateTime.Today.AddDays(7));

Guid? id = null;
bool isEmpty = id.IsNullOrEmpty();
string generatedId = GuidCode.NewId;

enum Status
{
    Active
}

Status status = "Active".GetEnum<Status>();
```

Use `StartOfDay`, `EndOfDay`, `StartOfMonth`, and `EndOfMonth` for date boundaries. `DateTime` extensions also provide conversions among `DateOnly`, `TimeOnly`, and `DateTime`.

### Files and paths

```csharp
using Linger.Extensions.IO;
using Linger.Helper;

FileHelper.CreateFile("data.txt", "Hello World");
if (FileHelper.TryReadText("data.txt", out string content))
{
    Console.WriteLine(content);
}

FileHelper.CopyFile("source.txt", "backup/dest.txt");
FileHelper.DeleteFileIfExists("temp.txt");
FileHelper.CopyDir("sourceFolder", "backupFolder");
FileHelper.ClearDirectory("temp");

string normalized = PathHelper.CleanAndNormalizePureString(
    @"temp\\folder//file.txt\\",
    preserveEndingSeparator: false);
bool hasInvalidChars = "file<name>.txt".ContainsInvalidPathChars();
```

### Collections and joins

```csharp
using Linger.Extensions.Collection;

var values = new List<int> { 1, 2, 3, 4, 5 };
var page = values.Paging(pageIndex: 2, pageSize: 2);
string text = values.ToSeparatedString(", ");
values.ForEach(Console.WriteLine);

var left = employees.LeftJoin(
    departments,
    employee => employee.DepartmentId,
    department => department.Id,
    (employee, department) => new
    {
        Employee = employee.Name,
        Department = department?.Name ?? "No department"
    });
```

`LeftJoin`, `RightJoin`, and `FullJoin` use BCL implementations on newer target frameworks and conditional compatibility implementations on older targets. The tuple-returning `LeftJoin` overload remains provided by Linger.

### DataTable mapping

```csharp
using Linger.Extensions.Data;

DataTable? table = GetDataTable();
List<UserDto>? users = table.ToList(row => new UserDto
{
    Id = Convert.ToInt32(row["Id"]),
    Name = row["Name"]?.ToString()
});
```

The explicit mapper above does not use reflection and is suitable for trimming and Native AOT. Property-based automatic mapping is provided by `Linger.Reflection`.

### Objects, arrays, and enums

```csharp
using Linger.Extensions.Core;

object value = "123";
int number = value.ToIntOrDefault();

int[] numbers = new[] { 1, 2, 3 };
numbers.ForEach((item, index) => Console.WriteLine($"{index}: {item}"));

Status status = 1.GetEnum<Status>();
string name = 1.GetEnumName<Status>();
```

Reading `DescriptionAttribute` and `DisplayAttribute` belongs to `Linger.Reflection`; basic enum parsing and numeric conversion are provided by `Linger.Utils`.

### Authenticated encryption

```csharp
using Linger.Extensions.Core;

string plaintext = "Sensitive data";
string encrypted = plaintext.AesEncryptAuthenticated(key);
string decrypted = encrypted.AesDecryptAuthenticated(key);
```

Authenticated encryption uses a random salt and IV and verifies ciphertext integrity with HMAC-SHA256. Store production keys in a dedicated key-management service; legacy ciphertext can still be read with the obsolete `AesDecrypt`, which is intended only for migration.

### Retries and cancellation

```csharp
using Linger.Helper;

var options = new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 1000,
    UseExponentialBackoff = true,
    MaxDelayMilliseconds = 30000
};

var retry = new RetryHelper(options);
var result = await retry.ExecuteAsync(
    ct => SendRequestAsync(ct),
    "Network request",
    shouldRetry: exception => exception is HttpRequestException or TimeoutException,
    cancellationToken: cancellationToken);
```

Pass a `CancellationToken` through asynchronous operations. Retryable exceptions must be selected explicitly with `shouldRetry`; omitting the predicate does not retry exceptions automatically.

### Argument validation

```csharp
public void ProcessData(string data, IEnumerable<int> values)
{
    ArgumentNullException.ThrowIfNull(data);
    ArgumentException.ThrowIfNullOrWhiteSpace(data);
    ArgumentNullException.ThrowIfNull(values);
}
```

`Linger.Utils` uses these BCL APIs internally on every target framework. Its compatibility copies for .NET Standard 2.0 and .NET Framework 4.7.2 are `internal` and are not exposed, so code that targets those frameworks must supply its own polyfills or use `GuardExtensions`.

## Feature index

| Feature | Main APIs or types |
| --- | --- |
| Strings | `StringExtensions`, `SplitToArray`, `SplitToList`, `ToSplitList` |
| Numeric and object conversion | `ToXxx`, `ToXxxOrNull`, `ToXxxOrDefault`, `TryToXxx` |
| Date and time | `DateTimeExtensions` (including `DateOnly` and `TimeOnly` conversion) |
| Collections and arrays | `IEnumerableExtensions`, `ListExtensions`, `ArrayExtensions` |
| DataTable | Explicit mapper and factory APIs in `DataTableExtensions` |
| Files and paths | `FileHelper`, `PathHelper`, `PathExtensions` |
| Encryption | `AesEncryptAuthenticated`, `AesDecryptAuthenticated` |
| GUIDs and enums | `GuidCode`, GUID extensions, `EnumExtensions` |
| Retries | `RetryHelper`, `RetryOptions` |
| Argument validation | `ThrowIfNull`, `ThrowIfNullOrEmpty`, `ThrowIfNullOrWhiteSpace` |

## Optional features

### `Linger.Reflection`

Install it when runtime metadata or expression trees are required:

```bash
dotnet add package Linger.Reflection --version 2.0.0-preview.4
```

The package provides expression composition, dynamic `IQueryable` ordering, runtime property access, enum attribute reading, and reflection-based `IEnumerable<T>.ToDataTable()` and `DataTable.ToList<T>()`. These APIs are not suitable for Native AOT or trimming-sensitive paths.

### `Linger.Json`

Install `Linger.Json` when `System.Text.Json` converters, DataTable JSON conversion, or `JsonDefaults` are required. See the [Linger.Json documentation](../Linger.Json/README.md).

## Compatibility and design conventions

- On .NET 10 and later, prefer the framework `LeftJoin` and `RightJoin` implementations; on .NET 11 and later, prefer the framework `FullJoin` implementation.
- .NET Standard 2.0 and .NET Framework 4.7.2 builds compile `internal` polyfills for argument validation, hashing, hexadecimal conversion, and nullable annotations. They are implementation details and are not exposed to consuming projects.
- For Native AOT or trimming, prefer explicit mapper/factory APIs and avoid runtime reflection.
- Numeric conversion uses lossless, low-allocation paths first; use `TryToXxx` or `ToXxxOrDefault` for ordinary failures instead of exceptions as control flow.
- Prefer asynchronous APIs for file, network, and database I/O, and forward cancellation tokens to the underlying operation.

## Dependencies

The core library keeps its dependency footprint small. `System.Data.DataSetExtensions` is used only for DataTable support on .NET Framework and .NET Standard 2.0.

## Contributing and license

Pull requests are welcome. New features should follow the existing code style and include appropriate tests and documentation.

This project is released under the MIT license.
