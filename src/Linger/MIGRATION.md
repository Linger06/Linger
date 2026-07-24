# Migration to 2.0.0-preview.1

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

This guide covers breaking API removals in `Linger.Utils` and
`Linger.Reflection`. It is intended for applications upgrading from the
previous API surface.

## Direct replacements

| Removed API | Replacement | Notes |
| --- | --- | --- |
| `DataTable.ToListAsync<T>(...)` | `DataTable.ToList<T>(...)` | The removed methods only wrapped synchronous work in `Task.FromResult`. |
| `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` | Accept and forward the supplied cancellation token. |
| `JsonExtensions.Serialize<T>` | `ToJsonString(...)` | See the JSON behavior change below. |
| `ObjectReflectionExtensions.ForIn` | `ForEachProperty` | Same property enumeration intent with a clearer name. |
| `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` | `AttrValues<T>` was only an alias. |
| `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` | Use the `IQueryable<T>` overload. |
| `DecimalExtensions.ToRounding` | `Round` | Both use the library's conventional decimal rounding behavior. |
| `byte[].ToImageBase64String` | `ToImageDataUri(mediaType)` | Specify the actual image media type. |
| `string[].ToEnumerable` | Use the array directly, or `value ?? Array.Empty<string>()` | Arrays already implement `IEnumerable<string>`. |
| `string[].ToList` and `ToListOrEmpty` | `new List<string>(value ?? Array.Empty<string>())` | Use `Enumerable.ToList()` when the array is known to be non-null. |
| `string.ToSplitList(char)` | `SplitToList(char)` | The `ToSplitList(string)` regex overload remains available. |
| `string.ToSplitArray(char)` | `SplitToArray(char)` | Uses a literal character delimiter. |
| `string.ToSplitArrayByCrlf` | `SplitToArray(Environment.NewLine)` | Make the line delimiter explicit. |
| `string.RemoveLastChar(string)` | `RemoveLastChar(char)` or `RemoveSuffixOnce(string, StringComparison)` | Choose a single character or an exact suffix operation. |
| `PathExtensions.IsStrictAbsolutePath` | `Path.IsPathFullyQualified` | Apply the application's UNC policy explicitly. |
| `int.ToFileSizeBytesString` | `FormatFileSize` | The new format is consistent with other file-size APIs. |
| `FileInfoExtensions.Delete(IEnumerable<FileInfo>)` | `Delete(files, consolidateExceptions)` | Pass `false` to retain fail-fast behavior, or `true` to aggregate eligible failures. |
| `FileInfoExtensions.GetVersionInfo` | `FileVersionInfo.GetVersionInfo(fileInfo.FullName)` | Apply any platform policy in the caller. |
| `string.GetFileVersion` | `new FileInfo(filePath).GetFileVersion()` | The replacement validates the file path and file existence. |
| `string.GetFilePath` | `Path.GetDirectoryName(path)` | Append a directory separator only when the caller requires one. |
| `FileInfoExtensions.FileSize` | `GetFileSizeFormatted` | Available for both file paths and `FileInfo`. |
| `GetFileNameNoExtension` | `Path.GetFileNameWithoutExtension` | Use `fileInfo.Name` for a `FileInfo` input. |
| `GetExtensionNotDotString` | `Path.GetExtension(path).TrimStart('.')` | Uses the platform path API directly. |
| `FileHelper.GetLineCount` | `File.ReadLines(path).Count()` | Prefer a dedicated streaming reader when further processing is required. |
| `FileHelper.GetFileSize` | `filePath.GetFileSize()` | Uses the maintained file extension API. |
| `FileHelper.GetDirectories` / `GetFileNames` boolean overloads | Overloads with `SearchOption` | Use `TopDirectoryOnly` or `AllDirectories` explicitly. |
| `FileHelper.CreateDirectoryIfNotExists` | `Directory.CreateDirectory` | `Directory.CreateDirectory` is already idempotent. |

## No one-to-one replacement

| Removed API | Migration approach | Reason for removal |
| --- | --- | --- |
| `AesEncrypt` | Use `AesEncryptAuthenticated`. Existing legacy ciphertext can still be read by `AesDecrypt`. | AES-CBC encryption without authentication does not protect ciphertext integrity. |
| `GuidCode.GetInt32UniqueCode` / `GetInt64UniqueCode` | Use `Guid.NewGuid()`; on .NET 9 or later, consider `Guid.CreateVersion7()` when time ordering is useful. | Truncating a GUID does not provide uniqueness. |
| `object.IsNullOrEmpty` / `IsNotNullOrEmpty` | Use a type-specific check such as `string.IsNullOrEmpty`, collection count checks, or `Guid?` extensions. | Arbitrary objects do not have a well-defined empty state. |
| `ToTarget<T>`, `TryToTarget<T>`, `ToTargetOrDefault<T>`, and `ToTargetOrNull<T>` | Use a specific `ToInt`, `TryToDateTime`, `TryToGuid`, and similar conversion API; for a runtime target type, use `TypeConverter.TryConvert`. | Generic conversion concealed supported types and failure semantics. |
| `TypeConverter.ConvertTo`, `TryConvertTo`, and `TryConvertKnownType` | Use `TypeConverter.TryConvert`. | The retained API has an explicit, AOT-friendly scalar support boundary. |
| `ToTree` | Build the hierarchy in the caller with an explicit node model, root rule, duplicate-key policy, and cycle handling. | The generic helper concealed essential tree-construction semantics. |
| `FileHelper.DeleteFolderFiles` | Perform the required `Directory` and `File` operations in the caller. | The recursive delete-by-name behavior was too implicit for a general utility API. |
| `ExtensionMethodSetting` defaults | Configure `Encoding`, `CultureInfo`, buffer sizes, and `JsonSerializerOptions` explicitly. Use `JsonDefaults.CreateRequestOptions`, `CreateResponseOptions`, or `ApplyDefaultConfiguration` for JSON. | Process-wide mutable defaults made behavior difficult to reason about. |
| `DataContractJsonSerializer` helpers | Use `System.Text.Json.JsonSerializer` or `ToJsonString` / `Deserialize<T>`. | The project standardizes on `System.Text.Json`. |
| `DeserializeDynamicJsonObject` | Use `JsonDocument`, `JsonElement`, or `JsonNode` according to the required mutability. | Dynamic JSON obscured schema and runtime failure modes. |

## Behavioral changes

- `ToJsonString(null)` returns the JSON literal `"null"`. The removed
  `Serialize<T>` wrapper returned a C# `null` reference for a null input.
- The removed `DataTable.ToListAsync` methods did not perform asynchronous
  I/O. Calling `ToList` is synchronous and avoids a misleading async API.
- String order expressions accept only `asc` or `desc` (case-insensitive).
  Invalid directions now throw `ArgumentException` rather than being treated
  as descending order.
- `FileInfoExtensions.Delete` no longer has a default exception policy. Call
  sites must choose fail-fast or consolidated exceptions explicitly.

## Examples

### Retry operations

```csharp
var result = await retryHelper.ExecuteAsync(
    async cancellationToken =>
    {
        return await client.GetStringAsync(uri, cancellationToken);
    },
    cancellationToken: cancellationToken);
```

### DataTable mapping

```csharp
List<Person> people = table.ToList(row => new Person
{
    Id = row.Field<int>("Id"),
    Name = row.Field<string>("Name")
});
```

### Authenticated encryption

```csharp
string ciphertext = plaintext.AesEncryptAuthenticated(key);
string plaintext = ciphertext.AesDecrypt(key);
```
