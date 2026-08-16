# Preparing for Linger 2.0

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

Linger 1.6.0 is a compatibility release for applications preparing to move
from 1.x to 2.0. Existing public APIs remain available in 1.6.0. APIs marked
with `Obsolete` have documented migration paths and are scheduled for removal
in 2.0.

## Recommended process

1. Upgrade all Linger packages to 1.6.0.
2. Build the application and resolve Linger-related obsolete warnings.
3. Run the application's complete test suite while still using 1.6.0.
4. Upgrade to a 2.0 preview and add `Linger.Reflection` when runtime reflection,
   expression, or dynamic query APIs are required.
5. Run trimming or Native AOT validation for applications that target those
   deployment models.

## Replacement APIs available in 1.6.0

| Deprecated API | Replacement |
| --- | --- |
| `DataTable.ToListAsync<T>(...)` | `DataTable.ToList<T>(...)` |
| Reflection-based `DataTable.ToList<T>()` | An overload accepting a row mapper, or a factory and explicit column setters |
| `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` |
| `ExtensionMethodSetting` JSON options | `JsonDefaults.CreateResponseOptions()` / `CreateRequestOptions()` |
| `SerializeJson<T>` / `JsonExtensions.Serialize<T>` | `ToJsonString(...)` |
| `DeserializeJson<T>` | `Deserialize<T>(...)` |
| `ObjectExtensions.ForIn` | `ForEachProperty` |
| `ArrayExtensions.ForEach` | `Array.ForEach` or the `IEnumerable<T>.ForEach` extension |
| `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` |
| `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` |
| `DecimalExtensions.ToRounding` | `Round` |
| `byte[].ToImageBase64String` | `ToImageDataUri(mediaType)` |
| `string[].ToEnumerable` | Use the array directly or `value ?? Array.Empty<string>()` |
| `string[].ToList` / `ToListOrEmpty` | `new List<string>(value ?? Array.Empty<string>())` |
| `string.ToSplitList(char)` | `SplitToList(char)` |
| `string.ToSplitArray(char)` | `SplitToArray(char)` |
| `string.ToSplitArrayByCrlf` | `SplitToArray(Environment.NewLine)` |
| `string.RemoveLastChar(string)` | `RemoveLastChar(char)` or `RemoveSuffixOnce(...)` |
| `PathExtensions.IsStrictAbsolutePath` | `Path.IsPathFullyQualified` plus an explicit UNC policy |
| `int.ToFileSizeBytesString` | `FormatFileSize` |
| `FileInfoExtensions.Delete(files)` | `Delete(files, consolidateExceptions)` |
| `FileInfoExtensions.GetVersionInfo` | `FileVersionInfo.GetVersionInfo(fileInfo.FullName)` with an explicit platform policy |
| `string.GetFileVersion` | `new FileInfo(filePath).GetFileVersion()` |
| `string.GetFilePath` | `Path.GetDirectoryName(filePath)` and an explicitly appended separator when needed |
| `FileInfoExtensions.FileSize` | `GetFileSizeFormatted` |
| `GetFileNameNoExtension` | `Path.GetFileNameWithoutExtension` |
| `GetExtensionNotDotString` | `Path.GetExtension(path).TrimStart('.')` |
| `FileHelper.GetLineCount` | `File.ReadLines(path).Count()` |
| `FileHelper.GetFileSize` | `filePath.GetFileSize()` |
| `FileHelper.GetExistingFileInfo` | `new FileInfo(path)`; open the file and call `ComputeHashMd5()` only when a content fingerprint is required |
| `ExtendedFileInfo` | `UploadedInfo` for upload results, or an application-specific DTO |
| `BaseFileInfo` | `UploadedInfo` for upload results, or an application-specific DTO |
| `FileHelper.GetDirectories` / `GetFileNames` boolean overloads | Overloads accepting `SearchOption` |
| `FileHelper.CreateDirectoryIfNotExists` | `Directory.CreateDirectory` |
| `GuidCode.GetInt32UniqueCode` / `GetInt64UniqueCode` | A full `Guid` |
| `ConfigureEmail` / `ConfigureMailKit` | `AddEmailService` |
| `CovertToObject<T>` | `ConvertToObject<T>` |
| `DataTableToFile` / `DataSetToFile` | `DataTableToExcel` / `DataSetToExcel` |
| `TryRefreshTokenAsync` | `RefreshTokenResultAsync` |
| `ExecuteResult` / `ExecuteResult<T>` | `Result` / `Result<T>` |
| `ErrorObj` | `Error` |
| `TypeConverter.ConvertTo` / `TryConvertTo` | `TypeConverter.TryConvert` for the supported scalar boundary, or a type-specific conversion API |

`TypeConverter.TryConvert` deliberately does not discover custom
`System.ComponentModel.TypeConverter` implementations. Use an explicit parser
or converter for application-specific target types.

## APIs without a direct replacement

- Use `AesEncryptAuthenticated` for new ciphertext. `AesDecrypt` continues to
  read existing legacy ciphertext.
- Replace `object.IsNullOrEmpty` and `IsNotNullOrEmpty` with a type-specific
  string, collection, or nullable-value check.
- Replace `ToTarget<T>` and related generic conversion helpers with specific
  `ToInt`, `TryToDateTime`, `TryToGuid`, and similar APIs.
- Build trees in application code with explicit root, duplicate-key, and cycle
  policies instead of `ToTree`.
- Replace `DeserializeDynamicJsonObject` and `JsonTextAccessor` with
  `JsonDocument`, `JsonElement`, or `JsonNode`.
- Replace `FileHelper.DeleteFolderFiles` with explicit `Directory` and `File`
  operations in the application.

## Reflection package change in 2.0

In 2.0, runtime-reflection and expression-based APIs move from `Linger.Utils`
to the optional `Linger.Reflection` package. This includes dynamic query
ordering, expression helpers, runtime property and attribute access, enum
metadata, reflection-based DataTable mapping, and `IEnumerable<T>.ToDataTable`.

| 1.6 source package | 1.6 API | 2.0 replacement | 2.0 location |
| --- | --- | --- | --- |
| `Linger.Utils` | `DataTable.ToListAsync<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` |
| `Linger.Utils` | Reflection-based `DataTable.ToList<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` |
| `Linger.Utils` | `ObjectExtensions.ForIn` | `ForEachProperty` | `Linger.Reflection` |
| `Linger.Utils` | `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` | `Linger.Reflection` |
| `Linger.Utils` | `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` | `Linger.Reflection` |
| `Linger.Utils` | Other runtime metadata, expression-tree, and reflection-based mapping APIs | Same API surface | `Linger.Reflection` |

The mapper-based and factory-based `DataTable.ToList<T>` overloads remain in
`Linger.Utils`. They are the preferred migration path when runtime reflection
is unnecessary.

Add the package when upgrading to 2.0 if the application uses those APIs:

```bash
dotnet add package Linger.Reflection
```

Keep explicit mapper, factory, scalar conversion, and other reflection-free
code on `Linger.Utils` for trimming and Native AOT scenarios.
