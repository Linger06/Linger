# Linger 2.0 migration guide

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

This guide covers API and behavioral changes across Linger packages. Check the current development-branch requirements first; the replacement tables and examples below also cover upgrades starting with `2.0.0-preview.1`.

## 2.0.0-preview.5 development branch: upgrade checklist

This section describes the current branch, not a claim about packages already published to NuGet. Source-compatible changes can still affect data formats, filtering and failure handling.

### Before deployment: configure identity and transport security

| Component | Configuration / new behavior | Acceptance check |
| --- | --- | --- |
| LDAP identity and passwords | If you ignored the profile and used only `IsValid`, switch to AD `ValidateCredentials` or Novell `ValidateCredentialsAsync` (inject `IActiveDirectoryCredentialValidator` / `ILdapCredentialValidator`). Neither checks OU or group membership. For profiles or OU restrictions, keep `ValidateUser` / `ValidateUserAsync`: a unique verified profile is required. There is no first-match display-name or business-filter fallback; never fall back to a password check on failure. | `LdapConfig.Security` defaults to `false`: AD uses signed and sealed Negotiate for validation; Novell user and service-account password binds require explicit `true` and a trusted LDAPS certificate. **Profile validation only:** AD needs an actual controller DNS name and read access to the domain SID, user SID and canonical name; Novell requires Who Am I and permission to read the bound profile. See [Novell](../Linger.Ldap.Novell/README.md) / [AD](../Linger.Ldap.ActiveDirectory/README.md). |
| SMTP (including the ASP.NET Core wrapper) | Set `UseStartTls=true` for required STARTTLS; set only `UseSsl=true` for TLS on connect. STARTTLS takes precedence if both are enabled. Both default to false, permitting anonymous plaintext only; any nonempty `UserName` or `Password` forbids plaintext. Deployments relying on `UseSsl=true` to negotiate STARTTLS automatically must change their settings. | Test the server's actual TLS mode in staging; failed negotiation must not fall back to plaintext. See [Email](../Linger.Email/README.md). |
| SFTP | Both password and private-key authentication require `SftpFileSystemOptions.HostKeyFingerprintSha256`: `SHA256:` followed by 43 unpadded Base64 characters. `CertificatePath` is the client key; it does not verify the server. | Obtain the fingerprint from an independent trusted source. Missing, malformed or mismatched pins must not succeed. Verify a rotated key before updating the pin and recreating the client. See [SFTP](../Linger.FileSystem.Sftp/README.md). |
| WinForms example | Set `LINGER_WINFORMS_API_ORIGIN` and `LINGER_WINFORMS_REFRESH_ORIGIN` before startup, even if both origins are identical. Provide `https://host[:port]` only; enter full endpoints in the form. Neither client follows redirects automatically. | Restart after changing configuration. Trust every path on each configured origin; do not use an origin shared with untrusted applications. See the [example configuration](../../examples/Linger.HttpClient.WinForms/README.md). |
| ASP.NET Core error responses | `ToActionResult` and `ToHttpResult` preserve the original status for 500–599, but return generic `title` and `detail` only: no application codes, exception messages or `errors` extension. Field-level 4xx behavior is unchanged. | Handle server failures by status, not raw text or the presence of `errors`. Retain diagnostics in server-side logs. |

### Data, file and client behavior

| Area | New behavior | Upgrade action |
| --- | --- | --- |
| Excel (both providers) | File exports write to a temporary file and replace the destination only after success. Automatic mapping now throws `InvalidCastException` with the cell location for a nonempty, matched value that cannot be converted, rather than leaving the property at its default. | Use an explicit `ExcelRow` mapper if invalid values must be skipped or reported without aborting the import. |
| Excel numbers and NPOI | Supported integers are written as exact text if the provider cannot preserve them in a numeric cell; existing `UInt64` overflow restrictions still apply. NPOI `.xls` conversion preserves all sheets, their names and order, and the date system. Repeated clones no longer overwrite earlier copies. Conversion and cross-workbook cloning preserve error cells and their codes instead of writing ordinary numbers. | Accept text or numeric cells for long identifiers. Do not rely on a single output sheet named `Sheet1`. See the [Excel contracts](../Linger.Excel.Contracts/README.md). |
| `Linger.HttpClient.Standard` | Whole-file downloads accept only `200` or `203`, including a valid empty file. Other 2xx responses and bodies that do not match their declared length fail without replacing the target. Unknown-length streams remain supported. Status-only writes skip parsing successful bodies; existing third-party `IHttpClient` implementations keep their previous dispatch. On legacy targets a timeout ends the caller's wait for string and byte-array reads, not necessarily the underlying I/O. | Use `SendAsync` for `202` status bodies, `206` ranges, or other raw responses, and dispose the response. |
| Local files and FTP | Local uploads validate their temporary file before replacement. `CleanupOnValidationFailure=false` retains an invalid temporary file for diagnosis, not the destination. FTP no longer deletes the target before replacing it or automatically retries the final rename. `FileHelper.CopyDir` rejects existing links and junctions on the destination path. | Use the real destination directory; verify that the FTP server supports rename-over-existing. Check the remote contents if the final acknowledgement is lost before retrying. |
| FTP deletion and SFTP uploads | FTP `DeleteAsync` no longer retries a delete command. SFTP can still retry the temporary upload for a seekable stream, but issues the final rename only once, including on overwrite. | A lost reply does not mean the operation failed on the server. Check the remote file and its version before retrying to avoid deleting or replacing another writer's file. |
| SQLite table enumeration | `GetTableNames` and `GetTableNamesAsync` now include previously omitted user tables such as `sqliteOrders`. System tables prefixed with `sqlite_` remain excluded. | Expect additional results; no schema change is required. |
| Deleted `DataTable` rows | JSON and both Excel providers skip `Deleted` rows when exporting a table or data set, preserving row order and source states. SQL Server bulk insert excludes deleted rows from its success count and returns zero if all rows are deleted. | Do not call `AcceptChanges()` to export. Read `DataRowVersion.Original` explicitly if you need deleted records; the bulk insert count need not equal `Rows.Count`. |
| SQL Server bulk insert (transaction behavior unchanged) | `BulkInsert` and `BulkInsertAsync` do not guarantee an atomic import. Earlier batches may already be committed when a later batch fails. | Reconcile by business key before retrying. For an all-or-nothing import, pass an explicit `SqlTransaction` to `SqlBulkCopy`. |
| `Linger.Json` | `DataTable` byte arrays become Base64 strings on the wire and remain strings on read. Column names are preserved when trimming would collide or yield an empty name. Mixed numeric columns may use `object` to avoid losing individual values. | Avoid casting an entire mixed column to `double`. Decode Base64 only for known binary fields. Historical `"System.Byte[]"` strings do not contain recoverable bytes. See the [JSON migration guide](../Linger.Json/MIGRATION.md). |
| Search and reflection | `SearchParameters` (AND) intersects with any nonempty `SearchOrParameters` group (OR within the group); text matching tolerates null properties. Reflection mapping skips deleted rows; property-path parsing terminates on unsupported receivers and `TrySetProperty` no longer writes a nested selection to the root. | Expect narrower search results; assign to child objects explicitly. |
| EF and background tasks | Global filters handle derived branches and shadow properties. The audit interceptor does not suppress save exceptions or retry automatically. If a save fails before commit and the caller explicitly saves again on the same `DbContext`, it regenerates pending audit rows only for entities that do not implement `ISoftDelete`. The task queue waits for asynchronous scope disposal before the next item. | Check custom filters and disposal code. Key-only soft deletes no longer write unloaded business columns. Do not rely on `Remove` to update untracked business fields. Load the entity first when a full audit snapshot is required. Dapper is unchanged; no database migration is required. |

