# Linger.Utils

A comprehensive .NET utility library providing extensive extension methods and helper classes for everyday development tasks.

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
  - [Object Extensions](#object-extensions)
  - [JSON Extensions](#json-extensions)
  - [GUID Extensions](#guid-extensions)
  - [Array Extensions](#array-extensions)
  - [Enum Extensions](#enum-extensions)  
  - [Parameter Validation](#parameter-validation)
- [Advanced Features](#advanced-features)
- [Best Practices](#best-practices)
- [API Standardization & Type Safety](#api-standardization--type-safety)
  - [.NET 10 Forward-Compatible Design](#net-10-forward-compatible-design)
  - [Strict Type Safety Principles](#strict-type-safety-principles)

## Features

### ?? Core Extensions
- **String Extensions**: Rich string operations, validation, conversion, and formatting utilities
- **String Cryptography Extensions**: Secure AES encryption/decryption functionality for data protection
- **DateTime Extensions**: Date and time manipulation, formatting, and calculations
- **Numeric Extensions**: Type-safe numeric conversions with **strict type safety principles**, **complete support for all .NET basic numeric types**
- **Enum Extensions**: Enhanced enum handling and conversion
- **Object Extensions**: General object operations and validation, **enhanced with complete numeric type support**
- **Array Extensions**: Array processing and manipulation utilities
- **GUID Extensions**: GUID operation and validation utilities

### ?? Collection Extensions
- **List Extensions**: Enhanced list operations and processing
- **Collection Extensions**: General collection utilities and transformations

### ?? Data Extensions
- **DataTable Extensions**: DataTable operation utilities
- **Data Conversion**: Safe data type conversion and transformation

### ?? File System Operations
- **File Helper**: Comprehensive file operations (read, write, copy, move, delete)
- **Path Helper**: Cross-platform path operations and validation
- **Directory Operations**: Directory management and traversal utilities

### ?? Helper Classes
- **Expression Helper**: Expression tree operations and utilities
- **Retry Helper**: Robust retry mechanisms for operations
- **Property Helper**: Reflection-based property operations
- **GUID Code**: Enhanced GUID generation and operations
- **OS Platform Helper**: Cross-platform operating system detection
- **Parameter Validation Extensions**: Defensive programming and input validation utilities

### ?? JSON Support
- **JSON Extensions**: Simplified JSON serialization and deserialization
- **Custom Converters**: Specialized JSON converters for complex types (DateTime, DataTable, JsonObject, etc.)
- **JSON Configuration Factory**: `JsonOptions` provides unified JSON serialization configuration
  - Detailed documentation: `Linger/Json/JsonOptions.README.md`

## Installation

```bash
dotnet add package Linger.Utils
```

## Target Frameworks

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

// String manipulation
string text = "  Hello World  ";
string cleaned = text.Trim(); // Removes whitespace from both ends (.NET native method)

// String extraction
string longText = "Hello World";
string leftPart = longText.Take(5); // Get left 5 characters: Hello
string rightPart = longText.TakeLast(5); // Get right 5 characters: World
string part = longText.Truncate(20); // Won't throw if length exceeds

// String checks
bool isEmpty = text.IsNullOrEmpty();
bool isNumber = number.IsNumber(); // Check if it's a number
bool isInt = number.IsInteger(); // Check if it's an integer
```

### String Cryptography Extensions

```csharp
using Linger.Extensions.Core;

string data = "Sensitive data to encrypt";
string aesKey = "mySecretKey12345"; // AES key

// AES Encryption/Decryption (Recommended - High Security)
string aesEncrypted = data.AesEncrypt(aesKey);    // AES-256-CBC mode, auto-generates random IV
string aesDecrypted = aesEncrypted.AesDecrypt(aesKey); // Auto-extracts IV and decrypts

// ?? Security Features:
// - Random IV per encryption, same plaintext produces different ciphertext
// - Variable key length, internally uses SHA256 to process to 32 bytes
// - IV automatically included in ciphertext, auto-extracted during decryption
// ?? Store keys securely, use professional key management solutions in production
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

// File operations
FileHelper.WriteText("data.txt", "Hello World");
string content = FileHelper.ReadText("data.txt");

// File copy with directory creation
FileHelper.CopyFile("source.txt", "backup/dest.txt");

// Safe file deletion
FileHelper.DeleteFileIfExists("temp.txt");

// Directory operations
FileHelper.EnsureDirectoryExists("logs/2024");
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

// Convert to DataTable
var dataTable = list.Select(x => new { Value = x }).ToDataTable();

// ?? .NET 10 Compatible Join Operations - Future-Ready Features Today
// ?? Note: These are polyfill implementations for .NET 10+ built-in methods

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

// ?? Simplified version: Returns tuples
var tupleResult = employees.LeftJoin(departments, e => e.DeptId, d => d.Id);
// Returns IEnumerable<Tuple<Employee, Department?>>

// ?? Support for custom equality comparers
var caseInsensitiveJoin = stringList1.LeftJoin(
    stringList2, s => s, s => s,
    (s1, s2) => new { Left = s1, Right = s2 },
    StringComparer.OrdinalIgnoreCase
);

// ?? .NET 10+ Built-in Compatible: Method signatures match the standard, no code changes required when upgrading
```

### Object Extensions

```csharp
using Linger.Extensions.Core;

// Type-safe object conversion
object stringObj = "123";
int intValue = stringObj.ToIntOrDefault(0);           // Success: 123
long longValue = stringObj.ToLongOrDefault(0L);       // Success: 123
double doubleValue = stringObj.ToDoubleOrDefault(0.0); // Success: 123.0

// Strict type safety: Non-string objects return default values
object numberObj = 123.45;
int invalidInt = numberObj.ToIntOrDefault(0);         // Returns 0 (default value)

// ?? Supported numeric type conversions

| Method | Range | Method | Range |
|--------|-------|--------|-------|
| `ToSByteOrDefault` | -128 to 127 | `ToByteOrDefault` | 0 to 255 |
| `ToShortOrDefault` | -32,768 to 32,767 | `ToUShortOrDefault` | 0 to 65,535 |
| `ToIntOrDefault` | ��2.1��10? | `ToUIntOrDefault` | 0 to 4.3��10? |
| `ToLongOrDefault` | ��9.2��101? | `ToULongOrDefault` | 0 to 1.8��101? |
| `ToFloatOrDefault` | Single precision | `ToDoubleOrDefault` | Double precision |
| `ToDecimalOrDefault` | High precision | - | - |

// Other type conversions
DateTime dateValue = stringObj.ToDateTimeOrDefault(DateTime.MinValue);
Guid guidValue = "550e8400-e29b-41d4-a716-446655440000".ToGuidOrDefault();
bool boolValue = stringObj.ToBoolOrDefault(false);

// Null-safe operations
object obj = GetSomeObject();
string result = obj.ToStringOrDefault("default"); // Returns default when null

// ?? Type checking methods (supports all numeric types)
object testObj = (byte)255;
bool isByte = testObj.IsByte();                      // Check if byte type
bool isNumeric = testObj.IsNumeric();                // Check if any numeric type
bool isUnsigned = testObj.IsAnyUnsignedInteger();    // Check if unsigned integer type

// ? Performance-optimized Try-style conversion - avoid default value masking failure
if ("123".TryToInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"bad data".TryToDecimal(out var decVal)) { /* decVal = 0, conversion failed */ }

// Try-style numeric conversions (to avoid masking failures with defaults)
if ("123".TryToInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"bad data".TryToDecimal(out var decVal)) { /* decVal = 0, conversion failed */ }

> Naming convention: All Try-style methods use `TryToXxx(out T)` and return `bool`. For string extensions the `out` parameter is a nullable value type (e.g., `out int?`), while for object extensions it is non-nullable (e.g., `out int`).

// Ensure prefix/suffix (idempotent, won't duplicate)
var apiUrl = "api/v1".EnsureStartsWith("/"); // => "/api/v1"
var folder = "logs".EnsureEndsWith("/");     // => "logs/"

// Range checking (for numeric values)
int value = 5;
bool inRange = value.InRange(1, 10); // Check if in range 1 to 10
```

### JSON Extensions

```csharp
using Linger.Extensions;
using Linger.Json;

// Object to JSON
var user = new { Name = "John", Age = 30 };
string json = user.ToJsonString(); // or user.SerializeJson()

// JSON to object
var userObj = json.Deserialize<User>(); // or json.DeserializeJson<User>()

// Dynamic JSON object
dynamic dynamicObj = json.DeserializeDynamicJsonObject();
string name = dynamicObj.Name; // Access properties

// JSON to DataTable (string extension)
string jsonArray = "[{\"Name\":\"John\",\"Age\":30}]";
DataTable? dataTable = jsonArray.ToDataTable();

// 🏭 Use JsonOptions for unified configuration
var responseOptions = JsonOptions.CreateResponseOptions();  // HTTP responses
var requestOptions = JsonOptions.CreateRequestOptions();    // HTTP requests

// Apply configuration in WebAPI
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        JsonOptions.ApplyDefaultConfiguration(options.JsonSerializerOptions));

// 💡 For detailed configuration documentation, see: Linger/Json/JsonOptions.README.md
```

### GUID Extensions

```csharp
using Linger.Extensions.Core;

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

// .NET 9+ feature: V7 GUID timestamp extraction
#if NET9_0_OR_GREATER
DateTimeOffset timestamp = guid.GetTimestamp(); // Only for V7 GUIDs
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
Status status = statusName.GetEnum<Status>(); // or statusName.ToEnum<Status>()

// Integer to enum
int statusValue = 1;
Status statusFromInt = statusValue.GetEnum<Status>();

// Get enum name
string enumName = statusValue.GetEnumName<Status>(); // Returns "Active"

// Get enum description (if Description attribute exists)
string description = status.GetDescription(); // Get description text
```

### Parameter Validation

```csharp
using Linger;

public void ProcessData(string data, IEnumerable<int> numbers)
{
    // ?? Parameter validation polyfill for pre-.NET 8 versions
    ArgumentNullException.ThrowIfNull(data);                    // Ensure not null
    ArgumentException.ThrowIfNullOrEmpty(data);                 // Ensure not null or empty string
    ArgumentException.ThrowIfNullOrWhiteSpace(data);            // Ensure not null, empty or whitespace
    ArgumentNullException.ThrowIfNull(numbers);                 // Ensure collection is not null
    
    // ? Framework support: .NET 6+ uses built-in implementation, .NET 5 and below uses Linger Polyfill
    // When upgrading to .NET 8+, just remove "using Linger;", no other code changes required
}
```

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
var result = await retryHelper.ExecuteAsync(
    async () => await SomeOperationThatMightFail(),
    "Operation Name"
);

// Or use default options
var defaultRetryHelper = new RetryHelper();
var result2 = await defaultRetryHelper.ExecuteAsync(
    async () => await AnotherOperationThatMightFail(),
    "Another Operation Name"
);

// Synchronous variant
defaultRetryHelper.Execute(() => DoSomething());

// Try-style file writing
FileHelper.TryWriteText("logs/app.log", "hello world");
FileHelper.TryAppendText("logs/app.log", "\nnext line");
```

### Expression Helper

```csharp
using Linger.Helper;
using Linger.Enums;

// Dynamic expression building
// Basic expressions
Expression<Func<User, bool>> trueExpression = ExpressionHelper.True<User>();
Expression<Func<User, bool>> falseExpression = ExpressionHelper.False<User>();

// Single condition expressions
Expression<Func<User, bool>> ageFilter = ExpressionHelper.CreateGreaterThan<User>("Age", "18");
Expression<Func<User, bool>> nameFilter = ExpressionHelper.GetContains<User>("Name", "John");

// Build complex expressions using condition collections
var conditions = new List<Condition>
{
    new Condition { Field = "Age", Op = CompareOperator.GreaterThan, Value = 18 },
    new Condition { Field = "Name", Op = CompareOperator.Contains, Value = "John" }
};
Expression<Func<User, bool>> complexFilter = ExpressionHelper.BuildLambda<User>(conditions);
```

### Path Operations

```csharp
using Linger.Helper.PathHelpers;

// Path normalization - handles relative paths, duplicate separators, etc.
string messyPath = @"C:\temp\..\folder\.\file.txt";
string normalized = StandardPathHelper.NormalizePath(messyPath);
// Result: "C:\folder\file.txt" (Windows) or "/folder/file.txt" (Unix)

// Path comparison - cross-platform safe path equality check
string path1 = @"C:\Users\Documents\file.txt";
string path2 = @"c:\users\documents\FILE.TXT"; // Different case
bool pathEquals = StandardPathHelper.PathEquals(path1, path2); // Windows: true, Linux: false

// Get relative path - from base path to target path
string basePath = @"C:\Projects\MyApp";
string targetPath = @"C:\Projects\MyApp\src\Components\Button.cs";
string relative = StandardPathHelper.GetRelativePath(basePath, targetPath);
// Result: "src\Components\Button.cs" (Windows) or "src/Components/Button.cs" (Unix)

// Resolve absolute path - convert relative path to absolute
string workingDir = @"C:\Projects";
string relativePath = @"MyApp\src\file.txt";
string absolutePath = StandardPathHelper.ResolveToAbsolutePath(workingDir, relativePath);
// Result: "C:\Projects\MyApp\src\file.txt"

// Check for invalid path characters
string suspiciousPath = "file<name>.txt"; // Contains invalid character '<'
bool hasInvalidChars = StandardPathHelper.ContainsInvalidPathChars(suspiciousPath); // true

// Check if file or directory exists
string filePath = @"C:\temp\data.txt";
bool fileExists = StandardPathHelper.Exists(filePath, checkAsFile: true); // Check as file
bool dirExists = StandardPathHelper.Exists(filePath, checkAsFile: false); // Check as directory

// Get parent directory path
string deepPath = @"C:\Projects\MyApp\src\Components\Button.cs";
string parentDir = StandardPathHelper.GetParentDirectory(deepPath, levels: 1);
// Result: "C:\Projects\MyApp\src\Components"
string grandParentDir = StandardPathHelper.GetParentDirectory(deepPath, levels: 2);
// Result: "C:\Projects\MyApp\src"
```

## API Standardization & Type Safety

### .NET 10 Forward-Compatible Design

Methods like `LeftJoin`, `RightJoin`, `FullJoin` are fully compatible with .NET 10 standard. Zero code changes required when upgrading. Automatically switches via conditional compilation `#if !NET10_0_OR_GREATER`.

### Strict Type Safety Principles

**Conversion Strategy** (Performance Optimized):
1. First checks direct type matching (zero overhead)
2. Then attempts `ToString()` to string parsing

```csharp
object intObj = 123;
int result = intObj.ToIntOrDefault(0);     // Direct match, zero overhead
object doubleObj = 123.45;
int failed = doubleObj.ToIntOrDefault(0);  // Returns 0 (conversion fails)
```

**Complete Numeric Type Support**: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal

### Performance Benefits

- ? Zero-overhead same-type conversion
- ? Avoids exceptions, returns default values for better performance
- ? Smart fallback strategy, string conversion only when needed
- ? Unified API naming pattern

## Best Practices

1. **Type Conversion**: Use `ToXxxOrDefault()` to avoid exception overhead; use `TryToXxx()` methods when you need to explicitly check if conversion succeeds
2. **Null Checking**: Leverage `IsNullOrEmpty()`, `EnsureIsNotNull()` and other extension methods
3. **Async Operations**: Use async versions for I/O-intensive tasks (file, network)
4. **Exception Handling**: Use `RetryHelper` for unstable operations, handle exceptions properly with user feedback
5. **Resource Management**: Use `using` statements to ensure proper resource disposal

## Polyfills Summary

Provides forward-compatible Polyfills for BCL APIs & language features (for .NET Framework/Standard 2.0/legacy versions). Uses conditional compilation to automatically defer to framework built-ins when upgraded.

| Category | Content | Source Location |
|----------|---------|-----------------|
| **Parameter Validation** | `ArgumentNullException.ThrowIfNull` (pre-.NET 6)<br>`ArgumentException.ThrowIfNullOrEmpty/WhiteSpace` (pre-.NET 8) | `Polyfills/ArgumentNullException.cs`<br>`Polyfills/ArgumentException.cs` |
| **Language Features** | `required` keyword support (C# 11)<br>`RequiredMemberAttribute`, `SetsRequiredMembersAttribute`, `CompilerFeatureRequiredAttribute` | `Polyfills/RequiredMemberAttribute.cs`<br>`Polyfills/SetsRequiredMembersAttribute.cs`<br>`Polyfills/CompilerFeatureRequiredAttribute.cs` |
| **Nullability Attributes** | 11 attributes: `AllowNull`, `NotNull`, `MaybeNullWhen`, `NotNullIfNotNull`, etc. | `Polyfills/NullableAttributes.cs` |
| **Collection Extensions** | `LeftJoin`, `RightJoin`, `FullJoin` (.NET 10 preview) | `Extensions/Collection/IEnumerableExtensions.Polyfills.cs` |
| **Caller Capture** | `CallerArgumentExpressionAttribute` (improves Guard experience) | `Polyfills/CallerArgumentExpressionAttribute.cs` |

## Dependencies

The library has minimal external dependencies:
- System.Text.Json (for JSON operations)
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

