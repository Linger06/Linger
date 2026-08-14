# 迁移到 2.0.0-preview.1

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

本文档说明各个 Linger 包中的破坏性 API 删除，适用于从旧版公共 API 升级的应用程序。

## 直接替代

| 原包 | 已删除 API | 替代方式 | 替代项位置 | 说明 |
| --- | --- | --- | --- | --- |
| `Linger.Utils` | `DataTable.ToListAsync<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` | 需要引用 `Linger.Reflection`；被删除的方法只是使用 `Task.FromResult` 包装基于反射的同步映射。 |
| `Linger.Utils` | 基于反射的 `DataTable.ToList<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` | 需要引用 `Linger.Reflection`。不需要运行时反射时，请使用 `Linger.Utils` 中的映射器或工厂重载。 |
| `Linger.Reflection` | `DataTable.ToList<T>(parallelProcessingThreshold: ...)` | `DataTable.ToList<T>()` | `Linger.Reflection` | 已移除自动并行映射；反射重载现在按顺序映射行。需要可预测性能和更高可读性时，请使用显式映射器重载。 |
| `Linger.Utils` | `DataTable.ToListAsync<T>(Func<DataRow, T>)` | `DataTable.ToList<T>(Func<DataRow, T>)` | `Linger.Utils` | 被删除的方法只是使用 `Task.FromResult` 包装同步映射。 |
| `Linger.Utils` | `DataTable.ToListAsync<T>(Func<T>, IReadOnlyDictionary<string, Action<T, object?>>)` | `DataTable.ToList<T>(Func<T>, IReadOnlyDictionary<string, Action<T, object?>>)` | `Linger.Utils` | 被删除的方法只是使用 `Task.FromResult` 包装同步映射。 |
| `Linger.Utils` | `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` | `Linger.Utils` | 接收并向实际操作传递取消令牌。省略 `shouldRetry` 时不再重试所有异常；需要重试时必须显式传入谓词。 |
| `Linger.Utils` | 不支持结果策略的 `RetryHelper.ExecuteAsync<T>` | `ExecuteAsync<T>(..., shouldRetryResult: ...)` | `Linger.Utils` | 泛型 API 现在可直接重试失败结果；达到最大次数后返回最后一次结果。公共签名已变化，调用方需要重新编译。 |
| `Linger.FileSystem` | `FileOperationResult.FileSize` | 本地使用 `GetFileSize(...)`；远程使用 `GetFileSizeAsync(...)` | `Linger.FileSystem` | 操作结果不再携带大小元数据，仅在需要时查询。 |
| `Linger.FileSystem` | `FileOperationResult.FullFilePath` / `FileHash` | `UploadedInfo.FullFilePath` / `HashData` | `Linger.FileSystem.Local` | 实现特有元数据仅由本地命名上传 API 返回。 |
| `Linger.FileSystem` | 本地命名版 `UploadAsync(...)` 重载 | `UploadWithNamingAsync(...)` 或 `UploadFileWithNamingAsync(...)` | `Linger.FileSystem.Local` | 公共 `UploadAsync` 现在仅表示从流上传到指定路径。 |
| `Linger.Utils` | 为 `ParameterList.Parameters` 整体赋值 | 修改 `Parameters` 集合，或调用 `SetValue`、`Add`、`Remove`、`Clear` | `Linger.Utils` | 字典引用已改为只读，不能再整体替换或设为 `null`。 |
| `Linger.Utils` | `GuidCode.NewDateGuid` | `Guid.NewGuid().ToString("N")`；.NET 9+ 可使用 `Guid.CreateVersion7().ToString("N")` | .NET BCL | 被删除的 10 位标识只有 4 位 Base36 随机字符，无法可靠保证唯一。短业务码必须配合唯一约束和冲突重试。 |
| `Linger.Utils` | `JsonExtensions.Serialize<T>` / `SerializeJson<T>(...)` | `JsonSerializer.Serialize(...)` | .NET BCL | 旧实现使用 `DataContractJsonSerializer`。 |
| `Linger.Utils` | `JsonExtensions.DeserializeJson<T>(...)` | `JsonSerializer.Deserialize<T>(...)` | .NET BCL | 旧实现使用 `DataContractJsonSerializer`。 |
| `Linger.Utils` | `JsonExtensions.ToJsonString(...)` | `JsonSerializer.Serialize(...)` | .NET BCL | 包装方法没有增加序列化能力，且无法保证 Native AOT 兼容性。 |
| `Linger.Utils` | `JsonExtensions.Deserialize<T>(...)` | `JsonSerializer.Deserialize<T>(...)` | .NET BCL | 包装方法没有增加反序列化能力，且无法保证 Native AOT 兼容性。 |
| `Linger.Utils` | `JsonObjectConverter` | 使用 `JsonElement`、`JsonNode` 或强类型模型。 | .NET BCL | 混合 CLR 基元与 JSON DOM 的结果难以预期，且转换器无法保证 Native AOT 兼容性。 |
| `Linger.Utils` | `JsonDefaults`、`DateTimeConverter`、`DateTimeNullConverter`、`DataTableJsonConverter`、`DataSetConverter` 和 `JsonStringConverter` | 相同 API | `Linger.Json` | 添加 `Linger.Json` 引用；公共命名空间和 API 名称不变。 |
| `Linger.Utils` | `string.ToDataTable()`、`DataTable.ToJsonString()` 和 `JsonElement.JsonElementToDataTable()` | 相同 API | `Linger.Json` | 添加 `Linger.Json` 引用；这些 API 为 JSON 专用功能，不再使 `Linger.Utils` 带入 JSON 依赖。 |
| `Linger.Utils` | `ObjectExtensions.ForIn` | `ForEachProperty` | `Linger.Reflection` | 需要引用 `Linger.Reflection`；替代方法具有相同的属性遍历意图，名称更明确。 |
| `Linger.Utils` | `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` | `Linger.Reflection` | 需要引用 `Linger.Reflection`；`AttrValues<T>` 只是一个别名。 |
| `Linger.Utils` | `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` | `Linger.Reflection` | 需要引用 `Linger.Reflection` 并使用 `IQueryable<T>` 重载。 |
| `Linger.Utils` | `DecimalExtensions.ToRounding` | `Round` | `Linger.Utils` | 两者均使用库中约定的 decimal 舍入行为。 |
| `Linger.Utils` | `byte[].ToImageBase64String` | `ToImageDataUri(mediaType)` | `Linger.Utils` | 显式指定真实的图片媒体类型。 |
| `Linger.Utils` | `ArrayExtensions.ForEach<T>(T[], Action<T>)` | `Array.ForEach(array, action)` 或 `IEnumerable<T>.ForEach(action)` | .NET BCL / `Linger.Utils` | 根据集合类型选择数组 API 或当前维护的可枚举扩展。 |
| `Linger.Utils` | `ArrayExtensions.Insert<T>(T[], T)` | `Add<T>(T[], T)` | `Linger.Utils` | `Insert` 仅在数组末尾追加元素，且不支持指定插入索引。 |
| `Linger.Utils` | `DateTimeExtensions.FirstDayOfMonth` | `StartOfMonth` | `Linger.Utils` | `FirstDayOfMonth` 是完全别名。 |
| `Linger.Utils` | `string[].ToEnumerable` | 直接使用数组，或使用 `value ?? Array.Empty<string>()` | .NET BCL | 数组已经实现 `IEnumerable<string>`。 |
| `Linger.Utils` | `string[].ToList` 和 `ToListOrEmpty` | `new List<string>(value ?? Array.Empty<string>())` | .NET BCL | 确定数组非空时可使用 `Enumerable.ToList()`。 |
| `Linger.Utils` | `string.ToSplitList(char)` | `SplitToList(char)` | `Linger.Utils` | 正则表达式重载 `ToSplitList(string)` 仍然保留。 |
| `Linger.Utils` | `string.ToSplitArray(char)` | `SplitToArray(char)` | `Linger.Utils` | 使用字面量字符作为分隔符。 |
| `Linger.Utils` | `string.ToSplitArrayByCrlf` | `SplitToArray(Environment.NewLine)` | `Linger.Utils` | 显式指定换行分隔符。 |
| `Linger.Utils` | `string.RemoveLastChar(string)` | `RemoveLastChar(char)` 或 `RemoveSuffixOnce(string, StringComparison)` | `Linger.Utils` | 根据需求选择单字符删除或精确后缀删除。 |
| `Linger.Utils` | `PathExtensions.IsStrictAbsolutePath` | `Path.IsPathFullyQualified` | .NET BCL | UNC 路径策略应由调用方明确处理。 |
| `Linger.Utils` | `PathExtensions.ToFullPath` | `Path.GetFullPath(path)` 或 `Path.GetFullPath(path, basePath)` | .NET BCL | 末尾分隔符策略由调用方处理；netstandard2.0 请改用 `Path.GetFullPath(Path.Combine(basePath, path))`。 |
| `Linger.Utils` | `PathExtensions.GetRelativePath` | `Path.GetRelativePath` | .NET BCL | 相同路径时 BCL 返回 `"."`，与原实现一致。 |
| `Linger.Utils` | `PathExtensions.GetParentDirectory(levels)` | 按层级调用 `Path.GetDirectoryName` | .NET BCL | BCL 到达根目录时返回 `null`，原实现返回根目录本身。 |
| `Linger.Utils` | `FileHelper.GetExistingFileInfo(...)` | `new FileInfo(path)` | .NET BCL / `Linger.Utils` | 仅在确实需要内容指纹时打开文件并调用 `ComputeHashMd5()`。 |
| `Linger.Utils` | `ExtendedFileInfo` | `UploadedInfo` 或业务自定义 DTO | `Linger.FileSystem` / 应用代码 | 上传结果使用 `UploadedInfo`；其他场景按业务元数据需求定义模型。 |
| `Linger.Utils` | `BaseFileInfo` | `UploadedInfo` 或业务自定义 DTO | `Linger.FileSystem` / 应用代码 | 该通用基类没有独立行为，也没有实际的多态用途。 |
| `Linger.Utils` | `int.ToFileSizeBytesString` | `FormatFileSize` | `Linger.Utils` | 新格式与其他文件大小 API 保持一致。 |
| `Linger.Utils` | `FileInfoExtensions.Delete(IEnumerable<FileInfo>)` | `Delete(files, consolidateExceptions)` | `Linger.Utils` | 传入 `false` 保留遇错即停行为，传入 `true` 汇总符合条件的异常。 |
| `Linger.Utils` | `FileInfoExtensions.GetVersionInfo` | `FileVersionInfo.GetVersionInfo(fileInfo.FullName)` | .NET BCL | 平台兼容策略由调用方处理。 |
| `Linger.Utils` | `string.GetFileVersion` | `new FileInfo(filePath).GetFileVersion()` | `Linger.Utils` | 替代方法会校验文件路径以及文件是否存在。 |
| `Linger.Utils` | `string.GetFilePath` | `Path.GetDirectoryName(path)` | .NET BCL | 仅在调用方确实需要时追加目录分隔符。 |
| `Linger.Utils` | `FileInfoExtensions.FileSize` | `GetFileSizeFormatted` | `Linger.Utils` | 文件路径和 `FileInfo` 均有对应重载。 |
| `Linger.Utils` | `GetFileNameNoExtension` | `Path.GetFileNameWithoutExtension` | .NET BCL | `FileInfo` 输入应使用 `fileInfo.Name`。 |
| `Linger.Utils` | `GetExtensionNotDotString` | `Path.GetExtension(path).TrimStart('.')` | .NET BCL | 直接使用平台路径 API。 |
| `Linger.Utils` | `FileHelper.GetLineCount` | `File.ReadLines(path).Count()` | .NET BCL | 需要继续处理文件内容时，建议使用专用的流式读取逻辑。 |
| `Linger.Utils` | `FileHelper.GetFileSize` | `filePath.GetFileSize()` | `Linger.Utils` | 使用当前维护的文件扩展 API。 |
| `Linger.Utils` | `FileHelper.GetDirectories` / `GetFileNames` 的布尔参数重载 | 接收 `SearchOption` 的重载 | `Linger.Utils` | 显式使用 `TopDirectoryOnly` 或 `AllDirectories`。 |
| `Linger.Utils` | `FileHelper.CreateDirectoryIfNotExists` | `Directory.CreateDirectory` | .NET BCL | `Directory.CreateDirectory` 本身已经具备幂等性。 |