## Direct replacements

| Source package | Removed API | Replacement | Replacement location | Notes |
| --- | --- | --- | --- | --- |
| `Linger.Utils` | `DataTable.ToListAsync<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` | Add a reference to `Linger.Reflection`; the removed method only wrapped synchronous reflection-based mapping in `Task.FromResult`. |
| `Linger.Utils` | Reflection-based `DataTable.ToList<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` | Add a reference to `Linger.Reflection`. Use the mapper or factory overloads in `Linger.Utils` when runtime reflection is unnecessary. |
| `Linger.Reflection` | `DataTable.ToList<T>(parallelProcessingThreshold: ...)` | `DataTable.ToList<T>()` | `Linger.Reflection` | Automatic parallel mapping was removed; the reflection-based overload now maps rows sequentially. Use an explicit mapper overload for predictable performance and readability. |
| `Linger.Utils` | `DataTable.ToListAsync<T>(Func<DataRow, T>)` | `DataTable.ToList<T>(Func<DataRow, T>)` | `Linger.Utils` | The removed method only wrapped synchronous mapping in `Task.FromResult`. |
| `Linger.Utils` | `DataTable.ToListAsync<T>(Func<T>, IReadOnlyDictionary<string, Action<T, object?>>)` | `DataTable.ToList<T>(Func<T>, IReadOnlyDictionary<string, Action<T, object?>>)` | `Linger.Utils` | The removed method only wrapped synchronous mapping in `Task.FromResult`. |
| `Linger.Utils` | `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` | `Linger.Utils` | Accept and forward the supplied cancellation token. An omitted `shouldRetry` no longer retries every exception; pass an explicit predicate to enable retries. |
| `Linger.Utils` | `RetryHelper.ExecuteAsync<T>` without result policy | `ExecuteAsync<T>(..., shouldRetryResult: ...)` | `Linger.Utils` | The generic API now supports retrying unsuccessful results directly. Result-based retries return the last result when attempts are exhausted. Recompile consumers because the public signature changed. |
| `Linger.FileSystem` | `FileOperationResult.FileSize` | Use `GetFileSize(...)` locally or `GetFileSizeAsync(...)` remotely | `Linger.FileSystem` | Operation results no longer carry size metadata. Query it only when needed. |
| `Linger.FileSystem` | `FileOperationResult.FullFilePath` / `FileHash` | `UploadedInfo.FullFilePath` / `HashData` | `Linger.FileSystem.Local` | Provider-specific metadata is returned only by the local naming upload APIs. |
| `Linger.FileSystem` | Local naming overloads of `UploadAsync(...)` | `UploadWithNamingAsync(...)` or `UploadFileWithNamingAsync(...)` | `Linger.FileSystem.Local` | The common `UploadAsync` now has one unambiguous stream-to-path meaning. |
| `Linger.Utils` | Assigning `ParameterList.Parameters` | Mutate `Parameters` or call `SetValue`, `Add`, `Remove`, or `Clear` | `Linger.Utils` | The dictionary reference is now read-only and can no longer be replaced or set to `null`. |
| `Linger.Utils` | `GuidCode.NewDateGuid` | `Guid.NewGuid().ToString("N")`, or `Guid.CreateVersion7().ToString("N")` on .NET 9+ | .NET BCL | The removed 10-character value had only four Base36 random characters and could not provide reliable uniqueness. Short business codes require a unique constraint and collision retry. |
| `Linger.Utils` | `GuidCode.CreateVersion7()` | `Guid.CreateVersion7()` | .NET BCL (.NET 9+) | The removed method only forwarded to the BCL and existed only on .NET 9 or later, so the public surface differed by target framework. |
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
| `Linger.Utils` | `FileHelper.GetExistingFileInfo(...)` | `new FileInfo(path)` | .NET BCL / `Linger.Utils` | Open the file and call `ComputeHashMd5()` only when a content fingerprint is required. |
| `Linger.Utils` | `ExtendedFileInfo` | `UploadedInfo` or an application-specific DTO | `Linger.FileSystem` / application code | Use `UploadedInfo` for upload results; otherwise define a model matching the application's metadata needs. |
| `Linger.Utils` | `BaseFileInfo` | `UploadedInfo` or an application-specific DTO | `Linger.FileSystem` / application code | The generic base type had no independent behavior or meaningful polymorphic use. |
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
| `Linger.Utils` | `double.Days/Hours/Minutes/Seconds/Milliseconds` | `TimeSpan.FromDays/FromHours/FromMinutes/FromSeconds/FromMilliseconds` | .NET BCL | The extensions only forward to the corresponding BCL factory methods. |
| `Linger.Utils` | `DateTime.DayInYear` | `DateTime.DayOfYear` | .NET BCL | The removed method returned `DayOfYear` directly. |
| `Linger.Utils` | `DateTime.IsBefore(other)` / `IsAfter(other)` | `date < other` / `date > other` | .NET BCL | The removed methods only compared the result of `DateTime.CompareTo`. |
| `Linger.Utils` | `string.IsGuid()` | `Guid.TryParse(value, out _)` | .NET BCL | The removed overload only forwarded to `Guid.TryParse`; `IsGuid(value, format)` remains available. |
| `Linger.Utils` | `OSPlatformHelper.GetOSVersion()` | `Environment.OSVersion.Version` | .NET BCL | The removed method only forwarded to this property. |
| `Linger.Utils` | `value.NotIn(values)` | `!value.In(values)` | `Linger.Utils` | The removed method only negated the result of `In`. |
| `Linger.Utils` | `StreamExtensions.ToFile` / `ToFileAsync` | Call `Directory.CreateDirectory` when needed, open the destination with `File.Create(path)`, and call `stream.CopyTo` / `CopyToAsync`; use `File.Copy` to copy an existing file | .NET BCL | The removed methods reset seekable streams to the beginning before copying, ignoring the caller's current position; atomic writes, permissions, and symbolic-link handling are caller policies. When writing to a temporary file and then replacing the destination is required, use the local file system in `Linger.FileSystem`. |

## Other Linger packages

> **Specialized guides**: breaking changes for `Linger.DataAccess` are documented in the [Linger.DataAccess migration guide](../Linger.DataAccess/MIGRATION.md); the `Linger.Json` package split is covered by the [Linger.Json migration guide](../Linger.Json/MIGRATION.md).

