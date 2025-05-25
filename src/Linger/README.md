# Linger.Utils

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A comprehensive .NET utility library providing extensive type conversion extensions, string manipulation utilities, date/time helpers, file system operations, collection extensions, and various helper classes for everyday development tasks.

## Overview

Linger.Utils is designed to be a developer's daily companion, providing a rich set of extension methods and helper classes that make common programming tasks easier and more efficient. The library follows modern C# coding practices and supports multiple .NET framework versions.

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
- [Error Handling](#error-handling)
- [Performance Considerations](#performance-considerations)
- [Best Practices](#best-practices)

## Features

### 🚀 Core Extensions
- **String Extensions**: Rich string manipulation, validation, conversion, and formatting utilities
- **DateTime Extensions**: Date and time operations, formatting, and calculations
- **Numeric Extensions**: Type-safe numeric conversions and operations
- **Enum Extensions**: Enhanced enum handling and conversion
- **Object Extensions**: General object manipulation and validation
- **Array Extensions**: Array processing and manipulation utilities
- **GUID Extensions**: GUID operations and validation utilities

### 📦 Collection Extensions
- **List Extensions**: Enhanced list operations and manipulations
- **Collection Extensions**: Generic collection utilities and transformations

### 💾 Data Extensions
- **DataTable Extensions**: Database and DataTable manipulation utilities
- **Data Conversion**: Safe data type conversions and transformations

### 📁 File System Operations
- **File Helper**: Comprehensive file operations (read, write, copy, move, delete)
- **Path Helpers**: Cross-platform path manipulation and validation
- **Directory Operations**: Directory management and traversal utilities

### 🔧 Helper Classes
- **Expression Helper**: Expression tree manipulation and utilities
- **Retry Helper**: Robust retry mechanisms for operations
- **Property Helper**: Reflection-based property operations
- **GUID Code**: Enhanced GUID generation and manipulation
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
bool isInt = number.IsInt(); // Check if it's an integer
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
DateTime startOfDay = date.StartOfDay(); // Start of day
DateTime endOfDay = date.EndOfDay(); // End of day
DateTime startOfMonth = date.StartOfMonth(); // Start of month
DateTime endOfMonth = date.EndOfMonth(); // End of month
```

### File Operations

```csharp
using Linger.Helper;

// File operations
FileHelper.WriteText("data.txt", "Hello World");
string content = FileHelper.ReadText("data.txt");

// File copying with directory creation
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

// Safe check collection status
bool isEmpty = list.IsNullOrEmpty(); // Check if the collection is null or empty

// Paging operations
var pagedResult = list.Paging(2, 2); // Page 2, 2 items per page: [3, 4]

// Convert to separated string
string result = list.ToSeparatedString(", "); // "1, 2, 3, 4, 5"

// Execute operation on each element
list.ForEach(Console.WriteLine); // Output each element

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
bool isInt = stringValue.IsInt();
bool isDouble = stringValue.IsDouble();

// Object conversion
var stringRepresentation = obj.ToStringOrNull();

// Range check (for numeric values)
int value = 5;
bool inRange = value.InRange(1, 10); // Check if within range
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
string name = dynamicObj.Name; // Access property

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

// .NET 9+ features: V7 GUID timestamp extraction
#if NET9_0_OR_GREATER
DateTimeOffset timestamp = guid.GetTimestamp(); // Only for V7 GUIDs
#endif
```

### Array Extensions

```csharp
using Linger.Extensions.Core;

int[] numbers = { 1, 2, 3, 4, 5 };

// Execute operation on each element
numbers.ForEach(n => Console.WriteLine(n)); // Output: 1 2 3 4 5

// Execute operation with index
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
string description = status.GetDescription(); // Gets description text
```

### Parameter Validation

```csharp
using Linger.Helper;

public void ProcessData(string data, IEnumerable<int> numbers, string filePath)
{
    // Basic validation
    data.EnsureIsNotNull(nameof(data)); // Ensure not null
    data.EnsureIsNotNullAndEmpty(nameof(data)); // Ensure not null or empty
    data.EnsureIsNotNullAndWhiteSpace(nameof(data)); // Ensure not null, empty or whitespace

    // Collection validation
    numbers.EnsureIsNotNullOrEmpty(nameof(numbers)); // Ensure collection is not null or empty

    // File system validation
    filePath.EnsureFileExist(nameof(filePath)); // Ensure file exists
    Path.GetDirectoryName(filePath).EnsureDirectoryExist(); // Ensure directory exists

    // Condition validation
    (data.Length > 0).EnsureIsTrue(nameof(data), "Data must not be empty");
    (numbers.Count() < 1000).EnsureIsTrue(nameof(numbers), "Too many items");

    // Range validation
    int value = 5;
    value.EnsureIsInRange(1, 10, nameof(value)); // Ensure value is in specified range

    // null checks
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

// Retry operations with configurable policy
var options = new RetryOptions 
{
    MaxRetries = 3,
    BaseDelayMs = 1000 // 1 second
};
var retryHelper = new RetryHelper(options);
var result = await retryHelper.ExecuteAsync(
    async () => await SomeOperationThatMightFail(),
    "OperationName"
);

// Or with default options
var defaultRetryHelper = new RetryHelper();
var result2 = await defaultRetryHelper.ExecuteAsync(
    async () => await AnotherOperationThatMightFail(),
    "AnotherOperationName"
);
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

// Cross-platform path operations
string normalized = StandardPathHelper.NormalizePath(@"C:\temp\..\folder\file.txt");
bool pathEquals = StandardPathHelper.PathEquals(path1, path2);
string relative = StandardPathHelper.GetRelativePath(basePath, targetPath);
string absolutePath = StandardPathHelper.ResolveToAbsolutePath(basePath, relativePath);
bool hasInvalidChars = StandardPathHelper.ContainsInvalidPathChars(somePath);
bool fileExists = StandardPathHelper.Exists(filePath, checkAsFile: true);
string parentDir = StandardPathHelper.GetParentDirectory(path, levels: 1);
```

## Error Handling

The library follows defensive programming practices:

- Most operations have safe variants that return default values instead of throwing exceptions
- Extensive input validation with meaningful error messages
- Consistent error handling patterns across all components

## Performance Considerations

- Optimized for performance with minimal allocations where possible
- Cached reflection operations for better performance
- Async/await support for I/O operations
- Lazy evaluation where appropriate

## Best Practices

1. **Use Safe Methods**: Prefer `ToIntOrNull()` over `ToInt()` when conversion might fail
2. **Check for Null**: Use extension methods like `IsNullOrEmpty()` for validation
3. **Parameter Validation**: Use methods like `EnsureIsNotNull()`, `EnsureIsNotNullAndEmpty()` from `GuardExtensions` for input validation
4. **Leverage Async**: Use async versions of file operations for better performance
5. **Error Handling**: Always handle potential exceptions in file operations
6. **Resource Management**: Use `using` statements for disposable resources
7. **GUID Operations**: Use extension methods like `IsEmpty()` and `IsNotEmpty()` instead of direct comparison
8. **Collection Processing**: Use `ForEach()` extension methods to simplify array and collection iterations

## Dependencies

This library has minimal external dependencies:
- System.Text.Json (for JSON operations)
- System.Data.DataSetExtensions (for .NET Framework and .NET Standard 2.0)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. Make sure to:
- Follow the existing code style
- Add unit tests for new features
- Update documentation as needed

## License

This project is licensed under the terms of the license provided with the Linger project.

---

For more information about the Linger framework and other related packages, visit the [Linger project repository](https://github.com/Linger06/Linger).