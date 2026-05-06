# Linger

[English](README.md) | [中文](README.zh-CN.md)

**Linger 是一个全面、模块化的 .NET 工具库集合，旨在加速企业级应用开发。它提供了从核心工具类、扩展方法到高级功能（如 JWT 认证、数据访问层、文件系统操作、邮件服务）的完整解决方案，具有严格的类型安全、高性能和跨平台支持特性。**

[![NuGet](https://img.shields.io/nuget/v/Linger.Utils.svg)](https://www.nuget.org/packages/Linger.Utils/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%209.0%20%7C%208.0%20%7C%20Standard%202.0-512BD4)](https://dotnet.microsoft.com/)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](.github/workflows/dotnet.yml)

## 特性

- 🚀 **高性能** - 零开销类型转换
- 🔒 **类型安全** - 严格的类型安全原则
- 🔮 **.NET 10 已支持** - 原生支持 .NET 10；同时保留 Polyfills 以兼容旧目标框架
- 📦 **模块化** - 按需引入
- 🌐 **跨平台** - 支持 .NET 10.0、9.0、8.0、Standard 2.0、Framework 4.7.2

## 快速开始

```bash
# 核心工具库
dotnet add package Linger.Utils

# Result 模式库
dotnet add package Linger.Results

# JWT 认证
dotnet add package Linger.AspNetCore.Jwt

# 数据访问
dotnet add package Linger.DataAccess.SqlServer
```

## 包选择指南

- 构建通用工具与扩展方法：安装 `Linger.Utils`
- 在服务边界需要明确成功/失败返回模型：安装 `Linger.Results`
- 构建 ASP.NET Core Token 认证：安装 `Linger.AspNetCore.Jwt`
- 使用统一抽象访问关系型数据库：按数据库选择其一
    `Linger.DataAccess.SqlServer`、`Linger.DataAccess.Oracle`、`Linger.DataAccess.Sqlite`
- 使用 EF Core 并需要增强能力：安装 `Linger.EFCore`
- 处理本地/FTP/SFTP 文件系统：安装 `Linger.FileSystem`
- 导入导出 Excel：安装 `Linger.Excel` 并按需选择具体 Provider 包
- 集成 LDAP 认证：安装 `Linger.Ldap.ActiveDirectory` 或 `Linger.Ldap.Novell`

## 基础用法

```csharp
using Linger.Extensions.Core;

// 字符串转换
int result = "123".ToIntOrDefault(0);

// 日期操作
int age = birthDate.CalculateAge();

// Result 模式
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    return user == null 
        ? Result<User>.NotFound($"用户 {id} 不存在")
        : Result<User>.Success(user);
}
```

## 核心包

| 包名 | 描述 |
|------|------|
| **Linger.Utils** | 核心工具库，提供扩展方法和 Helper 类 |
| **Linger.Results** | 函数式结果处理库 |
| **Linger.AspNetCore.Jwt** | JWT 认证服务 |
| **Linger.DataAccess** | 数据库抽象层（SQL Server、Oracle、SQLite） |
| **Linger.EFCore** | Entity Framework Core 扩展 |
| **Linger.FileSystem** | 统一的文件系统抽象（本地、FTP、SFTP） |
| **Linger.Email** | 电子邮件发送功能 |
| **Linger.Excel** | Excel 操作（EPPlus、NPOI、ClosedXML） |
| **Linger.Dapper** | Dapper 扩展 |
| **Linger.Ldap** | LDAP 认证（Active Directory、Novell） |

## 文档

- [Linger.Utils 文档](src/Linger/README.md)
- [Linger.Results 文档](src/Linger.Results/README.md)
- [Linger.AspNetCore.Jwt 文档](src/Linger.AspNetCore.Jwt/README.md)
- [Linger.DataAccess 文档](src/Linger.DataAccess/README.md)
- [Linger.EFCore 文档](src/Linger.EFCore/README.md)
- [Linger.FileSystem 文档](src/Linger.FileSystem/README.md)
- [Linger.Email 文档](src/Linger.Email/README.md)
- [Linger.Excel 文档](src/Linger.Excel/README.md)
- [Linger.Dapper 文档](src/Linger.Dapper/README.md)
- [Linger.Ldap.ActiveDirectory 文档](src/Linger.Ldap.ActiveDirectory/README.zh-CN.md)
- [Linger.Ldap.Novell 文档](src/Linger.Ldap.Novell/README.zh-CN.md)

## 贡献

欢迎贡献代码！请随时提交 Pull Request。

## 许可证

本项目采用 MIT 许可证。
