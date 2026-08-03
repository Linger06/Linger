# Migration to 2.0.0-preview.1

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

This guide covers breaking API removals across Linger packages. It is intended
for applications upgrading from the previous API surface.

## Direct replacements

| Source package | Removed API | Replacement | Replacement location | Notes |
| --- | --- | --- | --- | --- |
| `Linger.Utils` | `DataTable.ToListAsync<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` | Add a reference to `Linger.Reflection`; the removed method only wrapped synchronous reflection-based mapping in `Task.FromResult`. |
| `Linger.Utils` | Reflection-based `DataTable.ToList<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` | Add a reference to `Linger.Reflection`. Use the mapper or factory overloads in `Linger.Utils` when runtime reflection is unnecessary. |
| `Linger.Utils` | `DataTable.ToListAsync<T>(Func<DataRow, T>)` | `DataTable.ToList<T>(Func<DataRow, T>)` | `Linger.Utils` | The removed method only wrapped synchronous mapping in `Task.FromResult`. |
| `Linger.Utils` | `DataTable.ToListAsync<T>(Func<T>, IReadOnlyDictionary<string, Action<T, object?>>)` | `DataTable.ToList<T>(Func<T>, IReadOnlyDictionary<string, Action<T, object?>>)` | `Linger.Utils` | The removed method only wrapped synchronous mapping in `Task.FromResult`. |
| `Linger.Utils` | `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` | `Linger.Utils` | Accept and forward the supplied cancellation token. |
| `Linger.Utils` | `JsonExtensions.Serialize<T>` / `SerializeJson<T>(...)` | `JsonSerializer.Serialize(...)` | .NET BCL | The legacy implementation used `DataContractJsonSerializer`. |
| `Linger.Utils` | `JsonExtensions.DeserializeJson<T>(...)` | `JsonSerializer.Deserialize<T>(...)` | .NET BCL | The legacy implementation used `DataContractJsonSerializer`. |
| `Linger.Utils` | `JsonExtensions.ToJsonString(...)` | `JsonSerializer.Serialize(...)` | .NET BCL | The wrapper added no serialization capability and could not guarantee Native AOT compatibility. |
| `Linger.Utils` | `JsonExtensions.Deserialize<T>(...)` | `JsonSerializer.Deserialize<T>(...)` | .NET BCL | The wrapper added no deserialization capability and could not guarantee Native AOT compatibility. |
| `Linger.Utils` | `JsonObjectConverter` | Use `JsonElement`, `JsonNode`, or a strongly typed model. | .NET BCL | Mixed CLR primitive and JSON DOM results were surprising and the converter could not guarantee Native AOT compatibility. |
| `Linger.Utils` | `JsonDefaults`, `DateTimeConverter`, `DateTimeNullConverter`, `DataTableJsonConverter`, `DataSetConverter`, and `JsonStringConverter` | The same API | `Linger.Json` | Add a reference to `Linger.Json`; the public namespaces and API names are unchanged. |
| `Linger.Utils` | `string.ToDataTable()`, `DataTable.ToJsonString()`, and `JsonElement.JsonElementToDataTable()` | The same API | `Linger.Json` | Add a reference to `Linger.Json`; the APIs are JSON-specific and no longer bring a JSON dependency into `Linger.Utils`. |
| `Linger.Utils` | `ObjectExtensions.ForIn` | `ForEachProperty` | `Linger.Reflection` | Add a reference to `Linger.Reflection`; the replacement has the same property enumeration intent with a clearer name. |
| `Linger.Utils` | `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` | `Linger.Reflection` | Add a reference to `Linger.Reflection`; `AttrValues<T>` was only an alias. |
| `Linger.Utils` | `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` | `Linger.Reflection` | Add a reference to `Linger.Reflection` and use the `IQueryable<T>` overload. |
| `Linger.Utils` | `DecimalExtensions.ToRounding` | `Round` | `Linger.Utils` | Both use the library's conventional decimal rounding behavior. |
| `Linger.Utils` | `byte[].ToImageBase64String` | `ToImageDataUri(mediaType)` | `Linger.Utils` | Specify the actual image media type. |
| `Linger.Utils` | `ArrayExtensions.ForEach<T>(T[], Action<T>)` | `Array.ForEach(array, action)` or `IEnumerable<T>.ForEach(action)` | .NET BCL / `Linger.Utils` | Choose the array API or the maintained enumerable extension. |
| `Linger.Utils` | `ArrayExtensions.Insert<T>(T[], T)` | `Add<T>(T[], T)` | `Linger.Utils` | `Insert` only appended an element and did not accept an insertion index. |
| `Linger.Utils` | `DateTimeExtensions.FirstDayOfMonth` | `StartOfMonth` | `Linger.Utils` | `FirstDayOfMonth` was a complete alias. |
| `Linger.Utils` | `string[].ToEnumerable` | Use the array directly, or `value ?? Array.Empty<string>()` | .NET BCL | Arrays already implement `IEnumerable<string>`. |
| `Linger.Utils` | `string[].ToList` and `ToListOrEmpty` | `new List<string>(value ?? Array.Empty<string>())` | .NET BCL | Use `Enumerable.ToList()` when the array is known to be non-null. |
| `Linger.Utils` | `string.ToSplitList(char)` | `SplitToList(char)` | `Linger.Utils` | The `ToSplitList(string)` regex overload remains available. |
| `Linger.Utils` | `string.ToSplitArray(char)` | `SplitToArray(char)` | `Linger.Utils` | Uses a literal character delimiter. |
| `Linger.Utils` | `string.ToSplitArrayByCrlf` | `SplitToArray(Environment.NewLine)` | `Linger.Utils` | Make the line delimiter explicit. |
| `Linger.Utils` | `string.RemoveLastChar(string)` | `RemoveLastChar(char)` or `RemoveSuffixOnce(string, StringComparison)` | `Linger.Utils` | Choose a single character or an exact suffix operation. |
| `Linger.Utils` | `PathExtensions.IsStrictAbsolutePath` | `Path.IsPathFullyQualified` | .NET BCL | Apply the application's UNC policy explicitly. |
| `Linger.Utils` | `PathExtensions.ToFullPath` | `Path.GetFullPath(path)` or `Path.GetFullPath(path, basePath)` | .NET BCL | Trailing separator policy moves to the caller; on netstandard2.0 use `Path.GetFullPath(Path.Combine(basePath, path))`. |
| `Linger.Utils` | `PathExtensions.GetRelativePath` | `Path.GetRelativePath` | .NET BCL | The BCL also returns `"."` for identical paths. |
| `Linger.Utils` | `PathExtensions.GetParentDirectory(levels)` | Call `Path.GetDirectoryName` once per level | .NET BCL | The BCL returns `null` at the root; the old helper returned the root itself. |
| `Linger.Utils` | `FileHelper.GetExistingFileInfo(string, string?)` | `GetExistingFileInfo(string)` | `Linger.Utils` | `RelativeFilePath` is no longer computed; call `Path.GetRelativePath` when you need it. |
| `Linger.Utils` | `int.ToFileSizeBytesString` | `FormatFileSize` | `Linger.Utils` | The new format is consistent with other file-size APIs. |
| `Linger.Utils` | `FileInfoExtensions.Delete(IEnumerable<FileInfo>)` | `Delete(files, consolidateExceptions)` | `Linger.Utils` | Pass `false` to retain fail-fast behavior, or `true` to aggregate eligible failures. |
| `Linger.Utils` | `FileInfoExtensions.GetVersionInfo` | `FileVersionInfo.GetVersionInfo(fileInfo.FullName)` | .NET BCL | Apply any platform policy in the caller. |
| `Linger.Utils` | `string.GetFileVersion` | `new FileInfo(filePath).GetFileVersion()` | `Linger.Utils` | The replacement validates the file path and file existence. |
| `Linger.Utils` | `string.GetFilePath` | `Path.GetDirectoryName(path)` | .NET BCL | Append a directory separator only when the caller requires one. |
| `Linger.Utils` | `FileInfoExtensions.FileSize` | `GetFileSizeFormatted` | `Linger.Utils` | Available for both file paths and `FileInfo`. |
| `Linger.Utils` | `GetFileNameNoExtension` | `Path.GetFileNameWithoutExtension` | .NET BCL | Use `fileInfo.Name` for a `FileInfo` input. |
| `Linger.Utils` | `GetExtensionNotDotString` | `Path.GetExtension(path).TrimStart('.')` | .NET BCL | Uses the platform path API directly. |
| `Linger.Utils` | `FileHelper.GetLineCount` | `File.ReadLines(path).Count()` | .NET BCL | Prefer a dedicated streaming reader when further processing is required. |
| `Linger.Utils` | `FileHelper.GetFileSize` | `filePath.GetFileSize()` | `Linger.Utils` | Uses the maintained file extension API. |
| `Linger.Utils` | `FileHelper.GetDirectories` / `GetFileNames` boolean overloads | Overloads with `SearchOption` | `Linger.Utils` | Use `TopDirectoryOnly` or `AllDirectories` explicitly. |
| `Linger.Utils` | `FileHelper.CreateDirectoryIfNotExists` | `Directory.CreateDirectory` | .NET BCL | `Directory.CreateDirectory` is already idempotent. |