## 其他 Linger 包

> **专项迁移指南**：`Linger.DataAccess` 的破坏性变更详见 [Linger.DataAccess 迁移指南](../Linger.DataAccess/MIGRATION.zh-CN.md)；`Linger.Json` 的包拆分说明详见 [Linger.Json 迁移指南](../Linger.Json/MIGRATION.zh-CN.md)。

| 原包 | 已删除 API | 替代方式 | 替代项位置 | 说明 |
| --- | --- | --- | --- | --- |
| `Linger.Configuration` | `AppSettingsHelper.CovertToObject<T>` | `ConvertToObject<T>` | `Linger.Configuration` | 修正方法名称中的拼写错误。 |
| `Linger.AspNetCore.Jwt.Contracts` | `IJwtService.TryRefreshTokenAsync` | `RefreshTokenResultAsync` | `Linger.AspNetCore.Jwt.Contracts` | 替代方法同时提供错误消息。 |
| `Linger.Email.AspNetCore` | `ConfigureEmail` / `ConfigureMailKit` | `AddEmailService` | `Linger.Email.AspNetCore` | 替代方法返回 `IServiceCollection`，可继续链式调用。 |
| `Linger.Email` | 单个 `Email` 实例复用 SMTP 连接 | 无 API 替代；高频发送请改用批处理或队列 | `Linger.Email` | `SendAsync` 现在会在每次调用时新建并关闭 SMTP 客户端，因此单个 `Email` 实例支持并发发送。请勿依赖连接复用。 |
| `Linger.Excel.Contracts` | `DataTableToFile` / `DataSetToFile` | `DataTableToExcel` / `DataSetToExcel` | `Linger.Excel.Contracts` | 替代方法名称与 Excel 导出操作一致。 |
| `Linger.Excel.Contracts` | `DataTableToExcelAsync` / `CollectionToExcelAsync` | `DataTableToExcel` / `CollectionToExcel` | `Linger.Excel.Contracts` | 提供方以同步方式序列化工作簿，已删除的 API 只是先将完整工作簿缓冲到内存，再异步复制到文件。文件导出现在直接写入目标流。 |
| `Linger.Excel.Contracts` | `ExcelExtensions.DataTableToFileAsync` / `ListToFileAsync` | `IExcel<TWorksheet>.DataTableToExcel` / `CollectionToExcel` 回调重载 | `Linger.Excel.Contracts` | 保留提供方特定回调能力，同时将完成的工作簿直接写入目标文件。 |
| `Linger.Excel.Contracts` | 自定义 `IExcel<TWorksheet>` 实现 / `AbstractExcelService<TWorkbook, TWorksheet>` 派生类 | 实现新增的回调式 `DataTableToExcel(...)` 和 `CollectionToExcel(...)` 成员，或改为继承 `ExcelBase<TWorkbook, TWorksheet>` | `Linger.Excel.Contracts` | 这些成员现为接口和抽象基类的必需成员；直接自定义实现必须补齐它们才能编译。 |
| `Linger.Excel.Contracts` | `ExcelBase<TWorkbook, TWorksheet>.SaveWorkbookToStream(TWorkbook)` | `WriteWorkbook(TWorkbook, Stream)` | `Linger.Excel.Contracts` | 自定义提供方应同步写入传入流，避免文件导出必须分配完整 `MemoryStream`。 |
| `Linger.Excel.Contracts` | 接收 `Func<DataRow, T>` 的 Excel 导入扩展 | 接收 `Func<ExcelRow, T>` 的 `IExcelService.ExcelToList[Async]` / `StreamToList[Async]` | `Linger.Excel.Contracts` | 替代方法直接映射工作表行，不再创建中间 `DataTable`。可使用 `ExcelRow.Get<T>(columnName)` 进行强类型访问。 |
| `Linger.Excel.Contracts` | 接收 `Func<T>` 和 `columnSetters` 的 Excel 导入扩展 | 接收 `Func<ExcelRow, T>` 的 `IExcelService.ExcelToList[Async]` / `StreamToList[Async]` | `Linger.Excel.Contracts` | 在同一个映射委托中创建并填充目标对象。已经持有 `DataTable` 的调用方仍可使用通用工厂/setter 重载。 |
| `Linger.Excel.Contracts` | 自定义 `IExcelService` 实现 | 实现接收 `ExcelExportColumn<T>` 的集合导出重载 | `Linger.Excel.Contracts` | 显式列导出现在是正式服务契约，并直接写入工作表，不再构造中间 `DataTable`。 |
| `Linger.Excel.Contracts` | `ExcelOptions.ParallelProcessingThreshold`、`UseBatchWrite` 和 `BatchSize` | 删除这些配置 | `Linger.Excel.Contracts` | 旧批处理路径仅将取值并行复制到临时数组，工作表写入仍是串行操作。现在提供方统一使用直接顺序写入路径，避免线程调度和额外分配。 |
| `Linger.Results` | `ExecuteResult`、`ExecuteResult<T>` 和 `ResultCompat` | `Result` / `Result<T>` | `Linger.Results` | 使用新结果类型，移除旧类型及转换帮助方法。 |
| `Linger.Results` | `ErrorObj` 和 `ToErrorObj` | `Error` 以及由调用方维护的错误集合 | `Linger.Results` / 应用代码 | 旧的聚合对象没有一对一替代。 |
| `Linger.Results` | `ResultExtensions.Try` / `TryAsync` | 在调用边界捕获可转换为领域错误的明确异常 | 应用代码 | 通用结果库不再捕获所有异常；未预期异常和取消异常应自然传播。 |
| `Linger.Results.AspNetCore` | `ToProblemDetails()` / `ToProblemDetails<T>()` | `ToActionResult()` / `ToActionResult<T>()` | `Linger.Results.AspNetCore` | `ToActionResult` 的失败响应已统一为 `ProblemDetails`；可继续通过 `failureStatusCode` 指定失败状态码。 |
| `Linger.Results` | 在子类中设置 `Result.Status` / `Result.Errors` | 使用 `Success`、`Failure` 或 `NotFound` 工厂方法创建结果 | `Linger.Results` | 结果状态和错误集合现为只读快照。依赖 `protected set` 的自定义子类需要改用工厂方法或组合方式。 |
| `Linger.HttpClient.Contracts` | `HttpClientBase` | 实现包含 `SendAsync` 的 `IHttpClient`；需要自定义标准实现时继承 `StandardHttpClient` | `Linger.HttpClient.Contracts` / `Linger.HttpClient.Standard` | 契约包不再包含序列化、错误解析和文件传输实现。`Contracts` 与 `Standard` 仍是独立包。 |
| `Linger.HttpClient.Contracts` | `HttpMethodEnum` | `System.Net.Http.HttpMethod` | .NET BCL | 支持 `PATCH`、`HEAD`、`OPTIONS` 和自定义 HTTP 方法，不再维护受限枚举。 |
| `Linger.HttpClient.Contracts` | `HttpResponseMode` / `CallApiWithMode<T>` | 类型化响应使用 `CallApi<T>`；文件下载使用 `DownloadToFileAsync` | `Linger.HttpClient.Contracts` | 响应缓冲和资源所有权由具体操作确定，不再通过独立模式参数控制。 |
| `Linger.HttpClient.Contracts` | `CallApi<T>(url, queryParams, timeout, ...)` GET 重载 | `GetAsync<T>(url, queryParams, headers, cancellationToken)` | `Linger.HttpClient.Contracts` | GET 便利调用移动到扩展方法；`CallApi<T>` 现在必须接收 `HttpMethod`。 |
| `Linger.HttpClient.Contracts` | 各调用方法中的 `int? timeout` 参数 | 单次 GET 使用 `GetWithTimeoutAsync<T>`；其他请求使用带 `CancelAfter` 的调用方 `CancellationTokenSource` | `Linger.HttpClient.Contracts` / .NET BCL | 超时时间改用 `TimeSpan` 表达。单次 GET 超时返回失败结果，用户取消继续通过 `OperationCanceledException` 传播。 |
| `Linger.HttpClient.Contracts` | `SetToken`、`AddHeader` 和可变 `Options` | 通过 `headers` 参数传入动态请求头；固定设置使用 `AddHttpClient` 配置 | `Linger.HttpClient.Contracts` / .NET DI | 动态认证信息按请求隔离，避免共享客户端并发请求相互覆盖。 |
| `Linger.HttpClient.Contracts` | `CallApi<T>` 的表单和 `HttpContent` 专用重载 | 使用统一的 `CallApi<T>(..., HttpMethod, requestBody, ...)` | `Linger.HttpClient.Contracts` | `IDictionary<string, string>` 仍按表单发送，`HttpContent` 仍直接发送；请求结束时客户端负责释放内容。 |
| `Linger.HttpClient.Standard` | 非泛型 `CallApi(...)` | 使用非泛型 `PostAsync`、`PutAsync` 或 `DeleteAsync` 扩展 | `Linger.HttpClient.Contracts` | 无返回值调用不再由具体实现额外公开一套重载。 |
| `Linger.HttpClient.Contracts` | 接收 `byte[] fileData` 的文件上传重载 | `UploadFileAsync<T>(..., Stream fileStream, ...)` | `Linger.HttpClient.Contracts` | 上传使用 `StreamContent`，请求完成后会释放传入的流。 |
| `Linger.HttpClient.Contracts` | `DownloadStreamAsync`、`CallApi<Stream>` 和 `CallApi<HttpResponseMessage>` | 文件直接使用 `DownloadToFileAsync`；原始响应、SSE 和长连接使用 `SendAsync` | `Linger.HttpClient.Contracts` | 类型化调用不再根据泛型类型改变资源所有权。`SendAsync` 明确要求调用方释放返回的 `HttpResponseMessage`。 |
| `Linger.HttpClient.Contracts` | `CompressionHelper` / `MultipartHelper` | 通过 `HttpClientHandler.AutomaticDecompression` 配置压缩；上传使用 `UploadFileAsync` 或 BCL `MultipartFormDataContent` | .NET BCL / `Linger.HttpClient.Contracts` | 删除契约包中的具体实现 Helper。 |
| `Linger.HttpClient.Contracts` | `ProblemDetailsWithErrors.Status` 的不可空 `int` | 可空 `int?` 属性 | `Linger.HttpClient.Contracts` | RFC 7807 的 `status` 字段可以省略；读取方应处理 `null`。 |
| `Linger.HttpClient.Standard` | 接收 `HttpClientOptions` 的构造函数和 `Create(...)` | 简单场景使用 `StandardHttpClient(string, ILogger<StandardHttpClient>?)`；高级配置注入 `StandardHttpClient(HttpClient, ILogger<StandardHttpClient>?)` | `Linger.HttpClient.Standard` | URL 模式由客户端管理底层资源；Handler、证书、代理等高级设置仍由调用方配置。 |
| `Linger.AspNetCore.Jwt.Contracts` | `IJwtService.CreateTokenAsync(string)` | `CreateTokenAsync(string, CancellationToken)` | `Linger.AspNetCore.Jwt.Contracts` | 签发操作现在支持端到端取消。可选参数保留了源代码调用形式。 |
| `Linger.AspNetCore.Jwt.Contracts` | `IRefreshableJwtService.RefreshTokenAsync(Token)` 及对应扩展 | 接收 `CancellationToken` 的重载 | `Linger.AspNetCore.Jwt.Contracts` | 刷新操作传播调用方取消；`RefreshTokenResultAsync` 不再吞掉取消和未知异常。 |
| `Linger.AspNetCore.Jwt` | `JwtService.GetClaimsAsync(string)` | `GetClaimsAsync(string, CancellationToken)` | `Linger.AspNetCore.Jwt` | 自定义 Claims 查询现在可响应调用方取消；派生类需要更新重写签名并向下游异步调用传递令牌。 |
| `Linger.AspNetCore.Jwt` | `JwtServiceWithRefresh.GetExistRefreshTokenAsync` / `HandleRefreshToken` | `StoreRefreshTokenAsync` / `TryRotateRefreshTokenAsync` | `Linger.AspNetCore.Jwt` | 刷新轮换改为存储层原子比较并替换，防止并发请求重复使用同一个旧令牌。现有派生类必须迁移到新钩子。 |
| `Linger.AspNetCore.Jwt` | `JwtOption.EnableRefreshToken` 配置示例 | 删除该配置；通过注册 `IRefreshableJwtService` 启用刷新 | `Linger.AspNetCore.Jwt` | 该属性从未存在于实际选项类型，刷新能力由服务实现决定。 |
| `Linger.FileSystem` | 本地 `FileExistsAsync` / `DirectoryExistsAsync` | `FileExists` / `DirectoryExists` | `Linger.FileSystem.Local` | 本地元数据查询基于同步 BCL API，不再包装为伪异步。远程签名不变。 |
| `Linger.FileSystem` | 本地 `CreateDirectoryIfNotExistsAsync` / `DeleteFileIfExistsAsync` | `CreateDirectoryIfNotExists` / `DeleteFileIfExists` | `Linger.FileSystem.Local` | 本地目录和文件变更同步完成。 |
| `Linger.FileSystem` | 本地 `OpenReadAsync` / `OpenWriteAsync` / `GetReaderAsync` / `GetWriterAsync` | `OpenRead` / `OpenWrite` / `GetReader` / `GetWriter` | `Linger.FileSystem.Local` | 创建本地流是同步操作；返回的文件流仍支持真正的异步内容读写。 |
| `Linger.FileSystem` | 本地 `GetFileSizeAsync` / `DeleteAsync` | `GetFileSize` / `Delete` | `Linger.FileSystem.Local` | 本地元数据和删除不再返回已完成任务。 |
| `Linger.FileSystem` | `IFileSystem` | `IFileTransfer` | `Linger.FileSystem` | 共享契约改为直接表达其真实职责：异步文件内容传输。这是破坏源代码兼容性的接口重命名。 |
| `Linger.FileSystem` | `IFileSystemOperations` | 远程操作使用 `IRemoteFileSystem`；本地操作使用 `ILocalFileSystem` | `Linger.FileSystem` | 已删除无独立职责的中间接口。远程异步能力直接归入 `IRemoteFileSystem`；本地元数据和流工厂保持同步。 |
| `Linger.FileSystem` | `ILocalFileSystem.Exists()` / `ExistsAsync()` | `DirectoryExists(fileSystem.RootDirectoryPath)` | `Linger.FileSystem.Local` | 使用本地同步目录存在性 API。`LocalFileSystem` 还会在构造时创建配置的根目录。 |
| `Linger.FileSystem` | `ILocalFileSystem.CreateIfNotExists()` / `CreateIfNotExistsAsync()` | `CreateDirectoryIfNotExists(fileSystem.RootDirectoryPath)` | `Linger.FileSystem.Local` | 使用本地同步目录创建 API。 |
| `Linger.FileSystem` | `IFileSystemOperations.IsDirectoryAsync(path)` | `DirectoryExistsAsync(path)` | `Linger.FileSystem` | 被删除的方法是完全别名。 |
| `Linger.FileSystem` | `IBatchFileSystemOperations` | `ILocalBatchFileSystemOperations` | `Linger.FileSystem` | 批量操作仅由本地文件系统提供，接口名现在明确其能力边界。 |
| `Linger.FileSystem` | `IBatchFileSystemOperations.ListFilesAsync(...)` / `ListDirectoriesAsync(...)` | `ILocalBatchFileSystemOperations.ListFiles(...)` / `ListDirectories(...)` | `Linger.FileSystem` | 本地目录枚举基于同步 BCL API，不再包装为伪异步，也不再接收无实际取消能力的 `CancellationToken`；FTP/SFTP 继续通过 `IRemoteFileSystem` 使用真正的异步列表 API。 |
| `Linger.FileSystem` | `ILocalBatchFileSystemOperations.DeleteFilesAsync(...)` | `DeleteFiles(...)` | `Linger.FileSystem` | `File.Delete` 是同步操作。批量删除现在在调用线程上顺序执行，不再通过异步批处理 worker 返回已完成任务；取消会在文件之间检查。 |
| `Linger.FileSystem` | `LocalFileSystemOptions.MaxDegreeOfParallelism` | 删除该配置 | `Linger.FileSystem` | 本地批量操作现在按输入顺序执行。上传和下载保留异步文件 I/O，但不再调度并发 worker；删除保持同步。 |
| `Linger.FileSystem.Ftp` / `Linger.FileSystem.Sftp` | 通过 `FtpFileSystem` / `SftpFileSystem` 具体类型调用 `ListFilesAsync(...)` / `ListDirectoriesAsync(...)` | 通过 `IRemoteFileSystem` 调用同签名方法 | `Linger.FileSystem` | 远程目录枚举能力现在由远程接口正式声明；原有具体类型调用无需修改。 |
| `Linger.FileSystem` | `UploadFilesAsync(..., remoteDirectory, ...)` / `DownloadFilesAsync(remoteFilePaths, ...)` | `UploadFilesAsync(..., destinationDirectory, ...)` / `DownloadFilesAsync(sourceFilePaths, ...)` | `Linger.FileSystem` | 参数含义原本就是本地根目录下的目标/源路径；使用命名参数的调用方需要更新参数名。 |
| `Linger.FileSystem` | 可在创建后重新赋值 `BatchOperationResult.SucceededFiles` / `FailedFiles` | 仅可在对象初始化期间赋值 | `Linger.FileSystem` | 集合属性由 `set` 改为 `init`，防止返回结果在创建后被替换。 |
| `Linger.FileSystem` | 派生类调用 `FileSystemBase.ExecuteBatchItemAsync(...)` | 在派生实现中直接执行并记录批量项结果 | `Linger.FileSystem` | 未使用的受保护辅助方法已删除；外部派生类如有调用，需要内联对应的异常和结果记录逻辑。 |
| `Linger.FileSystem` | `IRemoteFileSystem.ServerDetails()` | `IRemoteFileSystem.ServerDetails` | `Linger.FileSystem` | 服务器详情是缓存数据，现改为只读属性。 |
| `Linger.FileSystem` | `RemoteSystemSetting` | `FtpFileSystemOptions` / `SftpFileSystemOptions` | `Linger.FileSystem.Ftp` / `Linger.FileSystem.Sftp` | 根据协议选择专用选项类型。`Type` 属性已删除，协议由具体实现确定。 |
| `Linger.FileSystem.Ftp` | `UploadFileAsync(localPath, destinationDirectory, destinationFileName, ...)` | `UploadFileAsync(localPath, destinationFilePath, ...)` | `Linger.FileSystem.Ftp` | 调用上传 API 前先构造完整目标路径。 |
| `Linger.FileSystem.Ftp` | `ListDirectoryAsync(...)` | `ListFilesAsync(...)` / `ListDirectoriesAsync(...)` | `Linger.FileSystem.Ftp` | 使用与协议无关的列表 API，不再使用 FluentFTP 特有的对象类型筛选。 |
| `Linger.FileSystem.Sftp` | `UploadFileAsync(localPath, destinationDirectory, destinationFileName, ...)` | `UploadFileAsync(localPath, destinationFilePath, ...)` | `Linger.FileSystem.Sftp` | 调用上传 API 前先构造完整目标路径。 |
| `Linger.FileSystem.Sftp` | `Connect()` / `Disconnect()` | `ConnectAsync()` / `DisconnectAsync()` | `Linger.FileSystem.Sftp` | 连接管理统一使用远程文件系统的异步契约。 |
| `Linger.FileSystem.Sftp` | `SetRootAsWorkingDirectoryAsync()` | `SetWorkingDirectoryAsync("/")` | `Linger.FileSystem.Sftp` | 被删除的方法只负责传入根路径。 |
| `Linger.Ldap.Novell` | `ConnectAsync(...)`、`Disconnect()`、`IsConnected()` 和 `IDisposable` | 无需替代，直接调用 `ILdapClient` 操作。 | `Linger.Ldap.Novell` | 每次操作现在会独立创建、绑定和释放连接，避免并发调用共享凭据或相互断开连接。 |
| `Linger.Ldap.Contracts` | `ILdap` | `ILdapClient` | `Linger.Ldap.Contracts` | 重命名以准确表达契约含义：连接目录服务器的客户端。 |
| `Linger.Ldap.Contracts` | `AdUserInfo` | `LdapUserInfo` | `Linger.Ldap.Contracts` | 该模型与提供者无关，并不局限于 Active Directory。 |
| `Linger.Ldap.ActiveDirectory` | `Ldap` | `AdLdapClient` | `Linger.Ldap.ActiveDirectory` | 原名称与 `Linger.Ldap` 命名空间冲突，且与 Novell 实现同名。 |
| `Linger.Ldap.ActiveDirectory` | `ILdapClient` 以及 `ValidateUserAsync` / `FindUserAsync` / `GetUsersAsync` / `SearchUsersByFilterAsync` / `UserExistsAsync` | `IActiveDirectoryClient` 以及 `ValidateUser` / `FindUser` / `GetUsers` / `SearchUsersByFilter` / `UserExists` | `Linger.Ldap.Contracts` | `System.DirectoryServices` 只提供同步目录操作；新公共契约直接表达真实执行模型，不再返回已完成任务。 |
| `Linger.Ldap.Contracts` | 查询返回 `Task<IEnumerable<LdapUserInfo>>` | `Task<IReadOnlyList<LdapUserInfo>>` | `Linger.Ldap.Contracts` | 提供者资源释放前会完成结果实体化，新契约直接表达这一行为。 |
| `Linger.Ldap.Contracts` | 使用分隔字符串表示 `ProxyAddresses` / `OtherTelephone` | `string[]?` 属性 | `Linger.Ldap.Contracts` | LDAP 多值属性不再依赖提供者特定的分隔符。 |
| `Linger.Ldap.Novell` | `Ldap` | `NovellLdapClient` | `Linger.Ldap.Novell` | 原名称与 `Linger.Ldap` 命名空间冲突，且与 Active Directory 实现同名。 |
| `Linger.Ldap.Novell` | `LdapEntry.ToAdUser()` | `LdapEntry.ToLdapUserInfo()` | `Linger.Ldap.Novell` | 与 `LdapUserInfo` 保持一致。 |
| `Linger.Excel.EPPlus` | `EPPlusExcel`、`ExcelWorksheetExtensions.TrimLastEmptyRows` / `IsLastRowEmpty` | 使用 `Linger.Excel.Npoi` 或 `Linger.Excel.ClosedXML` 提供者 | `Linger.Excel.Npoi` / `Linger.Excel.ClosedXML` | EPPlus 提供者已整体移除；其余提供者实现同一 `IExcelService` 契约，可直接替换。 |
| `Linger.SharedKernel` | `BaseSearchPageList` | `BaseSearchPagedList` | `Linger.SharedKernel` | 与 `BaseSearchPagedList` 内容等价的重复类型（继承 `BaseSearch`、实现 `IBaseSearchPagedList`），仅名称拼写不同。 |
| `Linger.Audit` | `IAuditUserProvider.UserName`；`GetUser()` 的 `string` 返回类型 | 实现 `GetUser()`，无可认证用户时返回 `null` | `Linger.Audit.Contracts` | 用户标识统一由 `GetUser()` 提供；返回类型改为 `string?`，接口实现者需要调整签名。 |
| `Linger.Audit` | `AuditTrailEntry.Changes` / `TempProperties` | `CurrentValuesSnapshot`，或自行对比 `OldValues` / `NewValues` | `Linger.EFCore.Audit` | 变更集合改为清晰的旧值/新值快照模型。 |
| `Linger.FileSystem` | `IRemoteFileSystem.IsConnected()` / `ConnectAsync()` / `DisconnectAsync()` | 无需显式调用；远程实现内部管理连接生命周期 | `Linger.FileSystem` | 连接管理移出共享接口；`FtpFileSystem` / `SftpFileSystem` 的对应方法改为内部使用。 |
| `Linger.Excel.Contracts` | `ExcelToDataSet(string, int, ...)`、`ExcelToDataSet(string, Func<string, int?>, ...)`、`StreamToDataSet(...)` 等旧重载 | `ExcelToDataSet(string, IEnumerable<string>? = null, int = 0, bool = false)` / `ExcelToDataSet(string, Func<string, int?>, IEnumerable<string>? = null, bool = false)` | `Linger.Excel.Contracts` | 表头行索引参数后移；`ExcelToDataSet(path, 1)` 这类位置参数调用需改为 `ExcelToDataSet(path, headerRowIndex: 1)` 或显式传入 `sheetNames`。 |
| `Linger.Excel.Contracts` | `ExcelToDataSetAsync` / `StreamToDataSetAsync` / `ExcelToDataTableAsync` / `StreamToDataTableAsync` | 同步方法 `ExcelToDataSet` / `StreamToDataSet` / `ExcelToDataTable` / `StreamToDataTable` | `Linger.Excel.Contracts` | 导入是同步解析，删除伪异步包装；调用方改用同步 API。 |
| `Linger.Excel.Contracts` | `IExcel<TWorksheet>.DataSetToExcel` / `DataTableToExcel` 的回调式参数顺序 | `DataSetToExcel(DataSet, string, Action<IWorksheetExportContext<TWorksheet>>, string defaultSheetName = ...)` / `DataTableToExcel(DataTable, string, Action<TWorksheet, DataColumnCollection, DataRowCollection>?, string sheetsName = ..., string title = "", Action<TWorksheet>? styleAction = null)` | `Linger.Excel.Contracts` | 回调参数提前到第三参；使用位置参数调用导出方法的调用方需要调整实参顺序。 |

