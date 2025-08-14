# Linger.Utils

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A comprehensive .NET utility library providing extensive type conversion extensions, string manipulation utilities, date/time helpers, file system operations, collection extensions, and various helper classes for everyday development tasks.

## Overview

Linger.Utils offer a rich collection of extension methods and helper classes that make common programming tasks simpler and more efficient. The library follows modern C# coding practices and supports multiple .NET framework versions.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Target Frameworks](#target-frameworks)
- [Quick Start](#quick-start)
  - [String Extensions](#string-extensions)
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

## Features

### 🚀 Core Extensions
- **String Extensions**: Rich string operations, validation, conversion, and formatting utilities
- **DateTime Extensions**: Date and time manipulation, formatting, and calculations
- **Numeric Extensions**: Type-safe numeric conversions and operations
- **Enum Extensions**: Enhanced enum handling and conversion
- **Object Extensions**: General object operations and validation
- **Array Extensions**: Array processing and manipulation utilities
- **GUID Extensions**: GUID operation and validation utilities

### 📦 Collection Extensions
- **List Extensions**: Enhanced list operations and processing
- **Collection Extensions**: General collection utilities and transformations

### 💾 Data Extensions
- **DataTable Extensions**: DataTable operation utilities
- **Data Conversion**: Safe data type conversion and transformation

### 📁 File System Operations
- **File Helper**: Comprehensive file operations (read, write, copy, move, delete)
- **Path Helper**: Cross-platform path operations and validation
- **Directory Operations**: Directory management and traversal utilities

### 🔧 Helper Classes
- **Expression Helper**: Expression tree operations and utilities
- **Retry Helper**: Robust retry mechanisms for operations
- **Property Helper**: Reflection-based property operations
- **GUID Code**: Enhanced GUID generation and operations
- **OS Platform Helper**: Cross-platform operating system detection
- **Parameter Validation Extensions**: Defensive programming and input validation utilities

### 🌐 JSON Support
- **JSON Extensions**: Simplified JSON serialization and deserialization
- **Custom Converters**: Specialized JSON converters for complex types

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
int result = number.ToInt(0); // Returns 123, or 0 if conversion fails
int? nullableResult = number.ToIntOrNull(); // Returns nullable type

// String manipulation
string text = "  Hello World  ";
string cleaned = text.Trim(); // Removes whitespace from both ends (.NET native method)

// String extraction
string longText = "Hello World";
string leftPart = longText.Left(5); // Get left 5 characters: Hello
string rightPart = longText.Right(5); // Get right 5 characters: World
string part = longText.SafeSubstring(0, 20); // Won't throw if length exceeds

// String checks
bool isEmpty = text.IsNullOrEmpty();
bool isNumber = number.IsNumber(); // Check if it's a number
bool isInt = number.IsInteger(); // Check if it's an integer
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
```

### Object Extensions

```csharp
using Linger.Extensions.Core;

// Null-safe operations
object obj = GetSomeObject();
string result = obj.ToSafeString("default");

// Type checking
string stringValue = obj.ToString(); // .NET native method
bool isNumber = stringValue.IsNumber();
bool isInt = stringValue.IsInteger();
bool isDouble = stringValue.IsDouble();

// Object conversion
var stringRepresentation = obj.ToStringOrNull();

// Try-style numeric conversions
if ("123".TryInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"abc".TryDecimal(out var decVal)) { /* failed, decVal = 0 */ }

// Ensure prefix/suffix (idempotent)
var apiUrl = "api/v1".EnsureStartsWith("/"); // => "/api/v1"
var folder = "logs".EnsureEndsWith("/");     // => "logs/"

// Range checking (for numeric values)
int value = 5;
bool inRange = value.InRange(1, 10); // Check if in range
```

### JSON Extensions

```csharp
using Linger.Extensions;

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
bool isNotNullAndEmpty = nullableGuid.IsNotNullAndEmpty(); // Check if neither null nor empty

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
using Linger.Helper;

public void ProcessData(string data, IEnumerable<int> numbers, string filePath)
{
    // Basic validation
    data.EnsureIsNotNull(nameof(data)); // Ensure not null
    data.EnsureIsNotNullOrEmpty(nameof(data)); // Ensure not null or empty
    data.EnsureIsNotNullOrWhiteSpace(nameof(data)); // Ensure not null, empty or whitespace

    // Collection validation
    numbers.EnsureIsNotNullOrEmpty(nameof(numbers)); // Ensure collection is not null or empty

    // File system validation
    filePath.EnsureFileExists(nameof(filePath)); // Ensure file exists
    Path.GetDirectoryName(filePath).EnsureDirectoryExists(); // Ensure directory exists

    // Condition validation
    (data.Length > 0).EnsureIsTrue(nameof(data), "Data must not be empty");
    (numbers.Count() < 1000).EnsureIsTrue(nameof(numbers), "Too many items");

    // Range validation
    int value = 5;
    value.EnsureIsInRange(1, 10, nameof(value)); // Ensure value is in range

    // Null checking
    object? obj = GetSomeObject();
    obj.EnsureIsNotNull(nameof(obj)); // If object should not be null
    // or
    obj.EnsureIsNull(nameof(obj)); // If object should be null
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

## Best Practices

1. **Use Safe Methods**: Prefer `ToIntOrNull()` over `ToInt()` when conversion might fail
2. **Null Checking**: Use extension methods like `IsNullOrEmpty()` for validation
3. **Parameter Validation**: Use `GuardExtensions` methods like `EnsureIsNotNull()`, `EnsureIsNotNullAndEmpty()` for input validation
4. **Leverage Async**: Use async versions of file operations for better performance
5. **Error Handling**: Always handle potential exceptions in file operations
6. **Resource Management**: Use `using` statements for disposable resources
7. **GUID Operations**: Use extension methods like `IsEmpty()` and `IsNotEmpty()` instead of direct comparison
8. **Collection Processing**: Use `ForEach()` extension methods to simplify array and collection iteration

## Migration Notes (0.8.2 → Next)

The following API refinements were introduced to improve naming consistency and usability. Old members remain available with `[Obsolete]` for one transitional release cycle (target removal: next minor release after 0.9.x) and still function the same. Plan to remove obsolete aliases in the first 1.0 pre-release.

### Renamed Guard Methods
| Old Name | New Name | Reason |
|----------|----------|--------|
| `EnsureIsNotNullAndEmpty` | `EnsureIsNotNullOrEmpty` | Correct logical conjunction wording (not null OR empty check wording) |
| `EnsureIsNotNullAndWhiteSpace` | `EnsureIsNotNullOrWhiteSpace` | Consistency with BCL `IsNullOrWhiteSpace` naming |
| `EnsureFileExist` | `EnsureFileExists` | Grammar (plural verb form) & .NET naming consistency |
| `EnsureDirectoryExist` | `EnsureDirectoryExists` | Same as above |

String extension counterparts also gained the new `IsNotNullOrEmpty` / `IsNotNullOrWhiteSpace` names with old names kept as obsolete shims.

### Exception Renaming
| Old | New | Notes |
|-----|-----|-------|
| `OutOfReTryCountException` | `OutOfRetryCountException` | Typo/casing fix. Old type now inherits from the new type and is marked obsolete. |

### RetryHelper Enhancements
| Change | Description |
|--------|-------------|
| Optional `operationName` | Now optional; if omitted the library captures the caller expression via `CallerArgumentExpression`. |
| Improved backoff | Uses full jitter strategy and validates `RetryOptions` values. |
| Timing info | Final aggregated exception message includes total elapsed milliseconds. |

### How to Update Your Code
1. Replace old Guard method names with the new ones (search & replace is safe).  
2. Remove explicit `operationName` arguments where they were just descriptive duplicates of the delegate (optional).  
3. Update exception catch blocks from `OutOfReTryCountException` to `OutOfRetryCountException` (you can temporarily catch the base type if supporting both).  
4. (Optional) Suppress obsolete warnings temporarily with `#pragma warning disable CS0618` if performing incremental migration.  

### Example Before / After
```csharp
// Before
data.EnsureIsNotNullAndEmpty();
filePath.EnsureFileExist();
directory.EnsureDirectoryExist();
try { await retry.ExecuteAsync(action, "MyAction"); } catch (OutOfReTryCountException ex) { ... }

// After
data.EnsureIsNotNullOrEmpty();
filePath.EnsureFileExists();
directory.EnsureDirectoryExists();
try { await retry.ExecuteAsync(action); } catch (OutOfRetryCountException ex) { ... }
```

No functional behavior changed—this is a surface naming / diagnostics improvement.

### New String & Guid API Enhancements (post 0.8.2)
| Category | New API | Purpose |
|----------|---------|---------|
| String | `RemoveSuffixOnce(string suffix, StringComparison comparison = Ordinal)` | 精确移除单个后缀（大小写可控），避免旧 `RemoveLastChar` 基于字符集合的潜在误解 |
| String | `EnsureStartsWith(string prefix, StringComparison comparison)` | 基于指定比较方式前缀确保，无需手动大小写判断 |
| String | `EnsureEndsWith(string suffix, StringComparison comparison)` | 同上（后缀） |
| String | `RemovePrefixAndSuffix(string token, StringComparison comparison)` | 提供文化/大小写控制的前后对称移除 |
| Guid | `IsNotNullOrEmpty()` | 语义统一，替代旧 `IsNotNullAndEmpty` |
| Object | `IsNotNullOrEmpty()` | 与 Guid / String 一致化 |

### Deprecated (Obsolete) Members – Scheduled Removal (Target: 1.0.0)
| Obsolete | Replacement | Notes |
|----------|-------------|-------|
| `GuidExtensions.IsNotNullAndEmpty` | `IsNotNullOrEmpty` | Naming consistency |
| `ObjectExtensions.IsNotNullAndEmpty` | `IsNotNullOrEmpty` | Same rationale |
| `StringExtensions.Substring2` | `Take` | Simpler, clearer verb |
| `StringExtensions.Substring3` | `TakeLast` | Mirrors new naming |
| `StringExtensions.IsNotNullAndEmpty` | `IsNotNullOrEmpty` | Consistency |
| `StringExtensions.IsNotNullAndWhiteSpace` | `IsNotNullOrWhiteSpace` | Consistency |
| `GuardExtensions.EnsureIsNotNullAndEmpty` | `EnsureIsNotNullOrEmpty` | Consistency |
| `GuardExtensions.EnsureIsNotNullAndWhiteSpace` | `EnsureIsNotNullOrWhiteSpace` | Consistency |
| `ObjectExtensions.ToNotSpaceString` | `ToTrimmedString` | Clearer naming |
| `ObjectExtensions.ToStringOrEmpty` | `ToSafeString` | Consolidated semantics |
| `RemoveLastChar` (behavioral caveat) | `RemoveSuffixOnce` | 非精确模式将长期保留但建议迁移 |

> Deletion Window: These will be removed after the first 0.9.x stable (or at latest before 1.0.0). Begin migrating now to avoid breaking changes.

### Usage Examples (New APIs)
```csharp
// Remove a single suffix ignoring case
var trimmed = "Report.DOCX".RemoveSuffixOnce(".docx", StringComparison.OrdinalIgnoreCase); // => "Report"

// Ensure prefix (case-insensitive)
var normalized = "api/values".EnsureStartsWith("/API", StringComparison.OrdinalIgnoreCase); // => "/api/values"

// Symmetric remove
var inner = "__value__".RemovePrefixAndSuffix("__", StringComparison.Ordinal); // => "value"

Guid? gid = Guid.NewGuid();
if (gid.IsNotNullOrEmpty()) { /* ... */ }
```

### 1.0.0 Planned Removal List (Preview)
Prepare for removal of all obsolete members listed above plus any marked with "Will be removed in 1.0.0" attributes in code. A final confirmation list will be published in the 1.0.0 release notes.


### New Additions (Post 0.8.2 Preview)

#### Non-Throwing Enum Parsing
Use the new `TryGetEnum` helpers to avoid exception-driven flow when converting from string or integer values:

```csharp
if ("Active".TryGetEnum<Status>(out var status))
{
    // use status
}

if (2.TryGetEnum<Status>(out var status2))
{
    // use status2
}
```

#### Additional Exception Constructors
`OutOfRetryCountException` now provides parameterless and message-only constructors:
```csharp
throw new OutOfRetryCountException(); // default message
throw new OutOfRetryCountException("Custom message");
throw new OutOfRetryCountException("Custom message", innerEx);
```
Legacy `OutOfReTryCountException` remains (obsolete) for one transition cycle.

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
