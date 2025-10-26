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
    - [JSON 选项与安全默认](#json-选项与安全默认)
  - [GUID 扩展](#guid-扩展)
  - [数组扩展](#数组扩展)
  - [枚举扩展](#枚举扩展)  
  - [参数验证](#参数验证)
- [高级功能](#高级功能)
- [最佳实践](#最佳实践)
- [API 标准化与类型安全](#api-标准化与类型安全)
  - [.NET 10 前瞻性兼容设计](#net-10-前瞻性兼容设计)
  - [严格类型安全原则](#严格类型安全原则)
- [迁移说明](#迁移说明)
  - [Join 方法标准化与 .NET 10 兼容性](#join-方法标准化与-net-10-兼容性)

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

// AES 加密解密（推荐使用，安全性高）
string data = "敏感数据需要加密";
string aesKey = "mySecretKey12345"; // AES 密钥，长度可变

try 
{
    // AES 加密 - 使用 AES-256-CBC 模式，自动生成随机 IV
    string aesEncrypted = data.AesEncrypt(aesKey);
    Console.WriteLine($"AES 加密结果: {aesEncrypted}");
    
    // AES 解密 - 自动从加密数据中提取 IV
    string aesDecrypted = aesEncrypted.AesDecrypt(aesKey);
    Console.WriteLine($"AES 解密结果: {aesDecrypted}"); // 输出: 敏感数据需要加密
}
catch (ArgumentException ex)
{
    Console.WriteLine($"参数错误: {ex.Message}");
}
catch (CryptographicException ex)
{
    Console.WriteLine($"加密/解密错误: {ex.Message}");
}

// AES 多次加密测试（每次结果不同，更安全）
for (int i = 1; i <= 3; i++)
{
    string encrypted = data.AesEncrypt(aesKey);
    Console.WriteLine($"第{i}次加密: {encrypted}");
    // 每次输出都不同，因为使用了随机 IV
}

// 🔐 安全特性说明：
// 1. AES 使用 AES-256-CBC 模式，每次加密都生成随机 IV
// 2. IV 自动包含在加密结果中，解密时自动提取
// 3. 相同明文每次加密结果都不同，提高安全性
// 4. DES 算法已过时，建议仅用于兼容旧系统

// ⚠️ 安全提示：
// 1. DES 算法已不建议用于新项目，推荐使用 AES
// 2. 密钥应该安全存储，不要硬编码在代码中
// 3. 在生产环境中应使用更强的密钥管理机制
// 4. AES 密钥长度可变，内部会自动使用 SHA256 处理为 32 字节
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

// 🔗 .NET 10 兼容的 Join 操作 - 提前享受未来功能
// ⚠️ 注意：这些是 .NET 10+ 内置方法的 Polyfill 实现

// Left Join（左外连接）
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

// Left Join：保留所有员工，没有部门的显示 null
var leftJoinResult = employees.LeftJoin(
    departments,
    emp => emp.DeptId,           // 外部键选择器
    dept => dept.Id,             // 内部键选择器
    (emp, dept) => new { 
        Employee = emp.Name, 
        Department = dept?.Name ?? "无部门" 
    }
);
// 输出: [{ Employee = "张三", Department = "开发部" }, 
//        { Employee = "李四", Department = "测试部" }, 
//        { Employee = "王五", Department = "无部门" }]

// Right Join（右外连接）
var rightJoinResult = employees.RightJoin(
    departments,
    emp => emp.DeptId,
    dept => dept.Id,
    (emp, dept) => new {
        Employee = emp?.Name ?? "暂无员工",
        Department = dept.Name
    }
);
// 输出: [{ Employee = "张三", Department = "开发部" },
//        { Employee = "李四", Department = "测试部" }]

// Full Join（全外连接）
var fullJoinResult = employees.FullJoin(
    departments,
    emp => emp.DeptId,
    dept => dept.Id,
    (emp, dept) => new {
        Employee = emp?.Name ?? "暂无员工",
        Department = dept?.Name ?? "无部门"
    }
);
// 输出: 包含所有员工和所有部门的记录，不匹配的部分显示默认值

// 🎯 简化版本：返回元组
var tupleResult = employees.LeftJoin(departments, e => e.DeptId, d => d.Id);
// 返回 IEnumerable<Tuple<Employee, Department?>>

// 🔧 支持自定义比较器
var caseInsensitiveJoin = stringList1.LeftJoin(
    stringList2,
    s => s,
    s => s,
    (s1, s2) => new { Left = s1, Right = s2 },
    StringComparer.OrdinalIgnoreCase  // 忽略大小写比较
);

// 📊 .NET 10 兼容性说明:
// - 在 .NET 10+ 中，这些方法将由 System.Linq.Enumerable 内置提供
// - 当前实现的方法签名与 .NET 10 标准完全一致
// - 升级到 .NET 10 时，可以无缝切换到内置实现
// - 参数名称: outer/inner, outerKeySelector/innerKeySelector, resultSelector
// - 泛型参数: TOuter, TInner, TKey, TResult
```

### 对象扩展

```csharp
using Linger.Extensions.Core;

// 类型安全的对象转换 - 新的标准化方法
object stringObj = "123";
int intValue = stringObj.ToIntOrDefault(0);        // 成功：123
long longValue = stringObj.ToLongOrDefault(0L);    // 成功：123
double doubleValue = stringObj.ToDoubleOrDefault(0.0); // 成功：123.0

// 严格类型安全：非字符串对象返回默认值
object numberObj = 123.45; // 非字符串类型
int invalidInt = numberObj.ToIntOrDefault(0);      // 返回 0（默认值）
bool invalidBool = numberObj.ToBoolOrDefault(false); // 返回 false（默认值）

// 同类型对象：通过字符串转换成功
object intObj = 123;
int result3 = intObj.ToIntOrDefault(0); // 返回：123（通过 ToString() 转换成功）

// 📊 完整的数值类型支持 - 包含所有 .NET 基本数值类型
// 有符号整数类型
object sbyteObj = "100";
sbyte sbyteValue = sbyteObj.ToSByteOrDefault(0);    // 支持：-128 到 127
short shortValue = sbyteObj.ToShortOrDefault(0);    // 支持：-32,768 到 32,767
int intValue2 = sbyteObj.ToIntOrDefault(0);         // 支持：-2,147,483,648 到 2,147,483,647
long longValue2 = sbyteObj.ToLongOrDefault(0L);     // 支持：-9,223,372,036,854,775,808 到 9,223,372,036,854,775,807

// 无符号整数类型
object ubyteObj = "255";
byte byteValue = ubyteObj.ToByteOrDefault(0);       // 支持：0 到 255
ushort ushortValue = ubyteObj.ToUShortOrDefault(0); // 支持：0 到 65,535
uint uintValue = ubyteObj.ToUIntOrDefault(0U);      // 支持：0 到 4,294,967,295
ulong ulongValue = ubyteObj.ToULongOrDefault(0UL);  // 支持：0 到 18,446,744,073,709,551,615

// 浮点数类型
float floatValue = stringObj.ToFloatOrDefault(0.0f);   // 单精度浮点
double doubleValue2 = stringObj.ToDoubleOrDefault(0.0); // 双精度浮点
decimal decimalValue = stringObj.ToDecimalOrDefault(0m); // 高精度小数

// 其他类型转换
DateTime dateValue = stringObj.ToDateTimeOrDefault(DateTime.MinValue);
Guid guidValue = "550e8400-e29b-41d4-a716-446655440000".ToGuidOrDefault();
bool boolValue = stringObj.ToBoolOrDefault(false);

// 空值安全处理
object obj = GetSomeObject();
string result = obj.ToStringOrDefault("default"); // 为 null 时返回默认值

// 🔍 增强的类型检查方法 - 支持所有数值类型
object testObj = (byte)255;
bool isByte = testObj.IsByte();           // 检查是否为 byte 类型
bool isSByte = testObj.IsSByte();         // 检查是否为 sbyte 类型
bool isUShort = testObj.IsUShort();       // 检查是否为 ushort 类型  
bool isUInt = testObj.IsUInt();           // 检查是否为 uint 类型
bool isULong = testObj.IsULong();         // 检查是否为 ulong 类型

// 组合类型检查
bool isNumeric = testObj.IsNumeric();               // 检查是否为任意数值类型
bool isUnsigned = testObj.IsAnyUnsignedInteger();   // 检查是否为无符号整数类型
bool isSigned = testObj.IsAnySignedInteger();       // 检查是否为有符号整数类型

// ⚡ 性能优化的 Try 风格转换 - 避免默认值掩盖失败
if ("123".TryToInt(out var parsedInt)) { /* parsedInt = 123 */ }
if (!"坏数据".TryToDecimal(out var decVal)) { /* decVal = 0，转换失败 */ }

// 无符号整数类型的 Try 转换
if ("255".TryToByte(out var byteResult)) { /* byteResult = 255 */ }
if ("65535".TryToUShort(out var ushortResult)) { /* ushortResult = 65535 */ }
if ("4294967295".TryToUInt(out var uintResult)) { /* uintResult = 4294967295 */ }
if ("18446744073709551615".TryToULong(out var ulongResult)) { /* ulongResult = 18446744073709551615 */ }

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

#### JSON 选项与安全默认

默认 JSON 采用“安全为先”的配置：

- `ExtensionMethodSetting.DefaultJsonSerializerOptions`
    - Encoder: `JavaScriptEncoder.Default`（更安全的转义策略）
    - 数字解析：严格（不默认允许字符串形式的数字）
    - 其他：大小写不敏感、CamelCase、忽略 null、禁止尾逗号、禁止注释、忽略循环引用
    - 自带转换器：`JsonObjectConverter`、`DateTimeConverter`、`DateTimeNullConverter`、`DataTableJsonConverter`

- 互通/宽松场景
    - 出站默认：`ExtensionMethodSetting.DefaultPostJsonOption`（允许尾逗号与数字写为字符串，便于对端兼容）
    - 入站可选：如需兼容不规范输入（注释、尾逗号、数字以字符串等），使用 `ExtensionMethodSetting.CreatePermissiveJsonOptions()` 显式获取宽松选项。

建议：库内与服务器默认使用严格选项；面向外部的非标准交互才显式选择宽松选项。

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

public void ProcessData(string data, IEnumerable<int> numbers, string filePath)
{
    // 🆕 新的参数验证方法（.NET 8 之前版本的 Polyfill）
    // 这些方法与 .NET 8+ 内置方法完全一致，提供无缝升级体验
    
    // 参数空值和内容验证
    ArgumentNullException.ThrowIfNull(data);                    // 确保不为 null
    ArgumentException.ThrowIfNullOrEmpty(data);                 // 确保不为 null 或空字符串
    ArgumentException.ThrowIfNullOrWhiteSpace(data);            // 确保不为 null、空或纯空白字符

    // 集合参数验证  
    ArgumentNullException.ThrowIfNull(numbers);                 // 确保集合不为 null
    
    // 🔍 框架支持说明：
    // - .NET 5 及以下：使用 Linger.ArgumentNullException.ThrowIfNull (polyfill)
    // - .NET 6+：使用内置 System.ArgumentNullException.ThrowIfNull
    // - .NET 7 及以下：使用 Linger.ArgumentException.ThrowIfNullOrEmpty (polyfill) 
    // - .NET 8+：使用内置 System.ArgumentException.ThrowIfNullOrEmpty
    
    // 📦 使用方式：
    using Linger;  // 只需要这一行 using
    
    // 在 .NET 8+ 项目中，升级时只需移除 using Linger; 即可
    // 其他代码完全不需要修改！
    
    // ⚠️ 重要：这些是工具类，不能实例化
    // var ex = new ArgumentException();           // ❌ 编译错误（这是好事！）
    // throw new System.ArgumentException("msg");  // ✅ 正确：手动抛出标准异常
    
    // 🎯 抛出的异常类型：
    // ArgumentNullException.ThrowIfNull() → 抛出 System.ArgumentNullException
    // ArgumentException.ThrowIfNullOrEmpty() → 抛出 System.ArgumentException 或 System.ArgumentNullException
    // ArgumentException.ThrowIfNullOrWhiteSpace() → 抛出 System.ArgumentException 或 System.ArgumentNullException
}

// 🔄 传统方式（仍然支持）
public void ProcessDataTraditional(string data, IEnumerable<int> numbers, string filePath)
{
    using Linger.Helper;
    
    // 基本验证（传统 Guard 方法）
    data.EnsureIsNotNull(nameof(data));                        // 确保不为 null
    data.EnsureIsNotNullOrEmpty(nameof(data));                 // 确保不为 null 或空
    data.EnsureIsNotNullOrWhiteSpace(nameof(data));            // 确保不为 null、空或空白

    // 集合验证
    numbers.EnsureIsNotNullOrEmpty(nameof(numbers));           // 确保集合不为 null 或空

    // 文件系统验证
    filePath.EnsureFileExists(nameof(filePath));               // 确保文件存在
    Path.GetDirectoryName(filePath).EnsureDirectoryExists();   // 确保目录存在

    // 条件验证
    (data.Length > 0).EnsureIsTrue(nameof(data), "数据不能为空");
    (numbers.Count() < 1000).EnsureIsTrue(nameof(numbers), "项目过多");

    // 范围验证
    int value = 5;
    value.EnsureIsInRange(1, 10, nameof(value));               // 确保值在范围内

    // 空值检查
    object? obj = GetSomeObject();
    obj.EnsureIsNotNull(nameof(obj));                          // 如果对象不应为 null
    // 或
    obj.EnsureIsNull(nameof(obj));                             // 如果对象应为 null
}

// 💡 最佳实践建议：
// 1. 新项目：优先使用新的 ArgumentException/ArgumentNullException 静态方法
// 2. 现有项目：可以继续使用传统 Guard 方法，或逐步迁移
// 3. .NET 8+ 项目：升级时移除 using Linger; 即可使用内置方法
// 4. 库开发：使用新方法确保与未来 .NET 版本的完美兼容性
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

从 0.8.2 版本开始，Linger.Utils 进行了重要的 API 标准化，强调类型安全和一致性。

### .NET 10 前瞻性兼容设计

Linger.Utils 已经为即将到来的 .NET 10 做好了准备，特别是在 Join 方法方面：

#### 🚀 未来就绪的 Join 操作
```csharp
// 🎯 当前代码（Linger polyfill）
var result = employees.LeftJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });

// 🔮 .NET 10 发布后（自动切换到内置实现）
var result = employees.LeftJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
// 完全相同的代码，但使用 System.Linq.Enumerable.LeftJoin

// 🎉 无需任何代码修改！
```

#### 📊 完整的方法兼容性矩阵
| 方法 | Linger Polyfill | .NET 10 内置 | 兼容性状态 |
|------|----------------|--------------|------------|
| `LeftJoin` | ✅ 已实现 | 🔮 即将提供 | 💯 完全兼容 |
| `RightJoin` | ✅ 已实现 | 🔮 即将提供 | 💯 完全兼容 |
| `FullJoin` | ✅ 已实现 | ❓ 待确认 | 📋 保持监控 |

#### 🔧 技术实现细节
```csharp
// 条件编译确保无缝过渡
#if !NET10_0_OR_GREATER
    // Linger 的 polyfill 实现
    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(...)
    {
        // 高性能实现，与 .NET 10 行为一致
    }
#endif

// .NET 10+ 环境下，自动使用内置方法
// 性能更优，功能完全一致
```

#### 🎁 提前享受 .NET 10 功能的好处
1. **学习成本降低**: 提前熟悉 .NET 10 API
2. **代码前瞻性**: 无需等待框架升级即可使用新功能  
3. **无缝迁移**: 升级框架时零代码修改
4. **性能一致**: polyfill 实现与未来内置版本性能相当
5. **标准化**: 统一的参数命名和行为模式

### 🔒 严格类型安全原则

**ObjectExtensions 类型安全策略:**
- 所有 `ToXxxOrDefault()` 和 `ToXxxOrNull()` 方法**采用性能优化的类型转换策略**
- **首先检查直接类型匹配**：如果对象已经是目标类型，直接返回（零开销）
- **然后尝试字符串转换**：调用 `ToString()` 方法转换为字符串，再解析为目标类型
- 这确保了**最佳性能**和**一致的转换行为**：同类型对象零开销转换，其他类型通过字符串转换

```csharp
// ✅ 推荐：字符串对象转换
object stringObj = "123";
int result1 = stringObj.ToIntOrDefault(0); // 成功：123

// 🚀 性能优化：直接类型匹配（零开销）
object intObj = 123;
int result2 = intObj.ToIntOrDefault(0); // 返回：123（直接类型匹配，无需 ToString()）

// ⚠️ 非兼容类型：通过 ToString() 转换
object doubleObj = 123.45;
int result3 = doubleObj.ToIntOrDefault(0); // 返回：0（"123.45" 无法转换为 int）
```

### 📊 API 命名标准化

所有类型转换方法采用统一的 `ToXxxOrDefault` 模式：

| 转换类型 | 新方法 | 旧方法（已废弃） |
|---------|--------|-----------------|
| 整数 | `ToIntOrDefault()` | `ToInt()` |
| 长整数 | `ToLongOrDefault()` | `ToLong()` |
| 单精度 | `ToFloatOrDefault()` | `ToFloat()` |
| 双精度 | `ToDoubleOrDefault()` | `ToDouble()` |
| 高精度 | `ToDecimalOrDefault()` | `ToDecimal()` |
| 布尔 | `ToBoolOrDefault()` | `ToBool()` |
| 日期时间 | `ToDateTimeOrDefault()` | `ToDateTime()` |
| GUID | `ToGuidOrDefault()` | `ToGuid()` |

### 🎯 使用示例

```csharp
// 字符串扩展方法（推荐用法）
string numberStr = "123";
double doubleResult = "123.45".ToDoubleOrDefault(0.0); // 成功：123.45

// 增强的布尔转换
bool success1 = "true".ToBoolOrDefault(false);      // true
bool success2 = "是".ToBoolOrDefault(false);        // true（中文支持）
bool success3 = "1".ToBoolOrDefault(false);         // true（数字支持）
bool success4 = "Y".ToBoolOrDefault(false);         // true（字母支持）
```

### ⚡ 性能优势

- **零开销同类型转换**: 直接类型匹配时无需字符串分配和解析（如 `object intObj = 123` → 直接返回）
- **完整数值类型支持**: 涵盖所有 .NET 基本数值类型（byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal）
- **避免异常**: 转换失败时返回默认值而非抛出异常，提升性能
- **智能回退策略**: 仅在需要时才进行字符串转换，最大化性能
- **类型安全**: 编译时和运行时都确保类型安全
- **一致性**: 统一的命名和行为模式，降低学习成本

## 最佳实践

1. **遵循类型安全原则**: 
   - 优先使用字符串扩展方法进行类型转换
   - 对于 object 类型，确保是字符串对象再进行转换
   - 使用 `ToXxxOrDefault()` 而非旧的 `ToXxx()` 方法

2. **优先使用安全方法**: 
   - 数据转换时推荐使用 `ToIntOrDefault()` 而非异常处理，避免转换失败时的性能开销
   - 需要区分转换失败和有效默认值时，使用 `ToIntOrNull()` 等可空方法

3. **善用空值检查**: 
   - 利用 `IsNullOrEmpty()`、`IsNotNullOrEmpty()` 等扩展方法，让代码更简洁可靠
   - 使用统一的 `IsNotNullOrEmpty()` 而非 `IsNotNullAndEmpty()`

4. **做好参数验证**: 
   - 在方法入口使用 `EnsureIsNotNull()`、`EnsureIsNotNullOrEmpty()` 等方法进行参数校验
   - 使用新的标准化 Guard 方法名称

5. **合理使用异步**: 
   - 文件操作等 I/O 密集型任务建议使用异步版本，提升程序响应性

6. **妥善处理异常**: 
   - 文件操作等可能失败的操作要做好异常处理和用户提示
   - 重试操作使用 `RetryHelper`，捕获 `OutOfRetryCountException`

7. **注意资源管理**: 
   - 使用 `using` 语句确保资源得到正确释放

8. **GUID 操作规范**: 
   - 使用 `IsEmpty()`、`IsNotEmpty()`、`IsNotNullOrEmpty()` 等方法而不是直接比较

9. **简化集合操作**: 
   - 善用 `ForEach()`、`IsNullOrEmpty()` 等扩展方法，让集合处理更加简洁

10. **代码迁移**: 
    - 及时迁移到新的 API，避免使用已标记 `[Obsolete]` 的方法
    - 关注编译警告，按照迁移指南进行更新

## 迁移说明 (0.8.2 → 下一版本)

为提升命名一致性、类型安全与可读性，本版本对 API 进行了重要的标准化改进。旧名称均以 `[Obsolete]` 标记并仍可使用（过渡期：0.9.x，计划在首个 1.0 预发布版本移除），建议尽快迁移。

### Join 方法标准化与 .NET 10 兼容性

为了与即将发布的 .NET 10 标准保持一致，Join 方法已进行重大重构：

#### Join 方法重命名
| 旧方法名 | 新方法名 | 状态 | 说明 |
|---------|---------|------|------|
| `LeftOuterJoin` | `LeftJoin` | ✅ 完成 | 与 .NET 10 内置方法名称一致 |
| `RightOuterJoin` | `RightJoin` | ✅ 完成 | 与 .NET 10 内置方法名称一致 |
| `FullOuterJoin` | `FullJoin` | ✅ 完成 | 简化命名，保持一致性 |
| `InnerJoin` | *已移除* | ❌ 移除 | 功能与内置 `Join` 重复 |

#### 参数名称标准化
所有 Join 方法现在使用与 .NET 10 完全一致的参数名称：

```csharp
// ✅ 新的标准化参数名称（与 .NET 10 一致）
public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
    this IEnumerable<TOuter> outer,           // 外部序列
    IEnumerable<TInner> inner,                // 内部序列  
    Func<TOuter, TKey> outerKeySelector,      // 外部键选择器
    Func<TInner, TKey> innerKeySelector,      // 内部键选择器
    Func<TOuter, TInner?, TResult> resultSelector  // 结果选择器
)

// ❌ 旧的参数名称（已废弃）
// left, right, leftKeySelector, rightKeySelector, resultSelector
```

#### .NET 10 Polyfill 策略
当前实现采用条件编译，为未来 .NET 10 升级做好准备：

```csharp
#if !NET10_0_OR_GREATER
// Linger 提供的 polyfill 实现
public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(...) { ... }
#endif

// 🔄 升级到 .NET 10 时的迁移路径：
// 1. 升级项目目标框架到 .NET 10
// 2. 代码自动使用 System.Linq.Enumerable.LeftJoin
// 3. 无需修改任何调用代码，因为方法签名完全一致
```

#### 迁移示例

```csharp
// 🔄 迁移前（已废弃的方法）
var result1 = employees.LeftOuterJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
var result2 = employees.RightOuterJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
var result3 = employees.FullOuterJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
var result4 = employees.InnerJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });

// ✅ 迁移后（.NET 10 兼容）
var result1 = employees.LeftJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
var result2 = employees.RightJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
var result3 = employees.FullJoin(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d });
var result4 = employees.Join(departments, e => e.DeptId, d => d.Id, (e, d) => new { e, d }); // 使用内置 Join
```

#### Join 方法优势对比

| 特性 | 旧实现 | 新实现 (.NET 10 兼容) |
|------|--------|----------------------|
| 方法名称 | 非标准长名称 | .NET 10 标准短名称 |
| 参数命名 | left/right 模式 | outer/inner 标准模式 |
| 泛型参数 | TLeft/TRight 模式 | TOuter/TInner 标准模式 |
| 未来兼容性 | 需要手动迁移 | 自动使用内置方法 |
| IntelliSense | 冗长的方法名 | 简洁标准的方法名 |
| 性能 | 相同 | 相同（底层实现一致） |

### 🔒 重要：类型安全增强

**ObjectExtensions 行为变更:**
- 所有类型转换方法现在**采用性能优化的转换策略**
- **首先进行直接类型匹配**：如果对象已经是目标类型，直接返回（零开销）
- **然后尝试字符串转换**：调用 `ToString()` 转换为字符串，再解析为目标类型
- 这确保了**最佳性能**和**可预测的转换行为**
- **新增完整数值类型支持**：现在支持所有 .NET 基本数值类型

```csharp
// 🆕 新行为（性能优化 + 类型安全）
// 🆕 新增无符号整数类型支持
object byteObj = (byte)255;
byte byteResult = byteObj.ToByteOrDefault(0); // 直接返回 255（零开销）

object ushortObj = "65535";
ushort ushortResult = ushortObj.ToUShortOrDefault(0); // 字符串解析为 65535

object uintStr = "4294967295";
uint uintResult = uintStr.ToUIntOrDefault(0); // 支持完整 uint 范围

object ulongStr = "18446744073709551615";
ulong ulongResult = ulongStr.ToULongOrDefault(0); // 支持完整 ulong 范围

// 🆕 有符号字节类型支持
object sbyteStr = "-100";
sbyte sbyteResult = sbyteStr.ToSByteOrDefault(0); // 支持 -128 到 127
```

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
// 迁移前 (Guard 方法)
data.EnsureIsNotNullAndEmpty();
filePath.EnsureFileExist();
directory.EnsureDirectoryExist();
try { await retry.ExecuteAsync(action, "操作"); } catch (OutOfReTryCountException ex) { /* ... */ }

// 迁移前 (类型转换)
int value = stringValue.ToInt(0);
double amount = stringValue.ToDouble(0.0);
bool flag = stringValue.ToBool(false);

// 迁移后 (Guard 方法)
data.EnsureIsNotNullOrEmpty();
filePath.EnsureFileExists();
directory.EnsureDirectoryExists();
try { await retry.ExecuteAsync(action); } catch (OutOfRetryCountException ex) { /* ... */ }

// 迁移后 (类型转换)
int value = stringValue.ToIntOrDefault(0);
double amount = stringValue.ToDoubleOrDefault(0.0);
bool flag = stringValue.ToBoolOrDefault(false);
```

功能行为未改变，仅是命名与诊断信息改进。

### 使用示例 (新 API)
```csharp
// 类型转换使用新的标准化方法
string numberStr = "123";
int result = numberStr.ToIntOrDefault(0);           // 返回 123，失败时返回 0
long longResult = numberStr.ToLongOrDefault(0L);    // 一致的命名模式
double doubleResult = "123.45".ToDoubleOrDefault(0.0, digits: 2); // 支持舍入

// 增强的布尔转换解析
bool success1 = "true".ToBoolOrDefault(false);      // 返回 true
bool success2 = "是".ToBoolOrDefault(false);        // 返回 true (增强解析)
bool success3 = "1".ToBoolOrDefault(false);         // 返回 true (数字支持)

// 日期时间转换
DateTime date = "2024-01-01".ToDateTimeOrDefault(DateTime.MinValue);

// GUID 转换
Guid guid = "550e8400-e29b-41d4-a716-446655440000".ToGuidOrDefault();

// 精确移除后缀（忽略大小写）
var trimmed = "报告.DOCX".RemoveSuffixOnce(".docx", StringComparison.OrdinalIgnoreCase); // => "报告"

// 确保前缀（忽略大小写）
var normalized = "api/values".EnsureStartsWith("/API", StringComparison.OrdinalIgnoreCase); // => "/api/values"
```

### 迁移示例
```csharp
// 迁移前 (已废弃)
int value1 = stringValue.ToInt(0);
long value2 = stringValue.ToLong(0L);
double value3 = stringValue.ToDouble(0.0);
bool value4 = stringValue.ToBool(false);
DateTime value5 = stringValue.ToDateTime(DateTime.MinValue);
Guid value6 = stringValue.ToGuid();

// 迁移后 (当前)
int value1 = stringValue.ToIntOrDefault(0);
long value2 = stringValue.ToLongOrDefault(0L);
double value3 = stringValue.ToDoubleOrDefault(0.0);
bool value4 = stringValue.ToBoolOrDefault(false);
DateTime value5 = stringValue.ToDateTimeOrDefault(DateTime.MinValue);
Guid value6 = stringValue.ToGuidOrDefault();
```

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

> 行为提醒：`RemoveLastChar(string)` 的多字符参数按“字符集合裁剪”（等同 `TrimEnd(char[])`），可能与“精确后缀删除”的预期不同。
>
> 示例：
> 
> ```csharp
> "abc".RemoveLastChar("c")   // => "ab" （单字符：精确移除一次）
> "abc".RemoveLastChar("x")   // => "abc"
> "abcc".RemoveLastChar("bc") // => "a"   （多字符：‘b’与‘c’均作为裁剪集合被移除）
> // 精确删除单字符：用 RemoveLastChar('c')
> // 精确删除多字符后缀：用 RemoveSuffixOnce(".ext", StringComparison.OrdinalIgnoreCase)
> ```

### 类型转换 API 标准化 (0.8.2+)
类型转换方法已统一采用 `ToXxxOrDefault` 命名模式，并新增完整的数值类型支持：

| 分类 | 新 API | 旧 API (已废弃) | 说明 |
|------|--------|----------------|------|
| **基本整数类型** | | | |
| 字符串 → 整数 | `ToIntOrDefault()` | `ToInt()` | 与 .NET 模式保持一致 |
| 字符串 → 长整数 | `ToLongOrDefault()` | `ToLong()` | 更好的语义清晰度 |
| 字符串 → 短整数 | `ToShortOrDefault()` | `ToShort()` | 完整类型支持 |
| **新增：无符号整数类型** | | | |
| 字符串 → 字节 | `ToByteOrDefault()` | *新增* | 0 到 255 |
| 字符串 → 无符号短整数 | `ToUShortOrDefault()` | *新增* | 0 到 65,535 |
| 字符串 → 无符号整数 | `ToUIntOrDefault()` | *新增* | 0 到 4,294,967,295 |
| 字符串 → 无符号长整数 | `ToULongOrDefault()` | *新增* | 0 到 18,446,744,073,709,551,615 |
| **新增：有符号字节类型** | | | |
| 字符串 → 有符号字节 | `ToSByteOrDefault()` | *新增* | -128 到 127 |
| **浮点数类型** | | | |
| 字符串 → 单精度 | `ToFloatOrDefault()` | `ToFloat()` | 统一参数顺序 |
| 字符串 → 双精度 | `ToDoubleOrDefault()` | `ToDouble()` | 一致的重载模式 |
| 字符串 → 高精度 | `ToDecimalOrDefault()` | `ToDecimal()` | 专业 API 设计 |
| **其他类型** | | | |
| 字符串 → 布尔 | `ToBoolOrDefault()` | `ToBool()` | 增强的布尔解析 |
| 字符串 → 日期 | `ToDateTimeOrDefault()` | `ToDateTime()` | 改进的空值处理 |
| 字符串 → GUID | `ToGuidOrDefault()` | `ToGuid()` | 一致的行为表现 |
| **对象扩展对应** | | | |
| 对象 → 各类型 | 所有对应 `OrDefault` 方法 | 旧方法 | ObjectExtensions 已更新 |

**🆕 新增类型检查方法：**
| 类型检查 | 方法 | 说明 |
|----------|------|------|
| 字节类型 | `IsByte()` | 检查是否为 byte 类型 |
| 有符号字节 | `IsSByte()` | 检查是否为 sbyte 类型 |
| 无符号短整数 | `IsUShort()` | 检查是否为 ushort 类型 |
| 无符号整数 | `IsUInt()` | 检查是否为 uint 类型 |
| 无符号长整数 | `IsULong()` | 检查是否为 ulong 类型 |
| 任意无符号整数 | `IsAnyUnsignedInteger()` | 检查是否为任意无符号整数类型 |
| 数值类型（增强） | `IsNumeric()` | 现在包含所有新增的数值类型 |

**🆕 新增 Try 转换方法：**
| Try 转换 | 方法 | 说明 |
|----------|------|------|
| 字节转换 | `TryToByte(out byte value)` | 安全转换到 byte 类型 |
| 有符号字节转换 | `TryToSByte(out sbyte value)` | 安全转换到 sbyte 类型 |
| 无符号短整数转换 | `TryToUShort(out ushort value)` | 安全转换到 ushort 类型 |
| 无符号整数转换 | `TryToUInt(out uint value)` | 安全转换到 uint 类型 |
| 无符号长整数转换 | `TryToULong(out ulong value)` | 安全转换到 ulong 类型 |

> 命名约定：Try 风格方法统一采用 `TryToXxx(out T)` 并返回 `bool`。字符串扩展使用 `out` 可空值类型（如 `out int?`），对象扩展使用 `out` 非可空值类型（如 `out int`）。

**新 API 的优势：**
- ✅ 完整的 .NET 数值类型支持（所有基本数值类型）
- ✅ 所有转换方法命名一致
- ✅ 统一的参数顺序：`(value, defaultValue, additionalParams)`
- ✅ 更好的 IntelliSense 可发现性
- ✅ 符合行业标准的专业 API 设计
- ✅ "失败时返回默认值"的语义更清晰
- ✅ 性能优化：直接类型匹配时零开销转换

### 已标记过时 (Obsolete) – 计划移除 (目标: 1.0.0)
| 过时成员 | 替代 | 说明 |
|----------|------|------|
| **类型转换方法** | | |
| `string.ToInt()` | `ToIntOrDefault()` | 统一命名模式 |
| `string.ToLong()` | `ToLongOrDefault()` | API 标准化 |
| `string.ToFloat()` | `ToFloatOrDefault()` | 统一参数顺序 |
| `string.ToDouble()` | `ToDoubleOrDefault()` | 更好的语义清晰度 |
| `string.ToDecimal()` | `ToDecimalOrDefault()` | 专业化命名 |
| `string.ToBool()` | `ToBoolOrDefault()` | 增强的布尔解析 |
| `string.ToDateTime()` | `ToDateTimeOrDefault()` | 改进的空值处理 |
| `string.ToGuid()` | `ToGuidOrDefault()` | 一致的行为表现 |
| `object.ToInt()` | `ToIntOrDefault()` | ObjectExtensions 对齐 |
| `object.ToLong()` | `ToLongOrDefault()` | 同上 |
| `object.ToFloat()` | `ToFloatOrDefault()` | 同上 |
| `object.ToDouble()` | `ToDoubleOrDefault()` | 同上 |
| `object.ToDecimal()` | `ToDecimalOrDefault()` | 同上 |
| `object.ToBool()` | `ToBoolOrDefault()` | 同上 |
| `object.ToDateTime()` | `ToDateTimeOrDefault()` | 同上 |
| `object.ToGuid()` | `ToGuidOrDefault()` | 同上 |
| `string.ToSafeString()` | `ToStringOrDefault()` | 命名一致性 |
| `object.ToShort()` | `ToShortOrDefault()` | 命名一致性（新增） |
| **新增类型转换方法** | | |
| *无新增的过时方法* | `ToByteOrDefault()` | 新增字节类型支持 |
| *无新增的过时方法* | `ToSByteOrDefault()` | 新增有符号字节类型支持 |
| *无新增的过时方法* | `ToUShortOrDefault()` | 新增无符号短整数类型支持 |
| *无新增的过时方法* | `ToUIntOrDefault()` | 新增无符号整数类型支持 |
| *无新增的过时方法* | `ToULongOrDefault()` | 新增无符号长整数类型支持 |
| **新增类型检查方法** | | |
| *无新增的过时方法* | `IsByte()` | 新增字节类型检查 |
| *无新增的过时方法* | `IsSByte()` | 新增有符号字节类型检查 |
| *无新增的过时方法* | `IsUShort()` | 新增无符号短整数类型检查 |
| *无新增的过时方法* | `IsUInt()` | 新增无符号整数类型检查 |
| *无新增的过时方法* | `IsULong()` | 新增无符号长整数类型检查 |
| **新增 Try 转换方法** | | |
| *无新增的过时方法* | `TryToByte()` | 新增安全字节转换 |
| *无新增的过时方法* | `TryToSByte()` | 新增安全有符号字节转换 |
| *无新增的过时方法* | `TryToUShort()` | 新增安全无符号短整数转换 |
| *无新增的过时方法* | `TryToUInt()` | 新增安全无符号整数转换 |
| *无新增的过时方法* | `TryToULong()` | 新增安全无符号长整数转换 |
| **其他 API 变更** | | |
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
| `RemoveLastChar(string)` | `RemoveLastChar(char)` / `RemoveSuffixOnce` | 多字符参数按字符集合 TrimEnd 的旧语义；推荐迁移到精确 API |

> 移除窗口：上述成员计划在 0.9.x 稳定版之后（或最迟 1.0.0 之前）删除。请尽早迁移。

### 1.0.0 预定移除列表 (预览)
将移除所有上表 Obsolete 成员以及代码内标记 “Will be removed in 1.0.0” 的项。最终确认列表会在 1.0.0 发布说明中公布。

### 新增功能 (0.8.2 之后增量)

#### 🔢 完整数值类型支持扩展
新增对所有 .NET 基本数值类型的完整支持，覆盖有符号和无符号整数：

```csharp
// 🆕 无符号整数类型支持
byte byteValue = "255".ToByteOrDefault(0);              // 0 到 255
ushort ushortValue = "65535".ToUShortOrDefault(0);      // 0 到 65,535  
uint uintValue = "4294967295".ToUIntOrDefault(0);       // 0 到 4,294,967,295
ulong ulongValue = "18446744073709551615".ToULongOrDefault(0); // 0 到 18,446,744,073,709,551,615

// 🆕 有符号字节类型支持
sbyte sbyteValue = "-100".ToSByteOrDefault(0);          // -128 到 127

// 🆕 增强的类型检查
bool isByte = ((byte)255).IsByte();                     // 检查具体数值类型
bool isUnsigned = ((uint)123).IsAnyUnsignedInteger();   // 检查无符号整数组
bool isNumeric = ((ushort)456).IsNumeric();             // 增强的数值类型检查

// 🆕 Try 风格安全转换
if ("200".TryToByte(out var b)) { /* 使用 b */ }
if ("50000".TryToUShort(out var us)) { /* 使用 us */ }
if ("3000000000".TryToUInt(out var ui)) { /* 使用 ui */ }
if ("15000000000000000000".TryToULong(out var ul)) { /* 使用 ul */ }
if ("-75".TryToSByte(out var sb)) { /* 使用 sb */ }

// ⚡ 性能优化：直接类型匹配时零开销
object directByte = (byte)200;
byte fastResult = directByte.ToByteOrDefault(0); // 直接返回，无字符串转换
```

**支持的完整数值类型矩阵：**
| 类型 | 范围 | ToXxxOrDefault | ToXxxOrNull | TryToXxx | IsXxx |
|------|------|----------------|-------------|----------|-------|
| `byte` | 0 到 255 | ✅ | ✅ | ✅ | ✅ |
| `sbyte` | -128 到 127 | ✅ | ✅ | ✅ | ✅ |
| `short` | -32,768 到 32,767 | ✅ | ✅ | ✅ | ✅ |
| `ushort` | 0 到 65,535 | ✅ | ✅ | ✅ | ✅ |
| `int` | -2,147,483,648 到 2,147,483,647 | ✅ | ✅ | ✅ | ✅ |
| `uint` | 0 到 4,294,967,295 | ✅ | ✅ | ✅ | ✅ |
| `long` | -9,223,372,036,854,775,808 到 9,223,372,036,854,775,807 | ✅ | ✅ | ✅ | ✅ |
| `ulong` | 0 到 18,446,744,073,709,551,615 | ✅ | ✅ | ✅ | ✅ |
| `float` | 单精度浮点 | ✅ | ✅ | ✅ | ✅ |
| `double` | 双精度浮点 | ✅ | ✅ | ✅ | ✅ |
| `decimal` | 高精度小数 | ✅ | ✅ | ✅ | ✅ |

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

## Polyfill 汇总（BCL 与语言特性）

本库对常用的 BCL API 以及部分语言特性提供了前向兼容的 Polyfill，以便在较旧的目标框架（例如 .NET Framework、.NET Standard 2.0、.NET 5）上获得一致的编译与运行体验。所有 Polyfill 都采用条件编译，升级到新 TFM 后将自动让位于框架内置实现。

### BCL 级 Polyfill（按来源组织）

- 参数验证
    - `ArgumentNullException.ThrowIfNull(object? argument, string? paramName = null)` 兼容 .NET 6 之前
        - 源码：`src/Linger/Polyfills/ArgumentNullException.cs`
    - `ArgumentException.ThrowIfNullOrEmpty(string? argument, string? paramName = null)` 兼容 .NET 8 之前
    - `ArgumentException.ThrowIfNullOrWhiteSpace(string? argument, string? paramName = null)` 兼容 .NET 8 之前
        - 源码：`src/Linger/Polyfills/ArgumentException.cs`
- 调用方参数名捕获
    - `System.Runtime.CompilerServices.CallerArgumentExpressionAttribute`（用于改进异常参数名与 Guard 体验）
        - 源码：`src/Linger/Polyfills/CallerArgumentExpressionAttribute.cs`

以上均为“静态工具方法/属性/特性”的 Polyfill，调用表现与内置版本一致；当目标框架满足条件（如 .NET 8+）时，这些 Polyfill 将被条件编译排除，代码自动转用框架内置实现。

### 语言特性 Polyfill：required 成员（旧 TFM 支持）

为在 .NET Framework / .NET Standard 上使用 C# 11 的 `required` 关键字并获得正确的编译器诊断与元数据，本库提供以下特性与支持类型：

- `System.Runtime.CompilerServices.RequiredMemberAttribute`
- `System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute`
- `System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute`（包含 `IsExternalInit` 支持，用于 `init` 访问器）
        - 源码：
            - `src/Linger/Polyfills/RequiredMemberAttribute.cs`
            - `src/Linger/Polyfills/SetsRequiredMembersAttribute.cs`
            - `src/Linger/Polyfills/CompilerFeatureRequiredAttribute.cs`

这些文件均使用条件编译保护（如 `#if !NET7_0_OR_GREATER`），在新框架下将自动失效，避免与内置类型重复。

使用示例（适用于 .NET Framework/.NET Standard，仅需确保 C# 语言版本 >= 11）：

```csharp
// 示例：在旧 TFM 下使用 required 成员（编译器会正确提示未赋值的 required 成员）
public class Person
{
        public required string Name { get; init; }
        public required int Age { get; init; }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Person()
        {
                Name = "Unknown";
                Age = 0;
        }
}

var ok = new Person { Name = "Alice", Age = 28 }; // ✅ 正常
var error = new Person(); // ❌ 编译器诊断：未设置 required 成员
```

注意事项：
- 语言版本需为 C# 11+（本仓库 `Directory.Build.props` 已设置 `LangVersion=latest`）。
- Polyfill 仅提供编译所需的特性定义与 `init` 支持，不改变运行时语义；required 的赋值检查由编译器负责。
- 迁移到 .NET 7+ 后，无需改动业务代码；Polyfill 将自动停用，继续使用框架内置特性。

### 可空性注解（CodeAnalysis）Polyfill

为较老的目标框架提供 `System.Diagnostics.CodeAnalysis` 可空性/流分析特性，确保注解在编译期可用并与新框架行为一致：

- 提供的特性：`AllowNull`、`DisallowNull`、`MaybeNull`、`NotNull`、`MaybeNullWhen`、`NotNullWhen`、`NotNullIfNotNull`、`DoesNotReturn`、`DoesNotReturnIf`、`MemberNotNull`、`MemberNotNullWhen`
- 源码位置：`src/Linger/Polyfills/NullableAttributes.cs`
- 适用范围：针对 `netstandard2.0`、`net4x`、`netcoreapp2.x/3.x` 等较老 TFM 启用；在较新框架（具备内置实现）下通过条件编译自动让位，避免重复定义。

### IEnumerable 扩展 Polyfill（面向未来的 .NET 10）

为提前对齐 .NET 10 API，本库提供 `LeftJoin` / `RightJoin` / `FullJoin` 等扩展方法的 Polyfill：

- 源码：`src/Linger/Extensions/Collection/IEnumerableExtensions.Polyfills.cs`
- 条件编译：`#if !NET10_0_OR_GREATER`，升级到 .NET 10 后自动切换为 `System.Linq.Enumerable` 内置实现
- 参数与泛型命名已与 .NET 10 对齐（outer/inner，outerKeySelector/innerKeySelector 等）

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
