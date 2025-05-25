# Linger.Utils

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

一个综合性的 .NET 实用工具库，提供了广泛的类型转换扩展、字符串操作实用工具、日期时间辅助方法、文件系统操作、集合扩展以及用于日常开发任务的各种辅助类。

## 概述

Linger.Utils 旨在成为开发者的日常伴侣，提供丰富的扩展方法和辅助类集合，使常见的编程任务变得更加简单和高效。该库遵循现代 C# 编码实践，支持多个 .NET 框架版本。

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
- [错误处理](#错误处理)
- [性能考虑](#性能考虑)
- [最佳实践](#最佳实践)

## 功能特性

### 🚀 核心扩展
- **字符串扩展**：丰富的字符串操作、验证、转换和格式化实用工具
- **日期时间扩展**：日期和时间操作、格式化和计算
- **数值扩展**：类型安全的数值转换和操作
- **枚举扩展**：增强的枚举处理和转换
- **对象扩展**：通用对象操作和验证
- **数组扩展**：数组处理和操作实用工具
- **GUID 扩展**：GUID 操作和验证实用工具

### 📦 集合扩展
- **列表扩展**：增强的列表操作和处理
- **集合扩展**：通用集合实用工具和转换

### 💾 数据扩展
- **DataTable 扩展**：数据库和 DataTable 操作实用工具
- **数据转换**：安全的数据类型转换和变换

### 📁 文件系统操作
- **文件辅助类**：全面的文件操作（读取、写入、复制、移动、删除）
- **路径辅助类**：跨平台路径操作和验证
- **目录操作**：目录管理和遍历实用工具

### 🔧 辅助类
- **表达式辅助类**：表达式树操作和实用工具
- **重试辅助类**：操作的健壮重试机制
- **属性辅助类**：基于反射的属性操作
- **GUID 代码**：增强的 GUID 生成和操作
- **操作系统平台辅助类**：跨平台操作系统检测
- **参数验证扩展**：保护性编程和输入验证实用工具

### 🌐 JSON 支持
- **JSON 扩展**：简化的 JSON 序列化和反序列化
- **自定义转换器**：复杂类型的专用 JSON 转换器

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

// 字符串验证
string email = "user@example.com";
bool isValid = email.IsEmail();

// 字符串转换
string number = "123";
int result = number.ToInt(0); // 返回 123，转换失败时返回 0
int? nullableResult = number.ToIntOrNull(); // 返回可空类型

// 字符串操作
string text = "  Hello World  ";
string cleaned = text.Trim(); // 移除首尾空白字符（.NET 原生方法，非扩展方法）

// 字符串截取
string longText = "Hello World";
string part = longText.Left(5); // 获取左侧5个字符：Hello
string rightPart = longText.Right(5); // 获取右侧5个字符：World
string safePart = longText.SafeSubstring(0, 20); // 不会因长度超出而抛出异常

// 字符串检查
bool isEmpty = text.IsNullOrEmpty();
bool isNumber = number.IsNumber(); // 检查是否为数字
bool isInt = number.IsInt(); // 检查是否为整数
```

### 日期时间扩展

```csharp
using Linger.Extensions.Core;

DateTime date = DateTime.Now;

// 年龄计算
DateTime birthDate = new DateTime(1990, 5, 15);
int age = birthDate.CalculateAge();

// 日期范围操作
bool isInRange = date.InRange(DateTime.Today, DateTime.Today.AddDays(7));

// 日期操作
DateTime startOfDay = date.StartOfDay(); // 当天开始时间
DateTime endOfDay = date.EndOfDay(); // 当天结束时间
DateTime startOfMonth = date.StartOfMonth(); // 月初
DateTime endOfMonth = date.EndOfMonth(); // 月末
```

### 文件操作

```csharp
using Linger.Helper;

// 文件操作
FileHelper.WriteText("data.txt", "Hello World");
string content = FileHelper.ReadText("data.txt");

// 带目录创建的文件复制
FileHelper.CopyFile("source.txt", "backup/dest.txt");

// 安全文件删除
FileHelper.DeleteFileIfExists("temp.txt");

// 目录操作
FileHelper.EnsureDirectoryExists("logs/2024");
```

### 集合扩展

```csharp
using Linger.Extensions.Collection;

var list = new List<int> { 1, 2, 3, 4, 5 };

// 安全检查集合状态
bool isEmpty = list.IsNullOrEmpty(); // 检查是否为空或 null

// 分页操作
var pagedResult = list.Paging(2, 2); // 第2页，每页2个元素：[3, 4]

// 转换为分隔字符串
string result = list.ToSeparatedString(", "); // "1, 2, 3, 4, 5"

// 对每个元素执行操作
list.ForEach(Console.WriteLine); // 输出每个元素

// 转换为 DataTable
var dataTable = list.Select(x => new { Value = x }).ToDataTable();
```

### 对象扩展

```csharp
using Linger.Extensions.Core;

// 空值安全操作
object obj = GetSomeObject();
string result = obj.ToSafeString("default");

// 类型检查
string stringValue = obj.ToString(); // .NET 原生方法，非扩展方法
bool isNumber = stringValue.IsNumber();
bool isInt = stringValue.IsInt();
bool isDouble = stringValue.IsDouble();

// 对象转换
var stringRepresentation = obj.ToStringOrNull();

// 范围检查（对于数值）
int value = 5;
bool inRange = value.InRange(1, 10); // 检查是否在范围内
```

### JSON 扩展

```csharp
using Linger.Extensions;

// 对象转 JSON
var user = new { Name = "John", Age = 30 };
string json = user.ToJsonString(); // 或者 user.SerializeJson()

// JSON 转对象
var userObj = json.Deserialize<User>(); // 或者 json.DeserializeJson<User>()

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

// .NET 9+ 特性：V7 GUID 时间戳提取
#if NET9_0_OR_GREATER
DateTimeOffset timestamp = guid.GetTimestamp(); // 仅支持 V7 GUID
#endif
```

### 数组扩展

```csharp
using Linger.Extensions.Core;

int[] numbers = { 1, 2, 3, 4, 5 };

// 对每个元素执行操作
numbers.ForEach(n => Console.WriteLine(n)); // 输出：1 2 3 4 5

// 带索引的遍历
numbers.ForEach((n, index) => Console.WriteLine($"Index {index}: {n}"));
// 输出：Index 0: 1, Index 1: 2, ...
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
Status status = statusName.GetEnum<Status>(); // 或者 statusName.ToEnum<Status>()

// 整数转枚举
int statusValue = 1;
Status statusFromInt = statusValue.GetEnum<Status>();

// 获取枚举名称
string enumName = statusValue.GetEnumName<Status>(); // 返回 "Active"

// 获取枚举描述（如果有 Description 特性）
string description = status.GetDescription(); // 获取描述文本
```

### 参数验证

```csharp
using Linger.Helper;

public void ProcessData(string data, IEnumerable<int> numbers, string filePath)
{
    // 基础验证
    data.EnsureIsNotNull(nameof(data)); // 确保不为 null
    data.EnsureIsNotNullAndEmpty(nameof(data)); // 确保不为 null 或空字符串
    data.EnsureIsNotNullAndWhiteSpace(nameof(data)); // 确保不为 null、空或空白字符

    // 集合验证
    numbers.EnsureIsNotNullOrEmpty(nameof(numbers)); // 确保集合不为 null 或空

    // 文件系统验证
    filePath.EnsureFileExist(nameof(filePath)); // 确保文件存在
    Path.GetDirectoryName(filePath).EnsureDirectoryExist(); // 确保目录存在

    // 条件验证
    (data.Length > 0).EnsureIsTrue(nameof(data), "Data must not be empty");
    (numbers.Count() < 1000).EnsureIsTrue(nameof(numbers), "Too many items");

    // 范围验证
    int value = 5;
    value.EnsureIsInRange(1, 10, nameof(value)); // 确保值在指定范围内

    // null 检查
    object? obj = GetSomeObject();
    obj.EnsureIsNotNull(nameof(obj)); // 如果对象应该不为 null
    // 或者
    obj.EnsureIsNull(nameof(obj)); // 如果对象应该为 null
}
```

## 高级功能

### 重试辅助类

```csharp
using Linger.Helper;

// 使用可配置策略重试操作
var options = new RetryOptions 
{
    MaxRetries = 3,
    BaseDelayMs = 1000 // 1秒
};
var retryHelper = new RetryHelper(options);
var result = await retryHelper.ExecuteAsync(
    async () => await SomeOperationThatMightFail(),
    "操作名称"
);

// 或使用默认选项
var defaultRetryHelper = new RetryHelper();
var result2 = await defaultRetryHelper.ExecuteAsync(
    async () => await AnotherOperationThatMightFail(),
    "另一个操作名称"
);
```

### 表达式辅助类

```csharp
using Linger.Helper;
using Linger.Enums;

// 动态表达式构建
// 基础表达式
Expression<Func<User, bool>> trueExpression = ExpressionHelper.True<User>();
Expression<Func<User, bool>> falseExpression = ExpressionHelper.False<User>();

// 单个条件表达式
Expression<Func<User, bool>> ageFilter = ExpressionHelper.CreateGreaterThan<User>("Age", "18");
Expression<Func<User, bool>> nameFilter = ExpressionHelper.GetContains<User>("Name", "John");

// 使用条件集合构建复杂表达式
var conditions = new List<Condition>
{
    new Condition { Field = "Age", Op = CompareOperator.GreaterThan, Value = 18 },
    new Condition { Field = "Name", Op = CompareOperator.Contains, Value = "John" }
};
Expression<Func<User, bool>> complexFilter = ExpressionHelper.BuildLambda<User>(conditions);
```

### 路径操作

```csharp
using Linger.Helper.PathHelpers;

// 跨平台路径操作
string normalized = StandardPathHelper.NormalizePath(@"C:\temp\..\folder\file.txt");
bool pathEquals = StandardPathHelper.PathEquals(path1, path2);
string relative = StandardPathHelper.GetRelativePath(basePath, targetPath);
string absolutePath = StandardPathHelper.ResolveToAbsolutePath(basePath, relativePath);
bool hasInvalidChars = StandardPathHelper.ContainsInvalidPathChars(somePath);
bool fileExists = StandardPathHelper.Exists(filePath, checkAsFile: true);
string parentDir = StandardPathHelper.GetParentDirectory(path, levels: 1);
```

## 错误处理

该库遵循防御性编程实践：

- 大多数操作都有安全变体，返回默认值而不是抛出异常
- 广泛的输入验证，提供有意义的错误消息
- 所有组件间一致的错误处理模式

## 性能考虑

- 在可能的情况下优化性能，最小化内存分配
- 缓存反射操作以获得更好的性能
- 对 I/O 操作支持 async/await
- 在适当的地方使用延迟求值

## 最佳实践

1. **使用安全方法**：当转换可能失败时，优先使用 `ToIntOrNull()` 而不是 `ToInt()`
2. **空值检查**：使用 `IsNullOrEmpty()` 等扩展方法进行验证
3. **参数验证**：使用 `GuardExtensions` 中的 `EnsureIsNotNull()`、`EnsureIsNotNullAndEmpty()` 等方法进行输入验证
4. **利用异步**：使用异步版本的文件操作以获得更好的性能
5. **错误处理**：始终处理文件操作中的潜在异常
6. **资源管理**：对可释放资源使用 `using` 语句
7. **GUID 操作**：使用 `IsEmpty()` 和 `IsNotEmpty()` 等扩展方法而不是直接比较
8. **集合处理**：使用 `ForEach()` 扩展方法简化数组和集合的迭代操作

## 依赖项

该库具有最少的外部依赖：
- System.Text.Json（用于 JSON 操作）
- System.Data.DataSetExtensions（用于 .NET Framework 和 .NET Standard 2.0）

## 贡献

欢迎贡献！请随时提交 Pull Request。请确保：
- 遵循现有的代码风格
- 为新功能添加单元测试
- 根据需要更新文档

## 许可证

本项目根据 Linger 项目提供的许可条款授权。

---

有关 Linger 框架和其他相关包的更多信息，请访问 [Linger 项目仓库](https://github.com/Linger06/Linger)。