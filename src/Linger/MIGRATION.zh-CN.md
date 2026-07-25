# 为 Linger 2.0 做迁移准备

[English](MIGRATION.md) | [中文](MIGRATION.zh-CN.md)

Linger 1.6.0 是帮助应用从 1.x 平稳迁移到 2.0 的兼容版本。1.6.0
保留现有公共 API。标记为 `Obsolete` 的 API 均有迁移方式，并计划在 2.0
中删除。

## 推荐迁移流程

1. 将所有 Linger 包统一升级到 1.6.0。
2. 编译应用并处理所有与 Linger 相关的弃用警告。
3. 在仍使用 1.6.0 时运行应用的完整测试套件。
4. 升级到 2.0 预览版；如果使用运行时反射、表达式或动态查询 API，安装
   `Linger.Reflection`。
5. 面向裁剪或 Native AOT 发布的应用应重新执行对应的兼容性验证。

## 1.6.0 已提供的替代 API

| 已弃用 API | 替代方式 |
| --- | --- |
| `DataTable.ToListAsync<T>(...)` | `DataTable.ToList<T>(...)` |
| 基于反射的 `DataTable.ToList<T>()` | 使用接收行映射器的重载，或使用工厂与显式列赋值器的重载 |
| `RetryHelper.ExecuteAsync(Func<Task...>)` | `ExecuteAsync(Func<CancellationToken, Task...>)` |
| `ExtensionMethodSetting` 中的 JSON 选项 | `JsonDefaults.CreateResponseOptions()` / `CreateRequestOptions()` |
| `SerializeJson<T>` / `JsonExtensions.Serialize<T>` | `ToJsonString(...)` |
| `DeserializeJson<T>` | `Deserialize<T>(...)` |
| `ObjectExtensions.ForIn` | `ForEachProperty` |
| `ArrayExtensions.ForEach` | `Array.ForEach` 或 `IEnumerable<T>.ForEach` 扩展方法 |
| `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` |
| `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` |
| `DecimalExtensions.ToRounding` | `Round` |
| `byte[].ToImageBase64String` | `ToImageDataUri(mediaType)` |
| `string[].ToEnumerable` | 直接使用数组，或使用 `value ?? Array.Empty<string>()` |
| `string[].ToList` / `ToListOrEmpty` | `new List<string>(value ?? Array.Empty<string>())` |
| `string.ToSplitList(char)` | `SplitToList(char)` |
| `string.ToSplitArray(char)` | `SplitToArray(char)` |
| `string.ToSplitArrayByCrlf` | `SplitToArray(Environment.NewLine)` |
| `string.RemoveLastChar(string)` | `RemoveLastChar(char)` 或 `RemoveSuffixOnce(...)` |
| `PathExtensions.IsStrictAbsolutePath` | `Path.IsPathFullyQualified`，并显式处理 UNC 策略 |
| `int.ToFileSizeBytesString` | `FormatFileSize` |
| `FileInfoExtensions.Delete(files)` | `Delete(files, consolidateExceptions)` |
| `FileInfoExtensions.GetVersionInfo` | `FileVersionInfo.GetVersionInfo(fileInfo.FullName)`，并显式处理平台策略 |
| `string.GetFileVersion` | `new FileInfo(filePath).GetFileVersion()` |
| `string.GetFilePath` | `Path.GetDirectoryName(filePath)`，并在需要时显式追加路径分隔符 |
| `FileInfoExtensions.FileSize` | `GetFileSizeFormatted` |
| `GetFileNameNoExtension` | `Path.GetFileNameWithoutExtension` |
| `GetExtensionNotDotString` | `Path.GetExtension(path).TrimStart('.')` |
| `FileHelper.GetLineCount` | `File.ReadLines(path).Count()` |
| `FileHelper.GetFileSize` | `filePath.GetFileSize()` |
| `FileHelper.GetDirectories` / `GetFileNames` 的布尔参数重载 | 接收 `SearchOption` 的重载 |
| `FileHelper.CreateDirectoryIfNotExists` | `Directory.CreateDirectory` |
| `GuidCode.GetInt32UniqueCode` / `GetInt64UniqueCode` | 使用完整 `Guid` |
| `ConfigureEmail` / `ConfigureMailKit` | `AddEmailService` |
| `CovertToObject<T>` | `ConvertToObject<T>` |
| `DataTableToFile` / `DataSetToFile` | `DataTableToExcel` / `DataSetToExcel` |
| `TryRefreshTokenAsync` | `RefreshTokenResultAsync` |
| `ExecuteResult` / `ExecuteResult<T>` | `Result` / `Result<T>` |
| `ErrorObj` | `Error` |
| `TypeConverter.ConvertTo` / `TryConvertTo` | 对受支持的标量类型使用 `TypeConverter.TryConvert`，或使用类型明确的转换 API |

`TypeConverter.TryConvert` 不会发现自定义的
`System.ComponentModel.TypeConverter` 实现。应用自定义目标类型时，应使用显式的
解析器或转换器。

## 没有一对一替代的 API

- 新密文使用 `AesEncryptAuthenticated`。`AesDecrypt` 仍可读取已有的旧格式密文。
- 将 `object.IsNullOrEmpty` 和 `IsNotNullOrEmpty` 替换为针对字符串、集合或
  可空值类型的明确判断。
- 将 `ToTarget<T>` 及相关泛型转换方法替换为 `ToInt`、`TryToDateTime`、
  `TryToGuid` 等类型明确的 API。
- 在应用层明确根节点、重复键和循环策略，不再依赖 `ToTree` 构建树。
- 使用 `JsonDocument`、`JsonElement` 或 `JsonNode` 替代
  `DeserializeDynamicJsonObject` 和 `JsonTextAccessor`。
- 使用明确的 `Directory` 和 `File` 操作替代 `FileHelper.DeleteFolderFiles`。

## 2.0 中的反射包变化

2.0 会将运行时反射和基于表达式的 API 从 `Linger.Utils` 移入可选的
`Linger.Reflection` 包，包括动态查询排序、表达式帮助方法、运行时属性和
特性访问、枚举元数据、基于反射的 DataTable 映射以及
`IEnumerable<T>.ToDataTable`。

| 1.6 原包 | 1.6 API | 2.0 替代方式 | 2.0 所在包 |
| --- | --- | --- | --- |
| `Linger.Utils` | `DataTable.ToListAsync<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` |
| `Linger.Utils` | 基于反射的 `DataTable.ToList<T>()` | `DataTable.ToList<T>()` | `Linger.Reflection` |
| `Linger.Utils` | `ObjectExtensions.ForIn` | `ForEachProperty` | `Linger.Reflection` |
| `Linger.Utils` | `TypeExtensions.AttrValues<T>` | `AttrPropValues<T>` | `Linger.Reflection` |
| `Linger.Utils` | `IQueryable.OrderByIf<T, TQueryable>` | `IQueryable.OrderByIf<T>(bool, string)` | `Linger.Reflection` |
| `Linger.Utils` | 其他运行时元数据、表达式树及基于反射的映射 API | 保持相同 API | `Linger.Reflection` |

接收映射器或工厂的 `DataTable.ToList<T>` 重载仍保留在 `Linger.Utils` 中。
不需要运行时反射时，应优先迁移到这些重载。

如果应用使用这些 API，升级到 2.0 时需要安装：

```bash
dotnet add package Linger.Reflection
```

面向裁剪和 Native AOT 的代码应继续使用 `Linger.Utils` 中的显式映射器、
工厂、标量转换以及其他不依赖反射的 API。
