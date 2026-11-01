# Linger.Utils

[2.0.0-preview.4 迁移指南](https://github.com/Linger06/Linger/blob/v2.0.0-preview.4/src/Linger/MIGRATION.zh-CN.md)

[![NuGet](https://img.shields.io/nuget/v/Linger.Utils.svg)](https://www.nuget.org/packages/Linger.Utils/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-11.0%20%7C%2010.0%20%7C%209.0%20%7C%208.0%20%7C%20Standard%202.0-512BD4)](https://dotnet.microsoft.com/)

`Linger.Utils` 是面向日常 .NET 开发的工具库，提供字符串、数值、日期、集合、文件、DataTable、加密和参数验证等扩展方法与帮助类。核心 API 保持轻量并兼容多个目标框架；运行时反射和表达式树功能位于可选的 `Linger.Reflection` 包。

需要完整示例、行为约定和兼容性说明时，请参阅 [详细使用指南](docs/usage.zh-CN.md)。

## 功能概览

- **字符串与类型转换**：验证、拆分、截断、格式化，以及可预测的数值和常用类型转换
- **日期与时间**：年龄计算、日期范围判断、日期边界和类型转换
- **集合与数据**：分页、遍历、连接操作、DataTable 映射和分隔字符串转换
- **文件与路径**：文件读写、复制、移动、删除、目录操作和跨平台路径处理
- **安全与可靠性**：认证加密、重试策略、GUID 工具和参数验证
- **兼容性**：为旧目标框架补齐部分 BCL API，并提供 AOT 友好的显式映射 API

## 安装

```bash
dotnet add package Linger.Utils --version 2.0.0-preview.4
```

支持的目标框架：

- .NET 11.0
- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## 快速开始

### 字符串与类型转换

```csharp
using Linger.Extensions.Core;

string value = "123";
int number = value.ToIntOrDefault();
int? nullableNumber = value.ToIntOrNull();
bool isEmail = "user@example.com".IsEmail();

string[] columns = "id,name,email".SplitToArray(',');
string shortened = "a long piece of text".Truncate(10);
```

`ToIntOrDefault`、`ToIntOrNull` 和 `TryToInt` 对小数转整数采用保守策略：只有小数部分为 0 且数值在目标类型范围内时才转换成功。需要正则表达式分隔符时使用 `ToSplitList(string)`；字面量分隔符优先使用 `SplitToArray` 或 `SplitToList`。

### 日期、GUID 与枚举

```csharp
using Linger.Extensions.Core;

DateTime birthDate = new(1990, 5, 15);
int age = birthDate.CalculateAge();
bool inRange = DateTime.Now.InRange(DateTime.Today, DateTime.Today.AddDays(7));

Guid? id = null;
bool isEmpty = id.IsNullOrEmpty();
string generatedId = GuidCode.NewId;

enum Status
{
    Active
}

Status status = "Active".GetEnum<Status>();
```

日期边界还可以通过 `StartOfDay`、`EndOfDay`、`StartOfMonth` 和 `EndOfMonth` 获取。`DateTime` 扩展同时提供 `DateOnly`、`TimeOnly` 与 `DateTime` 之间的转换。

### 文件与路径

```csharp
using Linger.Extensions.IO;
using Linger.Helper;

FileHelper.CreateFile("data.txt", "Hello World");
if (FileHelper.TryReadText("data.txt", out string content))
{
    Console.WriteLine(content);
}

FileHelper.CopyFile("source.txt", "backup/dest.txt");
FileHelper.DeleteFileIfExists("temp.txt");
FileHelper.CopyDir("sourceFolder", "backupFolder");
FileHelper.ClearDirectory("temp");

string normalized = PathHelper.CleanAndNormalizePureString(
    @"temp\\folder//file.txt\\",
    preserveEndingSeparator: false);
bool hasInvalidChars = "file<name>.txt".ContainsInvalidPathChars();
```

### 集合与连接

```csharp
using Linger.Extensions.Collection;

var values = new List<int> { 1, 2, 3, 4, 5 };
var page = values.Paging(pageIndex: 2, pageSize: 2);
string text = values.ToSeparatedString(", ");
values.ForEach(Console.WriteLine);

var left = employees.LeftJoin(
    departments,
    employee => employee.DepartmentId,
    department => department.Id,
    (employee, department) => new
    {
        Employee = employee.Name,
        Department = department?.Name ?? "无部门"
    });
```

`LeftJoin`、`RightJoin` 和 `FullJoin` 在新目标框架上使用 BCL 实现，在旧目标框架上由条件编译的兼容实现提供。无结果选择器的元组 `LeftJoin` 重载仍由 Linger 提供。

### DataTable 映射

```csharp
using Linger.Extensions.Data;

DataTable? table = GetDataTable();
List<UserDto>? users = table.ToList(row => new UserDto
{
    Id = Convert.ToInt32(row["Id"]),
    Name = row["Name"]?.ToString()
});
```

上面的显式 mapper 不依赖反射，适合裁剪和 Native AOT 场景。基于属性的自动映射位于 `Linger.Reflection`。

### 对象、数组与枚举

```csharp
using Linger.Extensions.Core;

object value = "123";
int number = value.ToIntOrDefault();

int[] numbers = new[] { 1, 2, 3 };
numbers.ForEach((item, index) => Console.WriteLine($"{index}: {item}"));

Status status = 1.GetEnum<Status>();
string name = 1.GetEnumName<Status>();
```

枚举的 `DescriptionAttribute` 和 `DisplayAttribute` 读取功能属于 `Linger.Reflection`；基础枚举解析和数值转换由 `Linger.Utils` 提供。

### 认证加密

```csharp
using Linger.Extensions.Core;

string plaintext = "敏感数据";
string encrypted = plaintext.AesEncryptAuthenticated(key);
string decrypted = encrypted.AesDecryptAuthenticated(key);
```

认证加密会使用随机 salt 和 IV，并通过 HMAC-SHA256 校验密文完整性。生产环境应使用专用密钥管理服务保存密钥；旧格式密文可继续使用已标记过时的 `AesDecrypt` 解密，该方法仅用于迁移。

### 重试与取消

```csharp
using Linger.Helper;

var options = new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 1000,
    UseExponentialBackoff = true,
    MaxDelayMilliseconds = 30000
};

var retry = new RetryHelper(options);
var result = await retry.ExecuteAsync(
    ct => SendRequestAsync(ct),
    "网络请求",
    shouldRetry: exception => exception is HttpRequestException or TimeoutException,
    cancellationToken: cancellationToken);
```

异步操作应传递 `CancellationToken`。重试异常需要通过 `shouldRetry` 明确指定；省略该谓词时不会自动重试异常。

### 参数验证

```csharp
public void ProcessData(string data, IEnumerable<int> values)
{
    ArgumentNullException.ThrowIfNull(data);
    ArgumentException.ThrowIfNullOrWhiteSpace(data);
    ArgumentNullException.ThrowIfNull(values);
}
```

`Linger.Utils` 在所有目标框架上都使用这些 BCL API。它为 .NET Standard 2.0 和 .NET Framework 4.7.2 编译的兼容副本是 `internal` 的，不会对外公开；面向这两个框架的代码需要自备 Polyfill，或改用 `GuardExtensions`。

## 功能索引

| 功能 | 主要 API 或类型 |
| --- | --- |
| 字符串 | `StringExtensions`、`SplitToArray`、`SplitToList`、`ToSplitList` |
| 数值和对象转换 | `ToXxx`、`ToXxxOrNull`、`ToXxxOrDefault`、`TryToXxx` |
| 日期时间 | `DateTimeExtensions`（包含 `DateOnly`、`TimeOnly` 转换） |
| 集合和数组 | `IEnumerableExtensions`、`ListExtensions`、`ArrayExtensions` |
| DataTable | `DataTableExtensions` 的显式 mapper/factory API |
| 文件和路径 | `FileHelper`、`PathHelper`、`PathExtensions` |
| 加密 | `AesEncryptAuthenticated`、`AesDecryptAuthenticated` |
| GUID 和枚举 | `GuidCode`、GUID 扩展、`EnumExtensions` |
| 重试 | `RetryHelper`、`RetryOptions` |
| 参数验证 | `ThrowIfNull`、`ThrowIfNullOrEmpty`、`ThrowIfNullOrWhiteSpace` |

## 可选功能

### `Linger.Reflection`

需要运行时元数据或表达式树时安装：

```bash
dotnet add package Linger.Reflection --version 2.0.0-preview.4
```

该包提供表达式组合、动态 `IQueryable` 排序、运行时属性访问、枚举特性读取，以及反射版 `IEnumerable<T>.ToDataTable()` 和 `DataTable.ToList<T>()`。这些 API 不适合 Native AOT 或对裁剪敏感的路径。

### `Linger.Json`

需要 `System.Text.Json` 转换器、DataTable JSON 转换或 `JsonDefaults` 时安装 `Linger.Json`。详见 [Linger.Json 中文文档](../Linger.Json/README.zh-CN.md)。

## 兼容性与设计约定

- .NET 10 及更高版本优先使用框架提供的 `LeftJoin` 和 `RightJoin`；.NET 11 及更高版本优先使用框架提供的 `FullJoin`。
- .NET Standard 2.0 和 .NET Framework 4.7.2 构建会编译 `internal` 的 Polyfill，覆盖参数验证、哈希、十六进制转换和可空性注解。它们属于实现细节，不会暴露给引用方项目。
- 需要 Native AOT 或裁剪支持时，优先使用显式 mapper/factory API，避免依赖运行时反射。
- 数值转换优先走无损、低分配路径；常规转换失败应使用 `TryToXxx` 或 `ToXxxOrDefault`，不要用异常作为流程控制。
- 文件、网络和数据库等 I/O 操作优先使用异步 API，并将取消令牌传递到底层调用。

## 依赖项

核心库保持轻量化设计。仅在 .NET Framework 和 .NET Standard 2.0 下使用 `System.Data.DataSetExtensions` 提供 DataTable 支持。

## 贡献与许可证

欢迎提交 Pull Request。新增功能应遵循现有代码风格，并补充对应的单元测试和文档。

本项目采用 MIT 许可证。
