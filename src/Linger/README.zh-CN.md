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
  - [.NET 10 前瞻性兼容设计](#net-10-前瞻性兼容设计)
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
// 4. DES 算法仅建议用于兼容性场景

// ⚠️ 安全提示：
// 1. 推荐使用 AES 算法进行新项目开发
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

所有类型转换方法采用统一的 `ToXxxOrDefault` 模式，**完整支持所有 .NET 数值类型**，提供一致的API体验。

**🆕 新增类型检查方法:**

| 分类 | 方法 | 说明 |
|------|------|------|
| 字节类型 | `IsByte()` | 检查是否为 byte 类型 |
| 有符号字节 | `IsSByte()` | 检查是否为 sbyte 类型 |
| 无符号短整型 | `IsUShort()` | 检查是否为 ushort 类型 |
| 无符号整型 | `IsUInt()` | 检查是否为 uint 类型 |
| 无符号长整型 | `IsULong()` | 检查是否为 ulong 类型 |

### 🎯 使用示例

```csharp
// 字符串扩展方法（推荐用法）
string numberStr = "123";
int result = numberStr.ToIntOrDefault(0);           // 成功：123
long longResult = numberStr.ToLongOrDefault(0L);    // 成功：123
double doubleResult = "123.45".ToDoubleOrDefault(0.0); // 成功：123.45

// 🆕 完整数值类型支持示例
byte byteResult = "255".ToByteOrDefault(0);         // 成功：255
sbyte sbyteResult = "-100".ToSByteOrDefault(0);     // 成功：-100  
ushort ushortResult = "65535".ToUShortOrDefault(0); // 成功：65535
uint uintResult = "4294967295".ToUIntOrDefault(0U); // 成功：4294967295
ulong ulongResult = "18446744073709551615".ToULongOrDefault(0UL); // 成功：最大 ulong 值

// 对象扩展方法（性能优化）
object validObj = "456";
int validResult = validObj.ToIntOrDefault(0);       // 成功：456

object directMatch = 789; // 直接类型匹配
int directResult = directMatch.ToIntOrDefault(0);   // 成功：789（零开销）

object invalidObj = 789.12; // 非兼容类型转换
int invalidResult = invalidObj.ToIntOrDefault(0);   // 返回：0（通过 ToString 转换失败）

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
   - 使用 `ToXxxOrDefault()` 系列方法进行安全转换

2. **优先使用安全方法**: 
   - 数据转换时推荐使用 `ToIntOrDefault()` 而非异常处理，避免转换失败时的性能开销
   - 需要区分转换失败和有效默认值时，使用 `ToIntOrNull()` 等可空方法

3. **善用空值检查**: 
   - 利用 `IsNullOrEmpty()`、`IsNotNullOrEmpty()` 等扩展方法，让代码更简洁可靠

4. **做好参数验证**: 
   - 使用 `GuardExtensions` 的 `EnsureIsNotNull()`、`EnsureIsNotNullOrEmpty()` 等方法进行输入验证

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

以上均为"静态工具方法/属性/特性"的 Polyfill，调用表现与内置版本一致；当目标框架满足条件（如 .NET 8+）时，这些 Polyfill 将被条件编译排除，代码自动转用框架内置实现。

### 语言特性 Polyfill：required 成员支持

为在旧版框架上使用 C# 11 的 `required` 关键字并获得正确的编译器诊断与元数据，本库提供以下特性与支持类型:

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
// 示例：使用 required 成员（编译器会正确提示未赋值的 required 成员）
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