| Source package | Removed API | Replacement | Replacement location | Notes |
| --- | --- | --- | --- | --- |
| `Linger.Configuration` | `AppSettingsHelper.CovertToObject<T>` | `ConvertToObject<T>` | `Linger.Configuration` | Corrects the method name typo. |
| `Linger.AspNetCore.Jwt.Contracts` | `IJwtService.TryRefreshTokenAsync` | `RefreshTokenResultAsync` | `Linger.AspNetCore.Jwt.Contracts` | The replacement also provides an error message. |
| `Linger.Email.AspNetCore` | `ConfigureEmail` / `ConfigureMailKit` | `AddEmailService` | `Linger.Email.AspNetCore` | The replacement returns `IServiceCollection` for chaining. |
| `Linger.Email` | Reusing an SMTP connection through one `Email` instance | No API replacement; batch or queue high-volume sends | `Linger.Email` | `SendAsync` now creates and closes an SMTP client per call, so one `Email` instance supports concurrent sends. Do not rely on connection reuse. |
| `Linger.Excel.Contracts` | `DataTableToFile` / `DataSetToFile` | `DataTableToExcel` / `DataSetToExcel` | `Linger.Excel.Contracts` | The replacement names match the Excel export operation. |
| `Linger.Excel.Contracts` | `DataTableToExcelAsync` / `CollectionToExcelAsync` | `DataTableToExcel` / `CollectionToExcel` | `Linger.Excel.Contracts` | The providers serialize workbooks synchronously, so the removed APIs only added an asynchronous file copy after first buffering the entire workbook in memory. File exports now write directly to the destination stream. |
| `Linger.Excel.Contracts` | `ExcelExtensions.DataTableToFileAsync` / `ListToFileAsync` | `IExcel<TWorksheet>.DataTableToExcel` / `CollectionToExcel` callback overloads | `Linger.Excel.Contracts` | Provider-specific callbacks remain available while their completed-workbook output is written directly to the target file. |
| `Linger.Excel.Contracts` | Custom `IExcel<TWorksheet>` implementations / `AbstractExcelService<TWorkbook, TWorksheet>` subclasses | Implement the new callback-based `DataTableToExcel(...)` and `CollectionToExcel(...)` members, or derive from `ExcelBase<TWorkbook, TWorksheet>` | `Linger.Excel.Contracts` | These are required interface and abstract-base members. Direct custom implementations must add them to compile. |
| `Linger.Excel.Contracts` | `ExcelBase<TWorkbook, TWorksheet>.SaveWorkbookToStream(TWorkbook)` | `WriteWorkbook(TWorkbook, Stream)` | `Linger.Excel.Contracts` | Custom providers must write synchronously to the supplied stream. This removes the mandatory full-size `MemoryStream` allocation for file exports. |
| `Linger.Excel.Contracts` | Excel import extensions accepting `Func<DataRow, T>` | `IExcelService.ExcelToList[Async]` / `StreamToList[Async]` accepting `Func<ExcelRow, T>` | `Linger.Excel.Contracts` | The replacement maps worksheet rows directly and no longer creates an intermediate `DataTable`. Use `ExcelRow.Get<T>(columnName)` for typed access. |
| `Linger.Excel.Contracts` | Excel import extensions accepting `Func<T>` and `columnSetters` | `IExcelService.ExcelToList[Async]` / `StreamToList[Async]` accepting `Func<ExcelRow, T>` | `Linger.Excel.Contracts` | Construct and populate the target object in one mapper delegate. The general `DataTable` factory/setter overloads remain available for callers that already have a `DataTable`. |
| `Linger.Excel.Contracts` | Custom `IExcelService` implementations | Implement the collection-export overloads accepting `ExcelExportColumn<T>` | `Linger.Excel.Contracts` | Explicit-column export is now part of the service contract and writes directly to worksheets without creating an intermediate `DataTable`. |
| `Linger.Excel.Contracts` | `ExcelOptions.ParallelProcessingThreshold`, `UseBatchWrite`, and `BatchSize` | Remove these settings | `Linger.Excel.Contracts` | The former batch path only parallelized value extraction into a temporary array while worksheet writes remained serial. Providers now use one direct sequential write path, avoiding thread scheduling and extra allocations. |
| `Linger.Results` | `ExecuteResult`, `ExecuteResult<T>`, and `ResultCompat` | `Result` / `Result<T>` | `Linger.Results` | Replace the legacy result types and conversion helpers. |
| `Linger.Results` | `ErrorObj` and `ToErrorObj` | `Error` and a caller-owned error collection | `Linger.Results` / application code | The legacy aggregation shape has no one-to-one replacement. |
| `Linger.Results` | `ResultExtensions.Try` / `TryAsync` | Catch specific exceptions that can be converted into domain errors at the call boundary | Application code | The general result library no longer catches every exception; unexpected and cancellation exceptions should propagate naturally. |
| `Linger.Results.AspNetCore` | `ToProblemDetails()` / `ToProblemDetails<T>()` | `ToActionResult()` / `ToActionResult<T>()` | `Linger.Results.AspNetCore` | `ToActionResult` now returns `ProblemDetails` for failures; continue passing `failureStatusCode` when a custom failure status is required. |
| `Linger.Results` | Setting `Result.Status` / `Result.Errors` in subclasses | Create results through the `Success`, `Failure`, or `NotFound` factory methods | `Linger.Results` | Result status and errors are now read-only snapshots. Custom subclasses that relied on the protected setters must use factories or composition. |
| `Linger.HttpClient.Contracts` | `HttpClientBase` | Implement `IHttpClient`, including `SendAsync`, or derive from `StandardHttpClient` to customize the standard implementation | `Linger.HttpClient.Contracts` / `Linger.HttpClient.Standard` | The contracts package no longer contains serialization, error parsing, or file-transfer implementation. `Contracts` and `Standard` remain separate packages. |
| `Linger.HttpClient.Contracts` | `HttpMethodEnum` | `System.Net.Http.HttpMethod` | .NET BCL | Supports `PATCH`, `HEAD`, `OPTIONS`, and custom methods without maintaining a restricted enum. |
| `Linger.HttpClient.Contracts` | `HttpResponseMode` / `CallApiWithMode<T>` | Use `CallApi<T>` for typed responses and `DownloadToFileAsync` for files | `Linger.HttpClient.Contracts` | Buffering and resource ownership are determined by the operation instead of a separate response-mode parameter. |
| `Linger.HttpClient.Contracts` | `CallApi<T>(url, queryParams, timeout, ...)` GET overload | `GetAsync<T>(url, queryParams, headers, cancellationToken)` | `Linger.HttpClient.Contracts` | GET convenience is now an extension; `CallApi<T>` requires an `HttpMethod`. |
| `Linger.HttpClient.Contracts` | The `int? timeout` parameter on call methods | Use `GetWithTimeoutAsync<T>` for a one-off GET deadline; use a caller-owned `CancellationTokenSource` with `CancelAfter` for other requests | `Linger.HttpClient.Contracts` / .NET BCL | Timeouts are now expressed as `TimeSpan`. A one-off GET timeout returns a failed result, while user cancellation still propagates as `OperationCanceledException`. |
| `Linger.HttpClient.Contracts` | `SetToken`, `AddHeader`, and mutable `Options` | Pass dynamic headers through the `headers` parameter; configure fixed settings through `AddHttpClient` | `Linger.HttpClient.Contracts` / .NET DI | Dynamic credentials are isolated per request and cannot overwrite concurrent requests on a shared client. |
| `Linger.HttpClient.Contracts` | Dedicated form and `HttpContent` overloads of `CallApi<T>` | Use the unified `CallApi<T>(..., HttpMethod, requestBody, ...)` | `Linger.HttpClient.Contracts` | `IDictionary<string, string>` is still sent as form data and `HttpContent` is still sent directly; the client disposes content after the request. |
| `Linger.HttpClient.Standard` | Non-generic `CallApi(...)` | Use the non-generic `PostAsync`, `PutAsync`, or `DeleteAsync` extensions | `Linger.HttpClient.Contracts` | Void-response calls no longer require an implementation-specific overload set. |
| `Linger.HttpClient.Contracts` | File-upload overload accepting `byte[] fileData` | `UploadFileAsync<T>(..., Stream fileStream, ...)` | `Linger.HttpClient.Contracts` | Uploads use `StreamContent`; the input stream is disposed when the request completes. |
| `Linger.HttpClient.Contracts` | `DownloadStreamAsync`, `CallApi<Stream>`, and `CallApi<HttpResponseMessage>` | Use `DownloadToFileAsync` for files; use `SendAsync` for raw responses, SSE, and long-lived protocols | `Linger.HttpClient.Contracts` | Typed calls no longer change resource ownership based on the generic type. `SendAsync` explicitly requires callers to dispose the returned `HttpResponseMessage`. |
| `Linger.HttpClient.Contracts` | `CompressionHelper` / `MultipartHelper` | Configure compression through `HttpClientHandler.AutomaticDecompression`; use `UploadFileAsync` or BCL `MultipartFormDataContent` for uploads | .NET BCL / `Linger.HttpClient.Contracts` | Concrete implementation helpers were removed from the contracts package. |
| `Linger.HttpClient.Contracts` | Non-nullable `int` on `ProblemDetailsWithErrors.Status` | Nullable `int?` property | `Linger.HttpClient.Contracts` | RFC 7807 allows the `status` member to be omitted; readers must handle `null`. |
| `Linger.HttpClient.Standard` | Constructors accepting `HttpClientOptions` and `Create(...)` | Use `StandardHttpClient(string, ILogger<StandardHttpClient>?)` for simple scenarios, or inject `StandardHttpClient(HttpClient, ILogger<StandardHttpClient>?)` for advanced configuration | `Linger.HttpClient.Standard` | URL mode manages its underlying resources; handlers, certificates, proxies, and other advanced settings remain caller-configured. |
| `Linger.AspNetCore.Jwt.Contracts` | `IJwtService.CreateTokenAsync(string)` | `CreateTokenAsync(string, CancellationToken)` | `Linger.AspNetCore.Jwt.Contracts` | Token issuance now supports end-to-end cancellation. The optional parameter preserves source call syntax. |
| `Linger.AspNetCore.Jwt.Contracts` | `IRefreshableJwtService.RefreshTokenAsync(Token)` and matching extensions | Overloads accepting `CancellationToken` | `Linger.AspNetCore.Jwt.Contracts` | Refresh propagates caller cancellation; `RefreshTokenResultAsync` no longer swallows cancellation or unexpected exceptions. |
| `Linger.AspNetCore.Jwt` | `JwtService.GetClaimsAsync(string)` | `GetClaimsAsync(string, CancellationToken)` | `Linger.AspNetCore.Jwt` | Custom claim lookup now observes caller cancellation. Derived services must update the override signature and pass the token to downstream asynchronous calls. |
| `Linger.AspNetCore.Jwt` | `JwtServiceWithRefresh.GetExistRefreshTokenAsync` / `HandleRefreshToken` | `StoreRefreshTokenAsync` / `TryRotateRefreshTokenAsync` | `Linger.AspNetCore.Jwt` | Refresh rotation now performs an atomic compare-and-replace in storage to prevent concurrent reuse of one old token. Existing derived services must migrate to the new hooks. |
| `Linger.AspNetCore.Jwt` | `JwtOption.EnableRefreshToken` configuration examples | Remove the setting; enable refresh by registering `IRefreshableJwtService` | `Linger.AspNetCore.Jwt` | The property never existed in the actual options type; refresh capability is selected by the service implementation. |
| `Linger.FileSystem` | Local `FileExistsAsync` / `DirectoryExistsAsync` | `FileExists` / `DirectoryExists` | `Linger.FileSystem.Local` | Local metadata uses synchronous BCL APIs and is no longer wrapped as pseudo-asynchronous. Remote signatures are unchanged. |
| `Linger.FileSystem` | Local `CreateDirectoryIfNotExistsAsync` / `DeleteFileIfExistsAsync` | `CreateDirectoryIfNotExists` / `DeleteFileIfExists` | `Linger.FileSystem.Local` | Local directory and file mutations complete synchronously. |
| `Linger.FileSystem` | Local `OpenReadAsync` / `OpenWriteAsync` / `GetReaderAsync` / `GetWriterAsync` | `OpenRead` / `OpenWrite` / `GetReader` / `GetWriter` | `Linger.FileSystem.Local` | Opening local streams is synchronous; the returned file streams still support true asynchronous content I/O. |
| `Linger.FileSystem` | Local `GetFileSizeAsync` / `DeleteAsync` | `GetFileSize` / `Delete` | `Linger.FileSystem.Local` | Local metadata and deletion no longer return completed tasks. |
| `Linger.FileSystem` | `IFileSystem` | `IFileTransfer` | `Linger.FileSystem` | The shared contract now states its actual responsibility: true asynchronous content transfer. This is a source-breaking interface rename. |
| `Linger.FileSystem` | `IFileSystemOperations` | `IRemoteFileSystem` for remote operations; `ILocalFileSystem` for local operations | `Linger.FileSystem` | The intermediate interface was removed. Remote asynchronous capabilities now live directly on `IRemoteFileSystem`; local metadata and stream factories remain synchronous. |
| `Linger.FileSystem` | `ILocalFileSystem.Exists()` / `ExistsAsync()` | `DirectoryExists(fileSystem.RootDirectoryPath)` | `Linger.FileSystem.Local` | Use the synchronous local directory-existence API. `LocalFileSystem` also creates its configured root directory during construction. |
| `Linger.FileSystem` | `ILocalFileSystem.CreateIfNotExists()` / `CreateIfNotExistsAsync()` | `CreateDirectoryIfNotExists(fileSystem.RootDirectoryPath)` | `Linger.FileSystem.Local` | Use the synchronous local directory-creation API. |
| `Linger.FileSystem` | `IFileSystemOperations.IsDirectoryAsync(path)` | `DirectoryExistsAsync(path)` | `Linger.FileSystem` | The removed method was a complete alias. |
| `Linger.FileSystem` | `IBatchFileSystemOperations` | `ILocalBatchFileSystemOperations` | `Linger.FileSystem` | Batch operations are provided only by the local file system, and the interface name now makes that capability boundary explicit. |
| `Linger.FileSystem` | `IBatchFileSystemOperations.ListFilesAsync(...)` / `ListDirectoriesAsync(...)` | `ILocalBatchFileSystemOperations.ListFiles(...)` / `ListDirectories(...)` | `Linger.FileSystem` | Local directory enumeration uses synchronous BCL APIs and is no longer wrapped as pseudo-asynchronous or given a `CancellationToken` that cannot cancel the enumeration; FTP/SFTP keep true asynchronous listing through `IRemoteFileSystem`. |
| `Linger.FileSystem` | `ILocalBatchFileSystemOperations.DeleteFilesAsync(...)` | `DeleteFiles(...)` | `Linger.FileSystem` | `File.Delete` is synchronous. Batch deletion now runs sequentially on the calling thread instead of returning completed tasks through the asynchronous batch worker. Cancellation is checked between files. |
| `Linger.FileSystem` | `LocalFileSystemOptions.MaxDegreeOfParallelism` | Remove the setting | `Linger.FileSystem` | Local batch operations now run in input order. Upload and download retain asynchronous file I/O without scheduling concurrent workers; deletion remains synchronous. |
| `Linger.FileSystem.Ftp` / `Linger.FileSystem.Sftp` | Calling `ListFilesAsync(...)` / `ListDirectoriesAsync(...)` through concrete `FtpFileSystem` / `SftpFileSystem` types | Call the same signatures through `IRemoteFileSystem` | `Linger.FileSystem` | Remote directory enumeration is now formally declared by the remote interface; existing concrete-type calls require no changes. |
| `Linger.FileSystem` | `UploadFilesAsync(..., remoteDirectory, ...)` / `DownloadFilesAsync(remoteFilePaths, ...)` | `UploadFilesAsync(..., destinationDirectory, ...)` / `DownloadFilesAsync(sourceFilePaths, ...)` | `Linger.FileSystem` | These parameters already represented destination/source paths under the local root; callers using named arguments must update the parameter names. |
| `Linger.FileSystem` | Reassigning `BatchOperationResult.SucceededFiles` / `FailedFiles` after construction | Assign only during object initialization | `Linger.FileSystem` | The collection properties changed from `set` to `init` so returned results cannot have their collection references replaced. |
| `Linger.FileSystem` | Derived classes calling `FileSystemBase.ExecuteBatchItemAsync(...)` | Execute and record the batch item directly in the derived implementation | `Linger.FileSystem` | The unused protected helper was removed; external derived classes that called it must inline the corresponding exception and result-recording logic. |
| `Linger.FileSystem` | `IRemoteFileSystem.ServerDetails()` | `IRemoteFileSystem.ServerDetails` | `Linger.FileSystem` | Server details are cached data and are now exposed as a read-only property. |
| `Linger.FileSystem` | `RemoteSystemSetting` | `FtpFileSystemOptions` / `SftpFileSystemOptions` | `Linger.FileSystem.Ftp` / `Linger.FileSystem.Sftp` | Select the protocol-specific options type. The configurable `Type` property was removed because the implementation determines the protocol. |
| `Linger.FileSystem.Ftp` | `UploadFileAsync(localPath, destinationDirectory, destinationFileName, ...)` | `UploadFileAsync(localPath, destinationFilePath, ...)` | `Linger.FileSystem.Ftp` | Construct the complete destination path before calling the upload API. |
| `Linger.FileSystem.Ftp` | `ListDirectoryAsync(...)` | `ListFilesAsync(...)` / `ListDirectoriesAsync(...)` | `Linger.FileSystem.Ftp` | Use the protocol-independent listing APIs instead of the FluentFTP-specific object-type filter. |
| `Linger.FileSystem.Sftp` | `UploadFileAsync(localPath, destinationDirectory, destinationFileName, ...)` | `UploadFileAsync(localPath, destinationFilePath, ...)` | `Linger.FileSystem.Sftp` | Construct the complete destination path before calling the upload API. |
| `Linger.FileSystem.Sftp` | `Connect()` / `Disconnect()` | No explicit call is required; the first operation connects automatically and instance disposal uses `using` | `Linger.FileSystem.Sftp` | SSH.NET does not provide a true asynchronous disconnect, so SFTP no longer exposes pseudo-asynchronous disposal. |
| `Linger.FileSystem` | `IRemoteFileSystem : IAsyncDisposable` | `IRemoteFileSystem : IDisposable`; implementations with true asynchronous disconnect use `IAsyncRemoteFileSystem` | `Linger.FileSystem` | Synchronous and asynchronous disposal capabilities are separated so SFTP does not expose a pseudo-asynchronous contract. |
| `Linger.FileSystem.Sftp` | `SetRootAsWorkingDirectoryAsync()` | `SetWorkingDirectoryAsync("/")` | `Linger.FileSystem.Sftp` | The removed method only supplied the root path. |
| `Linger.Ldap.Novell` | `ConnectAsync(...)`, `Disconnect()`, `IsConnected()`, and `IDisposable` | No replacement is required. Call `ILdapClient` operations directly. | `Linger.Ldap.Novell` | Each operation now creates, binds, and disposes its own connection so concurrent calls cannot share credentials or disconnect each other. |
| `Linger.Ldap.Contracts` | `ILdap` | `ILdapClient` | `Linger.Ldap.Contracts` | Renamed so the contract states what it represents: a client for a directory server. |
| `Linger.Ldap.Contracts` | `AdUserInfo` | `LdapUserInfo` | `Linger.Ldap.Contracts` | The model is provider-agnostic and is not limited to Active Directory. |
| `Linger.Ldap.ActiveDirectory` | `Ldap` | `AdLdapClient` | `Linger.Ldap.ActiveDirectory` | The previous name collided with the `Linger.Ldap` namespace and with the Novell implementation of the same name. |
| `Linger.Ldap.ActiveDirectory` | `ILdapClient` and `ValidateUserAsync` / `FindUserAsync` / `GetUsersAsync` / `SearchUsersByFilterAsync` / `UserExistsAsync` | `IActiveDirectoryClient` and `ValidateUser` / `FindUser` / `GetUsers` / `SearchUsersByFilter` / `UserExists` | `Linger.Ldap.Contracts` | `System.DirectoryServices` exposes synchronous directory operations; the public contract now reports that execution model instead of returning already-completed tasks. |
| `Linger.Ldap.Contracts` | `Task<IEnumerable<LdapUserInfo>>` search results | `Task<IReadOnlyList<LdapUserInfo>>` | `Linger.Ldap.Contracts` | Search operations materialize results before disposing provider resources, so the contract now states that behavior. |
| `Linger.Ldap.Contracts` | String-delimited `ProxyAddresses` / `OtherTelephone` | `string[]?` properties | `Linger.Ldap.Contracts` | LDAP multi-value attributes are preserved without provider-specific delimiters. |
| `Linger.Ldap.Novell` | `Ldap` | `NovellLdapClient` | `Linger.Ldap.Novell` | The previous name collided with the `Linger.Ldap` namespace and with the Active Directory implementation of the same name. |
| `Linger.Ldap.Novell` | `LdapEntry.ToAdUser()` | `LdapEntry.ToLdapUserInfo()` | `Linger.Ldap.Novell` | Renamed to match `LdapUserInfo`. |
| `Linger.Excel.EPPlus` | `EPPlusExcel`, `ExcelWorksheetExtensions.TrimLastEmptyRows` / `IsLastRowEmpty` | Use `Linger.Excel.Npoi` or `Linger.Excel.ClosedXML` | `Linger.Excel.Npoi` / `Linger.Excel.ClosedXML` | The EPPlus provider was removed entirely; the remaining providers implement the same `IExcelService` contract and can be swapped in directly. |
| `Linger.SharedKernel` | `BaseSearchPageList` | `BaseSearchPagedList` | `Linger.SharedKernel` | A duplicate type equivalent to `BaseSearchPagedList` (both inherit `BaseSearch` and implement `IBaseSearchPagedList`); only the name spelling differed. |
| `Linger.Audit` | `IAuditUserProvider.UserName`; the `string` return type of `GetUser()` | Implement `GetUser()` and return `null` when no authenticated user exists | `Linger.Audit.Contracts` | User identity now comes solely from `GetUser()`; the return type became `string?`, so interface implementers must adjust. |
| `Linger.Audit` | `AuditTrailEntry.Changes` / `TempProperties` | `CurrentValuesSnapshot`, or compare `OldValues` / `NewValues` yourself | `Linger.EFCore.Audit` | Change tracking now uses explicit old/new value snapshots. |
| `Linger.FileSystem` | `IRemoteFileSystem.IsConnected()` / `ConnectAsync()` / `DisconnectAsync()` | No explicit call needed; remote implementations manage connection lifetime internally | `Linger.FileSystem` | Connection management moved out of the shared contract; the corresponding members on `FtpFileSystem` / `SftpFileSystem` are internal implementation details. |
| `Linger.Excel.Contracts` | Legacy overloads such as `ExcelToDataSet(string, int, ...)`, `ExcelToDataSet(string, Func<string, int?>, ...)`, and `StreamToDataSet(...)` | `ExcelToDataSet(string, IEnumerable<string>? = null, int = 0, bool = false)` / `ExcelToDataSet(string, Func<string, int?>, IEnumerable<string>? = null, bool = false)` | `Linger.Excel.Contracts` | The header-row index parameter moved later in the signature; positional calls such as `ExcelToDataSet(path, 1)` must become `ExcelToDataSet(path, headerRowIndex: 1)` or pass `sheetNames` explicitly. |
| `Linger.Excel.Contracts` | `ExcelToDataSetAsync` / `StreamToDataSetAsync` / `ExcelToDataTableAsync` / `StreamToDataTableAsync` | The synchronous `ExcelToDataSet` / `StreamToDataSet` / `ExcelToDataTable` / `StreamToDataTable` | `Linger.Excel.Contracts` | Import parses synchronously, so the pseudo-async wrappers were removed; switch to the synchronous APIs. |
| `Linger.Excel.Contracts` | Positional parameter order of the callback-based `IExcel<TWorksheet>.DataSetToExcel` / `DataTableToExcel` | `DataSetToExcel(DataSet, string, Action<IWorksheetExportContext<TWorksheet>>, string defaultSheetName = ...)` / `DataTableToExcel(DataTable, string, Action<TWorksheet, DataColumnCollection, DataRowCollection>?, string sheetsName = ..., string title = "", Action<TWorksheet>? styleAction = null)` | `Linger.Excel.Contracts` | The callback parameter moved to the third position; callers using positional arguments must reorder them. |