## 没有一对一替代

| 原包 | 已删除 API | 迁移方式 | 替代项位置 | 删除原因 |
| --- | --- | --- | --- | --- |
| `Linger.Utils` | `AesEncrypt` | 使用 `AesEncryptAuthenticated`。已有的旧格式密文仍可通过 `AesDecrypt` 解密。 | `Linger.Utils` | 未经认证的 AES-CBC 无法保护密文完整性。 |
| `Linger.Utils` | `GuidCode.GetInt32UniqueCode` / `GetInt64UniqueCode` | 使用 `Guid.NewGuid()`；在 .NET 9 或更高版本中，需要按时间排序时可考虑 `Guid.CreateVersion7()`。 | .NET BCL | 截断 GUID 无法保证唯一性。 |
| `Linger.Utils` | `object.IsNullOrEmpty` / `IsNotNullOrEmpty` | 根据类型使用 `string.IsNullOrEmpty`、集合数量判断或 `Guid?` 扩展方法。 | .NET BCL / `Linger.Utils` | 任意对象不存在统一、明确的“空”状态。 |
| `Linger.Utils` | `ToTarget<T>`、`TryToTarget<T>`、`ToTargetOrDefault<T>` 和 `ToTargetOrNull<T>` | 使用明确的 `ToInt`、`TryToDateTime`、`TryToGuid` 等转换 API；目标类型只能在运行时确定时，使用 `TypeConverter.TryConvert`。 | `Linger.Utils` | 泛型转换隐藏了支持范围和失败语义。 |
| `Linger.Utils` | `TypeConverter.ConvertTo`、`TryConvertTo` 和 `TryConvertKnownType` | 使用 `TypeConverter.TryConvert`。 | `Linger.Utils` | 保留的 API 具有明确且兼容 AOT 的标量类型支持边界。 |
| `Linger.Utils` | `ToTree` | 在调用方基于明确的节点模型、根节点规则、重复键策略和循环处理规则构建树。 | 应用代码 | 通用帮助方法隐藏了树构建所需的关键业务语义。 |
| `Linger.Utils` | `FileHelper.DeleteFolderFiles` | 在调用方显式组合所需的 `Directory` 和 `File` 操作。 | .NET BCL / 应用代码 | 按文件名递归删除的行为对于通用工具 API 过于隐式。 |
| `Linger.Utils` | `ExtensionMethodSetting` 中的默认值 | 显式配置 `Encoding`、`CultureInfo`、缓冲区大小和 `JsonSerializerOptions`。JSON 可使用 `JsonDefaults.CreateRequestOptions`、`CreateResponseOptions` 或 `ApplyDefaultConfiguration`。 | .NET BCL / `Linger.Json` | 进程级可变默认值会使行为难以推断。 |
| `Linger.Utils` | 其他 `DataContractJsonSerializer` 相关帮助方法 | 使用 `System.Text.Json.JsonSerializer`。 | .NET BCL | 项目统一采用 `System.Text.Json`。 |
| `Linger.Utils` | `DeserializeDynamicJsonObject` 和 `JsonTextAccessor` | 根据是否需要可变 JSON，使用 `JsonDocument`、`JsonElement` 或 `JsonNode`。 | .NET BCL | 动态 JSON 隐藏了数据结构和运行时失败方式。 |
| `Linger.Ldap.ActiveDirectory` | `AdLdapClient` 仅 Logger 的构造函数 | 当前 Windows 域使用无参构造；需要自定义 Logger 和显式配置时，通过 Options 注册 `LdapConfig`。 | `Linger.Ldap.ActiveDirectory` | 仅提供 Logger 无法表达连接和查询配置。 |
| `Linger.Ldap.ActiveDirectory` | `GetEntryByUsername` | 常规用户查询使用 `IActiveDirectoryClient.FindUser`；确需原生 ADSI 时由应用直接使用 `System.DirectoryServices`。 | `Linger.Ldap.ActiveDirectory` / 应用代码 | 返回提供者资源会把所有权和释放责任泄露到客户端契约之外。 |
| `Linger.Ldap.ActiveDirectory` | 面向 `UserPrincipal`、`DirectoryEntry` 和 `SearchResultCollection` 的公开映射扩展 | 使用 `IActiveDirectoryClient` 查询操作。 | `Linger.Ldap.Contracts` | 三套映射路径存在重复，并会返回不一致的账户安全信息。 |

