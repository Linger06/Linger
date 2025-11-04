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
// 4. DES algorithm is recommended only for compatibility scenarios

// ⚠️ Security Recommendations:
// 1. Recommended to use AES algorithm for new project development
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

// 🔗 .NET 10 Compatible Join Operations - Future-Ready Features Today
// ⚠️ Note: These are polyfill implementations for .NET 10+ built-in methods

// Left Join (Left Outer Join)
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

// Left Join: Keep all employees, show null for unmatched departments
var leftJoinResult = employees.LeftJoin(
    departments,
    emp => emp.DeptId,           // Outer key selector
    dept => dept.Id,             // Inner key selector
    (emp, dept) => new { 
        Employee = emp.Name, 
        Department = dept?.Name ?? "No Department" 
    }
);
// Output: [{ Employee = "John", Department = "Development" }, 
//          { Employee = "Jane", Department = "Testing" }, 
//          { Employee = "Bob", Department = "No Department" }]

// Right Join (Right Outer Join)
var rightJoinResult = employees.RightJoin(
    departments,
    emp => emp.DeptId,
    dept => dept.Id,
    (emp, dept) => new {
        Employee = emp?.Name ?? "No Employee",
        Department = dept.Name
    }
);
// Output: [{ Employee = "John", Department = "Development" },
//          { Employee = "Jane", Department = "Testing" }]

// Full Join (Full Outer Join)
var fullJoinResult = employees.FullJoin(
    departments,
    emp => emp.DeptId,
    dept => dept.Id,
    (emp, dept) => new {
        Employee = emp?.Name ?? "No Employee",
        Department = dept?.Name ?? "No Department"
    }
);
// Output: Contains records for all employees and all departments, 
//         with default values for unmatched sides

// 🎯 Simplified version: Returns tuples
var tupleResult = employees.LeftJoin(departments, e => e.DeptId, d => d.Id);
// Returns IEnumerable<Tuple<Employee, Department?>>

// 🔧 Support for custom equality comparers
var caseInsensitiveJoin = stringList1.LeftJoin(
    stringList2,
    s => s,
    s => s,
    (s1, s2) => new { Left = s1, Right = s2 },
    StringComparer.OrdinalIgnoreCase  // Case-insensitive comparison
);

// 📊 .NET 10 Compatibility Notes:
// - In .NET 10+, these methods will be provided natively by System.Linq.Enumerable
// - Current implementation signatures are fully compatible with .NET 10 standard
// - Seamless migration to built-in implementation when upgrading to .NET 10
// - Parameter names: outer/inner, outerKeySelector/innerKeySelector, resultSelector
// - Generic parameters: TOuter, TInner, TKey, TResult
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

