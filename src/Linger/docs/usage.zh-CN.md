# Linger.Utils 使用指南

`Linger.Utils` 提供面向日常 .NET 开发的扩展方法和帮助类。本指南说明高频 API 的使用方式、失败行为、兼容性和适用边界；完整 API 以源代码 XML 注释和 IntelliSense 为准。

## 目录

- [安装与命名空间](#安装与命名空间)
- [类型转换](#类型转换)
- [字符串处理](#字符串处理)
- [日期时间、GUID 和枚举](#日期时间guid-和枚举)
- [集合操作](#集合操作)
- [DataTable 与 AOT](#datatable-与-aot)
- [文件、路径与流](#文件路径与流)
- [认证加密](#认证加密)
- [重试与取消](#重试与取消)
- [参数验证](#参数验证)
- [目标框架与 Polyfill](#目标框架与-polyfill)

## 安装与命名空间

```bash
dotnet add package Linger.Utils --version 2.0.0-preview.4
```

常用命名空间：

```csharp
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using Linger.Extensions.IO;
using Linger.Helper;
```

安装后不需要注册服务。大多数功能以扩展方法提供，`FileHelper`、`PathHelper` 和 `RetryHelper` 等帮助类可以直接调用或实例化。

## 类型转换

### 选择转换方法

转换 API 按失败策略分为四类：

| 方法 | 失败时的行为 | 适用场景 |
| --- | --- | --- |
| `ToXxx()` | 抛出异常 | 输入无效就应立即终止的场景 |
| `ToXxxOrNull()` | 返回 `null` | 需要区分“无值”和有效零值 |
| `ToXxxOrDefault()` | 返回调用方提供的默认值 | 解析配置或用户输入 |
| `TryToXxx(out value)` | 返回 `bool` | 需要显式处理成功或失败 |

```csharp
using Linger.Extensions.Core;

string input = "42";
int strict = input.ToInt();
int? nullable = input.ToIntOrNull();
int fallback = "invalid".ToIntOrDefault(-1);

if (input.TryToInt(out int parsed))
{
    Console.WriteLine(parsed);
}
```

字符串和 `object` 的转换使用不变区域性。`null`、`DBNull`、格式错误、超出范围或不满足整值要求时，`ToIntOrNull` 返回 `null`，`ToIntOrDefault` 返回默认值。

### 小数转换

整数目标类型只接受没有小数部分且处于目标类型范围内的值：

```csharp
decimal whole = 12.000m;
decimal fraction = 12.5m;

int accepted = whole.ToIntOrDefault(-1);  // 12
int rejected = fraction.ToIntOrDefault(-1); // -1
```

这项规则避免把截断误认为转换成功。需要保留小数时，应使用 `decimal`、`double` 或对应的目标类型。

### 对象转换

对象转换会优先处理已经匹配的类型，再处理常见数值类型，最后才使用字符串表示进行转换：

```csharp
object value = "123";
int result = value.ToIntOrDefault();

object invalid = new object();
int defaultValue = invalid.ToIntOrDefault(-1);
```

对不可接受的输入使用 `ToXxx()` 会抛出参数或格式相关异常；在外部输入场景优先选择 `TryToXxx` 或 `ToXxxOrDefault`。

## 字符串处理

### 拆分和验证

`SplitToArray` 和 `SplitToList` 使用字面量分隔符，并在输入为空时返回空集合：

```csharp
using Linger.Extensions.Core;

string[] fields = "id,name,email".SplitToArray(',');
IEnumerable<string> lines = "first\r\nsecond".SplitToList("\r\n");

bool validEmail = "user@example.com".IsEmail();
bool isNumber = "123.45".IsNumber();
bool isInteger = "123".IsInteger();
```

`StringSplitOptions` 可以传给数组和列表重载：

```csharp
string[] values = "a,,b".SplitToArray(',', StringSplitOptions.RemoveEmptyEntries);
```

### 截断和查询字符串

```csharp
string title = "This is a long title".Truncate(10);
string url = "https://example.com/items".AppendQuery("page=2&size=20");
```

`AppendQuery` 会对键和值进行 URL 编码，并保留原 URL 的片段标识符。传入字典或键值对列表时可以一次追加多个参数。

## 日期时间、GUID 和枚举

### 日期时间

```csharp
using Linger.Extensions.Core;

DateTime now = DateTime.Now;
DateTime start = now.StartOfDay();
DateTime end = now.EndOfDay();
DateTime monthStart = now.StartOfMonth();
DateTime monthEnd = now.EndOfMonth();

int age = new DateTime(1990, 5, 15).CalculateAge();
bool inRange = now.InRange(DateTime.Today, DateTime.Today.AddDays(7));
```

日期区间判断包含最小值和最大值。`DateTimeExtensions` 还提供 `DateOnly`、`TimeOnly` 和 `DateTimeOffset` 的转换；使用这些 API 时仍需根据业务明确时区语义。

### GUID

```csharp
using Linger.Extensions.Core;
using Linger.Helper;

Guid? optionalId = null;
bool empty = optionalId.IsNullOrEmpty();
long numericId = Guid.NewGuid().ToInt64();
string sortableId = GuidCode.NewId;
```

在 .NET 9 及更高版本，使用 `Guid.CreateVersion7()` 生成带时间排序特征的 GUID。不要把 GUID 转换成整数作为无碰撞的数据库主键保证。

### 枚举

```csharp
using Linger.Extensions.Core;

enum Status
{
    Active = 1,
    Inactive = 2
}

Status fromName = "Active".GetEnum<Status>();
Status fromValue = 1.GetEnum<Status>();
bool parsed = "Active".TryGetEnum(out Status status);
string? name = 1.GetEnumName<Status>();
```

`DescriptionAttribute` 和 `DisplayAttribute` 的读取属于 `Linger.Reflection`，不属于基础 `Linger.Utils`。

## 集合操作

### 空集合、遍历和分页

```csharp
using Linger.Extensions.Collection;

var values = new[] { 1, 2, 3, 4, 5 };
bool empty = values.IsNullOrEmpty();
string text = values.ToSeparatedString(", ");
values.ForEach((value, index) => Console.WriteLine($"{index}: {value}"));

IEnumerable<int> page = values.Paging(pageIndex: 2, pageSize: 2);
// page: 3, 4
```

`Paging` 使用从 1 开始的页码。源集合为 `null`、页码小于等于 0 或页大小小于等于 0 时返回空序列，不会抛出异常。

### Join

```csharp
var result = employees.LeftJoin(
    departments,
    employee => employee.DepartmentId,
    department => department.Id,
    (employee, department) => new
    {
        Employee = employee.Name,
        Department = department?.Name ?? "无部门"
    });
```

`LeftJoin` 保留左侧全部记录，`RightJoin` 保留右侧全部记录，`FullJoin` 保留两侧全部记录。没有匹配项时对应元素为其类型的默认值；重复键会产生多条结果。需要不区分大小写等规则时传入 `IEqualityComparer<TKey>`。

在 .NET 10 及更高版本，`LeftJoin` 和 `RightJoin` 优先使用框架实现；在 .NET 11 及更高版本，`FullJoin` 也优先使用框架实现。旧目标框架由 Linger 提供兼容实现。

## DataTable 与 AOT

### 显式映射

显式 mapper 不依赖反射，适合 Native AOT 和裁剪场景：

```csharp
using Linger.Extensions.Data;

DataTable? table = GetDataTable();
List<UserDto>? users = table.ToList(row => new UserDto
{
    Id = Convert.ToInt32(row["Id"]),
    Name = row["Name"]?.ToString()
});
```

需要复用属性赋值逻辑时，可以使用 `CreateColumnSetter`：

```csharp
var setters = new Dictionary<string, Action<UserDto, object?>>
{
    ["Id"] = DataTableExtensions.CreateColumnSetter<UserDto, int>((item, value) => item.Id = value),
    ["Name"] = DataTableExtensions.CreateColumnSetter<UserDto, string?>((item, value) => item.Name = value)
};

List<UserDto>? users = table.ToList(() => new UserDto(), setters);
```

### 反射映射

自动读取属性并映射到对象的 API 位于 `Linger.Reflection`。它适合常规运行时应用，但在 Native AOT 或启用裁剪时应优先使用显式 mapper 或编译期投影。

## 文件、路径与流

### 文件助手

```csharp
using Linger.Helper;

FileHelper.CreateFile("data.txt", content: "Hello World");

if (FileHelper.TryReadText("data.txt", out string? content))
{
    Console.WriteLine(content);
}

FileHelper.CopyFile("source.txt", "backup/dest.txt");
FileHelper.MoveFile("backup/dest.txt", "archive");
FileHelper.DeleteFileIfExists("temp.txt");
```

`CopyFile` 会创建目标目录。`TryReadText` 在文件不存在等读取失败时返回 `false`；需要区分权限错误、路径错误等原因时，应直接使用 BCL 文件 API并处理具体异常。

### 路径处理

```csharp
using Linger.Extensions.IO;
using Linger.Helper;

string normalized = PathHelper.CleanAndNormalizePureString(
    @"temp\\folder//file.txt\\",
    preserveEndingSeparator: false);

bool invalid = "file<name>.txt".ContainsInvalidPathChars();
bool fileExists = PathExtensions.Exists("data.txt", checkAsFile: true);
bool directoryExists = PathExtensions.Exists("data", checkAsFile: false);
```

`CleanAndNormalizePureString` 只规范化字符串，不访问文件系统。`ContainsInvalidPathChars` 的结果受当前操作系统规则影响；不要把它当作完整的路径安全校验。

### 流和取消

```csharp
using Linger.Extensions.IO;

using var source = File.OpenRead("source.bin");
await source.ToFileAsync("backup/source.bin", cancellationToken);
```

异步流操作应传递取消令牌。`ToFileAsync` 会创建目标目录并覆盖目标文件；对不可重复读取的流，调用前应确认当前位置和重试策略。

## 认证加密

新代码使用认证加密：

```csharp
using Linger.Extensions.Core;

string source = "敏感数据";
string encrypted = source.AesEncryptAuthenticated(key);
string decrypted = encrypted.AesDecryptAuthenticated(key);
```

该格式使用 PBKDF2-SHA256 派生加密密钥和认证密钥，随机生成 salt 与 IV，并使用 HMAC-SHA256 校验完整性。相同明文和密钥每次产生的密文不应相同。

使用约定：

- 密钥应存放在密钥管理服务或安全凭据存储中，不要写入源代码、日志或配置仓库。
- 认证失败表示密钥错误或密文被修改，应按安全错误处理，不要忽略异常后继续使用明文。
- `AesDecrypt` 已标记过时。它能识别认证格式，但会静默回退到旧版未认证 AES-CBC 密文，因此只应用于读取旧数据并用 `AesEncryptAuthenticated` 重新加密；解密当前数据请使用 `AesDecryptAuthenticated`。
- 不要使用已移除或不安全的 DES 方案。

## 重试与取消

### 异常重试

```csharp
using Linger.Helper;

var retry = new RetryHelper(new RetryOptions
{
    MaxRetryAttempts = 3,
    DelayMilliseconds = 500,
    UseExponentialBackoff = true,
    MaxDelayMilliseconds = 10000,
    Jitter = 0.2
});

var response = await retry.ExecuteAsync(
    ct => SendRequestAsync(ct),
    operationName: "发送请求",
    shouldRetry: exception => exception is HttpRequestException or TimeoutException,
    cancellationToken: cancellationToken);
```

`MaxRetryAttempts` 包含首次执行。省略 `shouldRetry` 时不会自动重试异常；不可恢复的参数错误和认证错误不应重试。调用方 `cancellationToken` 的取消总是立即传播；其他 `OperationCanceledException`（例如 `HttpClient` 超时抛出的 `TaskCanceledException`）交给 `shouldRetry` 判断。

设置 `Jitter` 后，每次延迟在 `[延迟 × (1 − Jitter), 延迟 × (1 + Jitter)]` 内取值，且不会超过 `MaxDelayMilliseconds`。

### 结果重试

当操作通过返回值表示暂时失败时，可以设置 `shouldRetryResult`：

```csharp
var result = await retry.ExecuteAsync(
    ct => TryUploadAsync(ct),
    operationName: "上传文件",
    shouldRetry: exception => exception is IOException,
    shouldRetryResult: success => !success,
    cancellationToken: cancellationToken);
```

达到最大次数后，结果重试会返回最后一次结果；异常重试耗尽则抛出 `OutOfRetryCountException`。底层操作必须接收并使用传入的 `CancellationToken`。

## 参数验证

现代目标框架可以直接使用这些 BCL 验证方法。.NET Standard 2.0 和 .NET Framework 4.7.2 没有它们，而 `Linger.Utils` 不会公开其内部 Polyfill，因此面向这两个框架的代码需要自备 Polyfill：

```csharp
public void ProcessData(string data, IEnumerable<int> values)
{
    ArgumentNullException.ThrowIfNull(data);
    ArgumentException.ThrowIfNullOrWhiteSpace(data);
    ArgumentNullException.ThrowIfNull(values);
}
```

需要链式表达式时，可以使用 `GuardExtensions`：

```csharp
string normalized = input
    .EnsureIsNotNullOrWhiteSpace(nameof(input))
    .Trim();

int pageSize = requestedSize.EnsureIsInRange(1, 100);
```

验证方法会抛出具体的 `ArgumentNullException`、`ArgumentException` 或 `ArgumentOutOfRangeException`。文件和目录检查分别使用 `EnsureFileExists` 与 `EnsureDirectoryExists`。

## 目标框架与 Polyfill

`Linger.Utils` 支持：

- .NET 11.0
- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

在 .NET Standard 2.0 和 .NET Framework 4.7.2 上，`Linger.Utils` 会为自身编译 `internal` 的部分新 BCL API 兼容副本，包括参数验证、哈希、十六进制转换和可空性注解。它们不属于公开 API；在这两个框架上引用 `Linger.Utils` 并希望使用同样 API 的项目，需要自备 Polyfill。

`DistinctBy`、`LeftJoin`、`RightJoin` 等集合辅助方法是 `Linger.Extensions.Collection` 中的公开扩展方法，只在框架没有对应 BCL 版本的目标上编译。

使用 Native AOT 或裁剪时：

1. 优先使用 `Linger.Utils` 的显式 mapper/factory API。
2. 避免依赖 `Linger.Reflection` 的运行时属性扫描和表达式树功能。
3. 对网络、文件和数据库调用传递取消令牌。
4. 对输入转换使用 `TryToXxx` 或 `ToXxxOrDefault`，不要用异常作为常规分支。

## 相关文档

- [Linger.Utils 中文 README](../README.zh-CN.md)
- [Linger.Utils 迁移指南](../MIGRATION.zh-CN.md)
- [Linger.Reflection 中文 README](../../Linger.Reflection/README.zh-CN.md)
- [Linger.Json 中文 README](../../Linger.Json/README.zh-CN.md)