## 行为变化

- **`Linger.AspNetCore.Jwt`**：`JwtOptions.Issuer` 和 `JwtOptions.Audience` 不再默认为 `http://localhost`，现在必须显式配置；签发、Bearer 认证和过期令牌刷新会使用同一套颁发者与受众验证规则。
- **`Linger.AspNetCore.Jwt`**：`JwtOptions.SecurityKey` 或 `SECRET` 环境变量提供的实际签名密钥必须至少包含 32 个 UTF-8 字节。无效配置会在 JWT 服务创建或 Bearer 认证注册时抛出异常。
- **`Linger.AspNetCore.Jwt`**：认证质询和禁止访问不再写入自定义中文 JSON，现使用 ASP.NET Core JwtBearer 的标准 `401 Unauthorized` 和 `403 Forbidden` 响应。依赖旧响应正文的客户端需要改为依据 HTTP 状态码处理。
- **`Linger.HttpClient`**：不再自动向每个 URL 添加 `culture` 查询参数。需要该约定时，请显式加入查询对象或使用 `DelegatingHandler`。
- **`Linger.HttpClient`**：用户主动取消现在抛出 `OperationCanceledException`，不再转换为失败的 `ApiResult`；`HttpClient.Timeout` 触发的超时仍返回失败结果。
- **`Linger.HttpClient`**：每个请求创建并释放自己的 `HttpRequestMessage` 和 `HttpResponseMessage`。传入的 `HttpContent` 及 `UploadFileAsync` 的文件流会在请求完成后释放。
- **`Linger.HttpClient`**：`ApiResult.IsSuccess` 现在要求状态码为 2xx，且不存在传输、反序列化或结构化错误。
- **`Linger.HttpClient`**：ProblemDetails 只有在媒体类型或标准字段能够确认时才会解析；旧版 `IEnumerable<Error>` 数组仍作为兼容回退。
- **`Linger.Utils` -> `Linger.Utils` / `Linger.Reflection`**：已删除的
  `DataTable.ToListAsync` 不执行异步 I/O。使用同步的 `ToList` 可以避免产生误导性的异步 API。
