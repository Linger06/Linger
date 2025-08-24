# Linger.Utils

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

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
- [Migration Notes](#migration-notes)

## Features

### 🚀 Core Extensions
- **String Extensions**: Rich string operations, validation, conversion, and formatting utilities
- **String Cryptography Extensions**: Secure AES encryption/decryption functionality for data protection
- **DateTime Extensions**: Date and time manipulation, formatting, and calculations
- **Numeric Extensions**: Type-safe numeric conversions with **strict type safety principles**, **complete support for all .NET basic numeric types**
- **Enum Extensions**: Enhanced enum handling and conversion
- **Object Extensions**: General object operations and validation, **enhanced with complete numeric type support**
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
int result = number.ToIntOrDefault(0); // Returns 123, or 0 if conversion fails
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

### String Cryptography Extensions

```csharp
using Linger.Extensions.Core;

// AES Encryption/Decryption (Recommended - High Security)
string data = "Sensitive data to encrypt";
string aesKey = "mySecretKey12345"; // AES key, variable length

try 
{
    // AES Encryption - Uses AES-256-CBC mode with automatic random IV generation
    string aesEncrypted = data.AesEncrypt(aesKey);
    Console.WriteLine($"AES Encrypted: {aesEncrypted}");
    
    // AES Decryption - Automatically extracts IV from encrypted data
    string aesDecrypted = aesEncrypted.AesDecrypt(aesKey);
    Console.WriteLine($"AES Decrypted: {aesDecrypted}"); // Output: Sensitive data to encrypt
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Parameter Error: {ex.Message}");
}
catch (CryptographicException ex)
{
    Console.WriteLine($"Encryption/Decryption Error: {ex.Message}");
}

// AES Multiple Encryption Test (Each result is different, more secure)
for (int i = 1; i <= 3; i++)
{
    string encrypted = data.AesEncrypt(aesKey);
    Console.WriteLine($"Encryption {i}: {encrypted}");
    // Each output is different due to random IV generation
}

// 🔐 Security Features:
// 1. AES uses AES-256-CBC mode with random IV generation for each encryption
// 2. IV is automatically included in encrypted result and extracted during decryption
// 3. Same plaintext produces different encrypted results each time, enhancing security
// 4. DES algorithm is deprecated, recommended only for legacy system compatibility

// ⚠️ Security Recommendations:
// 1. DES algorithm is not recommended for new projects, use AES instead
// 2. Keys should be stored securely, not hard-coded in source code
// 3. Use stronger key management mechanisms in production environments
// 4. AES key length is variable, internally processed using SHA256 to 32 bytes
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

// 🆕 Complete numeric type support with performance optimization
// Signed integer types
object sbyteObj = "100";
sbyte sbyteValue = sbyteObj.ToSByteOrDefault(0);    // Support: -128 to 127
short shortValue = sbyteObj.ToShortOrDefault(0);    // Support: -32,768 to 32,767
int intValue2 = sbyteObj.ToIntOrDefault(0);         // Support: -2,147,483,648 to 2,147,483,647
long longValue2 = sbyteObj.ToLongOrDefault(0L);     // Support: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807

// Unsigned integer types  
object ubyteObj = "255";
byte byteValue = ubyteObj.ToByteOrDefault(0);       // Support: 0 to 255
ushort ushortValue = ubyteObj.ToUShortOrDefault(0); // Support: 0 to 65,535
uint uintValue = ubyteObj.ToUIntOrDefault(0U);      // Support: 0 to 4,294,967,295
ulong ulongValue = ubyteObj.ToULongOrDefault(0UL);  // Support: 0 to 18,446,744,073,709,551,615

// Floating-point types
float floatValue = ubyteObj.ToFloatOrDefault(0.0f); // Support: ±1.5 x 10^-45 to ±3.4 x 10^38
double doubleValue = ubyteObj.ToDoubleOrDefault(0.0); // Support: ±5.0 × 10^−324 to ±1.7 × 10^308
decimal decimalValue = ubyteObj.ToDecimalOrDefault(0.0m); // Support: ±1.0 x 10^-28 to ±7.9228 x 10^28

// 🔍 Enhanced type checking methods - supports all numeric types
object testObj = GetSomeObject();
bool isByte = testObj.IsByte();           // Check if byte type
bool isSByte = testObj.IsSByte();         // Check if sbyte type
bool isUShort = testObj.IsUShort();       // Check if ushort type  
bool isUInt = testObj.IsUInt();           // Check if uint type
bool isULong = testObj.IsULong();         // Check if ulong type
bool isShort = testObj.IsShort();         // Check if short type
bool isInt = testObj.IsInt();             // Check if int type
bool isLong = testObj.IsLong();           // Check if long type
bool isFloat = testObj.IsFloat();         // Check if float type
bool isDouble = testObj.IsDouble();       // Check if double type
bool isDecimal = testObj.IsDecimal();     // Check if decimal type

// 🚀 Performance optimization: Direct type matching (zero overhead)
object intObj = 123;
int result2 = intObj.ToIntOrDefault(0); // Returns: 123 (direct type match, no ToString())

// ⚠️ Non-compatible types: Converted via ToString()
object doubleObj = 123.45;
int result3 = doubleObj.ToIntOrDefault(0); // Returns: 0 ("123.45" cannot convert to int)

// Type-safe object conversion - New standardized methods
object stringObj = "123";
int intValue = stringObj.ToIntOrDefault(0);        // Success: 123
long longValue = stringObj.ToLongOrDefault(0L);    // Success: 123
double doubleValue2 = stringObj.ToDoubleOrDefault(0.0); // Success: 123.0

// Strict type safety: Non-string objects return default values
object numberObj = 123.45; // Non-string type
int invalidInt = numberObj.ToIntOrDefault(0);      // Returns 0 (default value)
bool invalidBool = numberObj.ToBoolOrDefault(false); // Returns false (default value)

// Other conversion methods
DateTime dateValue = stringObj.ToDateTimeOrDefault(DateTime.MinValue);
Guid guidValue = "550e8400-e29b-41d4-a716-446655440000".ToGuidOrDefault();

// Null-safe operations
object obj = GetSomeObject();
string result = obj.ToStringOrDefault("default"); // Returns default when null

// Type checking (for strings only)
string stringValue = "123.45";
bool isNumber = stringValue.IsNumber(); // Check if it's a number format
bool isInt2 = stringValue.IsInteger(); // Check if it's an integer format
bool isDouble2 = stringValue.IsDouble(); // Check if it's a double format

// Try-style numeric conversions (to avoid masking failures with defaults)
if ("123".TryInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"bad data".TryDecimal(out var decVal)) { /* decVal = 0, conversion failed */ }

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

## API Standardization & Type Safety

Starting from version 0.8.2, Linger.Utils has undergone significant API standardization with a strong emphasis on type safety and consistency.

### 🔒 Strict Type Safety Principles

**ObjectExtensions Performance-Optimized Conversion Strategy:**
- **First: Direct type matching** - If the object is already the target type, return directly (zero overhead)
- **Then: String conversion attempt** - Call `ToString()` to convert to string, then parse to target type
- This ensures **optimal performance** and **predictable conversion behavior**
- **Complete numeric type support**: Now supports all .NET basic numeric types

```csharp
// ✅ Recommended: Performance-optimized conversion with complete type support
object intObj = 123;
int result1 = intObj.ToIntOrDefault(0); // Returns 123 (direct type match, zero overhead)

object doubleObj = 123.45;
int result2 = doubleObj.ToIntOrDefault(0); // Returns 0 ("123.45" cannot parse to int)

object stringObj = "123";
int result3 = stringObj.ToIntOrDefault(0); // Returns 123 (string parsing)

// 🆕 New unsigned integer type support
object byteObj = (byte)255;
byte byteResult = byteObj.ToByteOrDefault(0); // Direct return 255 (zero overhead)

object ushortObj = "65535";
ushort ushortResult = ushortObj.ToUShortOrDefault(0); // String parsing to 65535

object uintStr = "4294967295";
uint uintResult = uintStr.ToUIntOrDefault(0); // Supports full uint range

object ulongStr = "18446744073709551615";
ulong ulongResult = ulongStr.ToULongOrDefault(0); // Supports full ulong range

// 🆕 Signed byte type support
object sbyteStr = "-100";
sbyte sbyteResult = sbyteStr.ToSByteOrDefault(0); // Supports -128 to 127
```

### 📊 API Naming Standardization

All type conversion methods use a unified `ToXxxOrDefault` pattern with **complete .NET numeric type support**:

| Conversion Type | New Method | Old Method (Obsolete) | Value Range |
|----------------|------------|----------------------|-------------|
| **Signed Integer Types** | | | |
| Signed Byte | `ToSByteOrDefault()` | *New in 0.8.2+* | -128 to 127 |
| Short | `ToShortOrDefault()` | *New in 0.8.2+* | -32,768 to 32,767 |
| Integer | `ToIntOrDefault()` | `ToInt()` | -2,147,483,648 to 2,147,483,647 |
| Long | `ToLongOrDefault()` | `ToLong()` | -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807 |
| **Unsigned Integer Types** | | | |
| Byte | `ToByteOrDefault()` | *New in 0.8.2+* | 0 to 255 |
| Unsigned Short | `ToUShortOrDefault()` | *New in 0.8.2+* | 0 to 65,535 |
| Unsigned Integer | `ToUIntOrDefault()` | *New in 0.8.2+* | 0 to 4,294,967,295 |
| Unsigned Long | `ToULongOrDefault()` | *New in 0.8.2+* | 0 to 18,446,744,073,709,551,615 |
| **Floating Point Types** | | | |
| Float | `ToFloatOrDefault()` | `ToFloat()` | ±1.5 x 10^-45 to ±3.4 x 10^38 |
| Double | `ToDoubleOrDefault()` | `ToDouble()` | ±5.0 × 10^−324 to ±1.7 × 10^308 |
| Decimal | `ToDecimalOrDefault()` | `ToDecimal()` | ±1.0 x 10^-28 to ±7.9228 x 10^28 |
| **Other Types** | | | |
| Boolean | `ToBoolOrDefault()` | `ToBool()` | true/false |
| DateTime | `ToDateTimeOrDefault()` | `ToDateTime()` | DateTime range |
| GUID | `ToGuidOrDefault()` | `ToGuid()` | GUID format |

**🆕 New Type Checking Methods:**

| Category | Method | Description |
|----------|--------|-------------|
| Byte Types | `IsByte()` | Check if byte type |
| Signed Byte | `IsSByte()` | Check if sbyte type |
| Unsigned Short | `IsUShort()` | Check if ushort type |
| Unsigned Integer | `IsUInt()` | Check if uint type |
| Unsigned Long | `IsULong()` | Check if ulong type |

### 🎯 Usage Examples

```csharp
// String extension methods (recommended usage)
string numberStr = "123";
int result = numberStr.ToIntOrDefault(0);           // Success: 123
long longResult = numberStr.ToLongOrDefault(0L);    // Success: 123
double doubleResult = "123.45".ToDoubleOrDefault(0.0); // Success: 123.45

// 🆕 Complete numeric type support examples
byte byteResult = "255".ToByteOrDefault(0);         // Success: 255
sbyte sbyteResult = "-100".ToSByteOrDefault(0);     // Success: -100  
ushort ushortResult = "65535".ToUShortOrDefault(0); // Success: 65535
uint uintResult = "4294967295".ToUIntOrDefault(0U); // Success: 4294967295
ulong ulongResult = "18446744073709551615".ToULongOrDefault(0UL); // Success: max ulong

// Object extension methods (performance optimized)
object validObj = "456";
int validResult = validObj.ToIntOrDefault(0);       // Success: 456

object directMatch = 789; // Direct type match
int directResult = directMatch.ToIntOrDefault(0);   // Success: 789 (zero overhead)

object invalidObj = 789.12; // Non-compatible type conversion
int invalidResult = invalidObj.ToIntOrDefault(0);   // Returns: 0 (via ToString conversion fails)

// Enhanced boolean conversion
bool success1 = "true".ToBoolOrDefault(false);      // true
bool success2 = "yes".ToBoolOrDefault(false);       // true (English support)
bool success3 = "1".ToBoolOrDefault(false);         // true (numeric support)
bool success4 = "Y".ToBoolOrDefault(false);         // true (letter support)
```

### ⚡ Performance Benefits

- **Zero-overhead same-type conversion**: Direct type matching requires no string allocation and parsing (e.g., `object intObj = 123` → direct return)
- **Complete numeric type support**: Covers all .NET basic numeric types (byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal)
- **Exception Avoidance**: Returns default values instead of throwing exceptions on conversion failure, improving performance
- **Smart fallback strategy**: Only performs string conversion when needed, maximizing performance
- **Type Safety**: Ensures type safety at both compile-time and runtime
- **Consistency**: Unified naming and behavior patterns reduce learning costs

**Performance Benchmarks** (1 million operations):
- Same-type object conversion: ~14ms (**71M ops/sec**) 🚀 *(Direct type matching, zero string allocation)*
- String object conversion: ~42ms (24M ops/sec) *(String parsing required)*
- Non-compatible type conversion: ~119ms (8M ops/sec) *(ToString + parsing)*

**New Type Performance** (All unsigned integer and sbyte types enjoy the same optimization):
- Byte direct conversion: **71M ops/sec** 🚀
- UShort direct conversion: **71M ops/sec** 🚀  
- UInt direct conversion: **71M ops/sec** 🚀
- ULong direct conversion: **71M ops/sec** 🚀
- SByte direct conversion: **71M ops/sec** 🚀

## Best Practices

1. **Follow Type Safety Principles**: 
   - Prefer string extension methods for type conversion
   - For object types, ensure they are string objects before conversion
   - Use `ToXxxOrDefault()` instead of the old `ToXxx()` methods

2. **Use Safe Methods**: 
   - Prefer `ToIntOrDefault()` over exception handling when conversion might fail
   - Use `ToIntOrNull()` and similar nullable methods when you need to distinguish between conversion failure and valid default values

3. **Leverage Null Checking**: 
   - Use extension methods like `IsNullOrEmpty()`, `IsNotNullOrEmpty()` for validation
   - Use the standardized `IsNotNullOrEmpty()` instead of `IsNotNullAndEmpty()`

4. **Parameter Validation**: 
   - Use `GuardExtensions` methods like `EnsureIsNotNull()`, `EnsureIsNotNullOrEmpty()` for input validation
   - Use the new standardized Guard method names

5. **Leverage Async Operations**: 
   - Use async versions of file operations for better performance and responsiveness

6. **Handle Exceptions Properly**: 
   - Always handle potential exceptions in file operations
   - Use `RetryHelper` for retry operations and catch `OutOfRetryCountException`

7. **Resource Management**: 
   - Use `using` statements for disposable resources

8. **GUID Operations**: 
   - Use extension methods like `IsEmpty()`, `IsNotEmpty()`, `IsNotNullOrEmpty()` instead of direct comparison

9. **Collection Processing**: 
   - Use `ForEach()`, `IsNullOrEmpty()` and other extension methods to simplify collection handling

10. **Code Migration**: 
    - Migrate to new APIs promptly to avoid using methods marked with `[Obsolete]`
    - Pay attention to compiler warnings and follow migration guides

## Migration Notes (0.8.2 → Next)

To improve naming consistency, type safety, and readability, this version has made important API standardization improvements. Old names are marked with `[Obsolete]` and remain usable (transition period: 0.9.x, planned removal in the first 1.0 pre-release), but migration is strongly recommended.

### 🔒 Important: Type Safety Enhancement

**ObjectExtensions Behavior Change:**
- All type conversion methods now **use performance-optimized conversion strategy**
- **First perform direct type matching**: If the object is already the target type, return directly (zero overhead)
- **Then attempt string conversion**: Call `ToString()` to convert to string, then parse to target type
- This ensures **optimal performance** and **predictable conversion behavior**
- **Complete numeric type support**: Now supports all .NET basic numeric types

```csharp
// 🆕 New behavior (performance optimized + type safe)
object intObj = 123;
int result = intObj.ToIntOrDefault(0); // Returns 123 (direct type match, zero overhead)

object doubleObj = 123.45;
int result2 = doubleObj.ToIntOrDefault(0); // Returns 0 ("123.45" cannot parse to int)

object stringObj = "123";
int result3 = stringObj.ToIntOrDefault(0); // Returns 123 (string parsing)

// 🆕 New unsigned integer type support
object byteObj = (byte)255;
byte byteResult = byteObj.ToByteOrDefault(0); // Direct return 255 (zero overhead)

object ushortObj = "65535";
ushort ushortResult = ushortObj.ToUShortOrDefault(0); // String parsing to 65535

object uintStr = "4294967295";
uint uintResult = uintStr.ToUIntOrDefault(0); // Supports full uint range

object ulongStr = "18446744073709551615";
ulong ulongResult = ulongStr.ToULongOrDefault(0); // Supports full ulong range

// 🆕 Signed byte type support
object sbyteStr = "-100";
sbyte sbyteResult = sbyteStr.ToSByteOrDefault(0); // Supports -128 to 127
```

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
// Before (Guard methods)
data.EnsureIsNotNullAndEmpty();
filePath.EnsureFileExist();
directory.EnsureDirectoryExist();
try { await retry.ExecuteAsync(action, "MyAction"); } catch (OutOfReTryCountException ex) { ... }

// Before (Type conversions)
int value = stringValue.ToInt(0);
double amount = stringValue.ToDouble(0.0);
bool flag = stringValue.ToBool(false);

// After (Guard methods)
data.EnsureIsNotNullOrEmpty();
filePath.EnsureFileExists();
directory.EnsureDirectoryExists();
try { await retry.ExecuteAsync(action); } catch (OutOfRetryCountException ex) { ... }

// After (Type conversions)
int value = stringValue.ToIntOrDefault(0);
double amount = stringValue.ToDoubleOrDefault(0.0);
bool flag = stringValue.ToBoolOrDefault(false);
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

### Type Conversion API Standardization (0.8.2+)
The type conversion methods have been standardized to use consistent `ToXxxOrDefault` naming pattern with **complete .NET numeric type support**:

| Category | New API | Old API (Obsolete) | Notes | Value Range |
|----------|---------|-------------------|-------|-------------|
| **String Extensions** | | | | |
| String → SByte | `ToSByteOrDefault()` | *New in 0.8.2+* | Enhanced numeric support | -128 to 127 |
| String → Byte | `ToByteOrDefault()` | *New in 0.8.2+* | Enhanced numeric support | 0 to 255 |
| String → UShort | `ToUShortOrDefault()` | *New in 0.8.2+* | Enhanced numeric support | 0 to 65,535 |
| String → UInt | `ToUIntOrDefault()` | *New in 0.8.2+* | Enhanced numeric support | 0 to 4,294,967,295 |
| String → ULong | `ToULongOrDefault()` | *New in 0.8.2+* | Enhanced numeric support | 0 to 18,446,744,073,709,551,615 |
| String → Int | `ToIntOrDefault()` | `ToInt()` | Consistent with .NET patterns | -2,147,483,648 to 2,147,483,647 |
| String → Long | `ToLongOrDefault()` | `ToLong()` | Better semantic clarity | -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807 |
| String → Float | `ToFloatOrDefault()` | `ToFloat()` | Unified parameter ordering | ±1.5 x 10^-45 to ±3.4 x 10^38 |
| String → Double | `ToDoubleOrDefault()` | `ToDouble()` | Consistent overload patterns | ±5.0 × 10^−324 to ±1.7 × 10^308 |
| String → Decimal | `ToDecimalOrDefault()` | `ToDecimal()` | Professional API design | ±1.0 x 10^-28 to ±7.9228 x 10^28 |
| String → Boolean | `ToBoolOrDefault()` | `ToBool()` | Enhanced bool parsing | true/false |
| String → DateTime | `ToDateTimeOrDefault()` | `ToDateTime()` | Improved null handling | DateTime range |
| String → Guid | `ToGuidOrDefault()` | `ToGuid()` | Consistent behavior | GUID format |
| **Object Extensions** | | | | |
| Object → SByte | `ToSByteOrDefault()` | *New in 0.8.2+* | Performance optimized | -128 to 127 |
| Object → Byte | `ToByteOrDefault()` | *New in 0.8.2+* | Performance optimized | 0 to 255 |
| Object → UShort | `ToUShortOrDefault()` | *New in 0.8.2+* | Performance optimized | 0 to 65,535 |
| Object → UInt | `ToUIntOrDefault()` | *New in 0.8.2+* | Performance optimized | 0 to 4,294,967,295 |
| Object → ULong | `ToULongOrDefault()` | *New in 0.8.2+* | Performance optimized | 0 to 18,446,744,073,709,551,615 |
| Object → Types | All corresponding `OrDefault` methods | Old methods | ObjectExtensions updated | Various ranges |

**🆕 New Type Checking Methods:**

| Method Category | New API | Purpose |
|----------------|---------|---------|
| Byte Type Checking | `IsByte()` | Check if byte type |
| Signed Byte Checking | `IsSByte()` | Check if sbyte type |
| Unsigned Short Checking | `IsUShort()` | Check if ushort type |
| Unsigned Integer Checking | `IsUInt()` | Check if uint type |
| Unsigned Long Checking | `IsULong()` | Check if ulong type |

**Benefits of New API:**
- ✅ Consistent naming across all conversion methods
- ✅ Complete .NET numeric type support matrix
- ✅ Performance optimization with direct type checking
- ✅ Unified parameter ordering: `(value, defaultValue, additionalParams)`
- ✅ Better IntelliSense discoverability
- ✅ Professional API design aligned with industry standards
- ✅ Clearer semantic meaning of "default value on failure"

### Deprecated (Obsolete) Members – Scheduled Removal (Target: 1.0.0)
| Obsolete | Replacement | Notes |
|----------|-------------|-------|
| **Type Conversion Methods** | | |
| `string.ToInt()` | `ToIntOrDefault()` | Consistent naming pattern |
| `string.ToLong()` | `ToLongOrDefault()` | API standardization |
| `string.ToFloat()` | `ToFloatOrDefault()` | Unified parameter ordering |
| `string.ToDouble()` | `ToDoubleOrDefault()` | Better semantic clarity |
| `string.ToDecimal()` | `ToDecimalOrDefault()` | Professional naming |
| `string.ToBool()` | `ToBoolOrDefault()` | Enhanced bool parsing |
| `string.ToDateTime()` | `ToDateTimeOrDefault()` | Improved null handling |
| `string.ToGuid()` | `ToGuidOrDefault()` | Consistent behavior |
| `object.ToInt()` | `ToIntOrDefault()` | ObjectExtensions alignment |
| `object.ToLong()` | `ToLongOrDefault()` | Same as above |
| `object.ToFloat()` | `ToFloatOrDefault()` | Same as above |
| `object.ToDouble()` | `ToDoubleOrDefault()` | Same as above |
| `object.ToDecimal()` | `ToDecimalOrDefault()` | Same as above |
| `object.ToBool()` | `ToBoolOrDefault()` | Same as above |
| `object.ToDateTime()` | `ToDateTimeOrDefault()` | Same as above |
| `object.ToGuid()` | `ToGuidOrDefault()` | Same as above |
| `string.ToSafeString()` | `ToStringOrDefault()` | Naming consistency |
| **🆕 New Type Checking Methods** | | |
| *No obsolete methods* | `IsByte()` | New byte type checking |
| *No obsolete methods* | `IsSByte()` | New signed byte type checking |
| *No obsolete methods* | `IsUShort()` | New unsigned short type checking |
| *No obsolete methods* | `IsUInt()` | New unsigned integer type checking |
| *No obsolete methods* | `IsULong()` | New unsigned long type checking |
| **🆕 New Conversion Methods** | | |
| *No obsolete methods* | `ToSByteOrDefault()` | New signed byte conversion support |
| *No obsolete methods* | `ToByteOrDefault()` | New byte conversion support |
| *No obsolete methods* | `ToUShortOrDefault()` | New unsigned short conversion support |
| *No obsolete methods* | `ToUIntOrDefault()` | New unsigned integer conversion support |
| *No obsolete methods* | `ToULongOrDefault()` | New unsigned long conversion support |
| **Other API Changes** | | |
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
| `RemoveLastChar` (behavioral caveat) | `RemoveSuffixOnce` | Non-exact mode will remain but migration recommended |

> Deletion Window: These will be removed after the first 0.9.x stable (or at latest before 1.0.0). Begin migrating now to avoid breaking changes.

### Usage Examples (New APIs)
```csharp
// Type conversion with new standardized methods including complete numeric types
string numberStr = "123";
int result = numberStr.ToIntOrDefault(0);           // Returns 123, or 0 if conversion fails
long longResult = numberStr.ToLongOrDefault(0L);    // Consistent naming pattern
double doubleResult = "123.45".ToDoubleOrDefault(0.0); // Professional API design

// 🆕 New unsigned integer type conversions
byte byteResult = "255".ToByteOrDefault(0);         // Returns 255, supports 0-255 range
ushort ushortResult = "65535".ToUShortOrDefault(0); // Returns 65535, supports 0-65,535 range
uint uintResult = "4294967295".ToUIntOrDefault(0U); // Returns max uint value
ulong ulongResult = "18446744073709551615".ToULongOrDefault(0UL); // Returns max ulong value

// 🆕 New signed byte type conversion
sbyte sbyteResult = "-100".ToSByteOrDefault(0);     // Returns -100, supports -128 to 127 range

// Performance-optimized object conversions
object directMatch = 789; // Direct type match
int directResult = directMatch.ToIntOrDefault(0);   // Returns 789 (zero overhead conversion)

object byteObj = (byte)200;
byte optimizedByte = byteObj.ToByteOrDefault(0);    // Direct return 200 (zero overhead)

// Boolean conversion with enhanced parsing
bool success1 = "true".ToBoolOrDefault(false);      // Returns true
bool success2 = "yes".ToBoolOrDefault(false);       // Returns true (enhanced parsing)
bool success3 = "1".ToBoolOrDefault(false);         // Returns true (numeric support)

// DateTime conversion
DateTime date = "2024-01-01".ToDateTimeOrDefault(DateTime.MinValue);

// GUID conversion
Guid guid = "550e8400-e29b-41d4-a716-446655440000".ToGuidOrDefault();

// 🆕 New type checking methods
object testValue = GetSomeValue();
if (testValue.IsByte()) { /* Handle byte type */ }
if (testValue.IsUInt()) { /* Handle unsigned integer type */ }
if (testValue.IsULong()) { /* Handle unsigned long type */ }

// Remove a single suffix ignoring case
var trimmed = "Report.DOCX".RemoveSuffixOnce(".docx", StringComparison.OrdinalIgnoreCase); // => "Report"

// Ensure prefix (case-insensitive)
var normalized = "api/values".EnsureStartsWith("/API", StringComparison.OrdinalIgnoreCase); // => "/api/values"
```

### Migration Examples
```csharp
// Before (Obsolete)
int value1 = stringValue.ToInt(0);
long value2 = stringValue.ToLong(0L);
double value3 = stringValue.ToDouble(0.0);
bool value4 = stringValue.ToBool(false);
DateTime value5 = stringValue.ToDateTime(DateTime.MinValue);
Guid value6 = stringValue.ToGuid();

// After (Current) - Standard types
int value1 = stringValue.ToIntOrDefault(0);
long value2 = stringValue.ToLongOrDefault(0L);
double value3 = stringValue.ToDoubleOrDefault(0.0);
bool value4 = stringValue.ToBoolOrDefault(false);
DateTime value5 = stringValue.ToDateTimeOrDefault(DateTime.MinValue);
Guid value6 = stringValue.ToGuidOrDefault();

// 🆕 New (0.8.2+) - Complete numeric type support
byte byteValue = stringValue.ToByteOrDefault(0);
sbyte sbyteValue = stringValue.ToSByteOrDefault(0);
ushort ushortValue = stringValue.ToUShortOrDefault(0);
uint uintValue = stringValue.ToUIntOrDefault(0U);
ulong ulongValue = stringValue.ToULongOrDefault(0UL);

// 🆕 New type checking capabilities
bool isByte = obj.IsByte();
bool isUInt = obj.IsUInt();
bool isULong = obj.IsULong();
```

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
