# Linger.DataAccess.Sqlite

[English](README.md) | 中文

## 概述

Linger.DataAccess.Sqlite 是一个全面的 SQLite 数据库访问库，充分利用 SQLite 特有功能，同时提供安全、高性能的数据库操作。它提供工厂方法、高级 SQLite 功能以及对现代 .NET 应用程序的完整异步支持。

## 特性

- **🏭 工厂方法**: 轻松创建内存、文件和临时数据库
- **🔒 安全优先**: 所有查询都使用参数化语句防止 SQL 注入
- **⚡ SQLite 优化**: SQLite 特有功能
- **🔄 异步支持**: 完整的 async/await 支持，包含 CancellationToken
- **🎯 多框架**: 支持 .NET 9.0、.NET 8.0 和 .NET Framework 4.6.2
- **📊 数据库管理**: 备份、还原、表操作和架构管理

## 快速开始

```csharp
using Linger.DataAccess.Sqlite;

var fileDb = SqliteHelper.CreateFileDatabase("myapp.db");

// 安全的参数化查询
var users = fileDb.Query("SELECT * FROM users WHERE age > @age", 
    new SQLiteParameter("@age", 18));

// 自动分页的批处理
var userIds = new List<string> { "1", "2", "3", ..., "5000" };
var results = fileDb.Page("SELECT * FROM users WHERE id IN ({0})", userIds);

// SQLite 特有操作
await fileDb.VacuumDatabaseAsync();
await fileDb.AnalyzeDatabaseAsync();
```

## 工厂方法

### 数据库创建
```csharp
// 文件数据库（持久化）
var fileDb = SqliteHelper.CreateFileDatabase("app.db", createIfNotExists: true);
```

## 核心方法

### 批处理操作
- `Page(sql, parameters)` - 大参数列表的智能批处理
- `PageAsync(sql, parameters, cancellationToken)` - 异步批处理

### 存在性检查
- `Exists(sql, parameters)` - 参数化存在性验证
- `ExistsAsync(sql, parameters, cancellationToken)` - 异步存在性检查

### 查询操作
- `Query(sql, parameters)` - 参数化查询返回DataSet
- `QueryAsync(sql, parameters, cancellationToken)` - 异步查询返回DataSet

## SQLite 特有功能

### 性能优化
```csharp
// 优化数据库性能
await sqlite.VacuumDatabaseAsync();
await sqlite.AnalyzeDatabaseAsync();
```

### 数据库管理
```csharp
// 备份和还原
await sqlite.BackupDatabaseAsync("backup.db");

// 表操作
var tables = await sqlite.GetTableNamesAsync();
bool exists = await sqlite.TableExistsAsync("users");

// 完整性检查
var integrityResult = await sqlite.CheckIntegrityAsync();
```

### 事务支持
```csharp
// 自动事务管理
await sqlite.ExecuteInTransactionAsync(new[]
{
    "INSERT INTO users (name) VALUES ('John')",
    "INSERT INTO logs (action) VALUES ('User created')"
});
```

## 安全特性

### SQL 注入防护
```csharp
// ❌ 有漏洞（旧方法）
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ 安全（参数化）
var sql = "SELECT * FROM users WHERE name = @userName";
var result = sqlite.Query(sql, new SQLiteParameter("@userName", userName));
```

## 性能最佳实践

### 连接池
```csharp
// 对文件数据库使用连接池
var connectionString = "Data Source=app.db;Pooling=true;Max Pool Size=100;";
var sqlite = new SqliteHelper(connectionString);
```

### 批量插入
```csharp
// 高效的批量插入
var users = GenerateTestUsers(10000);
await sqlite.ExecuteInTransactionAsync(users.Select(user => 
    $"INSERT INTO users (name, email) VALUES ('{user.Name}', '{user.Email}')"));
```

## 高级功能

### 数据库管理
```csharp
// 获取数据库大小
var size = await sqlite.GetDatabaseSizeAsync();

// 检查数据库完整性
var integrityResult = await sqlite.CheckIntegrityAsync();

// 获取所有表名
var tableNames = await sqlite.GetTableNamesAsync();
```