- **`Linger.Utils` -> `Linger.Reflection`**：字符串排序表达式只接受 `asc` 或 `desc`，忽略大小写。无效方向现在会抛出
  `ArgumentException`，不再被静默视为降序。
- **`Linger.Utils`**：`FileInfoExtensions.Delete` 不再提供默认异常处理策略。调用方必须明确选择
  遇错即停或汇总异常。
- **`Linger.Excel.Contracts`**：Excel 导入 `DataTable` 时会推断每列的公共 CLR 类型，保留数值、布尔值和
  `DateTime` 等值，不再把所有非空单元格强制转换为 `string`；空列或混合类型列使用 `object`。需要文本时
  请显式调用 `Convert.ToString`。
- **`Linger.Json`**：`DataTableJsonConverter` 现在根据 `object` 列中每个值的运行时类型进行序列化，
  数值和布尔值会继续输出为 JSON 数值和布尔值。反序列化时，如果同一列包含不兼容的 JSON token
  类型，则使用 `object` 列并保留各 token 对应的 .NET 值类型。
- **`Linger.Audit`**：`CreationAuditEntity.CreationTime` 不再自动初始化为 `DateTimeOffset.Now`，创建实体时必须显式赋值；`CreatorId` 默认值由 `null!` 改为 `string.Empty`。