## No one-to-one replacement

| Source package | Removed API | Migration approach | Replacement location | Reason for removal |
| --- | --- | --- | --- | --- |
| `Linger.Utils` | `AesEncrypt` | Use `AesEncryptAuthenticated`. Existing legacy ciphertext can still be read by `AesDecrypt`, which is now obsolete and intended only for migrating legacy ciphertext. | `Linger.Utils` | AES-CBC encryption without authentication does not protect ciphertext integrity. |
| `Linger.Utils` | `GuidCode.GetInt32UniqueCode` / `GetInt64UniqueCode` | Use `Guid.NewGuid()`; on .NET 9 or later, consider `Guid.CreateVersion7()` when time ordering is useful. | .NET BCL | Truncating a GUID does not provide uniqueness. |
| `Linger.Utils` | `object.IsNullOrEmpty` / `IsNotNullOrEmpty` | Use a type-specific check such as `string.IsNullOrEmpty`, collection count checks, or `Guid?` extensions. | .NET BCL / `Linger.Utils` | Arbitrary objects do not have a well-defined empty state. |
| `Linger.Utils` | `ToTarget<T>`, `TryToTarget<T>`, `ToTargetOrDefault<T>`, and `ToTargetOrNull<T>` | Use a specific `ToInt`, `TryToDateTime`, `TryToGuid`, and similar conversion API; for a runtime target type, use `TypeConverter.TryConvert`. | `Linger.Utils` | Generic conversion concealed supported types and failure semantics. |
| `Linger.Utils` | `TypeConverter.ConvertTo`, `TryConvertTo`, and `TryConvertKnownType` | Use `TypeConverter.TryConvert`. | `Linger.Utils` | The retained API has an explicit, AOT-friendly scalar support boundary. |
| `Linger.Utils` | `ToTree` | Build the hierarchy in the caller with an explicit node model, root rule, duplicate-key policy, and cycle handling. | Application code | The generic helper concealed essential tree-construction semantics. |
| `Linger.Utils` | `FileHelper.DeleteFolderFiles` | Perform the required `Directory` and `File` operations in the caller. | .NET BCL / application code | The recursive delete-by-name behavior was too implicit for a general utility API. |
| `Linger.Utils` | `ExtensionMethodSetting` defaults | Configure `Encoding`, `CultureInfo`, buffer sizes, and `JsonSerializerOptions` explicitly. Use `JsonDefaults.CreateRequestOptions`, `CreateResponseOptions`, or `ApplyDefaultConfiguration` for JSON. | .NET BCL / `Linger.Json` | Process-wide mutable defaults made behavior difficult to reason about. |
| `Linger.Utils` | Other `DataContractJsonSerializer` helpers | Use `System.Text.Json.JsonSerializer`. | .NET BCL | The project standardizes on `System.Text.Json`. |
| `Linger.Utils` | `DeserializeDynamicJsonObject` and `JsonTextAccessor` | Use `JsonDocument`, `JsonElement`, or `JsonNode` according to the required mutability. | .NET BCL | Dynamic JSON obscured schema and runtime failure modes. |
| `Linger.Ldap.ActiveDirectory` | Logger-only `AdLdapClient` constructor | Use the parameterless constructor for the current Windows domain, or register `LdapConfig` with options when a custom logger and explicit configuration are required. | `Linger.Ldap.ActiveDirectory` | A logger alone does not express connection or search configuration. |
| `Linger.Ldap.ActiveDirectory` | `GetEntryByUsername` | Use `IActiveDirectoryClient.FindUser` for the shared user model. Use `System.DirectoryServices` directly when native ADSI access is required. | `Linger.Ldap.ActiveDirectory` / application code | Returning provider resources exposed ownership and disposal concerns outside the client contract. |
| `Linger.Ldap.ActiveDirectory` | Public `UserPrincipal`, `DirectoryEntry`, and `SearchResultCollection` mapping extensions | Use `IActiveDirectoryClient` search operations. | `Linger.Ldap.Contracts` | Three mapping paths duplicated behavior and returned inconsistent account-security details. |

