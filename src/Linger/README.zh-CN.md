# Linger.Utils

<a href="https://www.nuget.org/packages/Linger.Utils"><img src="https://img.shields.io/nuget/v/Linger.Utils.svg" alt="NuGet 版本" /></a> 
<a href="https://www.nuget.org/packages/Linger.Utils"><img src="https://img.shields.io/nuget/dt/Linger.Utils.svg" alt="NuGet 下载次数" /></a>

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

## 概述

一个全面的 .NET 实用工具库，提供丰富的类型转换扩展、字符串处理工具、日期时间辅助函数、文件系统操作、集合扩展以及各种适用于日常开发任务的辅助类。

## 主要特性

- **类型转换** - 针对所有常见数据类型的强大转换方法
- **字符串处理** - 高级字符串处理和格式化工具
- **日期时间处理** - 日期和时间计算、格式化与操作
- **文件系统操作** - 文件和目录操作辅助函数
- **集合扩展** - 集合的增强型 LINQ 风格操作
- **加密工具** - 哈希、加密和安全辅助工具
- **验证辅助** - 数据验证和校验工具
- **JSON 工具** - 简化的 JSON 序列化和反序列化

## 支持的 .NET 版本

- .NET 9.0
- .NET 8.0
- .NET Standard 2.0
- .NET Framework 4.7.2

## 安装方法

### 通过 Visual Studio

1. 打开`解决方案资源管理器`
2. 右键点击您的项目
3. 点击`管理 NuGet 包...`
4. 点击`浏览`选项卡，搜索 "Linger.Utils"
5. 点击 `Linger.Utils` 包，选择适当的版本并点击安装

### 通过 Package Manager 控制台

```
PM> Install-Package Linger.Utils
```

### 通过 .NET CLI 命令行

```
dotnet add package Linger.Utils
```

## 基础用法示例

### 字符串扩展

```csharp
// 字符串转换
int number = "123".ToInt();
decimal price = "45.67".ToDecimal();
bool isActive = "true".ToBool();
DateTime date = "2025-05-23".ToDateTime();

// 字符串操作
string trimmed = " Hello World ".TrimBoth();
bool isEmail = "user@example.com".IsEmail();
bool containsLetters = "abc123".ContainsLetters();
```

### 集合扩展

```csharp
// 集合操作
var list = new List<string> { "apple", "banana", "cherry" };
bool hasApple = list.ContainsIgnoreCase("APPLE");
var distinct = list.DistinctBy(item => item.Length);

// 安全操作
var firstItem = list.FirstOrDefault("default");
```

### 文件操作

```csharp
// 文件操作
var file = new FileInfo("document.txt");
string text = file.ReadAllText();
byte[] bytes = file.ReadAllBytes();
```

## 许可证

MIT 许可证
