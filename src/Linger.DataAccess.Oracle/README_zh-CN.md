# Linger.DataAccess.Oracle

[English](README.md) | 中文

一个安全且功能丰富的 Oracle 数据库访问库，专为企业级应用程序设计。

## 特性

- **🔒 安全优先**: 参数化查询防止 SQL 注入
- **⚡ 高性能**: 智能分页批处理（每批 1000 项）
- **🔄 异步支持**: 完整的 async/await 支持
- **🎯 多框架**: 支持 .NET 9.0、.NET 8.0 和 .NET Framework 4.6.2


## 基本用法

```csharp
using Linger.DataAccess.Oracle;

// 初始化 Oracle 助手
var oracle = new OracleHelper("Data Source=localhost:1521/XE;User Id=hr;Password=password;");

// 参数化查询
var users = await oracle.QueryAsync<User>("SELECT * FROM users WHERE department = :dept", 
    new OracleParameter(":dept", "IT"));

// 批处理大列表
var userIds = new List<string> { "1", "2", "3", ..., "5000" };
var results = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", userIds);

// 存在性检查
bool exists = await oracle.ExistsAsync("SELECT 1 FROM users WHERE email = :email",
    new OracleParameter(":email", "user@example.com"));
```

## 核心方法

### QueryInBatchesAsync
```csharp
public async Task<IEnumerable<T>> QueryInBatchesAsync<T>(string sql, IEnumerable<string> parameters, 
    CancellationToken cancellationToken = default)
```
大参数列表的智能批处理查询。

### ExistsAsync
```csharp
public async Task<bool> ExistsAsync(string sql, params OracleParameter[] parameters, 
    CancellationToken cancellationToken = default)
```
参数化存在性验证。

## 安全特性

```csharp
// ❌ 不安全
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ 安全
var sql = "SELECT * FROM users WHERE name = :userName";
var result = oracle.Query<User>(sql, new OracleParameter(":userName", userName));
```

## 最佳实践

- 始终使用参数化查询
- 对 I/O 操作使用异步方法
- 适当处理取消令牌
- 在数据库调用前验证输入
