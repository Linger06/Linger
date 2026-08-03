# Linger.Utils

[2.0.0-preview.1 迁移指南](https://github.com/Linger06/Linger/blob/v2.0.0-preview.1/src/Linger/MIGRATION.zh-CN.md)

一个功能丰富的 .NET 工具库，包含大量实用的扩展方法和帮助类，让您的日常开发工作更加轻松高效。反射、表达式树、属性元数据和动态查询能力已拆分至可选的 `Linger.Reflection` 包。

## 概述

Linger.Utils 是专为 .NET 开发者打造的实用工具集合。无论您是在处理字符串、操作日期时间、进行文件管理，还是需要进行数据转换，这个库都能为您提供简洁易用的解决方案。它采用现代 C# 语法，支持多个 .NET 版本，让您的代码更加优雅。

## 目录

- [功能特性](#功能特性)
- [安装](#安装)
- [目标框架](#目标框架)
- [快速开始](#快速开始)
  - [字符串扩展](#字符串扩展)
  - [字符串加密扩展](#字符串加密扩展)
  - [日期时间扩展](#日期时间扩展)
  - [文件操作](#文件操作)
  - [集合扩展](#集合扩展)
    - [DataTable 扩展（AOT 友好）](#datatable-扩展aot-友好)
  - [对象扩展](#对象扩展)
  - [可选 JSON 包](#可选-json-包)
  - [GUID 扩展](#guid-扩展)
  - [数组扩展](#数组扩展)
  - [枚举扩展](#枚举扩展)
  - [参数验证](#参数验证)
- [高级功能](#高级功能)
  - [重试助手](#重试助手)
  - [路径操作](#路径操作)
- [可选反射功能](#可选反射功能)
- [最佳实践](#最佳实践)
- [API 标准化与类型安全](#api-标准化与类型安全)
  - [.NET 10 兼容性（已支持）](#net-10-兼容性已支持)
  - [严格类型安全原则](#严格类型安全原则)

## 功能特性

### 核心扩展
- **字符串扩展**: 提供丰富的字符串处理功能，包括验证、转换、格式化等实用方法
- **字符串加密扩展**: 提供安全的 AES 加密解密功能，保护数据安全
- **日期时间扩展**: 简化日期时间的计算、格式化和各种常用操作
- **数值扩展**: 安全可靠的数值类型转换，遵循**偏保守、可预测的转换策略**，覆盖常用整数与小数转换场景

    说明：已增强小数到整数的转换能力。`DecimalExtensions` 新增 `ToIntOrNull`、`ToIntOrDefault` 与 `TryToInt`（含 nullable 重载）。这些方法仅在小数部分为 0 且数值位于 `Int32` 范围内时返回有效值。示例：`(decimal)1.0000m.ToIntOrDefault() => 1`。

    此外，对象/字符串转换路径现在会将类似 "1.00000" 的字符串视为可转换为整数（在适用时转换为 `1`）。
- **枚举扩展**: 让枚举操作更加便捷，支持字符串解析、数值转换和有效性检查
- **对象扩展**: 通用的对象处理方法，提供空值检查和对象/字符串的常用类型转换能力
- **数组扩展**: 简化数组操作，提供遍历和处理的便捷方法
- **GUID 扩展**: 完善的 GUID 操作工具，包括验证和转换功能

### 集合扩展
- **列表扩展**: 增强列表的操作能力，提供分页、遍历等实用功能
- **集合扩展**: 通用的集合处理工具，让数据操作更简单

### 数据扩展
- **DataTable 扩展**: 简化 DataTable 的操作和处理
- **数据转换**: 提供安全的数据类型转换方法

### 文件系统操作
- **文件助手**: 涵盖文件的读写、复制、移动、删除等所有常用操作
- **路径助手**: 跨平台的路径处理，支持路径验证和规范化
- **目录操作**: 完整的目录管理功能，包括创建、遍历等

### 助手类
- **重试助手**: 为不稳定的操作提供智能重试机制
- **GUID 工具**: 高级 GUID 生成和处理功能
- **平台助手**: 跨平台的操作系统检测和兼容性处理
- **参数验证**: 提供防御性编程所需的输入验证工具

### 可选反射包
- **`Linger.Reflection`**: 提供表达式组合、动态 `IQueryable` 排序、运行时属性访问、枚举元数据和基于反射的 DataTable 转换

### 可选 JSON 包
- **`Linger.Json`**: 提供 `System.Text.Json` 转换器、`DataTable` JSON 转换和 `JsonDefaults` 选项工厂。请参阅 [Linger.Json 文档](../Linger.Json/README.zh-CN.md)。

## 安装

```bash
dotnet add package Linger.Utils
```

仅当应用需要运行时元数据或表达式树 API 时，再安装反射包：

```bash
dotnet add package Linger.Reflection
```

`Linger.Reflection` 单向依赖 `Linger.Utils`。需要裁剪或 Native AOT 的应用应只使用 `Linger.Utils` 的显式 mapper/factory API。

## 目标框架

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## 快速开始

### 字符串扩展

```csharp
using Linger.Extensions.Core;

// 邮箱格式验证
string email = "user@example.com";
bool isValid = email.IsEmail();

// 字符串转数字（带默认值）
string number = "123";
int result = number.ToIntOrDefault(0); // 转换成功返回 123，失败则返回 0
int? nullableResult = number.ToIntOrNull(); // 返回可为空的整数

// 字符串截取
string text = "Hello World";
string leftPart = text.Take(5); // 取前 5 个字符："Hello"
string rightPart = text.TakeLast(5); // 取后 5 个字符："World"
string part = text.Truncate(20, ""); // 安全截取，超长不会报错

// 字符串检查
bool isEmpty = text.IsNullOrEmpty(); // 检查是否为空
bool isNumber = number.IsNumber(); // 检查是否为数字
bool isInt = number.IsInteger(); // 检查是否为整数

// 字面量分隔符
string[] columns = "id,name,email".SplitToArray(',');
IEnumerable<string> lines = "第一行\r\n第二行".SplitToList("\r\n");
```

字面量分隔符请使用 `SplitToArray` / `SplitToList`。`ToSplitList(string)` 仅为正则表达式分隔符保留，并设置了一秒匹配超时。

### 字符串加密扩展

```csharp
using Linger.Extensions.Core;

string data = "敏感数据需要加密";
string aesKey = "mySecretKey12345"; // AES 密钥

// 认证加密解密（推荐）
string aesEncrypted = data.AesEncryptAuthenticated(aesKey);
string aesDecrypted = aesEncrypted.AesDecryptAuthenticated(aesKey);

// 🔐 安全特性：
// - 使用 PBKDF2 派生加密密钥和认证密钥
// - 使用随机 Salt 与 IV，相同明文产生不同密文
// - 使用 HMAC-SHA256 校验密文完整性
// ⚠️ 密钥应安全存储，生产环境使用专业密钥管理方案
```

### 日期时间扩展

```csharp
using Linger.Extensions.Core;

DateTime date = DateTime.Now;

// 计算年龄
DateTime birthDate = new DateTime(1990, 5, 15);
int age = birthDate.CalculateAge(); // 根据生日计算年龄

// 判断日期是否在指定范围内
bool isInRange = date.InRange(DateTime.Today, DateTime.Today.AddDays(7));

// 日期边界处理
DateTime startOfDay = date.StartOfDay(); // 当天 00:00:00
DateTime endOfDay = date.EndOfDay(); // 当天 23:59:59
DateTime startOfMonth = date.StartOfMonth(); // 当月第一天
DateTime endOfMonth = date.EndOfMonth(); // 当月最后一天
```

### 文件操作

```csharp
using Linger.Helper;

// 基本文件操作
FileHelper.CreateFile("data.txt", content: "Hello World"); // 创建并写入文本文件

// Try 风格文件读取（文件不存在不会抛出异常）
if (FileHelper.TryReadText("data.txt", out string content))
{
    Console.WriteLine(content);
}

// 文件复制（自动创建目录）
FileHelper.CopyFile("source.txt", "backup/dest.txt");

// 安全删除文件
FileHelper.DeleteFileIfExists("temp.txt"); // 文件存在才删除，不会报错

// 目录操作
FileHelper.CopyDir("sourceFolder", "backupFolder"); // 递归复制目录
FileHelper.ClearDirectory("temp"); // 清空目录中的所有文件和子目录
```

### 集合扩展

```csharp
using Linger.Extensions.Collection;

var list = new List<int> { 1, 2, 3, 4, 5 };

// 空值安全检查
bool isEmpty = list.IsNullOrEmpty(); // 同时检查 null 和空集合

// 数据分页
var pagedResult = list.Paging(2, 2); // 第 2 页，每页 2 条：返回 [3, 4]

// 转为分隔字符串
string result = list.ToSeparatedString(", "); // 输出："1, 2, 3, 4, 5"

// 遍历处理
list.ForEach(Console.WriteLine); // 对每个元素执行操作

// 使用反射将集合转换为 DataTable，请安装 Linger.Reflection：
// var dataTable = list.Select(x => new { Value = x }).ToDataTable();

// .NET 10+ Join 操作兼容
// LeftJoin/RightJoin 在旧目标框架使用 Polyfill，在 .NET 10 使用框架实现；FullJoin 始终由 Linger 提供

// Left Join（左外连接）- 保留所有左侧记录
var employees = new List<Employee>
{
    new Employee { Id = 1, Name = "张三", DeptId = 1 },
    new Employee { Id = 2, Name = "李四", DeptId = 2 },
    new Employee { Id = 3, Name = "王五", DeptId = 99 } // 没有对应部门
};

var departments = new List<Department>
{
    new Department { Id = 1, Name = "开发部" },
    new Department { Id = 2, Name = "测试部" }
};

var leftJoinResult = employees.LeftJoin(
    departments,
    emp => emp.DeptId,           // 外部键选择器
    dept => dept.Id,             // 内部键选择器
    (emp, dept) => new {
        Employee = emp.Name,
        Department = dept?.Name ?? "无部门"
    }
);
// 输出: 张三-开发部, 李四-测试部, 王五-无部门

// Right Join（右外连接）- 保留所有右侧记录
var rightJoinResult = employees.RightJoin(
    departments, emp => emp.DeptId, dept => dept.Id,
    (emp, dept) => new { Employee = emp?.Name ?? "暂无员工", Department = dept.Name }
);

// Full Join（全外连接）- 保留所有记录
var fullJoinResult = employees.FullJoin(
    departments, emp => emp.DeptId, dept => dept.Id,
    (emp, dept) => new { Employee = emp?.Name ?? "暂无员工", Department = dept?.Name ?? "无部门" }
);

// 简化版本：返回元组
var tupleResult = employees.LeftJoin(departments, e => e.DeptId, d => d.Id);
// 返回 IEnumerable<Tuple<Employee, Department?>>

// 支持自定义比较器
var caseInsensitiveJoin = stringList1.LeftJoin(
    stringList2, s => s, s => s,
    (s1, s2) => new { Left = s1, Right = s2 },
    StringComparer.OrdinalIgnoreCase
);

// 元组便利重载与 FullJoin 在所有目标框架上均由 Linger 提供
```

### DataTable 扩展（AOT 友好）

```csharp
using Linger.Extensions.Data;

// 在 AOT / Trim 场景下推荐使用“无反射映射”
DataTable? table = GetDataTable();

List<UserDto>? users = table.ToList(row => new UserDto
{
    Id = Convert.ToInt32(row["Id"]),
    Name = row["Name"]?.ToString()
});

// 属性映射风格（同样无反射）
List<UserDto>? usersBySetters = table.ToList(
    () => new UserDto(),
    new Dictionary<string, Action<UserDto, object?>>
    {
        ["Id"] = Linger.Extensions.Data.DataTableExtensions.CreateColumnSetter<UserDto, int>((x, v) => x.Id = v),
        ["Name"] = Linger.Extensions.Data.DataTableExtensions.CreateColumnSetter<UserDto, string?>((x, v) => x.Name = v)
    });

```

属性自动映射的 `DataTable.ToList<T>()` 位于 `Linger.Reflection`，不应在 AOT 或裁剪敏感路径使用。

### 对象扩展

```csharp
using Linger.Extensions.Core;

// 类型安全的对象转换
object stringObj = "123";
int intValue = stringObj.ToIntOrDefault(0);           // 成功：123
long longValue = stringObj.ToLongOrDefault(0L);       // 成功：123
decimal decimalValue = stringObj.ToDecimalOrDefault(0m); // 成功：123

// 对常见数值对象也会走安全转换路径
object numberObj = 123.00m;
int convertedInt = numberObj.ToIntOrDefault(0);       // 成功：123

// 仅当值不能无损转换时才返回默认值
object fractionalNumberObj = 123.45;
int invalidInt = fractionalNumberObj.ToIntOrDefault(0); // 小数部分非 0，返回 0
```

**当前内置的常用转换方法**

| 方法 | 目标类型 | 说明 |
|------|---------|------|
| `ToShortOrDefault` | `short` | 支持字符串、常见数值对象与整值 `decimal/double/float` |
| `ToIntOrDefault` | `int` | 支持字符串、常见数值对象与整值 `decimal/double/float` |
| `ToLongOrDefault` | `long` | 支持字符串、常见数值对象与整值 `decimal/double/float` |
| `ToDecimalOrDefault` | `decimal` | 支持字符串及常见数值对象 |
| `ToDateTimeOrDefault` | `DateTime` | 支持字符串、`DateTime`、`DateTimeOffset` |
| `ToBoolOrDefault` | `bool` | 支持字符串，以及 `0/1` 数值语义 |
| `ToGuidOrDefault` | `Guid` | 支持字符串、`Guid` 与 16 字节数组 |

```csharp
// 其他类型转换
object dateObj = "2025-01-01";
DateTime dateValue = dateObj.ToDateTimeOrDefault(DateTime.MinValue);

object guidObj = "550e8400-e29b-41d4-a716-446655440000";
Guid guidValue = guidObj.ToGuidOrDefault();

object boolObj = "true";
bool boolValue = boolObj.ToBoolOrDefault(false);

// 空值安全处理
object obj = GetSomeObject();
string result = obj.ToStringOrDefault("default"); // 为 null 时返回默认值

// 类型检查方法
object testObj = (byte)255;
bool isNumeric = testObj.IsNumeric();                // 检查是否为任意数值类型

// 性能优化的 Try 风格转换 - 避免默认值掩盖失败
if ("123".TryToInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"坏数据".TryToDecimal(out var decVal)) { /* decVal = 0，转换失败 */ }

// 确保前后缀（幂等，不重复添加）
var apiUrl = "api/v1".EnsureStartsWith("/"); // => "/api/v1"
var folder = "logs".EnsureEndsWith("/");     // => "logs/"

// 数值范围验证（使用 Guard 扩展）
int value = 5;
try
{
    // 使用 EnsureIsInRange 进行范围验证（推荐方式）
    int validatedValue = value.EnsureIsInRange(1, 10); // 验证值在 1-10 范围内
    Console.WriteLine($"验证通过，值为: {validatedValue}"); // 输出: 验证通过，值为: 5
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"数值超出范围: {ex.Message}");
}

// 💡 范围验证说明：
// - EnsureIsInRange() 是 Guard 方法，验证失败时抛出 ArgumentOutOfRangeException
// - 验证成功时返回原值，可以链式调用
// - 支持所有实现 IComparable<T> 的类型（int, double, DateTime 等）
// - 范围是包含边界值的（闭区间）[min, max]

// 其他数值范围应用示例：
double price = 99.99;
double validPrice = price.EnsureIsInRange(0.0, 1000.0); // 价格验证

DateTime date = DateTime.Now;
DateTime validDate = date.EnsureIsInRange(DateTime.Today, DateTime.Today.AddDays(30)); // 日期范围验证
```

### 可选 JSON 包

```csharp
using Linger.Extensions;
using Linger.Json;
using System.Text.Json;

// 对象转 JSON
var user = new { Name = "John", Age = 30 };
string json = JsonSerializer.Serialize(user);

// JSON 转对象
var userObj = JsonSerializer.Deserialize<User>(json);

// JSON 转 DataTable（字符串扩展）
string jsonArray = "[{\"Name\":\"John\",\"Age\":30}]";
DataTable? dataTable = jsonArray.ToDataTable();

// 使用 JsonDefaults 获取统一配置
var responseOptions = JsonDefaults.CreateResponseOptions();  // HTTP 响应
var requestOptions = JsonDefaults.CreateRequestOptions();    // HTTP 请求

// 在 WebAPI 中应用配置
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        JsonDefaults.ApplyDefaultConfiguration(options.JsonSerializerOptions));

// 详细配置说明请参考: ../Linger.Json/README.zh-CN.md
```

### GUID 扩展

```csharp
using Linger.Extensions.Core;
using Linger.Helper;

// GUID 检查
Guid guid = Guid.NewGuid();
bool isEmpty = guid.IsEmpty(); // 检查是否为空 GUID
bool isNotEmpty = guid.IsNotEmpty(); // 检查是否不为空

// 可空 GUID 操作
Guid? nullableGuid = null;
bool isNull = nullableGuid.IsNull(); // 检查是否为 null
bool isNotNull = nullableGuid.IsNotNull(); // 检查是否不为 null
bool isNullOrEmpty = nullableGuid.IsNullOrEmpty(); // 检查是否为 null 或空
bool isNotNullOrEmpty = nullableGuid.IsNotNullOrEmpty(); // 检查是否既不为 null 也不为空

// GUID 转换
long longValue = guid.ToInt64(); // 转换为 Int64
int intValue = guid.ToInt32(); // 转换为 Int32

// GuidCode 工具类 - 生成唯一标识符
string uniqueId = GuidCode.NewId; // 基于日期时间 + GUID 的唯一 ID
string dateGuid = GuidCode.NewDateGuid; // 短日期格式的唯一 ID

// .NET 9+ 功能：V7 GUID 生成和时间戳提取
#if NET9_0_OR_GREATER
Guid v7Guid = Guid.CreateVersion7(); // 创建 V7 GUID
DateTimeOffset timestamp = v7Guid.GetTimestamp(); // 从 V7 GUID 提取时间戳
#endif
```

### 数组扩展

```csharp
using Linger.Extensions.Core;

int[] numbers = { 1, 2, 3, 4, 5 };

// 对每个元素执行操作
numbers.ForEach(n => Console.WriteLine(n)); // 输出：1 2 3 4 5

// 带索引迭代
numbers.ForEach((n, index) => Console.WriteLine($"索引 {index}: {n}"));
// 输出：索引 0: 1, 索引 1: 2, ...
```

### 枚举扩展

```csharp
using Linger.Extensions.Core;

public enum Status
{
    Active = 1,
    Inactive = 2,
    Pending = 3
}

// 字符串转枚举
string statusName = "Active";
Status status = statusName.GetEnum<Status>(); // 或 statusName.ToEnum<Status>()

// 整数转枚举
int statusValue = 1;
Status statusFromInt = statusValue.GetEnum<Status>();

// 获取枚举名称
string enumName = statusValue.GetEnumName<Status>(); // 返回 "Active"

```

读取 `DescriptionAttribute` 或 `DisplayAttribute` 的 `GetDescription()` 和 `GetDisplay()` 位于 `Linger.Reflection`。

### 参数验证

```csharp
public void ProcessData(string data, IEnumerable<int> numbers)
{
    // 新框架直接使用 BCL 原生 API
    // 低版本目标框架由 Linger 自动补齐兼容 Polyfill，无需额外 using
    ArgumentNullException.ThrowIfNull(data);                    // 确保不为 null
    ArgumentException.ThrowIfNullOrEmpty(data);                 // 确保不为 null 或空字符串
    ArgumentException.ThrowIfNullOrWhiteSpace(data);            // 确保不为 null、空或纯空白字符
    ArgumentNullException.ThrowIfNull(numbers);                 // 确保集合不为 null

    // 框架支持：
    // - ThrowIfNull: .NET 6+ 使用内置实现，.NET 5 及以下由 Linger 补齐
    // - ThrowIfNullOrEmpty / ThrowIfNullOrWhiteSpace: .NET 8+ 使用内置实现，.NET 7 及以下由 Linger 补齐
}
```

## 可选反射功能

`Linger.Reflection` 提供以下运行时元数据能力：

- 表达式树组合、条件构建和谓词帮助方法
- 按属性名称进行动态 `IQueryable` 排序
- 运行时属性读取、属性枚举和元数据缓存
- 枚举的 `DescriptionAttribute` 与 `DisplayAttribute` 读取
- `IEnumerable<T>.ToDataTable()` 和 `DataTable.ToList<T>()` 的反射转换

这些 API 依赖运行时反射，不适用于 Native AOT 或对裁剪敏感的路径。此类应用应使用 `Linger.Utils` 提供的显式 mapper/factory API 或编译期投影。

完整示例请参阅 [Linger.Reflection 中文 README](../Linger.Reflection/README.zh-CN.md)。

## 高级功能

### 重试助手

```csharp
using Linger.Helper;

// 自定义重试策略
var options = new RetryOptions
{
    MaxRetryAttempts = 3,           // 最多重试 3 次
    DelayMilliseconds = 1000,       // 基础延迟时间 1 秒
    UseExponentialBackoff = true,   // 使用指数退避
    MaxDelayMilliseconds = 30000,   // 最大延迟时间 30 秒
    Jitter = 0.2                    // 抖动因子 20%
};
var retryHelper = new RetryHelper(options);
CancellationToken cancellationToken = GetCancellationToken();
var result = await retryHelper.ExecuteAsync(
    ct => SomeOperationThatMightFail(ct),
    "网络请求",
    cancellationToken: cancellationToken
);

// 使用默认重试策略
var defaultRetryHelper = new RetryHelper();
var result2 = await defaultRetryHelper.ExecuteAsync(
    ct => AnotherOperationThatMightFail(ct),
    "数据库操作",
    cancellationToken: cancellationToken
);
```

### 路径操作

```csharp
using Linger.Extensions.IO;
using Linger.Helper;

// 路径标准化 - 处理重复分隔符和尾部分隔符策略
string messyPath = @"temp\\folder//file.txt\";
string normalized = PathHelper.CleanAndNormalizePureString(messyPath, preserveEndingSeparator: false);
// 结果会移除重复分隔符，并按当前平台规范处理路径分隔符与末尾分隔符

// 获取相对路径 - 改用 BCL
string basePath = @"C:\Projects\MyApp";
string targetPath = @"C:\Projects\MyApp\src\Components\Button.cs";
string relative = Path.GetRelativePath(basePath, targetPath);
// 结果: "src\Components\Button.cs" (Windows) 或 "src/Components/Button.cs" (Unix)

// 解析绝对路径 - 改用 BCL
string workingDir = @"C:\Projects";
string relativePath = @"MyApp\src\file.txt";
string absolutePath = Path.GetFullPath(relativePath, workingDir);
// 结果: "C:\Projects\MyApp\src\file.txt"

// 检查路径中的非法字符
string suspiciousPath = "file<name>.txt";
bool hasInvalidChars = suspiciousPath.ContainsInvalidPathChars(); // Windows: true；Unix: false

// 检查文件或目录是否存在
string filePath = @"C:\temp\data.txt";
bool fileExists = PathExtensions.Exists(filePath, checkAsFile: true); // 检查文件
bool dirExists = PathExtensions.Exists(filePath, checkAsFile: false); // 检查目录

// 获取父目录路径 - 改用 BCL
string deepPath = @"C:\Projects\MyApp\src\Components\Button.cs";
string? parentDir = Path.GetDirectoryName(deepPath);
// 结果: "C:\Projects\MyApp\src\Components"
string? grandParentDir = Path.GetDirectoryName(parentDir);
// 结果: "C:\Projects\MyApp\src"
```

## API 标准化与类型安全

### .NET 10 兼容性（已支持）

`LeftJoin` 和 `RightJoin` 在 .NET 10 目标上使用框架原生实现，在较旧目标框架上由条件编译的 Polyfill 提供。无结果选择器的元组 `LeftJoin` 重载以及 `FullJoin` 始终由 Linger 提供，因此这些便利 API 在所有受支持的目标框架上保持一致。

### 严格类型安全原则

**类型转换策略**（偏保守，优先低开销路径）:
1. 优先检查直接类型匹配和常见数值类型分支，避免不必要的字符串解析
2. 对整数目标类型，仅接受可无损转换的数值输入
3. 仅在前述路径都不适用时，才回退到 `ToString()` 后再解析

```csharp
object intObj = 123;
int result = intObj.ToIntOrDefault(0);     // 直接匹配，无需字符串解析
object doubleObj = 123.45;
int failed = doubleObj.ToIntOrDefault(0);  // 小数部分非 0，返回 0（转换失败）
```

**当前核心数值转换支持**: `short`、`int`、`long`、`decimal`，并在对象/字符串路径上提供 `DateTime`、`bool`、`Guid` 等常用转换

### 性能优势

- 同类型与常见数值类型走快速分支，避免不必要的字符串解析
- `TryToXxx()` / `ToXxxOrDefault()` 避免用异常表示常规失败路径
- 智能回退策略，仅在必要时才进行字符串转换
- 统一 API 命名模式

## 最佳实践

1. **类型转换**: 输入必须有效、失败应尽早暴露时使用 `ToXxx()`；可接受空结果时使用 `ToXxxOrNull()`；需要回退值时使用 `ToXxxOrDefault()`；需要显式判断是否转换成功时使用 `TryToXxx()`
2. **空值检查**: 使用 `IsNullOrEmpty()` 做状态判断，使用 `EnsureIsNotNull()` 等 Guard 方法做参数保护
3. **异步操作**: 对已提供异步 API 的 I/O 密集型任务（文件、网络）优先使用异步版本
4. **异常处理**: 对不稳定操作使用 `RetryHelper`，并做好异常处理和用户提示
5. **资源管理**: 使用 `using` 语句确保资源正确释放

## Polyfill 汇总

提供 BCL API 与语言特性的前向兼容 Polyfill（.NET Framework/Standard 2.0/旧版本），采用条件编译，升级新框架后自动让位。使用 C# 14 扩展成员语法实现，代码更简洁。

| 类别 | 内容 | 源码位置 |
|------|------|----------|
| **参数验证** | `ArgumentNullException.ThrowIfNull` (.NET 6 前)<br>`ArgumentException.ThrowIfNullOrEmpty/WhiteSpace` (.NET 8 前)<br>`ArgumentOutOfRangeException.ThrowIfNegative/Zero/...` (.NET 8 前) | `Polyfills/ArgumentNullException.cs`<br>`Polyfills/ArgumentException.cs`<br>`Polyfills/ArgumentOutOfRangeException.cs` |
| **哈希算法** | `MD5.HashData`、`SHA256.HashData`、`SHA384.HashData`、`SHA512.HashData` (.NET 5 前) | `Polyfills/HashAlgorithm.cs` |
| **转换工具** | `Convert.ToHexStringLower` (.NET 9 前) | `Polyfills/Convert.cs` |
| **语言特性** | `required` 关键字支持 (C# 11)<br>`RequiredMemberAttribute`、`SetsRequiredMembersAttribute`、`CompilerFeatureRequiredAttribute` | `Polyfills/RequiredMemberAttribute.cs`<br>`Polyfills/SetsRequiredMembersAttribute.cs`<br>`Polyfills/CompilerFeatureRequiredAttribute.cs` |
| **可空性注解** | `AllowNull`、`NotNull`、`MaybeNullWhen`、`NotNullIfNotNull` 等 11 个特性 | `Polyfills/NullableAttributes.cs` |
| **集合扩展** | `LeftJoin`、`RightJoin`（.NET 10 前使用 Polyfill）；元组 `LeftJoin` 与 `FullJoin`（Linger 扩展） | `Extensions/Collection/IEnumerableExtensions.Polyfills.cs`<br>`Extensions/Collection/IEnumerableExtensions.cs` |
| **调用方捕获** | `CallerArgumentExpressionAttribute` (改进 Guard 体验) | `Polyfills/CallerArgumentExpressionAttribute.cs` |

## 依赖项

这个库保持轻量化设计，只依赖少量必要的外部包：
- **System.Data.DataSetExtensions** - 为 .NET Framework 和 .NET Standard 2.0 提供 DataTable 支持

## 贡献代码

我们欢迎您为这个项目贡献代码！在提交 Pull Request 时，请确保：
- 代码风格与现有代码保持一致
- 为新功能添加相应的单元测试
- 及时更新相关文档

## 许可证

该项目在 Linger 项目提供的许可证条款下授权。

---

有关 Linger 框架和其他相关包的更多信息，请访问 [Linger 项目仓库](https://github.com/Linger06/Linger)。