## Other Linger packages

| Source package | Removed API | Replacement | Replacement location | Notes |
| --- | --- | --- | --- | --- |
| `Linger.Configuration` | `AppSettingsHelper.CovertToObject<T>` | `ConvertToObject<T>` | `Linger.Configuration` | Corrects the method name typo. |
| `Linger.AspNetCore.Jwt.Contracts` | `IJwtService.TryRefreshTokenAsync` | `RefreshTokenResultAsync` | `Linger.AspNetCore.Jwt.Contracts` | The replacement also provides an error message. |
| `Linger.Email.AspNetCore` | `ConfigureEmail` / `ConfigureMailKit` | `AddEmailService` | `Linger.Email.AspNetCore` | The replacement returns `IServiceCollection` for chaining. |
| `Linger.Excel.Contracts` | `DataTableToFile` / `DataSetToFile` | `DataTableToExcel` / `DataSetToExcel` | `Linger.Excel.Contracts` | The replacement names match the Excel export operation. |
| `Linger.Results` | `ExecuteResult`, `ExecuteResult<T>`, and `ResultCompat` | `Result` / `Result<T>` | `Linger.Results` | Replace the legacy result types and conversion helpers. |
| `Linger.Results` | `ErrorObj` and `ToErrorObj` | `Error` and a caller-owned error collection | `Linger.Results` / application code | The legacy aggregation shape has no one-to-one replacement. |
| `Linger.FileSystem` | `ILocalFileSystem.Exists()` / `ExistsAsync()` | `DirectoryExistsAsync(fileSystem.RootDirectoryPath)` | `Linger.FileSystem` | Use the standard directory-existence API. `LocalFileSystem` also creates its configured root directory during construction. |
| `Linger.FileSystem` | `ILocalFileSystem.CreateIfNotExists()` / `CreateIfNotExistsAsync()` | `CreateDirectoryIfNotExistsAsync(fileSystem.RootDirectoryPath)` | `Linger.FileSystem` | Use the standard directory-creation API. |
| `Linger.FileSystem` | `IFileSystemOperations.IsDirectoryAsync(path)` | `DirectoryExistsAsync(path)` | `Linger.FileSystem` | The removed method was a complete alias. |
| `Linger.FileSystem` | `IRemoteFileSystem.ServerDetails()` | `IRemoteFileSystem.ServerDetails` | `Linger.FileSystem` | Server details are cached data and are now exposed as a read-only property. |
| `Linger.FileSystem` | `RemoteSystemSetting` | `FtpFileSystemOptions` / `SftpFileSystemOptions` | `Linger.FileSystem.Ftp` / `Linger.FileSystem.Sftp` | Select the protocol-specific options type. The configurable `Type` property was removed because the implementation determines the protocol. |
| `Linger.FileSystem.Ftp` | `UploadFileAsync(localPath, destinationDirectory, destinationFileName, ...)` | `UploadFileAsync(localPath, destinationFilePath, ...)` | `Linger.FileSystem.Ftp` | Construct the complete destination path before calling the upload API. |
| `Linger.FileSystem.Ftp` | `ListDirectoryAsync(...)` | `ListFilesAsync(...)` / `ListDirectoriesAsync(...)` | `Linger.FileSystem.Ftp` | Use the protocol-independent listing APIs instead of the FluentFTP-specific object-type filter. |
| `Linger.FileSystem.Sftp` | `UploadFileAsync(localPath, destinationDirectory, destinationFileName, ...)` | `UploadFileAsync(localPath, destinationFilePath, ...)` | `Linger.FileSystem.Sftp` | Construct the complete destination path before calling the upload API. |
| `Linger.FileSystem.Sftp` | `Connect()` / `Disconnect()` | `ConnectAsync()` / `DisconnectAsync()` | `Linger.FileSystem.Sftp` | Connection management now uses the common asynchronous remote-file-system contract. |
| `Linger.FileSystem.Sftp` | `SetRootAsWorkingDirectoryAsync()` | `SetWorkingDirectoryAsync("/")` | `Linger.FileSystem.Sftp` | The removed method only supplied the root path. |
| `Linger.Ldap.Novell` | `ConnectAsync(...)`, `Disconnect()`, `IsConnected()`, and `IDisposable` | No replacement is required. Call `ILdapClient` operations directly. | `Linger.Ldap.Novell` | Each operation now creates, binds, and disposes its own connection so concurrent calls cannot share credentials or disconnect each other. |
| `Linger.Ldap.Contracts` | `ILdap` | `ILdapClient` | `Linger.Ldap.Contracts` | Renamed so the contract states what it represents: a client for a directory server. |
| `Linger.Ldap.Contracts` | `AdUserInfo` | `LdapUserInfo` | `Linger.Ldap.Contracts` | The model is provider-agnostic and is not limited to Active Directory. |
| `Linger.Ldap.ActiveDirectory` | `Ldap` | `AdLdapClient` | `Linger.Ldap.ActiveDirectory` | The previous name collided with the `Linger.Ldap` namespace and with the Novell implementation of the same name. |
| `Linger.Ldap.ActiveDirectory` | `UserPrincipal.ToAdUser()`, `DirectoryEntry.ToAdUserInfo()`, `SearchResultCollection.ToAdUsersInfo()` | `ToLdapUserInfo()`, `ToLdapUserInfo()`, `ToLdapUsersInfo()` | `Linger.Ldap.ActiveDirectory` | Renamed to match `LdapUserInfo`. |
| `Linger.Ldap.Novell` | `Ldap` | `NovellLdapClient` | `Linger.Ldap.Novell` | The previous name collided with the `Linger.Ldap` namespace and with the Active Directory implementation of the same name. |
| `Linger.Ldap.Novell` | `LdapEntry.ToAdUser()` | `LdapEntry.ToLdapUserInfo()` | `Linger.Ldap.Novell` | Renamed to match `LdapUserInfo`. |