## Behavioral changes

- **`Linger.AspNetCore.Jwt`**: `JwtOptions.Issuer` and `JwtOptions.Audience` no longer default to `http://localhost` and must now be configured explicitly. Token issuance, bearer authentication, and expired-token refresh use the same issuer and audience validation rules.
- **`Linger.AspNetCore.Jwt`**: The effective signing key supplied by `JwtOptions.SecurityKey` or the `SECRET` environment variable must contain at least 32 UTF-8 bytes. Invalid configuration throws when the JWT service is created or bearer authentication is registered.
- **`Linger.AspNetCore.Jwt`**: Authentication challenges and forbidden responses no longer write custom Chinese JSON. They now use the standard ASP.NET Core JwtBearer `401 Unauthorized` and `403 Forbidden` responses. Clients that depended on the previous response body must use the HTTP status code instead.
- **`Linger.HttpClient`**: The client no longer appends a `culture` query parameter to every URL. Add it explicitly through query parameters or a `DelegatingHandler` when required.
- **`Linger.HttpClient`**: Caller cancellation now throws `OperationCanceledException` instead of returning a failed `ApiResult`; timeouts caused by `HttpClient.Timeout` still return a failed result.
- **`Linger.HttpClient`**: Each operation creates and disposes its own `HttpRequestMessage` and `HttpResponseMessage`. Supplied `HttpContent` and file streams passed to `UploadFileAsync` are disposed when the request completes.
- **`Linger.HttpClient`**: `ApiResult.IsSuccess` now requires a 2xx status and no transport, deserialization, or structured errors.
- **`Linger.HttpClient`**: ProblemDetails is parsed only when its media type or standard fields identify it. Legacy `IEnumerable<Error>` arrays remain a compatibility fallback.
- **`Linger.Utils` -> `Linger.Utils` / `Linger.Reflection`**: The removed
  `DataTable.ToListAsync` methods did not perform asynchronous I/O. Calling
  `ToList` is synchronous and avoids a misleading async API.
