# Linger

[English](README.md) | [中文](README.zh-CN.md)

**Linger is a comprehensive, modular .NET utility library collection designed to accelerate enterprise application development. It provides everything from core utilities and extension methods to advanced features like JWT authentication, data access layers, file system operations, and email services—all with strict type safety, high performance, and cross-platform support.**

[![NuGet](https://img.shields.io/nuget/v/Linger.Utils.svg)](https://www.nuget.org/packages/Linger.Utils/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0%20%7C%209.0%20%7C%208.0%20%7C%20Standard%202.0-512BD4)](https://dotnet.microsoft.com/)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](.github/workflows/dotnet.yml)

## Features

- 🚀 **High Performance** - Zero-overhead type conversions
- 🔒 **Type Safety** - Strict type safety principles
- 🔮 **.NET 10 Supported** - Native support for .NET 10; Polyfills are retained to provide backward compatibility for older target frameworks
- 📦 **Modular** - Install only what you need
- 🌐 **Cross-platform** - Supports .NET 10.0, 9.0, 8.0, Standard 2.0, Framework 4.7.2

## Quick Start

```bash
# Core utilities
dotnet add package Linger.Utils

# Result pattern
dotnet add package Linger.Results

# JWT authentication
dotnet add package Linger.AspNetCore.Jwt

# Data access
dotnet add package Linger.DataAccess.SqlServer
```

## Package Selection Guide

- Build common utilities and extension methods: install `Linger.Utils`
- Need explicit success/failure return models in service boundaries: install `Linger.Results`
- Build ASP.NET Core token authentication: install `Linger.AspNetCore.Jwt`
- Access relational databases with provider abstraction: install one of
    `Linger.DataAccess.SqlServer`, `Linger.DataAccess.Oracle`, or `Linger.DataAccess.Sqlite`
- Use EF Core with extra helpers: install `Linger.EFCore`
- Work with files across local, FTP, or SFTP: install `Linger.FileSystem`
- Export/import spreadsheets: install `Linger.Excel` plus a provider package as needed
- Integrate LDAP authentication: install `Linger.Ldap.ActiveDirectory` or `Linger.Ldap.Novell`

## Basic Usage

```csharp
using Linger.Extensions.Core;

// String conversion
int result = "123".ToIntOrDefault(0);

// Date operations
int age = birthDate.CalculateAge();

// Result pattern
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    return user == null 
        ? Result<User>.NotFound($"User {id} not found")
        : Result<User>.Success(user);
}
```

## Core Packages

| Package | Description |
|---------|-------------|
| **Linger.Utils** | Core utilities with extension methods and helpers |
| **Linger.Results** | Functional result handling library |
| **Linger.AspNetCore.Jwt** | JWT authentication service |
| **Linger.DataAccess** | Database abstraction layer (SQL Server, Oracle, SQLite) |
| **Linger.EFCore** | Entity Framework Core extensions |
| **Linger.FileSystem** | Unified file system abstraction (Local, FTP, SFTP) |
| **Linger.Email** | Email sending functionality |
| **Linger.Excel** | Excel operations (EPPlus, NPOI, ClosedXML) |
| **Linger.Dapper** | Dapper extensions |
| **Linger.Ldap** | LDAP authentication (Active Directory, Novell) |

## Documentation

- [Linger.Utils Documentation](src/Linger/README.md)
- [Linger.Results Documentation](src/Linger.Results/README.md)
- [Linger.AspNetCore.Jwt Documentation](src/Linger.AspNetCore.Jwt/README.md)
- [Linger.DataAccess Documentation](src/Linger.DataAccess/README.md)
- [Linger.EFCore Documentation](src/Linger.EFCore/README.md)
- [Linger.FileSystem Documentation](src/Linger.FileSystem/README.md)
- [Linger.Email Documentation](src/Linger.Email/README.md)
- [Linger.Excel Documentation](src/Linger.Excel/README.md)
- [Linger.Dapper Documentation](src/Linger.Dapper/README.md)
- [Linger.Ldap.ActiveDirectory Documentation](src/Linger.Ldap.ActiveDirectory/README.md)
- [Linger.Ldap.Novell Documentation](src/Linger.Ldap.Novell/README.md)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.