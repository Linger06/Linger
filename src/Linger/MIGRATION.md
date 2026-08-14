# Migration to 2.0.0-preview.1

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

This guide covers breaking API removals across Linger packages. It is intended
for applications upgrading from the previous API surface.

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

## Other Linger packages

| Source package | Removed API | Replacement | Replacement location | Notes |
| --- | --- | --- | --- | --- |
| `Linger.Configuration` | `AppSettingsHelper.CovertToObject<T>` | `ConvertToObject<T>` | `Linger.Configuration` | Corrects the method name typo. |
| `Linger.AspNetCore.Jwt.Contracts` | `IJwtService.TryRefreshTokenAsync` | `RefreshTokenResultAsync` | `Linger.AspNetCore.Jwt.Contracts` | The replacement also provides an error message. |
| `Linger.Email.AspNetCore` | `ConfigureEmail` / `ConfigureMailKit` | `AddEmailService` | `Linger.Email.AspNetCore` | The replacement returns `IServiceCollection` for chaining. |
| `Linger.DataAccess` | .NET Framework 4.7.2 async transaction APIs: `BeginTransAsync`, `CommitAsync`, `RollbackAsync`, and `ExecuteTransactionAsync` | `BeginTrans`, `Commit`, `Rollback`, and `ExecuteTransaction` | `Linger.DataAccess` | Async transaction APIs are available on .NET 8 and later only. On .NET Framework 4.7.2, call the synchronous APIs directly rather than wrapping them in `Task.Run`. |
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
| `Linger.FileSystem.Sftp` | `Connect()` / `Disconnect()` | `ConnectAsync()` / `DisconnectAsync()` | `Linger.FileSystem.Sftp` | Connection management now uses the common asynchronous remote-file-system contract. |
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
- **`Linger.Excel.Contracts`**: Excel-to-`DataTable` imports now infer each column's common CLR type
  and preserve values such as numbers, booleans, and `DateTime` instead of coercing every non-null
  cell to `string`. Empty or mixed-type columns use `object`. Call `Convert.ToString` explicitly when
  text output is required.
- **`Linger.Json`**: `DataTableJsonConverter` serializes values in `object` columns according to each
  value's runtime type, so numbers and booleans remain JSON numbers and booleans. When JSON values
  of incompatible token types share a column, deserialization uses an `object` column and preserves
  each token's corresponding .NET value type.

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

For a single-user client such as WinForms, a dedicated instance can set its authorization header when the underlying `HttpClient` is created:

```csharp
var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.example.com/")
};
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", accessToken);

var client = new StandardHttpClient(httpClient);
```

In external-`HttpClient` mode, reuse `httpClient` and `client` for the application lifetime, and let the caller dispose `httpClient` when the application closes. If token refresh can overlap active requests, use a `DelegatingHandler` that reads the current token while sending. Continue using per-request `headers` when one client represents multiple users or the token varies by request.

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
string plaintext = ciphertext.AesDecrypt(key);
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