- **`Linger.Utils` -> `Linger.Reflection`**: String order expressions accept only `asc` or `desc` (case-insensitive).
  Invalid directions now throw `ArgumentException` rather than being treated
  as descending order.
- **`Linger.Utils`**: `FileInfoExtensions.Delete` no longer has a default exception policy. Call
  sites must choose fail-fast or consolidated exceptions explicitly.
- **`Linger.Utils`**: `AesDecrypt` is obsolete because it silently falls back to the unauthenticated
  legacy format. Decrypt `AesEncryptAuthenticated` output with `AesDecryptAuthenticated`; keep
  `AesDecrypt` only for re-encrypting legacy ciphertext.
- **`Linger.Utils`**: `RetryHelper` no longer treats every `OperationCanceledException` as final. Only
  cancellation of the caller's `cancellationToken` propagates immediately; other cancellations, such as
  `HttpClient` timeouts, are passed to `shouldRetry`. The synchronous `Execute` has no caller token and
  passes every exception to `shouldRetry`.
- **`Linger.Utils`**: `RetryOptions.Jitter` now selects each delay from
  `[delay × (1 − Jitter), delay × (1 + Jitter)]` and caps it at `MaxDelayMilliseconds`. Previously the
  delay could be zero or exceed the configured maximum.
- **`Linger.Utils`**: `IsEmail`, `IsMultipleEmail`, `IsDomainName`, and `IsIpv4` reject input with a
  trailing newline. `IsMultipleEmail` accepts top-level domains longer than four letters, and `IsIpv4`
  accepts only ASCII digits on every target framework.
