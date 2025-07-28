# Linger.DataAccess.Sqlite

[English](README.md) | 中文

## 概述

Linger.DataAccess.Sqlite 是一个全面的 SQLite 数据库访问库，充分利用 SQLite 特有功能，同时提供安全、高性能的数据库操作。它提供工厂方法、高级 SQLite 功能以及对现代 .NET 应用程序的完整异步支持。

## 特性

- **🏭 工厂方法**: 轻松创建内存、文件和临时数据库
- **🔒 安全优先**: 所有查询都使用参数化语句防止 SQL 注入
- **⚡ SQLite 优化**: WAL 模式、VACUUM、ANALYZE 和 SQLite 特有功能
- **🔄 异步支持**: 完整的 async/await 支持，包含 CancellationToken
- **🎯 多框架**: 支持 .NET 9.0、.NET 8.0 和 .NET Framework 4.6.2
- **📊 数据库管理**: 备份、还原、表操作和架构管理
- **🧪 充分测试**: 全面的单元测试覆盖（45+ 测试方法）

## 安装

```xml
<PackageReference Include="Linger.DataAccess.Sqlite" Version="0.8.0-preview" />
```

## 快速开始

```csharp
using Linger.DataAccess.Sqlite;

// 创建不同类型的数据库
var inMemoryDb = SqliteHelper.CreateInMemory();
var fileDb = SqliteHelper.CreateFileDatabase("myapp.db");
var tempDb = SqliteHelper.CreateTemporary();

// 安全的参数化查询
var users = await fileDb.QueryAsync<User>("SELECT * FROM users WHERE age > @age", 
    new SQLiteParameter("@age", 18));

// 自动分页的批处理
var userIds = new List<string> { "1", "2", "3", ..., "5000" };
var results = fileDb.Page("SELECT * FROM users WHERE id IN ({0})", userIds);

// SQLite 特有操作
await fileDb.EnableWalModeAsync();
await fileDb.VacuumAsync();
await fileDb.AnalyzeAsync();
```

## 工厂方法

### 数据库创建
```csharp
// 内存数据库（最快，临时）
var memDb = SqliteHelper.CreateInMemory();

// 文件数据库（持久化）
var fileDb = SqliteHelper.CreateFileDatabase("app.db", createIfNotExists: true);

// 临时数据库（自动清理）
var tempDb = SqliteHelper.CreateTemporary();
```

## 核心方法

### 批处理操作
- `Page(sql, parameters)` - 大参数列表的智能批处理
- `PageAsync(sql, parameters, cancellationToken)` - 异步批处理

### 存在性检查
- `Exists(sql, parameters)` - 参数化存在性验证
- `ExistsAsync(sql, parameters, cancellationToken)` - 异步存在性检查

### 查询操作
- `Query<T>(sql, parameters)` - 强类型参数化查询
- `QueryAsync<T>(sql, parameters, cancellationToken)` - 异步类型化查询

## SQLite 特有功能

### 性能优化
```csharp
// 启用 WAL（预写日志）模式以获得更好的并发性
await sqlite.EnableWalModeAsync();

// 优化数据库性能
await sqlite.VacuumAsync();    // 回收空间并整理碎片
await sqlite.AnalyzeAsync();   // 更新查询规划器统计信息

// 设置性能 pragma
await sqlite.SetPragmaAsync("cache_size", "-64000");  // 64MB 缓存
await sqlite.SetPragmaAsync("temp_store", "MEMORY");   // 临时表存储在内存
```

### 数据库管理
```csharp
// 备份和还原
await sqlite.BackupAsync("backup.db");
await sqlite.RestoreAsync("backup.db");

// 表操作
var tables = await sqlite.GetTableNamesAsync();
bool exists = await sqlite.TableExistsAsync("users");
await sqlite.DropTableAsync("temp_table");

// 架构信息
var columns = await sqlite.GetTableSchemaAsync("users");
var indexes = await sqlite.GetIndexesAsync("users");
```

### 事务支持
```csharp
// 自动事务管理
await sqlite.ExecuteInTransactionAsync(async (transaction) =>
{
    await sqlite.ExecuteNonQueryAsync("INSERT INTO users (name) VALUES (@name)", 
        new SQLiteParameter("@name", "John"), transaction);
    await sqlite.ExecuteNonQueryAsync("INSERT INTO logs (action) VALUES (@action)", 
        new SQLiteParameter("@action", "User created"), transaction);
});
```

## 安全特性

### SQL 注入防护
```csharp
// ❌ 有漏洞（旧方法）
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ 安全（参数化）
var sql = "SELECT * FROM users WHERE name = @userName";
var result = sqlite.Query<User>(sql, new SQLiteParameter("@userName", userName));
```

### 参数验证
所有方法都包含全面的参数验证：
- 空引用检查
- 空字符串验证
- 类型安全强制
- 连接状态验证

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
await sqlite.ExecuteInTransactionAsync(async (transaction) =>
{
    foreach (var user in users)
    {
        await sqlite.ExecuteNonQueryAsync(
            "INSERT INTO users (name, email) VALUES (@name, @email)",
            new[] { 
                new SQLiteParameter("@name", user.Name),
                new SQLiteParameter("@email", user.Email) 
            }, transaction);
    }
});
```

## 依赖项

- **System.Data.SQLite.Core** 1.0.119 (所有框架)
- **Linger.DataAccess** (核心抽象)

## 测试

包含 45+ 测试方法的全面单元测试套件，覆盖：
- 工厂方法创建
- SQL 注入防护
- SQLite 特有功能
- 性能优化
- 事务管理
- 错误处理

```bash
dotnet test Linger.DataAccess.Sqlite.UnitTests
```

## 高级功能

### 自定义函数
```csharp
// 注册自定义 SQLite 函数
sqlite.RegisterFunction("REGEXP", (pattern, input) => 
    Regex.IsMatch(input.ToString(), pattern.ToString()));

// 在查询中使用
var results = await sqlite.QueryAsync<User>(
    "SELECT * FROM users WHERE REGEXP(@pattern, email)",
    new SQLiteParameter("@pattern", @".*@company\.com$"));
```

### 全文搜索
```csharp
// 创建 FTS 表
await sqlite.ExecuteNonQueryAsync(@"
    CREATE VIRTUAL TABLE documents_fts USING fts5(title, content);
");

// 使用 FTS 搜索
var documents = await sqlite.QueryAsync<Document>(
    "SELECT * FROM documents_fts WHERE documents_fts MATCH @query",
    new SQLiteParameter("@query", "database AND sqlite"));
```

## 最佳实践

1. **根据需求使用适当的数据库类型**
   - 内存: 测试、缓存、临时数据
   - 文件: 持久化应用程序数据
   - 临时: 会话特定数据

2. **为并发访问启用 WAL 模式**
3. **对批量操作使用事务**
4. **定期 VACUUM 和 ANALYZE 进行维护**
5. **实现适当的释放模式**
6. **对 I/O 操作使用异步方法**

## 许可证

本项目是 Linger 框架生态系统的一部分。