public void ProcessData(string data, IEnumerable<int> numbers, string filePath)
{
    // 🆕 New parameter validation methods (.NET 8 polyfill for earlier versions)
    // These methods are identical to .NET 8+ built-in methods, providing seamless upgrade experience
    
    // Parameter null and content validation
    ArgumentNullException.ThrowIfNull(data);                    // Ensure not null
    ArgumentException.ThrowIfNullOrEmpty(data);                 // Ensure not null or empty string
    ArgumentException.ThrowIfNullOrWhiteSpace(data);            // Ensure not null, empty or whitespace

    // Collection parameter validation  
    ArgumentNullException.ThrowIfNull(numbers);                 // Ensure collection is not null
    
    // 🔍 Framework support details:
    // - .NET 5 and below: Uses Linger.ArgumentNullException.ThrowIfNull (polyfill)
    // - .NET 6+: Uses built-in System.ArgumentNullException.ThrowIfNull
    // - .NET 7 and below: Uses Linger.ArgumentException.ThrowIfNullOrEmpty (polyfill) 
    // - .NET 8+: Uses built-in System.ArgumentException.ThrowIfNullOrEmpty
    
    // 📦 Usage:
    using Linger;  // Only need this one using statement
    
    // When upgrading to .NET 8+, just remove "using Linger;" 
    // All other code remains unchanged!
    
    // ⚠️ Important: These are utility classes, cannot be instantiated
    // var ex = new ArgumentException();           // ❌ Compile error (this is good!)
    // throw new System.ArgumentException("msg");  // ✅ Correct: manually throw standard exception
    
    // 🎯 Exception types thrown:
    // ArgumentNullException.ThrowIfNull() → throws System.ArgumentNullException
    // ArgumentException.ThrowIfNullOrEmpty() → throws System.ArgumentException or System.ArgumentNullException
    // ArgumentException.ThrowIfNullOrWhiteSpace() → throws System.ArgumentException or System.ArgumentNullException
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

### � .NET 10 Forward-Compatible Design

Linger.Utils is already prepared for the upcoming .NET 10, especially regarding Join methods:

#### 🚀 Future-Ready Join Operations
```csharp
// 🎯 Current code (Linger polyfill)
var result = employees.LeftJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });

// 🔮 After .NET 10 release (automatically switches to built-in implementation)
var result = employees.LeftJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
// Exactly the same code, but using System.Linq.Enumerable.LeftJoin

// 🎉 No code changes required!
```

#### 📊 Complete Method Compatibility Matrix
| Method | Linger Polyfill | .NET 10 Built-in | Compatibility Status |
|--------|----------------|-------------------|---------------------|
| `LeftJoin` | ✅ Implemented | 🔮 Coming Soon | 💯 Fully Compatible |
| `RightJoin` | ✅ Implemented | 🔮 Coming Soon | 💯 Fully Compatible |
| `FullJoin` | ✅ Implemented | ❓ TBD | 📋 Monitoring |

#### 🔧 Technical Implementation Details
```csharp
// Conditional compilation ensures seamless transition
#if !NET10_0_OR_GREATER
    // Linger's polyfill implementation
    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(...)
    {
        // High-performance implementation consistent with .NET 10 behavior
    }
#endif

// In .NET 10+ environments, automatically uses built-in methods
// Better performance, fully consistent functionality
```

#### 🎁 Benefits of Early .NET 10 Feature Access
1. **Reduced Learning Curve**: Get familiar with .NET 10 APIs early
2. **Future-Proof Code**: Use new features without waiting for framework upgrades  
3. **Seamless Migration**: Zero code changes when upgrading frameworks
4. **Consistent Performance**: Polyfill implementation performs comparably to future built-in versions
5. **Standardization**: Unified parameter naming and behavior patterns

### �🔒 Strict Type Safety Principles

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

All type conversion methods use a unified `ToXxxOrDefault` pattern with **complete .NET numeric type support**, providing consistent API experience.

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

## Best Practices

1. **Follow Type Safety Principles**: 
   - Prefer string extension methods for type conversion
   - For object types, ensure they are string objects before conversion
   - Use `ToXxxOrDefault()` series methods for safe conversion

2. **Use Safe Methods**: 
   - Prefer `ToIntOrDefault()` over exception handling when conversion might fail
   - Use `ToIntOrNull()` and similar nullable methods when you need to distinguish between conversion failure and valid default values

3. **Leverage Null Checking**: 
   - Use extension methods like `IsNullOrEmpty()`, `IsNotNullOrEmpty()` for validation

4. **Parameter Validation**: 
   - Use `GuardExtensions` methods like `EnsureIsNotNull()`, `EnsureIsNotNullOrEmpty()` for input validation

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
## Polyfills Summary (BCL & Language Features)

This library provides forward-compatible polyfills for common BCL APIs and select language features to ensure consistent compilation and behavior on older target frameworks (e.g., .NET Framework, .NET Standard 2.0, .NET 5). All polyfills use conditional compilation so they automatically defer to in-box implementations when you upgrade your TFM.

### BCL Polyfills (by source)

- Parameter validation
    - `ArgumentNullException.ThrowIfNull(object? argument, string? paramName = null)` — compatibility for pre-.NET 6
        - Source: `src/Linger/Polyfills/ArgumentNullException.cs`
    - `ArgumentException.ThrowIfNullOrEmpty(string? argument, string? paramName = null)` — compatibility for pre-.NET 8
    - `ArgumentException.ThrowIfNullOrWhiteSpace(string? argument, string? paramName = null)` — compatibility for pre-.NET 8
        - Source: `src/Linger/Polyfills/ArgumentException.cs`
- Caller argument capture
    - `System.Runtime.CompilerServices.CallerArgumentExpressionAttribute` (improves parameter name capture for diagnostics/Guards)
        - Source: `src/Linger/Polyfills/CallerArgumentExpressionAttribute.cs`

These are static utility methods/attributes mirroring the in-box APIs; once the TFM meets the version requirements (e.g., .NET 8+), the polyfills are excluded and your code uses the framework implementation automatically.

### Language Feature Polyfill: required members support

To use C# 11 `required` members and get correct compiler diagnostics/metadata on older frameworks, the following attributes are provided:

- `System.Runtime.CompilerServices.RequiredMemberAttribute`
- `System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute`
- `System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute` (includes `IsExternalInit` support for `init` accessors)
        - Sources:
            - `src/Linger/Polyfills/RequiredMemberAttribute.cs`
            - `src/Linger/Polyfills/SetsRequiredMembersAttribute.cs`
            - `src/Linger/Polyfills/CompilerFeatureRequiredAttribute.cs`

These files are guarded (e.g., `#if !NET7_0_OR_GREATER`) so they won’t conflict with newer TFMs.

Usage example (works on .NET Framework/.NET Standard; ensure C# language version ≥ 11):

```csharp
public class Person
{
        public required string Name { get; init; }
        public required int Age { get; init; }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Person()
        {
                Name = "Unknown";
                Age = 0;
        }
}

var ok = new Person { Name = "Alice", Age = 28 }; // ✅ OK
var error = new Person(); // ❌ Compiler diagnostic: required members not set
```

Notes:
- Language version must be C# 11+ (this repo sets `LangVersion=latest` in `Directory.Build.props`).
- Polyfills provide the attributes and `init` plumbing only; required-member checks are performed by the compiler.
- After upgrading to .NET 7+, no code changes are needed; the polyfills compile out.

### CodeAnalysis Nullable Annotations Polyfill

Provides `System.Diagnostics.CodeAnalysis` nullability/flow analysis attributes for older target frameworks so that annotations are available at compile time and behave consistently with newer TFMs:

- Provided attributes: `AllowNull`, `DisallowNull`, `MaybeNull`, `NotNull`, `MaybeNullWhen`, `NotNullWhen`, `NotNullIfNotNull`, `DoesNotReturn`, `DoesNotReturnIf`, `MemberNotNull`, `MemberNotNullWhen`
- Source: `src/Linger/Polyfills/NullableAttributes.cs`
- Applicability: enabled for older TFMs such as `netstandard2.0`, `net4x`, and `netcoreapp2.x/3.x`; automatically excluded on newer frameworks that include these attributes inbox via conditional compilation, preventing duplicate definitions.

### IEnumerable Extension Polyfills (future .NET 10)

To align early with .NET 10 APIs, this library ships polyfills for `LeftJoin` / `RightJoin` / `FullJoin`:

- Source: `src/Linger/Extensions/Collection/IEnumerableExtensions.Polyfills.cs`
- Conditional compilation: `#if !NET10_0_OR_GREATER` — automatically switches to `System.Linq.Enumerable` built-ins on .NET 10+
- Parameter and generic naming matches .NET 10 (outer/inner, outerKeySelector/innerKeySelector, etc.)

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