## No one-to-one replacement

| Source package | Removed API | Migration approach | Replacement location | Reason for removal |
| --- | --- | --- | --- | --- |
| `Linger.Utils` | `AesEncrypt` | Use `AesEncryptAuthenticated`. Existing legacy ciphertext can still be read by `AesDecrypt`. | `Linger.Utils` | AES-CBC encryption without authentication does not protect ciphertext integrity. |
| `Linger.Utils` | `GuidCode.GetInt32UniqueCode` / `GetInt64UniqueCode` | Use `Guid.NewGuid()`; on .NET 9 or later, consider `Guid.CreateVersion7()` when time ordering is useful. | .NET BCL | Truncating a GUID does not provide uniqueness. |
| `Linger.Utils` | `object.IsNullOrEmpty` / `IsNotNullOrEmpty` | Use a type-specific check such as `string.IsNullOrEmpty`, collection count checks, or `Guid?` extensions. | .NET BCL / `Linger.Utils` | Arbitrary objects do not have a well-defined empty state. |
| `Linger.Utils` | `ToTarget<T>`, `TryToTarget<T>`, `ToTargetOrDefault<T>`, and `ToTargetOrNull<T>` | Use a specific `ToInt`, `TryToDateTime`, `TryToGuid`, and similar conversion API; for a runtime target type, use `TypeConverter.TryConvert`. | `Linger.Utils` | Generic conversion concealed supported types and failure semantics. |
| `Linger.Utils` | `TypeConverter.ConvertTo`, `TryConvertTo`, and `TryConvertKnownType` | Use `TypeConverter.TryConvert`. | `Linger.Utils` | The retained API has an explicit, AOT-friendly scalar support boundary. |
| `Linger.Utils` | `ToTree` | Build the hierarchy in the caller with an explicit node model, root rule, duplicate-key policy, and cycle handling. | Application code | The generic helper concealed essential tree-construction semantics. |
| `Linger.Utils` | `FileHelper.DeleteFolderFiles` | Perform the required `Directory` and `File` operations in the caller. | .NET BCL / application code | The recursive delete-by-name behavior was too implicit for a general utility API. |
| `Linger.Utils` | `ExtensionMethodSetting` defaults | Configure `Encoding`, `CultureInfo`, buffer sizes, and `JsonSerializerOptions` explicitly. Use `JsonDefaults.CreateRequestOptions`, `CreateResponseOptions`, or `ApplyDefaultConfiguration` for JSON. | .NET BCL / `Linger.Json` | Process-wide mutable defaults made behavior difficult to reason about. |
| `Linger.Utils` | Other `DataContractJsonSerializer` helpers | Use `System.Text.Json.JsonSerializer`. | .NET BCL | The project standardizes on `System.Text.Json`. |
| `Linger.Utils` | `DeserializeDynamicJsonObject` and `JsonTextAccessor` | Use `JsonDocument`, `JsonElement`, or `JsonNode` according to the required mutability. | .NET BCL | Dynamic JSON obscured schema and runtime failure modes. |

## Behavioral changes

- **`Linger.Utils` -> `Linger.Utils` / `Linger.Reflection`**: The removed
  `DataTable.ToListAsync` methods did not perform asynchronous I/O. Calling
  `ToList` is synchronous and avoids a misleading async API.
- **`Linger.Utils` -> `Linger.Reflection`**: String order expressions accept only `asc` or `desc` (case-insensitive).
  Invalid directions now throw `ArgumentException` rather than being treated
  as descending order.
- **`Linger.Utils`**: `FileInfoExtensions.Delete` no longer has a default exception policy. Call
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
