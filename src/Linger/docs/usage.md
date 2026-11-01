# Linger.Utils Usage Guide

`Linger.Utils` provides extension methods and helper classes for everyday .NET development. This guide covers common APIs, failure behavior, compatibility, and important usage boundaries. The XML documentation and IntelliSense remain the reference for the complete API surface.

## Contents

- [Installation and namespaces](#installation-and-namespaces)
- [Type conversion](#type-conversion)
- [String operations](#string-operations)
- [DateTime, GUID, and enum operations](#datetime-guid-and-enum-operations)
- [Collection operations](#collection-operations)
- [DataTable and Native AOT](#datatable-and-native-aot)
- [Files, paths, and streams](#files-paths-and-streams)
- [Authenticated encryption](#authenticated-encryption)
- [Retries and cancellation](#retries-and-cancellation)
- [Argument validation](#argument-validation)
- [Target frameworks and polyfills](#target-frameworks-and-polyfills)

## Installation and namespaces

```bash
dotnet add package Linger.Utils --version 2.0.0-preview.4
```

Common namespaces:

```csharp
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using Linger.Extensions.IO;
using Linger.Helper;
```

No service registration is required. Most features are extension methods. Helper types such as `FileHelper`, `PathHelper`, and `RetryHelper` can be called directly or instantiated as needed.

## Type conversion

### Choosing a conversion method

Conversion APIs use four failure strategies:

| Method | Failure behavior | Use it when |
| --- | --- | --- |
| `ToXxx()` | Throws an exception | Invalid input should stop the operation |
| `ToXxxOrNull()` | Returns `null` | You need to distinguish no value from zero |
| `ToXxxOrDefault()` | Returns the supplied default | Parsing configuration or user input |
| `TryToXxx(out value)` | Returns `bool` | Success and failure need explicit handling |

```csharp
using Linger.Extensions.Core;

string input = "42";
int strict = input.ToInt();
int? nullable = input.ToIntOrNull();
int fallback = "invalid".ToIntOrDefault(-1);

if (input.TryToInt(out int parsed))
{
    Console.WriteLine(parsed);
}
```

String and `object` conversions use the invariant culture. `null`, `DBNull`, invalid formats, out-of-range values, or values that do not meet the whole-number requirement cause `ToIntOrNull` to return `null` and `ToIntOrDefault` to return its default value.

### Decimal conversion

Integer target types accept only values with no fractional part and within the target range:

```csharp
decimal whole = 12.000m;
decimal fraction = 12.5m;

int accepted = whole.ToIntOrDefault(-1);  // 12
int rejected = fraction.ToIntOrDefault(-1); // -1
```

This rule prevents truncation from being mistaken for a successful conversion. Use `decimal`, `double`, or another fractional target type when the fractional part matters.

### Object conversion

Object conversion first handles an already matching type, then common numeric types, and only then falls back to the string representation:

```csharp
object value = "123";
int result = value.ToIntOrDefault();

object invalid = new object();
int defaultValue = invalid.ToIntOrDefault(-1);
```

Use `ToXxx()` for inputs that must be valid and should fail immediately. For external input, prefer `TryToXxx` or `ToXxxOrDefault`.

## String operations

### Splitting and validation

`SplitToArray` and `SplitToList` use literal delimiters and return an empty collection for empty input:

```csharp
using Linger.Extensions.Core;

string[] fields = "id,name,email".SplitToArray(',');
IEnumerable<string> lines = "first\r\nsecond".SplitToList("\r\n");

bool validEmail = "user@example.com".IsEmail();
bool isNumber = "123.45".IsNumber();
bool isInteger = "123".IsInteger();
```

`StringSplitOptions` can be passed to the array and list overloads:

```csharp
string[] values = "a,,b".SplitToArray(',', StringSplitOptions.RemoveEmptyEntries);
```

### Truncation and query strings

```csharp
string title = "This is a long title".Truncate(10);
string url = "https://example.com/items".AppendQuery("page=2&size=20");
```

`AppendQuery` URL-encodes keys and values and preserves the URL fragment. Dictionaries and key-value lists can be appended in one call.

## DateTime, GUID, and enum operations

### Date and time

```csharp
using Linger.Extensions.Core;

DateTime now = DateTime.Now;
DateTime start = now.StartOfDay();
DateTime end = now.EndOfDay();
DateTime monthStart = now.StartOfMonth();
DateTime monthEnd = now.EndOfMonth();

int age = new DateTime(1990, 5, 15).CalculateAge();
bool inRange = now.InRange(DateTime.Today, DateTime.Today.AddDays(7));
```

Range checks include both the minimum and maximum values. `DateTimeExtensions` also provides conversions among `DateOnly`, `TimeOnly`, `DateTime`, and `DateTimeOffset`; define the time-zone semantics explicitly in business code.

### GUIDs

```csharp
using Linger.Extensions.Core;
using Linger.Helper;

Guid? optionalId = null;
bool empty = optionalId.IsNullOrEmpty();
long numericId = Guid.NewGuid().ToInt64();
string sortableId = GuidCode.NewId;
```

On .NET 9 and later, use `Guid.CreateVersion7()` for time-sortable GUIDs. Do not treat converting a GUID to an integer as a collision-free database key guarantee.

### Enums

```csharp
using Linger.Extensions.Core;

enum Status
{
    Active = 1,
    Inactive = 2
}

Status fromName = "Active".GetEnum<Status>();
Status fromValue = 1.GetEnum<Status>();
bool parsed = "Active".TryGetEnum(out Status status);
string? name = 1.GetEnumName<Status>();
```

Reading `DescriptionAttribute` and `DisplayAttribute` belongs to `Linger.Reflection`, not the base `Linger.Utils` package.

## Collection operations

### Empty checks, iteration, and paging

```csharp
using Linger.Extensions.Collection;

var values = new[] { 1, 2, 3, 4, 5 };
bool empty = values.IsNullOrEmpty();
string text = values.ToSeparatedString(", ");
values.ForEach((value, index) => Console.WriteLine($"{index}: {value}"));

IEnumerable<int> page = values.Paging(pageIndex: 2, pageSize: 2);
// page: 3, 4
```

`Paging` uses one-based page indexes. A `null` source, a page index less than or equal to zero, or a page size less than or equal to zero returns an empty sequence without throwing.

### Joins

```csharp
var result = employees.LeftJoin(
    departments,
    employee => employee.DepartmentId,
    department => department.Id,
    (employee, department) => new
    {
        Employee = employee.Name,
        Department = department?.Name ?? "No department"
    });
```

`LeftJoin` keeps all left-side records, `RightJoin` keeps all right-side records, and `FullJoin` keeps records from both sides. When there is no match, the corresponding element is its type's default value. Duplicate keys produce multiple rows. Pass an `IEqualityComparer<TKey>` when the key comparison needs a custom rule such as case-insensitive matching.

On .NET 10 and later, `LeftJoin` and `RightJoin` use framework implementations. On .NET 11 and later, `FullJoin` does the same. Older target frameworks use the conditional compatibility implementations supplied by Linger.

## DataTable and Native AOT

### Explicit mapping

Explicit mappers do not use reflection and are suitable for Native AOT and trimming:

```csharp
using Linger.Extensions.Data;

DataTable? table = GetDataTable();
List<UserDto>? users = table.ToList(row => new UserDto
{
    Id = Convert.ToInt32(row["Id"]),
    Name = row["Name"]?.ToString()
});
```

Use `CreateColumnSetter` when column assignment logic should be reused:

```csharp
var setters = new Dictionary<string, Action<UserDto, object?>>
{
    ["Id"] = DataTableExtensions.CreateColumnSetter<UserDto, int>((item, value) => item.Id = value),
    ["Name"] = DataTableExtensions.CreateColumnSetter<UserDto, string?>((item, value) => item.Name = value)
};

List<UserDto>? users = table.ToList(() => new UserDto(), setters);
```

### Reflection-based mapping

The APIs that discover properties at runtime and map them automatically are provided by `Linger.Reflection`. They are suitable for normal runtime applications, but Native AOT and trimming-sensitive paths should prefer explicit mappers or compile-time projections.

## Files, paths, and streams

### File helper

```csharp
using Linger.Helper;

FileHelper.CreateFile("data.txt", content: "Hello World");

if (FileHelper.TryReadText("data.txt", out string? content))
{
    Console.WriteLine(content);
}

FileHelper.CopyFile("source.txt", "backup/dest.txt");
FileHelper.MoveFile("backup/dest.txt", "archive");
FileHelper.DeleteFileIfExists("temp.txt");
```

`CopyFile` creates the destination directory. `TryReadText` returns `false` for a missing file and selected path or access failures. Use the BCL file APIs directly when the caller needs to distinguish individual I/O errors.

### Paths

```csharp
using Linger.Extensions.IO;
using Linger.Helper;

string normalized = PathHelper.CleanAndNormalizePureString(
    @"temp\\folder//file.txt\\",
    preserveEndingSeparator: false);

bool invalid = "file<name>.txt".ContainsInvalidPathChars();
bool fileExists = PathExtensions.Exists("data.txt", checkAsFile: true);
bool directoryExists = PathExtensions.Exists("data", checkAsFile: false);
```

`CleanAndNormalizePureString` normalizes a string without accessing the file system. `ContainsInvalidPathChars` follows the current operating system's rules and is not a complete path security check.

### Streams and cancellation

```csharp
using Linger.Extensions.IO;

using var source = File.OpenRead("source.bin");
await source.ToFileAsync("backup/source.bin", cancellationToken);
```

Pass a cancellation token to asynchronous stream operations. `ToFileAsync` creates the destination directory and overwrites the destination file. For non-repeatable streams, confirm the position and retry behavior before retrying an operation.

## Authenticated encryption

Use authenticated encryption for new data:

```csharp
using Linger.Extensions.Core;

string source = "Sensitive data";
string encrypted = source.AesEncryptAuthenticated(key);
string decrypted = encrypted.AesDecryptAuthenticated(key);
```

The format derives separate encryption and authentication keys with PBKDF2-SHA256, generates a random salt and IV, and authenticates the payload with HMAC-SHA256. Encrypting the same plaintext with the same key should produce different ciphertext each time.

Usage rules:

- Store keys in a key-management service or secure credential store, never in source code, logs, or a configuration repository.
- Treat authentication failure as a wrong-key or tampered-data error. Do not ignore it and continue with the plaintext.
- `AesDecrypt` is obsolete. It recognizes the authenticated format but silently falls back to legacy unauthenticated AES-CBC ciphertext, so use it only to read legacy data and re-encrypt it with `AesEncryptAuthenticated`. Decrypt current data with `AesDecryptAuthenticated`.
- Do not use DES or other obsolete encryption schemes.

## Retries and cancellation

### Exception retries

```csharp
using Linger.Helper;

var retry = new RetryHelper(new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 500,
    UseExponentialBackoff = true,
    MaxDelayMilliseconds = 10000,
    Jitter = 0.2
});

var response = await retry.ExecuteAsync(
    ct => SendRequestAsync(ct),
    operationName: "Send request",
    shouldRetry: exception => exception is HttpRequestException or TimeoutException,
    cancellationToken: cancellationToken);
```

`MaxRetryAttempts` includes the initial attempt. When `shouldRetry` is omitted, exceptions are not retried automatically. Non-recoverable argument and authentication errors should not be retried. Cancellation of the caller's `cancellationToken` always propagates immediately; other `OperationCanceledException` instances, such as the `TaskCanceledException` raised by an `HttpClient` timeout, are passed to `shouldRetry`.

With `Jitter`, each delay is chosen from `[delay × (1 − Jitter), delay × (1 + Jitter)]` and never exceeds `MaxDelayMilliseconds`.

### Result retries

When an operation reports a temporary failure through its return value, set `shouldRetryResult`:

```csharp
var result = await retry.ExecuteAsync(
    ct => TryUploadAsync(ct),
    operationName: "Upload file",
    shouldRetry: exception => exception is IOException,
    shouldRetryResult: success => !success,
    cancellationToken: cancellationToken);
```

After result-based attempts are exhausted, the last result is returned. When exception retries are exhausted, `OutOfRetryCountException` is thrown. The underlying operation must accept and use the supplied `CancellationToken`.

## Argument validation

On modern target frameworks these BCL validation methods are available directly. .NET Standard 2.0 and .NET Framework 4.7.2 do not include them, and `Linger.Utils` does not expose its internal polyfills, so code targeting those frameworks must supply its own polyfills:

```csharp
public void ProcessData(string data, IEnumerable<int> values)
{
    ArgumentNullException.ThrowIfNull(data);
    ArgumentException.ThrowIfNullOrWhiteSpace(data);
    ArgumentNullException.ThrowIfNull(values);
}
```

Use `GuardExtensions` when a fluent style is useful:

```csharp
string normalized = input
    .EnsureIsNotNullOrWhiteSpace(nameof(input))
    .Trim();

int pageSize = requestedSize.EnsureIsInRange(1, 100);
```

Validation methods throw specific `ArgumentNullException`, `ArgumentException`, or `ArgumentOutOfRangeException` instances. Use `EnsureFileExists` and `EnsureDirectoryExists` for file and directory checks.

## Target frameworks and polyfills

`Linger.Utils` supports:

- .NET 11.0
- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

On .NET Standard 2.0 and .NET Framework 4.7.2, `Linger.Utils` compiles `internal` copies of selected newer BCL APIs for its own use, including argument validation, hashing, hexadecimal conversion, and nullable annotations. They are not part of the public API. Projects that reference `Linger.Utils` on those frameworks and want the same APIs must supply their own polyfills.

Collection helpers such as `DistinctBy`, `LeftJoin`, and `RightJoin` are public extension methods in `Linger.Extensions.Collection`. They are compiled only on target frameworks that do not provide the BCL version.

For Native AOT or trimming:

1. Prefer the explicit mapper and factory APIs in `Linger.Utils`.
2. Avoid runtime property scanning and expression-tree APIs from `Linger.Reflection`.
3. Pass cancellation tokens through network, file, and database calls.
4. Use `TryToXxx` or `ToXxxOrDefault` for ordinary conversion failures instead of exceptions as control flow.

## Related documentation

- [Linger.Utils README](../README.md)
- [Linger.Utils migration guide](../MIGRATION.md)
- [Linger.Reflection README](../../Linger.Reflection/README.md)
- [Linger.Json README](../../Linger.Json/README.md)