## 示例

### 重试异步操作

```csharp
var result = await retryHelper.ExecuteAsync(
    async cancellationToken =>
    {
        return await client.GetStringAsync(uri, cancellationToken);
    },
    shouldRetry: exception => exception is HttpRequestException or TimeoutException,
    cancellationToken: cancellationToken);
```

### DataTable 映射

```csharp
List<Person> people = table.ToList(row => new Person
{
    Id = row.Field<int>("Id"),
    Name = row.Field<string>("Name")
});
```

### HttpClient

注册方式改为配置并注入底层 `HttpClient`：

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

无需自定义底层客户端时，可以直接传入基础地址；此模式应释放 `StandardHttpClient`：

```csharp
using var client = new StandardHttpClient("https://api.example.com/");
```

WinForms 等单用户客户端直接创建专属实例时，可以在创建底层 `HttpClient` 时设置认证头：

```csharp
var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.example.com/")
};
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", accessToken);

var client = new StandardHttpClient(httpClient);
```

外部 `HttpClient` 模式应在应用生命周期内复用 `httpClient` 和 `client`，并在应用关闭时由调用方释放 `httpClient`。令牌刷新可能与请求并发时，使用 `DelegatingHandler` 在发送请求时读取当前令牌；一个客户端代表多个用户或令牌按请求变化时，继续使用每请求 `headers`。

