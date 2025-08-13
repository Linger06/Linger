# Linger.Utils

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

一个功能丰富的 .NET 工具库，包含大量实用的扩展方法和帮助类，让您的日常开发工作更加轻松高效。

## 概述

Linger.Utils 是专为 .NET 开发者打造的实用工具集合。无论您是在处理字符串、操作日期时间、进行文件管理，还是需要进行数据转换，这个库都能为您提供简洁易用的解决方案。它采用现代 C# 语法，支持多个 .NET 版本，让您的代码更加优雅。

## 目录

- [功能特性](#功能特性)
- [安装](#安装)
- [目标框架](#目标框架)
- [快速开始](#快速开始)
  - [字符串扩展](#字符串扩展)
  - [日期时间扩展](#日期时间扩展)
  - [文件操作](#文件操作)
  - [集合扩展](#集合扩展)
  - [对象扩展](#对象扩展)
  - [JSON 扩展](#json-扩展)
  - [GUID 扩展](#guid-扩展)
  - [数组扩展](#数组扩展)
  - [枚举扩展](#枚举扩展)  
  - [参数验证](#参数验证)
- [高级功能](#高级功能)
- [最佳实践](#最佳实践)

## 功能特性

### 🚀 核心扩展
- **字符串扩展**: 提供丰富的字符串处理功能，包括验证、转换、格式化等实用方法
- **日期时间扩展**: 简化日期时间的计算、格式化和各种常用操作
- **数值扩展**: 安全可靠的数值类型转换，避免异常抛出
- **枚举扩展**: 让枚举操作更加便捷，支持字符串转换和描述获取
- **对象扩展**: 通用的对象处理方法，提供空值检查和类型转换
- **数组扩展**: 简化数组操作，提供遍历和处理的便捷方法
- **GUID 扩展**: 完善的 GUID 操作工具，包括验证和转换功能

### 📦 集合扩展
- **列表扩展**: 增强列表的操作能力，提供分页、遍历等实用功能
- **集合扩展**: 通用的集合处理工具，让数据操作更简单

### 💾 数据扩展
- **DataTable 扩展**: 简化 DataTable 的操作和处理
- **数据转换**: 提供安全的数据类型转换方法

### 📁 文件系统操作
- **文件助手**: 涵盖文件的读写、复制、移动、删除等所有常用操作
- **路径助手**: 跨平台的路径处理，支持路径验证和规范化
- **目录操作**: 完整的目录管理功能，包括创建、遍历等

### 🔧 助手类
- **表达式助手**: 动态构建表达式树，适用于条件查询等场景
- **重试助手**: 为不稳定的操作提供智能重试机制
- **属性助手**: 基于反射的属性操作工具
- **GUID 工具**: 高级 GUID 生成和处理功能
- **平台助手**: 跨平台的操作系统检测和兼容性处理
- **参数验证**: 提供防御性编程所需的输入验证工具

### 🌐 JSON 支持
- **JSON 扩展**: 简化 JSON 的序列化和反序列化操作
- **自定义转换器**: 针对特殊类型提供专门的 JSON 处理方案

## 安装

```bash
dotnet add package Linger.Utils
```

## 目标框架

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
int result = number.ToInt(0); // 转换成功返回 123，失败则返回 0
int? nullableResult = number.ToIntOrNull(); // 返回可为空的整数

// 字符串处理
string text = "  Hello World  ";
string cleaned = text.Trim(); // 去除首尾空格

// 字符串截取
string longText = "Hello World";
string leftPart = longText.Left(5); // 取前 5 个字符："Hello"
string rightPart = longText.Right(5); // 取后 5 个字符："World"
string part = longText.SafeSubstring(0, 20); // 安全截取，超长不会报错

// 字符串检查
bool isEmpty = text.IsNullOrEmpty(); // 检查是否为空
bool isNumber = number.IsNumber(); // 检查是否为数字
bool isInt = number.IsInteger(); // 检查是否为整数
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
FileHelper.WriteText("data.txt", "Hello World"); // 写入文本文件
string content = FileHelper.ReadText("data.txt"); // 读取文本文件

// 文件复制（自动创建目录）
FileHelper.CopyFile("source.txt", "backup/dest.txt");

// 安全删除文件
FileHelper.DeleteFileIfExists("temp.txt"); // 文件存在才删除，不会报错

// 确保目录存在
FileHelper.EnsureDirectoryExists("logs/2024"); // 目录不存在则创建
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

// 转换为数据表
var dataTable = list.Select(x => new { Value = x }).ToDataTable();
```

### 对象扩展

```csharp
using Linger.Extensions.Core;

// 空值安全处理
object obj = GetSomeObject();
string result = obj.ToSafeString("default"); // 为 null 时返回默认值

// 类型判断
string stringValue = obj.ToString();
bool isNumber = stringValue.IsNumber(); // 是否为数字格式
bool isInt = stringValue.IsInteger(); // 是否为整数格式
bool isDouble = stringValue.IsDouble(); // 是否为浮点数格式

// 安全转换
var stringRepresentation = obj.ToStringOrNull(); // 转换失败返回 null

// Try 风格数值转换（避免默认值掩盖失败）
if ("123".TryInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"坏数据".TryDecimal(out var decVal)) { /* decVal = 0，转换失败 */ }

// 确保前后缀（幂等，不重复添加）
var apiUrl = "api/v1".EnsureStartsWith("/"); // => "/api/v1"
var folder = "logs".EnsureEndsWith("/");     // => "logs/"

// 数值范围检查
int value = 5;
bool inRange = value.InRange(1, 10); // 检查是否在 1 到 10 之间
```

### JSON 扩展

```csharp
using Linger.Extensions;

// 对象转 JSON
var user = new { Name = "John", Age = 30 };
string json = user.ToJsonString(); // 或 user.SerializeJson()

// JSON 转对象
var userObj = json.Deserialize<User>(); // 或 json.DeserializeJson<User>()

// 动态 JSON 对象
dynamic dynamicObj = json.DeserializeDynamicJsonObject();
string name = dynamicObj.Name; // 访问属性

// JSON 转 DataTable（字符串扩展）
string jsonArray = "[{\"Name\":\"John\",\"Age\":30}]";
DataTable? dataTable = jsonArray.ToDataTable();
```

### GUID 扩展

```csharp
using Linger.Extensions.Core;

// GUID 检查
Guid guid = Guid.NewGuid();
bool isEmpty = guid.IsEmpty(); // 检查是否为空 GUID
bool isNotEmpty = guid.IsNotEmpty(); // 检查是否不为空

// 可空 GUID 操作
Guid? nullableGuid = null;
bool isNull = nullableGuid.IsNull(); // 检查是否为 null
bool isNotNull = nullableGuid.IsNotNull(); // 检查是否不为 null
bool isNullOrEmpty = nullableGuid.IsNullOrEmpty(); // 检查是否为 null 或空
bool isNotNullAndEmpty = nullableGuid.IsNotNullAndEmpty(); // 检查是否既不为 null 也不为空

// GUID 转换
long longValue = guid.ToInt64(); // 转换为 Int64
int intValue = guid.ToInt32(); // 转换为 Int32

// .NET 9+ 功能：V7 GUID 时间戳提取
#if NET9_0_OR_GREATER
DateTimeOffset timestamp = guid.GetTimestamp(); // 仅适用于 V7 GUID
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

// 获取枚举描述（如果存在 Description 特性）
string description = status.GetDescription(); // 获取描述文本
```

### 参数验证

```csharp
using Linger.Helper;

public void ProcessData(string data, IEnumerable<int> numbers, string filePath)
{
    // 基本验证
    data.EnsureIsNotNull(nameof(data)); // 确保不为 null
    data.EnsureIsNotNullOrEmpty(nameof(data)); // 确保不为 null 或空
    data.EnsureIsNotNullOrWhiteSpace(nameof(data)); // 确保不为 null、空或空白

    // 集合验证
    numbers.EnsureIsNotNullOrEmpty(nameof(numbers)); // 确保集合不为 null 或空

    // 文件系统验证
    filePath.EnsureFileExists(nameof(filePath)); // 确保文件存在
    Path.GetDirectoryName(filePath).EnsureDirectoryExists(); // 确保目录存在

    // 条件验证
    (data.Length > 0).EnsureIsTrue(nameof(data), "数据不能为空");
    (numbers.Count() < 1000).EnsureIsTrue(nameof(numbers), "项目过多");

    // 范围验证
    int value = 5;
    value.EnsureIsInRange(1, 10, nameof(value)); // 确保值在范围内

    // 空值检查
    object? obj = GetSomeObject();
    obj.EnsureIsNotNull(nameof(obj)); // 如果对象不应为 null
    // 或
    obj.EnsureIsNull(nameof(obj)); // 如果对象应为 null
}
```

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
var result = await retryHelper.ExecuteAsync(
    async () => await SomeOperationThatMightFail(), // 可能失败的操作
    "网络请求"  // 操作描述
);

// 使用默认重试策略
var defaultRetryHelper = new RetryHelper();
var result2 = await defaultRetryHelper.ExecuteAsync(
    async () => await AnotherOperationThatMightFail(),
    "数据库操作"
);
```

### 表达式助手

```csharp
using Linger.Helper;
using Linger.Enums;

// 构建动态查询条件
// 基础表达式
Expression<Func<User, bool>> trueExpression = ExpressionHelper.True<User>();   // 永远为真
Expression<Func<User, bool>> falseExpression = ExpressionHelper.False<User>(); // 永远为假

// 单个条件
Expression<Func<User, bool>> ageFilter = ExpressionHelper.CreateGreaterThan<User>("Age", "18");
Expression<Func<User, bool>> nameFilter = ExpressionHelper.GetContains<User>("Name", "John");

// 组合多个条件
var conditions = new List<Condition>
{
    new Condition { Field = "Age", Op = CompareOperator.GreaterThan, Value = 18 },    // 年龄大于 18
    new Condition { Field = "Name", Op = CompareOperator.Contains, Value = "John" }   // 姓名包含 "John"
};
Expression<Func<User, bool>> complexFilter = ExpressionHelper.BuildLambda<User>(conditions);
```

### 路径操作

```csharp
using Linger.Helper.PathHelpers;

// 路径标准化 - 处理相对路径、重复分隔符等
string messyPath = @"C:\temp\..\folder\.\file.txt";
string normalized = StandardPathHelper.NormalizePath(messyPath);
// 结果: "C:\folder\file.txt" (Windows) 或 "/folder/file.txt" (Unix)

// 路径比较 - 跨平台安全的路径相等性检查
string path1 = @"C:\Users\Documents\file.txt";
string path2 = @"c:\users\documents\FILE.TXT"; // 大小写不同
bool pathEquals = StandardPathHelper.PathEquals(path1, path2); // Windows: true, Linux: false

// 获取相对路径 - 从基础路径到目标路径的相对路径
string basePath = @"C:\Projects\MyApp";
string targetPath = @"C:\Projects\MyApp\src\Components\Button.cs";
string relative = StandardPathHelper.GetRelativePath(basePath, targetPath);
// 结果: "src\Components\Button.cs" (Windows) 或 "src/Components/Button.cs" (Unix)

// 解析绝对路径 - 将相对路径转换为绝对路径
string workingDir = @"C:\Projects";
string relativePath = @"MyApp\src\file.txt";
string absolutePath = StandardPathHelper.ResolveToAbsolutePath(workingDir, relativePath);
// 结果: "C:\Projects\MyApp\src\file.txt"

// 检查路径中的非法字符
string suspiciousPath = "file<name>.txt"; // 包含非法字符 '<'
bool hasInvalidChars = StandardPathHelper.ContainsInvalidPathChars(suspiciousPath); // true

// 检查文件或目录是否存在
string filePath = @"C:\temp\data.txt";
bool fileExists = StandardPathHelper.Exists(filePath, checkAsFile: true); // 检查文件
bool dirExists = StandardPathHelper.Exists(filePath, checkAsFile: false); // 检查目录

// 获取父目录路径
string deepPath = @"C:\Projects\MyApp\src\Components\Button.cs";
string parentDir = StandardPathHelper.GetParentDirectory(deepPath, levels: 1);
// 结果: "C:\Projects\MyApp\src\Components"
string grandParentDir = StandardPathHelper.GetParentDirectory(deepPath, levels: 2);
// 结果: "C:\Projects\MyApp\src"
```

## 最佳实践

1. **优先使用安全方法**: 数据转换时推荐使用 `ToIntOrNull()` 而非 `ToInt()`，避免转换失败时抛出异常
2. **善用空值检查**: 利用 `IsNullOrEmpty()` 等扩展方法，让代码更简洁可靠
3. **做好参数验证**: 在方法入口使用 `EnsureIsNotNull()`、`EnsureIsNotNullAndEmpty()` 等方法进行参数校验
4. **合理使用异步**: 文件操作等 I/O 密集型任务建议使用异步版本，提升程序响应性
5. **妥善处理异常**: 文件操作等可能失败的操作要做好异常处理和用户提示
6. **注意资源管理**: 使用 `using` 语句确保资源得到正确释放
7. **GUID 操作规范**: 使用 `IsEmpty()`、`IsNotEmpty()` 等方法而不是直接比较
8. **简化集合操作**: 善用 `ForEach()` 等扩展方法，让集合处理更加简洁

## 迁移说明 (0.8.2 → 下一版本)

为提升命名一致性与可读性，本版本对部分 API 进行了重命名与增强。旧名称均以 `[Obsolete]` 标记并仍可使用（过渡期：0.9.x，计划在首个 1.0 预发布版本移除），建议尽快迁移。

### Guard 方法重命名
| 旧名称 | 新名称 | 原因 |
|--------|--------|------|
| `EnsureIsNotNullAndEmpty` | `EnsureIsNotNullOrEmpty` | 语义更准确（与 .NET 命名模式统一） |
| `EnsureIsNotNullAndWhiteSpace` | `EnsureIsNotNullOrWhiteSpace` | 与 BCL `IsNullOrWhiteSpace` 对齐 |
| `EnsureFileExist` | `EnsureFileExists` | 语法与命名规范修正 |
| `EnsureDirectoryExist` | `EnsureDirectoryExists` | 同上 |

字符串扩展同样新增 `IsNotNullOrEmpty` / `IsNotNullOrWhiteSpace`，旧方法保留为过时包装。

### 异常类型重命名
| 旧类型 | 新类型 | 说明 |
|--------|--------|------|
| `OutOfReTryCountException` | `OutOfRetryCountException` | 修正拼写/大小写。旧类型继承新类型并标记过时。 |

### RetryHelper 增强
| 变化 | 说明 |
|------|------|
| 可选 `operationName` | 现在可省略，库会通过 `CallerArgumentExpression` 自动捕获调用表达式。 |
| 退避策略优化 | 使用 Full Jitter，并对 `RetryOptions` 参数进行有效性验证。 |
| 异常信息改进 | 最终异常消息中包含总耗时（毫秒）。 |

### 迁移建议
1. 全局替换旧的 Guard 名称为新名称（一次性脚本或 IDE 重构）。  
2. 可移除 Retry 调用中纯描述性的 `operationName` 参数（非必需）。  
3. 捕获重试异常的地方改为 `OutOfRetryCountException`。若需兼容仍在使用旧类型的代码，可暂时 catch 新类型或基类。  
4. 若短期内无法全部更新，可用 `#pragma warning disable CS0618` 暂时屏蔽过时警告。  

### 前后对比
```csharp
// 迁移前
data.EnsureIsNotNullAndEmpty();
filePath.EnsureFileExist();
directory.EnsureDirectoryExist();
try { await retry.ExecuteAsync(action, "操作"); } catch (OutOfReTryCountException ex) { /* ... */ }

// 迁移后
data.EnsureIsNotNullOrEmpty();
filePath.EnsureFileExists();
directory.EnsureDirectoryExists();
try { await retry.ExecuteAsync(action); } catch (OutOfRetryCountException ex) { /* ... */ }
```

功能行为未改变，仅是命名与诊断信息改进。

### 新增的字符串 / GUID API 增强 (0.8.2 之后)
| 分类 | 新 API | 作用 |
|------|--------|------|
| 字符串 | `RemoveSuffixOnce(string suffix, StringComparison comparison = Ordinal)` | 精确移除单个后缀（可控大小写），避免旧 `RemoveLastChar` 基于字符集合的潜在误解 |
| 字符串 | `EnsureStartsWith(string prefix, StringComparison comparison)` | 用指定比较方式确保前缀，无需手写大小写判断 |
| 字符串 | `EnsureEndsWith(string suffix, StringComparison comparison)` | 同上（后缀） |
| 字符串 | `RemovePrefixAndSuffix(string token, StringComparison comparison)` | 提供文化/大小写控制的前后对称移除 |
| 字符串 | `RemovePrefixAndSuffix(char)` / `RemovePrefixAndSuffix(string)` | 原有基础版本，继续保留 |
| Guid | `IsNotNullOrEmpty()` | 统一语义，替代旧 `IsNotNullAndEmpty` |
| Object | `IsNotNullOrEmpty()` | 与 Guid / String 一致化 |

### 已标记过时 (Obsolete) – 计划移除 (目标: 1.0.0)
| 过时成员 | 替代 | 说明 |
|----------|------|------|
| `GuidExtensions.IsNotNullAndEmpty` | `IsNotNullOrEmpty` | 语义命名统一 |
| `ObjectExtensions.IsNotNullAndEmpty` | `IsNotNullOrEmpty` | 同上 |
| `StringExtensions.Substring2` | `Take` | 更清晰的动词命名 |
| `StringExtensions.Substring3` | `TakeLast` | 与新命名对称 |
| `StringExtensions.IsNotNullAndEmpty` | `IsNotNullOrEmpty` | 命名统一 |
| `StringExtensions.IsNotNullAndWhiteSpace` | `IsNotNullOrWhiteSpace` | 命名统一 |
| `GuardExtensions.EnsureIsNotNullAndEmpty` | `EnsureIsNotNullOrEmpty` | 命名统一 |
| `GuardExtensions.EnsureIsNotNullAndWhiteSpace` | `EnsureIsNotNullOrWhiteSpace` | 命名统一 |
| `ObjectExtensions.ToNotSpaceString` | `ToTrimmedString` | 意义更明确 |
| `ObjectExtensions.ToStringOrEmpty` | `ToSafeString` | 语义收敛 |
| `RemoveLastChar` (行为提醒) | `RemoveSuffixOnce` | 非精准模式将保留但建议迁移 |

> 移除窗口：上述成员计划在 0.9.x 稳定版之后（或最迟 1.0.0 之前）删除。请尽早迁移。

### 使用示例（新方法）
```csharp
// 精确移除后缀（忽略大小写）
var trimmed = "Report.DOCX".RemoveSuffixOnce(".docx", StringComparison.OrdinalIgnoreCase); // => "Report"

// 确保前缀（忽略大小写）
var normalized = "api/values".EnsureStartsWith("/API", StringComparison.OrdinalIgnoreCase); // => "/api/values"

// 对称去除前后缀
var inner = "__value__".RemovePrefixAndSuffix("__", StringComparison.Ordinal); // => "value"

Guid? gid = Guid.NewGuid();
if (gid.IsNotNullOrEmpty()) { /* ... */ }
```

### 1.0.0 预定移除列表 (预览)
将移除所有上表 Obsolete 成员以及代码内标记 “Will be removed in 1.0.0” 的项。最终确认列表会在 1.0.0 发布说明中公布。

### 新增功能 (0.8.2 之后增量)

#### 非抛异常的枚举解析
新增 `TryGetEnum` 系列方法，避免使用异常控制流程：

```csharp
if ("Active".TryGetEnum<Status>(out var status))
{
    // 使用 status
}

if (2.TryGetEnum<Status>(out var status2))
{
    // 使用 status2
}
```

#### 重试异常构造函数扩展
`OutOfRetryCountException` 现在支持：
```csharp
throw new OutOfRetryCountException();              // 默认消息
throw new OutOfRetryCountException("自定义消息");
throw new OutOfRetryCountException("自定义消息", innerEx);
```
旧类型 `OutOfReTryCountException` 仍保留（标记 `[Obsolete]`，仅过渡期）。

## 依赖项

这个库保持轻量化设计，只依赖少量必要的外部包：
- **System.Text.Json** - 用于 JSON 序列化和反序列化
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
