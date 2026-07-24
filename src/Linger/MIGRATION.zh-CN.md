# 迁移到 2.0.0-preview.1

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

本文档说明 `Linger.Utils` 和 `Linger.Reflection` 中具有破坏性的 API 删除，
适用于从旧版公共 API 升级的应用程序。

## 直接替代

| 已删除 API | 替代方式 | 说明 |
| --- | --- | --- |
| `DataTable.ToListAsync<T>(...)` | `DataTable.ToList<T>(...)` | 被删除的方法只是使用 `Task.FromResult` 包装同步操作。 |
| `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` | 接收并向实际操作传递取消令牌。 |
| `JsonExtensions.Serialize<T>` | `ToJsonString(...)` | 请同时参阅下文的 JSON 行为变化。 |
| `ObjectReflectionExtensions.ForIn` | `ForEachProperty` | 属性遍历行为不变，名称更明确。 |
| `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` | `AttrValues<T>` 只是一个别名。 |
| `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` | 使用 `IQueryable<T>` 重载。 |
| `DecimalExtensions.ToRounding` | `Round` | 两者均使用库中约定的 decimal 舍入行为。 |
| `byte[].ToImageBase64String` | `ToImageDataUri(mediaType)` | 显式指定真实的图片媒体类型。 |
| `string[].ToEnumerable` | 直接使用数组，或使用 `value ?? Array.Empty<string>()` | 数组已经实现 `IEnumerable<string>`。 |
| `string[].ToList` 和 `ToListOrEmpty` | `new List<string>(value ?? Array.Empty<string>())` | 确定数组非空时可使用 `Enumerable.ToList()`。 |
| `string.ToSplitList(char)` | `SplitToList(char)` | 正则表达式重载 `ToSplitList(string)` 仍然保留。 |
| `string.ToSplitArray(char)` | `SplitToArray(char)` | 使用字面量字符作为分隔符。 |
| `string.ToSplitArrayByCrlf` | `SplitToArray(Environment.NewLine)` | 显式指定换行分隔符。 |
| `string.RemoveLastChar(string)` | `RemoveLastChar(char)` 或 `RemoveSuffixOnce(string, StringComparison)` | 根据需求选择单字符删除或精确后缀删除。 |
| `PathExtensions.IsStrictAbsolutePath` | `Path.IsPathFullyQualified` | UNC 路径策略应由调用方明确处理。 |
| `int.ToFileSizeBytesString` | `FormatFileSize` | 新格式与其他文件大小 API 保持一致。 |
| `FileInfoExtensions.Delete(IEnumerable<FileInfo>)` | `Delete(files, consolidateExceptions)` | 传入 `false` 保留遇错即停行为，传入 `true` 汇总符合条件的异常。 |
| `FileInfoExtensions.GetVersionInfo` | `FileVersionInfo.GetVersionInfo(fileInfo.FullName)` | 平台兼容策略由调用方处理。 |
| `string.GetFileVersion` | `new FileInfo(filePath).GetFileVersion()` | 替代方法会校验文件路径以及文件是否存在。 |
| `string.GetFilePath` | `Path.GetDirectoryName(path)` | 仅在调用方确实需要时追加目录分隔符。 |
| `FileInfoExtensions.FileSize` | `GetFileSizeFormatted` | 文件路径和 `FileInfo` 均有对应重载。 |
| `GetFileNameNoExtension` | `Path.GetFileNameWithoutExtension` | `FileInfo` 输入应使用 `fileInfo.Name`。 |
| `GetExtensionNotDotString` | `Path.GetExtension(path).TrimStart('.')` | 直接使用平台路径 API。 |
| `FileHelper.GetLineCount` | `File.ReadLines(path).Count()` | 需要继续处理文件内容时，建议使用专用的流式读取逻辑。 |
| `FileHelper.GetFileSize` | `filePath.GetFileSize()` | 使用当前维护的文件扩展 API。 |
| `FileHelper.GetDirectories` / `GetFileNames` 的布尔参数重载 | 接收 `SearchOption` 的重载 | 显式使用 `TopDirectoryOnly` 或 `AllDirectories`。 |
| `FileHelper.CreateDirectoryIfNotExists` | `Directory.CreateDirectory` | `Directory.CreateDirectory` 本身已经具备幂等性。 |

## 没有一对一替代

| 已删除 API | 迁移方式 | 删除原因 |
| --- | --- | --- |
| `AesEncrypt` | 使用 `AesEncryptAuthenticated`。已有的旧格式密文仍可通过 `AesDecrypt` 解密。 | 未经认证的 AES-CBC 无法保护密文完整性。 |
| `GuidCode.GetInt32UniqueCode` / `GetInt64UniqueCode` | 使用 `Guid.NewGuid()`；在 .NET 9 或更高版本中，需要按时间排序时可考虑 `Guid.CreateVersion7()`。 | 截断 GUID 无法保证唯一性。 |
| `object.IsNullOrEmpty` / `IsNotNullOrEmpty` | 根据类型使用 `string.IsNullOrEmpty`、集合数量判断或 `Guid?` 扩展方法。 | 任意对象不存在统一、明确的“空”状态。 |
| `ToTarget<T>`、`TryToTarget<T>`、`ToTargetOrDefault<T>` 和 `ToTargetOrNull<T>` | 使用明确的 `ToInt`、`TryToDateTime`、`TryToGuid` 等转换 API；目标类型只能在运行时确定时，使用 `TypeConverter.TryConvert`。 | 泛型转换隐藏了支持范围和失败语义。 |
| `TypeConverter.ConvertTo`、`TryConvertTo` 和 `TryConvertKnownType` | 使用 `TypeConverter.TryConvert`。 | 保留的 API 具有明确且兼容 AOT 的标量类型支持边界。 |
| `ToTree` | 在调用方基于明确的节点模型、根节点规则、重复键策略和循环处理规则构建树。 | 通用帮助方法隐藏了树构建所需的关键业务语义。 |
| `FileHelper.DeleteFolderFiles` | 在调用方显式组合所需的 `Directory` 和 `File` 操作。 | 按文件名递归删除的行为对于通用工具 API 过于隐式。 |
| `ExtensionMethodSetting` 中的默认值 | 显式配置 `Encoding`、`CultureInfo`、缓冲区大小和 `JsonSerializerOptions`。JSON 可使用 `JsonDefaults.CreateRequestOptions`、`CreateResponseOptions` 或 `ApplyDefaultConfiguration`。 | 进程级可变默认值会使行为难以推断。 |
| `DataContractJsonSerializer` 相关帮助方法 | 使用 `System.Text.Json.JsonSerializer`，或使用 `ToJsonString` / `Deserialize<T>`。 | 项目统一采用 `System.Text.Json`。 |
| `DeserializeDynamicJsonObject` | 根据是否需要可变 JSON，使用 `JsonDocument`、`JsonElement` 或 `JsonNode`。 | 动态 JSON 隐藏了数据结构和运行时失败方式。 |

## 行为变化

- `ToJsonString(null)` 返回 JSON 字面量 `"null"`。已删除的 `Serialize<T>`
  包装方法在输入为 null 时返回 C# 的 `null` 引用。
- 已删除的 `DataTable.ToListAsync` 不执行异步 I/O。使用同步的 `ToList`
  可以避免产生误导性的异步 API。
- 字符串排序表达式只接受 `asc` 或 `desc`，忽略大小写。无效方向现在会抛出
  `ArgumentException`，不再被静默视为降序。
- `FileInfoExtensions.Delete` 不再提供默认异常处理策略。调用方必须明确选择
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