- **`Linger.Utils`**: Compatibility polyfills are now `internal`. Earlier previews exposed them publicly on
  .NET Standard 2.0 and .NET Framework 4.7.2, and `Base64Url` and `Convert.ToHexStringLower` on .NET 8.0 as well:
  the `ArgumentNullException`, `ArgumentException`, and `ArgumentOutOfRangeException` `Throw*` helpers,
  `MD5`/`SHA*` `HashData`, `System.Buffers.Text.Base64Url`, the nullable-annotation attributes,
  `IsExternalInit`, `RequiredMemberAttribute`, and `CallerArgumentExpressionAttribute`. Code that used them
  through a `Linger.Utils` reference must supply its own polyfills on those target frameworks. The attributes
  that annotate `Linger.Utils` APIs, such as `[NotNullWhen]` and `required`, keep working because the compiler
  matches them by name. The other `Linger.*` packages compile their own `internal` copies from shared source,
  so none of them exposes these types either.
- **`Linger.Utils`**: `AesEncryptAuthenticated` now writes the `LNG4` payload, which records the PBKDF2-HMAC-SHA256
  iteration count (600,000) in an authenticated header. `AesDecryptAuthenticated` and `AesDecrypt` still read
  `LNG2`, `LNG3`, and legacy ciphertext, but earlier previews cannot read `LNG4` ciphertext. On decryption the
  header value must be between 100,000 and 10,000,000, otherwise a `CryptographicException` is thrown before any
  key derivation. The higher cost is noticeable on .NET Framework: each call costs about six times as much as the
  previous format, which is a fraction of a second on .NET 8 and later but often several seconds on .NET Framework,
  depending on the hardware.
- **`Linger.Utils`**: The .NET Standard 2.0 build no longer contains `AesEncryptAuthenticated`, `AesDecryptAuthenticated`,
  or `AesDecrypt`. .NET Standard 2.0 has no compile-time PBKDF2-HMAC-SHA256 API, and the reflection lookup and
  hand-written fallback that filled the gap have been removed. Projects that compile against the .NET Standard 2.0
  asset, such as libraries targeting .NET Standard 2.0 and .NET 6 or .NET 7 applications, now fail to compile. Target
  .NET 8 or later, or .NET Framework 4.7.2, to keep using these APIs and to decrypt existing ciphertext.
- **`Linger.Utils`**: `DeleteBrackets` now removes every balanced pair of parentheses, including nested pairs, and
  keeps unmatched parentheses. Previously `"a(b(c)d)e"` returned `"a(be"` and `"a(b)c)d"` returned `"ad"`; they now
  return `"ae"` and `"ac)d"`. Input without nested or unmatched parentheses is unaffected.
- **`Linger.Utils`**: Exception messages thrown by the AES helpers, `RetryHelper`, and `TableRowTurnToColumn` are now
  English. Code that matched the previous Chinese text must match the new text or, preferably, the exception type.
- **`Linger.Utils`**: `FileInfo.Rename`, `RenameFileWithoutExtension`, and `ChangeExtension` accept only a plain file
  name. A name that is empty or whitespace, is `.` or `..`, or contains a directory or root now throws
  `ArgumentException`; previously `Rename("../x.txt")` moved the file out of its directory.
- **`Linger.Utils`**: Numeric parsing on strings and objects (`TryToInt`, `ToDecimal`, `TryToDouble`, and the rest of
  the family) validates thousands separators. The first group may have one to three digits and every later group exactly
  three, so `"1,234"` and `"1,234.5"` still parse, while `"3,14"` and `"1,5"`, which used to become 314 and 15, now fail.
- **`Linger.Utils`**: The `ToXxx`, `TryToXxx`, `ToXxxOrNull`, and `ToXxxOrDefault` methods on `object` share one rule:
  `ToXxx` returns the `TryToXxx` result or throws. Every integer type and every enum converts by numeric value, `bool`
  does not convert to a number, and other types are converted through their invariant-culture string. As a result
  `ToInt('A')` throws `InvalidCastException` instead of returning 65, `ToInt(true)` and `ToDouble(true)` throw instead of
  returning 1, `ToFloat(1e39)` throws `OverflowException` instead of returning infinity, `ToBool((ushort)5)` and
  `ToBool(DayOfWeek.Tuesday)` throw instead of returning `true`, and `ToIntOrNull(DayOfWeek.Monday)` returns 1 instead
  of `null`.
- **`Linger.Utils`**: Floating-point inputs at the upper edge of an integer range are rejected. `TryToInt(2147483648f)`
  and `TryToLong(9223372036854775808d)` used to report success with a saturated value. `TryToDecimal` no longer throws
  for a double equal to 2^96.
- **`Linger.Utils`**: `DataTable.Join` throws `ArgumentException` when paired key columns have different data types,
  unless one of them is `object`, instead of silently returning no matches. Key column names that contain commas are
  now accepted. `ClearEmptyRow` is unchanged, and its documentation now states that it modifies the table in place.
- **`Linger.Dapper`, `Linger.DataAccess.SqlServer`, `Linger.Excel.Contracts`, `Linger.FileSystem`, `Linger.FileSystem.Ftp`,
  `Linger.FileSystem.Sftp`, `Linger.Reflection`**: Exception messages and the error messages returned in file operation
  results are now English. Code that matched the previous Chinese text must be updated.
- **`Linger.Excel.Contracts`**: Excel-to-`DataTable` imports now infer each column's common CLR type
  and preserve values such as numbers, booleans, and `DateTime` instead of coercing every non-null
  cell to `string`. Empty or mixed-type columns use `object`. Call `Convert.ToString` explicitly when
  text output is required.
- **`Linger.Json`**: `DataTableJsonConverter` serializes values in `object` columns according to each
  value's runtime type, so numbers and booleans remain JSON numbers and booleans. When JSON values
  of incompatible token types share a column, deserialization uses an `object` column and preserves
  each token's corresponding .NET value type.
- **`Linger.Audit`**: `CreationAuditEntity.CreationTime` is no longer initialized to `DateTimeOffset.Now`; assign it explicitly when creating an entity. `CreatorId` now defaults to `string.Empty` instead of `null!`.

