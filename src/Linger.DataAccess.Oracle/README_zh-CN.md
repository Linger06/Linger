# Linger.DataAccess.Oracle

[English](README.md) | 中文

## 概述

Linger.DataAccess.Oracle 是一个安全且功能丰富的 Oracle 数据库访问库，专为企业级应用程序设计。通过参数化查询提供增强的安全性，防止 SQL 注入攻击，并提供具有异步支持的全面数据库操作。

## 特性

- **🔒 安全优先**: 所有查询都使用参数化语句防止 SQL 注入
- **⚡ 高性能**: 智能分页批处理（每批 1000 项）
- **🔄 异步支持**: 完整的 async/await 支持，包含 CancellationToken
- **🎯 多框架**: 支持 .NET 9.0、.NET 8.0 和 .NET Framework 4.6.2


## 快速开始

```csharp
using Linger.DataAccess.Oracle;

// 初始化 Oracle 助手
var oracle = new OracleHelper("Data Source=localhost:1521/XE;User Id=hr;Password=password;");

// 安全的参数化查询
var users = await oracle.QueryAsync<User>("SELECT * FROM users WHERE department = :dept", 
    new OracleParameter(":dept", "IT"));

// 自动分页的批处理
var userIds = new List<string> { "1", "2", "3", ..., "5000" }; // 大列表
var results = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", userIds);

// 安全检查存在性
bool exists = await oracle.ExistsAsync("SELECT 1 FROM users WHERE email = :email",
    new OracleParameter(":email", "user@example.com"));
```

## 核心方法

### 批处理操作
- `QueryInBatches(sql, parameters)` - 大参数列表的智能批处理
- `QueryInBatchesAsync(sql, parameters, cancellationToken)` - 异步批处理

### 存在性检查
- `Exists(sql, parameters)` - 参数化存在性验证
- `ExistsAsync(sql, parameters, cancellationToken)` - 异步存在性检查

### 查询操作
- `Query<T>(sql, parameters)` - 强类型参数化查询
- `QueryAsync<T>(sql, parameters, cancellationToken)` - 异步类型化查询

## 安全特性

### SQL 注入防护
```csharp
// ❌ 有漏洞（旧方法）
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ 安全（参数化）
var sql = "SELECT * FROM users WHERE name = :userName";
var result = oracle.Query<User>(sql, new OracleParameter(":userName", userName));
```

### 参数验证
所有方法都包含全面的参数验证：
- 空引用检查
- 空字符串验证
- 类型安全强制

## 性能优化

### 自动批处理
大参数列表自动分割为 1000 项的批次：

```csharp
// 高效处理 10,000+ ID
var massiveIdList = Enumerable.Range(1, 10000).Select(i => i.ToString()).ToList();
var results = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", massiveIdList);
// 自动分为 10 批，每批 1000 项
```

### 异步操作
所有数据库操作都支持取消：

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var users = await oracle.QueryAsync<User>("SELECT * FROM users", cancellationToken: cts.Token);
```

## 依赖项

- **Oracle.ManagedDataAccess.Core** 23.9.1 (.NET 8.0, .NET 9.0)
- **Oracle.ManagedDataAccess** 21.19.0 (.NET Framework 4.6.2)
- **Linger.DataAccess** (核心抽象)


## 最佳实践

1. **始终使用参数化查询**
2. **实现适当的释放模式**
3. **对 I/O 操作使用异步方法**
4. **适当处理取消令牌**
5. **在数据库调用前验证输入**

## 许可证

本项目是 Linger 框架生态系统的一部分。
