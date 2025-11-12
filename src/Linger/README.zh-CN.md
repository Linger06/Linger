# Linger.Utils

一个功能丰富的 .NET 工具库，包含大量实用的扩展方法和帮助类，让您的日常开发工作更加轻松高效。

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
  - [对象扩展](#对象扩展)
  - [JSON 扩展](#json-扩展)
  - [GUID 扩展](#guid-扩展)
  - [数组扩展](#数组扩展)
  - [枚举扩展](#枚举扩展)  
  - [参数验证](#参数验证)
- [高级功能](#高级功能)
- [最佳实践](#最佳实践)
- [API 标准化与类型安全](#api-标准化与类型安全)
  - [.NET 10 兼容性（已支持）](#net-10-兼容性已支持)
  - [严格类型安全原则](#严格类型安全原则)

## 功能特性

### 🚀 核心扩展
- **字符串扩展**: 提供丰富的字符串处理功能，包括验证、转换、格式化等实用方法
- **字符串加密扩展**: 提供安全的 AES 加密解密功能，保护数据安全
- **日期时间扩展**: 简化日期时间的计算、格式化和各种常用操作
- **数值扩展**: 安全可靠的数值类型转换，**严格的类型安全原则**，**完整支持所有 .NET 基本数值类型**
- **枚举扩展**: 让枚举操作更加便捷，支持字符串转换和描述获取
- **对象扩展**: 通用的对象处理方法，提供空值检查和类型转换，**新增完整数值类型支持**
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
- **自定义转换器**: 针对特殊类型提供专门的 JSON 处理方案 (DateTime、DataTable、JsonObject 等)
- **JSON 默认配置**: `JsonDefaults` 提供统一的 JSON 序列化配置
    - 详细文档: `Linger/Json/JsonDefaults.README.zh-CN.md`

## 安装

```bash
dotnet add package Linger.Utils
```

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

// 字符串处理
string text = "  Hello World  ";
string cleaned = text.Trim(); // 去除首尾空格

// 字符串截取
string longText = "Hello World";
string leftPart = longText.Take(5); // 取前 5 个字符："Hello"
string rightPart = longText.TakeLast(5); // 取后 5 个字符："World"
string part = longText.Truncate(20, ""); // 安全截取，超长不会报错

// 字符串检查
bool isEmpty = text.IsNullOrEmpty(); // 检查是否为空
bool isNumber = number.IsNumber(); // 检查是否为数字
bool isInt = number.IsInteger(); // 检查是否为整数
```

### 字符串加密扩展

```csharp
using Linger.Extensions.Core;

string data = "敏感数据需要加密";
string aesKey = "mySecretKey12345"; // AES 密钥

// AES 加密解密（推荐，安全性高）
string aesEncrypted = data.AesEncrypt(aesKey);    // AES-256-CBC 模式，自动生成随机 IV
string aesDecrypted = aesEncrypted.AesDecrypt(aesKey); // 自动提取 IV 并解密

// 🔐 安全特性：
// - 每次加密使用随机 IV，相同明文产生不同密文
// - 密钥长度可变，内部自动使用 SHA256 处理为 32 字节
// - IV 自动包含在密文中，解密时自动提取
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

// 🔗 .NET 10+ Join 操作兼容（已支持）
// ⚠️ 注意：在较旧目标框架上使用 Polyfill；在 .NET 10+ 目标上将使用框架原生实现

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

// 🎯 简化版本：返回元组
var tupleResult = employees.LeftJoin(departments, e => e.DeptId, d => d.Id);
// 返回 IEnumerable<Tuple<Employee, Department?>>

// 🔧 支持自定义比较器
var caseInsensitiveJoin = stringList1.LeftJoin(
    stringList2, s => s, s => s,
    (s1, s2) => new { Left = s1, Right = s2 },
    StringComparer.OrdinalIgnoreCase
);

// 📊 .NET 10+ 内置兼容：方法签名与标准一致，升级时无需修改代码
```

### 对象扩展

```csharp
using Linger.Extensions.Core;

// 类型安全的对象转换
object stringObj = "123";
int intValue = stringObj.ToIntOrDefault(0);           // 成功：123
long longValue = stringObj.ToLongOrDefault(0L);       // 成功：123
double doubleValue = stringObj.ToDoubleOrDefault(0.0); // 成功：123.0

// 严格类型安全：非字符串对象返回默认值
object numberObj = 123.45;
int invalidInt = numberObj.ToIntOrDefault(0);         // 返回 0（默认值）

// 📊 支持的数值类型转换

| 方法 | 支持范围 | 方法 | 支持范围 |
|------|---------|------|---------|
| `ToSByteOrDefault` | -128 到 127 | `ToByteOrDefault` | 0 到 255 |
| `ToShortOrDefault` | -32,768 到 32,767 | `ToUShortOrDefault` | 0 到 65,535 |
| `ToIntOrDefault` | ±2.1×10⁹ | `ToUIntOrDefault` | 0 到 4.3×10⁹ |
| `ToLongOrDefault` | ±9.2×10¹⁸ | `ToULongOrDefault` | 0 到 1.8×10¹⁹ |
| `ToFloatOrDefault` | 单精度浮点 | `ToDoubleOrDefault` | 双精度浮点 |
| `ToDecimalOrDefault` | 高精度小数 | - | - |

// 其他类型转换
DateTime dateValue = stringObj.ToDateTimeOrDefault(DateTime.MinValue);
Guid guidValue = "550e8400-e29b-41d4-a716-446655440000".ToGuidOrDefault();
bool boolValue = stringObj.ToBoolOrDefault(false);

// 空值安全处理
object obj = GetSomeObject();
string result = obj.ToStringOrDefault("default"); // 为 null 时返回默认值

// 🔍 类型检查方法（支持所有数值类型）
object testObj = (byte)255;
bool isByte = testObj.IsByte();                      // 检查是否为 byte 类型
bool isNumeric = testObj.IsNumeric();                // 检查是否为任意数值类型
bool isUnsigned = testObj.IsAnyUnsignedInteger();    // 检查是否为无符号整数类型

// ⚡ 性能优化的 Try 风格转换 - 避免默认值掩盖失败
if ("123".TryToInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"坏数据".TryToDecimal(out var decVal)) { /* decVal = 0，转换失败 */ }

// 有符号字节类型
if ("-100".TryToSByte(out var sbyteResult)) { /* sbyteResult = -100 */ }

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

// 🔍 其他数值范围应用示例：
double price = 99.99;
double validPrice = price.EnsureIsInRange(0.0, 1000.0); // 价格验证

DateTime date = DateTime.Now;
DateTime validDate = date.EnsureIsInRange(DateTime.Today, DateTime.Today.AddDays(30)); // 日期范围验证
```

### JSON 扩展

```csharp
using Linger.Extensions;
using Linger.Json;

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

// 🏭 使用 JsonDefaults 获取统一配置
var responseOptions = JsonDefaults.CreateResponseOptions();  // HTTP 响应
var requestOptions = JsonDefaults.CreateRequestOptions();    // HTTP 请求

// 在 WebAPI 中应用配置
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        JsonDefaults.ApplyDefaultConfiguration(options.JsonSerializerOptions));

// 💡 详细配置说明请参考: Linger/Json/JsonDefaults.README.zh-CN.md
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
bool isNotNullOrEmpty = nullableGuid.IsNotNullOrEmpty(); // 检查是否既不为 null 也不为空

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
using Linger;

public void ProcessData(string data, IEnumerable<int> numbers)
{
    // 🆕 .NET 8 之前版本的参数验证 Polyfill
    ArgumentNullException.ThrowIfNull(data);                    // 确保不为 null
    ArgumentException.ThrowIfNullOrEmpty(data);                 // 确保不为 null 或空字符串
    ArgumentException.ThrowIfNullOrWhiteSpace(data);            // 确保不为 null、空或纯空白字符
    ArgumentNullException.ThrowIfNull(numbers);                 // 确保集合不为 null
    
    // 📦 框架支持：.NET 6+ 使用内置实现，.NET 5 及以下使用 Linger Polyfill
    // 升级到 .NET 8+ 时，只需移除 using Linger; 即可，其他代码无需修改
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

## API 标准化与类型安全

### .NET 10 兼容性（已支持）

提供的 `LeftJoin`、`RightJoin`、`FullJoin` 等方法与 .NET 10 标准完全兼容。由于项目已包含对 .NET 10 的支持，在 .NET 10 目标上这些 API 将直接使用框架原生实现；同时我们保留条件编译的 Polyfill（例如 `#if !NET10_0_OR_GREATER` 分支），以确保向后兼容仍然可用，便于在低版本目标框架上继续工作且升级时无需修改业务代码。

### 严格类型安全原则

**类型转换策略**（性能优化）:
1. 首先检查直接类型匹配（零开销）
2. 然后尝试 `ToString()` 转字符串再解析

```csharp
object intObj = 123;
int result = intObj.ToIntOrDefault(0);     // 直接匹配，零开销
object doubleObj = 123.45;
int failed = doubleObj.ToIntOrDefault(0);  // 返回 0（转换失败）
```

**完整数值类型支持**: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal

### 性能优势

- ✅ 零开销同类型转换
- ✅ 避免异常，返回默认值提升性能
- ✅ 智能回退策略，仅在需要时字符串转换
- ✅ 统一 API 命名模式

## 最佳实践

1. **类型转换**: 使用 `ToXxxOrDefault()` 避免异常开销;需要明确判断转换是否成功时使用 `TryToXxx()` 方法
2. **空值检查**: 善用 `IsNullOrEmpty()`、`EnsureIsNotNull()` 等扩展方法
3. **异步操作**: I/O 密集型任务(文件、网络)使用异步版本
4. **异常处理**: 不稳定操作使用 `RetryHelper`,做好异常处理和用户提示
5. **资源管理**: 使用 `using` 语句确保资源正确释放

## Polyfill 汇总

提供 BCL API 与语言特性的前向兼容 Polyfill（.NET Framework/Standard 2.0/旧版本），采用条件编译，升级新框架后自动让位。

| 类别 | 内容 | 源码位置 |
|------|------|----------|
| **参数验证** | `ArgumentNullException.ThrowIfNull` (.NET 6 前)<br>`ArgumentException.ThrowIfNullOrEmpty/WhiteSpace` (.NET 8 前) | `Polyfills/ArgumentNullException.cs`<br>`Polyfills/ArgumentException.cs` |
| **语言特性** | `required` 关键字支持 (C# 11)<br>`RequiredMemberAttribute`、`SetsRequiredMembersAttribute`、`CompilerFeatureRequiredAttribute` | `Polyfills/RequiredMemberAttribute.cs`<br>`Polyfills/SetsRequiredMembersAttribute.cs`<br>`Polyfills/CompilerFeatureRequiredAttribute.cs` |
| **可空性注解** | `AllowNull`、`NotNull`、`MaybeNullWhen`、`NotNullIfNotNull` 等 11 个特性 | `Polyfills/NullableAttributes.cs` |
| **集合扩展** | `LeftJoin`、`RightJoin`、`FullJoin` ( .NET 10 兼容，向下保留 Polyfill ) | `Extensions/Collection/IEnumerableExtensions.Polyfills.cs` |
| **调用方捕获** | `CallerArgumentExpressionAttribute` (改进 Guard 体验) | `Polyfills/CallerArgumentExpressionAttribute.cs` |

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
