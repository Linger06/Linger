# 迁移到 2.0.0-preview.1

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

本文档说明各个 Linger 包中的破坏性 API 删除，适用于从旧版公共 API 升级的应用程序。

## 直接替代

| 原包 | 已删除 API | 替代方式 | 替代项位置 | 说明 |
| --- | --- | --- | --- | --- |
| `Linger.Utils` | `DataTable.ToListAsync<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` | 需要引用 `Linger.Reflection`；被删除的方法只是使用 `Task.FromResult` 包装基于反射的同步映射。 |
| `Linger.Utils` | `DataTable.ToListAsync<T>(Func<DataRow, T>)` | `DataTable.ToList<T>(Func<DataRow, T>)` | `Linger.Utils` | 被删除的方法只是使用 `Task.FromResult` 包装同步映射。 |
| `Linger.Utils` | `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` | `Linger.Utils` | 接收并向实际操作传递取消令牌。 |
| `Linger.Utils` | `JsonExtensions.Serialize<T>` | `ToJsonString(...)` | `Linger.Utils` | 请同时参阅下文的 JSON 行为变化。 |
| `Linger.Utils` | `ObjectExtensions.ForIn` | `ForEachProperty` | `Linger.Reflection` | 需要引用 `Linger.Reflection`；替代方法具有相同的属性遍历意图，名称更明确。 |
| `Linger.Utils` | `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` | `Linger.Reflection` | 需要引用 `Linger.Reflection`；`AttrValues<T>` 只是一个别名。 |
| `Linger.Utils` | `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` | `Linger.Reflection` | 需要引用 `Linger.Reflection` 并使用 `IQueryable<T>` 重载。 |
| `Linger.Utils` | `DecimalExtensions.ToRounding` | `Round` | `Linger.Utils` | 两者均使用库中约定的 decimal 舍入行为。 |
| `Linger.Utils` | `byte[].ToImageBase64String` | `ToImageDataUri(mediaType)` | `Linger.Utils` | 显式指定真实的图片媒体类型。 |
| `Linger.Utils` | `string[].ToEnumerable` | 直接使用数组，或使用 `value ?? Array.Empty<string>()` | .NET BCL | 数组已经实现 `IEnumerable<string>`。 |
| `Linger.Utils` | `string[].ToList` 和 `ToListOrEmpty` | `new List<string>(value ?? Array.Empty<string>())` | .NET BCL | 确定数组非空时可使用 `Enumerable.ToList()`。 |
| `Linger.Utils` | `string.ToSplitList(char)` | `SplitToList(char)` | `Linger.Utils` | 正则表达式重载 `ToSplitList(string)` 仍然保留。 |
| `Linger.Utils` | `string.ToSplitArray(char)` | `SplitToArray(char)` | `Linger.Utils` | 使用字面量字符作为分隔符。 |
| `Linger.Utils` | `string.ToSplitArrayByCrlf` | `SplitToArray(Environment.NewLine)` | `Linger.Utils` | 显式指定换行分隔符。 |
| `Linger.Utils` | `string.RemoveLastChar(string)` | `RemoveLastChar(char)` 或 `RemoveSuffixOnce(string, StringComparison)` | `Linger.Utils` | 根据需求选择单字符删除或精确后缀删除。 |
| `Linger.Utils` | `PathExtensions.IsStrictAbsolutePath` | `Path.IsPathFullyQualified` | .NET BCL | UNC 路径策略应由调用方明确处理。 |
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

| 原包 | 已删除 API | 替代方式 | 替代项位置 | 说明 |
| --- | --- | --- | --- | --- |
| `Linger.Configuration` | `AppSettingsHelper.CovertToObject<T>` | `ConvertToObject<T>` | `Linger.Configuration` | 修正方法名称中的拼写错误。 |
| `Linger.AspNetCore.Jwt.Contracts` | `IJwtService.TryRefreshTokenAsync` | `RefreshTokenResultAsync` | `Linger.AspNetCore.Jwt.Contracts` | 替代方法同时提供错误消息。 |
| `Linger.Email.AspNetCore` | `ConfigureEmail` / `ConfigureMailKit` | `AddEmailService` | `Linger.Email.AspNetCore` | 替代方法返回 `IServiceCollection`，可继续链式调用。 |
| `Linger.Excel.Contracts` | `DataTableToFile` / `DataSetToFile` | `DataTableToExcel` / `DataSetToExcel` | `Linger.Excel.Contracts` | 替代方法名称与 Excel 导出操作一致。 |
| `Linger.Results` | `ExecuteResult`、`ExecuteResult<T>` 和 `ResultCompat` | `Result` / `Result<T>` | `Linger.Results` | 使用新结果类型，移除旧类型及转换帮助方法。 |
| `Linger.Results` | `ErrorObj` 和 `ToErrorObj` | `Error` 以及由调用方维护的错误集合 | `Linger.Results` / 应用代码 | 旧的聚合对象没有一对一替代。 |

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
| `Linger.Utils` | `ExtensionMethodSetting` 中的默认值 | 显式配置 `Encoding`、`CultureInfo`、缓冲区大小和 `JsonSerializerOptions`。JSON 可使用 `JsonDefaults.CreateRequestOptions`、`CreateResponseOptions` 或 `ApplyDefaultConfiguration`。 | .NET BCL / `Linger.Utils` | 进程级可变默认值会使行为难以推断。 |
| `Linger.Utils` | `DataContractJsonSerializer` 相关帮助方法 | 使用 `System.Text.Json.JsonSerializer`，或使用 `ToJsonString` / `Deserialize<T>`。 | .NET BCL / `Linger.Utils` | 项目统一采用 `System.Text.Json`。 |
| `Linger.Utils` | `DeserializeDynamicJsonObject` | 根据是否需要可变 JSON，使用 `JsonDocument`、`JsonElement` 或 `JsonNode`。 | .NET BCL | 动态 JSON 隐藏了数据结构和运行时失败方式。 |

## 行为变化

- **`Linger.Utils`**：`ToJsonString(null)` 返回 JSON 字面量 `"null"`。已删除的 `Serialize<T>`
  包装方法在输入为 null 时返回 C# 的 `null` 引用。
- **`Linger.Utils` -> `Linger.Utils` / `Linger.Reflection`**：已删除的
  `DataTable.ToListAsync` 不执行异步 I/O。使用同步的 `ToList` 可以避免产生误导性的异步 API。
- **`Linger.Utils` -> `Linger.Reflection`**：字符串排序表达式只接受 `asc` 或 `desc`，忽略大小写。无效方向现在会抛出
  `ArgumentException`，不再被静默视为降序。
- **`Linger.Utils`**：`FileInfoExtensions.Delete` 不再提供默认异常处理策略。调用方必须明确选择
  遇错即停或汇总异常。

## 示例

### 重试异步操作

```csharp
var result = await retryHelper.ExecuteAsync(
    async cancellationToken =>
    {
        return await client.GetStringAsync(uri, cancellationToken);
    },
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

### 认证加密

```csharp
string ciphertext = plaintext.AesEncryptAuthenticated(key);
string plaintext = ciphertext.AesDecrypt(key);
```