HTTP 方法使用 BCL 类型，动态认证头按请求传入：

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

单次调用需要更短超时时间时，由调用方组合取消令牌：

```csharp
using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
timeoutSource.CancelAfter(TimeSpan.FromSeconds(5));

var result = await httpClient.GetAsync<User>(
    "users/42",
    cancellationToken: timeoutSource.Token);
```

文件上传改为流式 API：

```csharp
var fileStream = File.OpenRead("report.pdf");
var result = await httpClient.UploadFileAsync<UploadResponse>(
    "files",
    HttpMethod.Post,
    fileStream,
    "report.pdf",
    cancellationToken: cancellationToken);
```

`UploadFileAsync` 完成后会释放 `fileStream`。需要读取原始 `HttpResponseMessage`、SSE 或其他长连接协议时，请使用 `SendAsync`，并通过 `using` 释放返回的响应。

### 认证加密

```csharp
string ciphertext = plaintext.AesEncryptAuthenticated(key);
string plaintext = ciphertext.AesDecrypt(key);
```

### 向上回溯多级目录

`GetParentDirectory(levels)` 到达文件系统根目录会停住，而 `Path.GetDirectoryName`
越过根目录后返回 `null`。因此直接链式调用 BCL 而不做判空，在 `levels` 超过实际
层级深度时会出错。需要显式夹取：

```csharp
static string GetAncestor(string path, int levels)
{
    var current = Path.GetFullPath(path);
    for (var i = 0; i < levels; i++)
    {
        var parent = Path.GetDirectoryName(current);
        if (parent is null) break;   // 已到根目录，停止
        current = parent;
    }
    return current;
}
```
