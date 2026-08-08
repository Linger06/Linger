# Linger.Utils

[Migration guide for 2.0.0-preview.1](https://github.com/Linger06/Linger/blob/v2.0.0-preview.1/src/Linger/MIGRATION.md)

A comprehensive .NET utility library providing AOT-friendly extension methods and helper classes for everyday development tasks. Reflection, expression-tree, property metadata, and dynamic query APIs are provided by the optional `Linger.Reflection` package.

## Overview

Linger.Utils offers a rich collection of extension methods and helper classes that make common programming tasks simpler and more efficient. The library follows modern C# coding practices, emphasizes **strict type safety**, and supports multiple .NET framework versions.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Target Frameworks](#target-frameworks)
- [Quick Start](#quick-start)
  - [String Extensions](#string-extensions)
  - [String Cryptography Extensions](#string-cryptography-extensions)
  - [DateTime Extensions](#datetime-extensions)
  - [File Operations](#file-operations)
  - [Collection Extensions](#collection-extensions)
    - [DataTable Extensions (AOT-Friendly)](#datatable-extensions-aot-friendly)
  - [Object Extensions](#object-extensions)
  - [Optional JSON Package](#optional-json-package)
  - [GUID Extensions](#guid-extensions)
  - [Array Extensions](#array-extensions)
  - [Enum Extensions](#enum-extensions)
  - [Parameter Validation](#parameter-validation)
- [Optional Reflection Features](#optional-reflection-features)
- [Advanced Features](#advanced-features)
  - [Retry Helper](#retry-helper)
  - [Path Operations](#path-operations)
- [Best Practices](#best-practices)
- [API Standardization & Type Safety](#api-standardization--type-safety)
  - [.NET 10 Compatibility (Now Supported)](#net-10-compatibility-now-supported)
  - [Strict Type Safety Principles](#strict-type-safety-principles)

## Features

### Core Extensions
- **String Extensions**: Rich string operations, validation, conversion, and formatting utilities
- **String Cryptography Extensions**: Secure AES encryption/decryption functionality for data protection
- **DateTime Extensions**: Date and time manipulation, formatting, and calculations
- **Numeric Extensions**: Type-safe numeric conversions with **strict type safety principles**, **complete support for all .NET basic numeric types**

    Note: Decimal-to-int conversions were enhanced. `DecimalExtensions` now provides `ToIntOrNull`, `ToIntOrDefault` and `TryToInt` (including nullable overloads). These methods return a value only when the decimal has no fractional part (fractional == 0) and the numeric value fits in `Int32`. Example: `(decimal)1.0000m.ToIntOrDefault() => 1`.

    Also, object/string conversion paths now accept strings like "1.00000" as integer-compatible and will convert them to `1` when appropriate.
- **Enum Extensions**: Enum parsing, validation, and value conversion
- **Object Extensions**: General object operations and validation, **enhanced with complete numeric type support**
- **Array Extensions**: Array processing and manipulation utilities
- **GUID Extensions**: GUID operation and validation utilities

### Collection Extensions
- **List Extensions**: Enhanced list operations and processing
- **Collection Extensions**: General collection utilities and transformations

### Data Extensions
- **DataTable Extensions**: DataTable operation utilities
- **Data Conversion**: Safe data type conversion and transformation

### File System Operations
- **File Helper**: Comprehensive file operations (read, write, copy, move, delete)
- **Path Helper**: Cross-platform path operations and validation
- **Directory Operations**: Directory management and traversal utilities

### Helper Classes
- **Retry Helper**: Robust retry mechanisms for operations
- **GUID Code**: Enhanced GUID generation and operations
- **OS Platform Helper**: Cross-platform operating system detection
- **Parameter Validation Extensions**: Defensive programming and input validation utilities

### Optional Reflection Package
- **`Linger.Reflection`**: Expression composition, dynamic `IQueryable` ordering, runtime property access, enum metadata, and reflection-based DataTable conversion

### Optional JSON Package
- **`Linger.Json`**: `System.Text.Json` converters, `DataTable` JSON conversion, and `JsonDefaults` option factories. See the [Linger.Json documentation](../Linger.Json/README.md).

## Installation

```bash
dotnet add package Linger.Utils
```

Install `Linger.Reflection` only when the application needs runtime metadata or expression-tree APIs:

```bash
dotnet add package Linger.Reflection
```

`Linger.Reflection` depends on `Linger.Utils`. Keep applications that use trimming or Native AOT on the `Linger.Utils` mapper/factory APIs.

## Target Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## Quick Start

### String Extensions

```csharp
using Linger.Extensions.Core;

// String validation
string email = "user@example.com";
bool isValid = email.IsEmail();

// String conversion
string number = "123";
int result = number.ToIntOrDefault(0); // Returns 123, or 0 if conversion fails
int? nullableResult = number.ToIntOrNull(); // Returns nullable type

// String extraction
string text = "Hello World";
string leftPart = text.Take(5); // Get left 5 characters: Hello
string rightPart = text.TakeLast(5); // Get right 5 characters: World
string part = text.Truncate(20); // Won't throw if length exceeds

// String checks
bool isEmpty = text.IsNullOrEmpty();
bool isNumber = number.IsNumber(); // Check if it's a number
bool isInt = number.IsInteger(); // Check if it's an integer

// Literal delimiters
string[] columns = "id,name,email".SplitToArray(',');
IEnumerable<string> lines = "first\r\nsecond".SplitToList("\r\n");
```

Use `SplitToArray` / `SplitToList` for literal delimiters. `ToSplitList(string)` is retained only for regular-expression delimiters and enforces a one-second match timeout.

### String Cryptography Extensions

```csharp
using Linger.Extensions.Core;

string data = "Sensitive data to encrypt";
string aesKey = "mySecretKey12345"; // AES key

// Authenticated encryption/decryption (recommended)
string aesEncrypted = data.AesEncryptAuthenticated(aesKey);
string aesDecrypted = aesEncrypted.AesDecryptAuthenticated(aesKey);

// Security Features:
// - PBKDF2 derives separate encryption and authentication keys
// - Random salt and IV produce different ciphertext for the same plaintext
// - HMAC-SHA256 authenticates the encrypted payload
// ⚠️ Store keys securely, use professional key management solutions in production
```

### DateTime Extensions

```csharp
using Linger.Extensions.Core;

DateTime date = DateTime.Now;

// Age calculation
DateTime birthDate = new DateTime(1990, 5, 15);
int age = birthDate.CalculateAge();

// Date range operations
bool isInRange = date.InRange(DateTime.Today, DateTime.Today.AddDays(7));

// Date operations
DateTime startOfDay = date.StartOfDay(); // Beginning of the day
DateTime endOfDay = date.EndOfDay(); // End of the day
DateTime startOfMonth = date.StartOfMonth(); // Beginning of the month
DateTime endOfMonth = date.EndOfMonth(); // End of the month
```

### File Operations

```csharp
using Linger.Helper;

// Create and write a text file
FileHelper.CreateFile("data.txt", content: "Hello World");

// Try-style file reading (won't throw if file doesn't exist)
if (FileHelper.TryReadText("data.txt", out string content))
{
    Console.WriteLine(content);
}

// File copy with directory creation
FileHelper.CopyFile("source.txt", "backup/dest.txt");

// Safe file deletion
FileHelper.DeleteFileIfExists("temp.txt");

// Directory operations
FileHelper.CopyDir("sourceFolder", "backupFolder"); // Recursive copy
FileHelper.ClearDirectory("temp"); // Clear all files and subdirectories
```

### Collection Extensions

```csharp
using Linger.Extensions.Collection;

var list = new List<int> { 1, 2, 3, 4, 5 };

// Safe collection state checking
bool isEmpty = list.IsNullOrEmpty(); // Check if null or empty

// Pagination
var pagedResult = list.Paging(2, 2); // Page 2, 2 items per page: [3, 4]

// Convert to delimited string
string result = list.ToSeparatedString(", "); // "1, 2, 3, 4, 5"

// Execute action on each element
list.ForEach(Console.WriteLine); // Print each element

// For reflection-based IEnumerable<T>.ToDataTable(), install Linger.Reflection.

// .NET 10+ Join compatibility
// LeftJoin/RightJoin use polyfills on older targets and framework implementations on .NET 10; FullJoin is always provided by Linger

// Left Join (Left Outer Join) - Keep all left-side records
var employees = new List<Employee>
{
    new Employee { Id = 1, Name = "John", DeptId = 1 },
    new Employee { Id = 2, Name = "Jane", DeptId = 2 },
    new Employee { Id = 3, Name = "Bob", DeptId = 99 } // No matching department
};

var departments = new List<Department>
{
    new Department { Id = 1, Name = "Development" },
    new Department { Id = 2, Name = "Testing" }
};

var leftJoinResult = employees.LeftJoin(
    departments,
    emp => emp.DeptId,           // Outer key selector
    dept => dept.Id,             // Inner key selector
    (emp, dept) => new {
        Employee = emp.Name,
        Department = dept?.Name ?? "No Department"
    }
);
// Output: John-Development, Jane-Testing, Bob-No Department

// Right Join (Right Outer Join) - Keep all right-side records
var rightJoinResult = employees.RightJoin(
    departments, emp => emp.DeptId, dept => dept.Id,
    (emp, dept) => new { Employee = emp?.Name ?? "No Employee", Department = dept.Name }
);

// Full Join (Full Outer Join) - Keep all records
var fullJoinResult = employees.FullJoin(
    departments, emp => emp.DeptId, dept => dept.Id,
    (emp, dept) => new { Employee = emp?.Name ?? "No Employee", Department = dept?.Name ?? "No Department" }
);

// Simplified version: Returns tuples
var tupleResult = employees.LeftJoin(departments, e => e.DeptId, d => d.Id);
// Returns IEnumerable<Tuple<Employee, Department?>>

// Support for custom equality comparers
var caseInsensitiveJoin = stringList1.LeftJoin(
    stringList2, s => s, s => s,
    (s1, s2) => new { Left = s1, Right = s2 },
    StringComparer.OrdinalIgnoreCase
);

// The tuple convenience overload and FullJoin are available from Linger on every target framework
```

### DataTable Extensions (AOT-Friendly)

```csharp
using Linger.Extensions.Data;

// Reflection-free mapping for AOT/trim scenarios
DataTable? table = GetDataTable();

List<UserDto>? users = table.ToList(row => new UserDto
{
    Id = Convert.ToInt32(row["Id"]),
    Name = row["Name"]?.ToString()
});

// Property-mapping style (still reflection-free)
List<UserDto>? usersBySetters = table.ToList(
    () => new UserDto(),
    new Dictionary<string, Action<UserDto, object?>>
    {
        ["Id"] = Linger.Extensions.Data.DataTableExtensions.CreateColumnSetter<UserDto, int>((x, v) => x.Id = v),
        ["Name"] = Linger.Extensions.Data.DataTableExtensions.CreateColumnSetter<UserDto, string?>((x, v) => x.Name = v)
    });

// Reflection-based mapping is provided by the optional Linger.Reflection package:
// var mappedByProperties = table.ToList<UserDto>();
```

### Object Extensions

```csharp
using Linger.Extensions.Core;

// Type-safe object conversion
object stringObj = "123";
int intValue = stringObj.ToIntOrDefault(0);           // Success: 123
long longValue = stringObj.ToLongOrDefault(0L);       // Success: 123
decimal decimalValue = stringObj.ToDecimalOrDefault(0m); // Success: 123

// Common numeric objects also go through safe conversion paths
object numberObj = 123.00m;
int convertedInt = numberObj.ToIntOrDefault(0);       // Success: 123

// Default values are returned only when the value cannot be converted losslessly
object fractionalNumberObj = 123.45;
int invalidInt = fractionalNumberObj.ToIntOrDefault(0); // Returns 0 because the value is not a whole number
```

**Built-in Common Conversion Methods**

| Method | Target Type | Notes |
|--------|-------------|-------|
| `ToShortOrDefault` | `short` | Supports strings, common numeric objects, and whole-number `decimal/double/float` values |
| `ToIntOrDefault` | `int` | Supports strings, common numeric objects, and whole-number `decimal/double/float` values |
| `ToLongOrDefault` | `long` | Supports strings, common numeric objects, and whole-number `decimal/double/float` values |
| `ToDecimalOrDefault` | `decimal` | Supports strings and common numeric objects |
| `ToDateTimeOrDefault` | `DateTime` | Supports strings, `DateTime`, and `DateTimeOffset` |
| `ToBoolOrDefault` | `bool` | Supports strings and `0/1` numeric semantics |
| `ToGuidOrDefault` | `Guid` | Supports strings, `Guid`, and 16-byte arrays |

```csharp
// Other type conversions
object dateObj = "2025-01-01";
DateTime dateValue = dateObj.ToDateTimeOrDefault(DateTime.MinValue);

object guidObj = "550e8400-e29b-41d4-a716-446655440000";
Guid guidValue = guidObj.ToGuidOrDefault();

object boolObj = "true";
bool boolValue = boolObj.ToBoolOrDefault(false);

// Null-safe operations
object obj = GetSomeObject();
string result = obj.ToStringOrDefault("default"); // Returns default when null

// Type checking method
object testObj = (byte)255;
bool isNumeric = testObj.IsNumeric();                // Check if any numeric type

// Performance-optimized Try-style conversion - avoid default value masking failure
if ("123".TryToInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"bad data".TryToDecimal(out var decVal)) { /* decVal = 0, conversion failed */ }

// Ensure prefix/suffix (idempotent, won't duplicate)
var apiUrl = "api/v1".EnsureStartsWith("/"); // => "/api/v1"
var folder = "logs".EnsureEndsWith("/");     // => "logs/"

// Range checking (for numeric values)
int value = 5;
bool inRange = value.InRange(1, 10); // Check if in range 1 to 10
```

### Optional JSON Package

Install `Linger.Json` for JSON converters, `DataTable` JSON extensions, and `JsonDefaults`. Its APIs are intentionally separate from `Linger.Utils`; see the [Linger.Json documentation](../Linger.Json/README.md) and [migration guide](../Linger.Json/MIGRATION.md).

### GUID Extensions

```csharp
using Linger.Extensions.Core;
using Linger.Helper;

// GUID checking
Guid guid = Guid.NewGuid();
bool isEmpty = guid.IsEmpty(); // Check if empty GUID
bool isNotEmpty = guid.IsNotEmpty(); // Check if not empty

// Nullable GUID operations
Guid? nullableGuid = null;
bool isNull = nullableGuid.IsNull(); // Check if null
bool isNotNull = nullableGuid.IsNotNull(); // Check if not null
bool isNullOrEmpty = nullableGuid.IsNullOrEmpty(); // Check if null or empty
bool isNotNullOrEmpty = nullableGuid.IsNotNullOrEmpty(); // Check if neither null nor empty

// GUID conversion
long longValue = guid.ToInt64(); // Convert to Int64
int intValue = guid.ToInt32(); // Convert to Int32

// GuidCode helper class - Generate a date/time-prefixed identifier
string id = GuidCode.NewId;

// .NET 9+ feature: V7 GUID generation and timestamp extraction
#if NET9_0_OR_GREATER
Guid v7Guid = Guid.CreateVersion7(); // Create V7 GUID
DateTimeOffset timestamp = v7Guid.GetTimestamp(); // Extract timestamp from V7 GUID
#endif
```

### Array Extensions

```csharp
using Linger.Extensions.Core;

int[] numbers = { 1, 2, 3, 4, 5 };

// Execute action on each element
numbers.ForEach(n => Console.WriteLine(n)); // Output: 1 2 3 4 5

// Iterate with index
numbers.ForEach((n, index) => Console.WriteLine($"Index {index}: {n}"));
// Output: Index 0: 1, Index 1: 2, ...
```

### Enum Extensions

```csharp
using Linger.Extensions.Core;

public enum Status
{
    Active = 1,
    Inactive = 2,
    Pending = 3
}

// String to enum
string statusName = "Active";
Status status = statusName.GetEnum<Status>();

// Integer to enum
int statusValue = 1;
Status statusFromInt = statusValue.GetEnum<Status>();

// Get enum name
string enumName = statusValue.GetEnumName<Status>(); // Returns "Active"

// GetDescription() and GetDisplay() read enum attributes and are provided by Linger.Reflection.
```

### Parameter Validation

```csharp
public void ProcessData(string data, IEnumerable<int> numbers)
{
    // Newer frameworks use the built-in BCL APIs directly
    // Older target frameworks get compatible polyfills from Linger automatically
    ArgumentNullException.ThrowIfNull(data);                    // Ensure not null
    ArgumentException.ThrowIfNullOrEmpty(data);                 // Ensure not null or empty string
    ArgumentException.ThrowIfNullOrWhiteSpace(data);            // Ensure not null, empty or whitespace
    ArgumentNullException.ThrowIfNull(numbers);                 // Ensure collection is not null

    // Framework support:
    // - ThrowIfNull: built-in on .NET 6+, polyfilled by Linger on .NET 5 and below
    // - ThrowIfNullOrEmpty / ThrowIfNullOrWhiteSpace: built-in on .NET 8+, polyfilled by Linger on .NET 7 and below
}
```

## Optional Reflection Features

Install `Linger.Reflection` for runtime property access, `IEnumerable<T>.ToDataTable()`, enum descriptions/display names, expression composition, and dynamic `IQueryable` ordering. It has a one-way dependency on `Linger.Utils` and should not be used in trimmed or Native AOT applications.

## Advanced Features

### Retry Helper

```csharp
using Linger.Helper;

// Retry operation with configurable policy
var options = new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 1000, // 1 second
    MaxDelayMilliseconds = 5000,
    UseExponentialBackoff = true,
    Jitter = 0.2
};
var retryHelper = new RetryHelper(options);
CancellationToken cancellationToken = GetCancellationToken();
var result = await retryHelper.ExecuteAsync(
    ct => SomeOperationThatMightFail(ct),
    "Operation Name",
    shouldRetry: exception => exception is HttpRequestException or TimeoutException,
    cancellationToken: cancellationToken
);

// Or use default delay options with an explicit exception policy
var defaultRetryHelper = new RetryHelper();
var result2 = await defaultRetryHelper.ExecuteAsync(
    ct => AnotherOperationThatMightFail(ct),
    "Another Operation Name",
    shouldRetry: exception => exception is TimeoutException,
    cancellationToken: cancellationToken
);

// Synchronous variant
defaultRetryHelper.Execute(
    () => DoSomething(),
    shouldRetry: exception => exception is IOException);
```

When `shouldRetry` is omitted, the original exception is rethrown without retrying.

### Path Operations

```csharp
using Linger.Extensions.IO;
using Linger.Helper;

// Path normalization - handles duplicate separators and trailing separator policy
string messyPath = @"temp\\folder//file.txt\";
string normalized = PathHelper.CleanAndNormalizePureString(messyPath, preserveEndingSeparator: false);
// Result: duplicate separators are removed and trailing separators follow the selected policy

// Check for invalid path characters
string suspiciousPath = "file<name>.txt";
bool hasInvalidChars = suspiciousPath.ContainsInvalidPathChars(); // Windows: true; Unix: false

// Check if file or directory exists
string filePath = @"C:\temp\data.txt";
bool fileExists = PathExtensions.Exists(filePath, checkAsFile: true); // Check as file
bool dirExists = PathExtensions.Exists(filePath, checkAsFile: false); // Check as directory
```

## API Standardization & Type Safety

### .NET 10 Compatibility (Now Supported)

`LeftJoin` and `RightJoin` use the framework implementations on .NET 10 and conditional polyfills on older targets. The tuple-returning `LeftJoin` convenience overload and `FullJoin` remain Linger APIs on every supported target framework.

### Strict Type Safety Principles

**Conversion Strategy** (Conservative and optimized for common paths):
1. Prefer direct type matches and common numeric branches to avoid unnecessary string parsing
2. For integer targets, accept only lossless numeric conversions
3. Fall back to `ToString()` parsing only when the earlier paths do not apply

```csharp
object intObj = 123;
int result = intObj.ToIntOrDefault(0);     // Direct match, no string parsing needed
object doubleObj = 123.45;
int failed = doubleObj.ToIntOrDefault(0);  // Returns 0 because the value is not a whole number
```

**Current Core Numeric Conversion Support**: `short`, `int`, `long`, and `decimal`, plus common object/string conversions for `DateTime`, `bool`, and `Guid`

### Performance Benefits

- Fast paths for same-type and common numeric conversions without unnecessary string parsing
- `TryToXxx()` / `ToXxxOrDefault()` avoid using exceptions for routine conversion failures
- Smart fallback strategy, with string conversion only when needed
- Unified API naming pattern

## Best Practices

1. **Type Conversion**: Use `ToXxx()` when the input must be valid and failures should surface immediately; use `ToXxxOrNull()` when an empty result is acceptable; use `ToXxxOrDefault()` when you want a fallback value; use `TryToXxx()` when you need an explicit success/failure check
2. **Null Checking**: Leverage `IsNullOrEmpty()`, `EnsureIsNotNull()` and other extension methods
3. **Async Operations**: Use async versions for I/O-intensive tasks (file, network)
4. **Exception Handling**: Use `RetryHelper` for unstable operations, handle exceptions properly with user feedback
5. **Resource Management**: Use `using` statements to ensure proper resource disposal

## Polyfills Summary

Provides forward-compatible Polyfills for BCL APIs & language features (for .NET Framework/Standard 2.0/legacy versions). Uses conditional compilation to automatically defer to framework built-ins when upgraded. Implemented using C# 14 extension members syntax for cleaner code.

| Category | Content | Source Location |
|----------|---------|-----------------|
| **Parameter Validation** | `ArgumentNullException.ThrowIfNull` (pre-.NET 6)<br>`ArgumentException.ThrowIfNullOrEmpty/WhiteSpace` (pre-.NET 8)<br>`ArgumentOutOfRangeException.ThrowIfNegative/Zero/...` (pre-.NET 8) | `Polyfills/ArgumentNullException.cs`<br>`Polyfills/ArgumentException.cs`<br>`Polyfills/ArgumentOutOfRangeException.cs` |
| **Hash Algorithms** | `MD5.HashData`, `SHA256.HashData`, `SHA384.HashData`, `SHA512.HashData` (pre-.NET 5) | `Polyfills/HashAlgorithm.cs` |
| **Conversion Utilities** | `Convert.ToHexStringLower` (pre-.NET 9) | `Polyfills/Convert.cs` |
| **Language Features** | `required` keyword support (C# 11)<br>`RequiredMemberAttribute`, `SetsRequiredMembersAttribute`, `CompilerFeatureRequiredAttribute` | `Polyfills/RequiredMemberAttribute.cs`<br>`Polyfills/SetsRequiredMembersAttribute.cs`<br>`Polyfills/CompilerFeatureRequiredAttribute.cs` |
| **Nullability Attributes** | 11 attributes: `AllowNull`, `NotNull`, `MaybeNullWhen`, `NotNullIfNotNull`, etc. | `Polyfills/NullableAttributes.cs` |
| **Collection Extensions** | `LeftJoin`, `RightJoin` (polyfilled before .NET 10); tuple `LeftJoin` and `FullJoin` (Linger extensions) | `Extensions/Collection/IEnumerableExtensions.Polyfills.cs`<br>`Extensions/Collection/IEnumerableExtensions.cs` |
| **Caller Capture** | `CallerArgumentExpressionAttribute` (improves Guard experience) | `Polyfills/CallerArgumentExpressionAttribute.cs` |

## Dependencies

The library has minimal external dependencies:
- System.Data.DataSetExtensions (for .NET Framework and .NET Standard 2.0)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. Please ensure:
- Follow existing code style
- Add unit tests for new features
- Update documentation as needed

## License

This project is licensed under the terms of the license provided with the Linger project.

---

For more information about the Linger framework and other related packages, visit the [Linger Project Repository](https://github.com/Linger06/Linger).