- **`Linger.Utils`**: `Truncate`, `Take`, and `TakeLast` keep their UTF-16 length limits but omit a surrogate pair
  crossing the cutoff. `RemoveLastNewLine` removes one trailing CRLF, LF, or CR sequence instead of all trailing newlines.
- **`Linger.Utils`**: Default `EnsureStartsWith`, `EnsureEndsWith`, and `RemovePrefixAndSuffix` comparisons are ordinal
  across target frameworks. Pass an explicit `StringComparison` when linguistic comparison is required.
- **`Linger.Utils`**: `IsCombinationOfEnglishNumberSymbol` requires Unicode punctuation or symbols; other letters,
  whitespace, and control characters no longer qualify as symbols. Email validation accepts apostrophes in the local part.
- **`Linger.Utils`**: Base64Url decoding on older targets now rejects standard Base64 `+` and `/` like the BCL.
  Padding and ASCII space, tab, CR, and LF remain accepted; whitespace is ignored before calculating padding.
- **`Linger.Utils`**: `IsToday` converts UTC inputs to the local calendar day. `GetDays` honors the configured calendar
  year and era, including the maximum Gregorian year; an omitted second date in `GetDateDifference` uses UTC for UTC inputs.
  `GetWeekCountOfYear` uses Gregorian years and culture-specific week rules, excluding next year's first ISO week.
  `ToDateTimeOffset` retains the BCL's range validation when a local offset makes UTC overflow.
- **`Linger.Utils`**: `DataTable` `Sum`, `Combine`, `Join`, `Paging`, `ToList`, and `TableRowTurnToColumn` now ignore rows
  whose `RowState` is `Deleted` and leave the source table's row states unchanged; `ClearEmptyRow` skips such rows. `Sum` throws
  `ArgumentException` when the column does not exist, including on an empty table, which previously returned 0.
  `ContainAllColumns` throws `ArgumentNullException` when `columnString` is `null` and falls back to the trimmed name when the
  original name is not found.
- **`Linger.Utils`**: `TableRowTurnToColumn` formats caption column names with `CultureInfo.InvariantCulture`, so the
  current culture no longer changes the result schema. An empty-string caption produces an `EMPTY` column and a `null` or `DBNull`
  caption produces a `NULL` column; values with an empty caption were previously dropped silently. Different captions that
  produce the same column name throw `DuplicateNameException`.
- **`Linger.Utils`**: `FullJoin` emits rows in outer-sequence order, each outer item followed by its matching inner items; unmatched
  inner items are appended in their original order, and `null` keys never match each other. It previously emitted rows grouped by key.
- **`Linger.Utils`**: `List<string>.ToSeparatedString` doubles single quotes inside elements when `singleQuoted` is `true`.
  Callers that already escape single quotes will now escape them twice; the method is not a substitute for parameterized queries.
- **`Linger.Utils`**: `ParameterList.Get<T>` throws an `InvalidCastException` that names the key when the value is `null` or not
  assignable to `T`; a `null` value for a reference type `T` previously returned `null`.
- **`Linger.Utils`**: Path string normalization treats both `/` and `\` as separators on every platform, so a `\` on Unix is
  converted to `/`. Recursive `FileHelper.Contains` no longer follows directory symbolic links or junctions and skips
  inaccessible subdirectories; files reachable only through a link are no longer found. `FileHelper.CopyFile` throws
  `ArgumentException` when the destination has no file name (for example, it ends with a directory separator).
  `FileHelper.GetDirectories` returns the enumerated paths when `filter` is supplied instead of converting them to
  `DirectoryInfo.FullName`. `FileHelper.GetFileNames` keeps the directory part when `containPath` is `true` and `containExtension`
  is `false`; it previously returned only the file name without its extension.
- **`Linger.Utils`**: When `paramName` is omitted, `EnsureIsNotNull`, `EnsureIsNotNullOrEmpty`, and `EnsureIsNotNullOrWhiteSpace`
  set the exception's `ParamName` to the caller's argument expression; `EnsureFileExists` and `EnsureDirectoryExists` keep the
  supplied parameter name when the path is `null`.
- **`Linger.Utils`**: Extension methods on arrays, `DataColumnCollection`, `DataRowCollection`, and `DataTableCollection` throw
  `ArgumentNullException` for a `null` target or callback, and array `RemoveRange` accepts a `startIndex` equal to the array length
  (it returns a copy). `ICollection<T>.RemoveAll` removes by position for `IList<T>` and by node for `LinkedList<T>`, so retained
  items that are `Equals`-equal to removed ones are no longer removed by mistake. `FileInfo.ChangeExtension` removes the extension
  when given an empty string; it previously produced a file name ending in a dot.

## Examples

### Retry operations

```csharp
var result = await retryHelper.ExecuteAsync(
    async cancellationToken =>
    {
        return await client.GetStringAsync(uri, cancellationToken);
    },
    shouldRetry: exception => exception is HttpRequestException or TimeoutException,
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

### HttpClient

Configure and inject the underlying `HttpClient`:

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

When no custom underlying client is required, pass the base URL directly and dispose `StandardHttpClient`:

```csharp
using var client = new StandardHttpClient("https://api.example.com/");
```

Do not place a bearer token in `HttpClient.DefaultRequestHeaders` on a desktop client that accepts arbitrary absolute URLs. `BaseAddress` does not restrict request destinations. Configure trusted HTTPS origins independently, validate the actual destination before attaching credentials, constrain the refresh client separately, and do not follow redirects automatically. See the [WinForms example](../../examples/Linger.HttpClient.WinForms/README.md).

In external-`HttpClient` mode, reuse the clients for the application lifetime and dispose the underlying `HttpClient` at shutdown. For multiple users or per-request tokens, add authorization only after validating the destination.

Use the BCL HTTP method type and pass dynamic authentication per request:

```csharp
var result = await httpClient.CallApi<User>(
    "users",
    HttpMethod.Post,
    requestBody: new CreateUserRequest("Ada"),
    headers: new Dictionary<string, string>
    {
        ["Authorization"] = $"Bearer {accessToken}"
    },
    cancellationToken: cancellationToken);
```

Use a caller-owned linked token for a shorter one-off deadline:

```csharp
using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
timeoutSource.CancelAfter(TimeSpan.FromSeconds(5));

var result = await httpClient.GetAsync<User>(
    "users/42",
    cancellationToken: timeoutSource.Token);
```

File uploads now use streams:

```csharp
var fileStream = File.OpenRead("report.pdf");
var result = await httpClient.UploadFileAsync<UploadResponse>(
    "files",
    HttpMethod.Post,
    fileStream,
    "report.pdf",
    cancellationToken: cancellationToken);
```

`UploadFileAsync` disposes `fileStream` when it completes. Use `SendAsync` for raw `HttpResponseMessage`, SSE, and other long-lived protocols, and dispose the returned response with `using`.

### Authenticated encryption

```csharp
string ciphertext = plaintext.AesEncryptAuthenticated(key);
string plaintext = ciphertext.AesDecryptAuthenticated(key);
```

### Walking up several directory levels

`GetParentDirectory(levels)` stopped at the filesystem root, while
`Path.GetDirectoryName` returns `null` once it passes the root. Chaining the
BCL call without a null check therefore breaks when `levels` exceeds the
actual depth. Clamp explicitly:

```csharp
static string GetAncestor(string path, int levels)
{
    var current = Path.GetFullPath(path);
    for (var i = 0; i < levels; i++)
    {
        var parent = Path.GetDirectoryName(current);
        if (parent is null) break;   // already at the root, stop here
        current = parent;
    }
    return current;
}
```
