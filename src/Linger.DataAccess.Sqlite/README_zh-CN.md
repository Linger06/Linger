# Linger.DataAccess.Sqlite

[English](README.md) | 中文

一个全面的 SQLite 数据库访问库，提供安全、高性能的数据库操作。

## 特性

- **🏭 工厂方法**: 轻松创建内存、文件和临时数据库
- **🔒 安全优先**: 参数化查询防止 SQL 注入
- **⚡ SQLite 优化**: SQLite 特有功能
- **🔄 异步支持**: 完整的 async/await 支持
- **🎯 多框架**: 支持 .NET 9.0、.NET 8.0 和 .NET Framework 4.6.2

## 基本用法

```csharp
using Linger.DataAccess.Sqlite;

// 创建文件数据库
var fileDb = SqliteHelper.CreateFileDatabase("myapp.db");

// 参数化查询
var users = fileDb.Query("SELECT * FROM users WHERE age > @age", 
    new SQLiteParameter("@age", 18));

// 批处理查询
var userIds = new List<string> { "1", "2", "3", ..., "5000" };
var results = fileDb.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", userIds);

// SQLite 优化
await fileDb.VacuumDatabaseAsync();
await fileDb.AnalyzeDatabaseAsync();
```

## 核心方法

### 工厂方法
```csharp
public static SqliteHelper CreateFileDatabase(string filePath, bool createIfNotExists = true)
```
创建文件数据库实例。

### QueryInBatchesAsync
```csharp
public async Task<DataSet> QueryInBatchesAsync(string sql, IEnumerable<string> parameters, 
    CancellationToken cancellationToken = default)
```
大参数列表的智能批处理查询。

### SQLite 特有功能
```csharp
// 性能优化
await sqlite.VacuumDatabaseAsync();
await sqlite.AnalyzeDatabaseAsync();

// 数据库管理
await sqlite.BackupDatabaseAsync("backup.db");
var tables = await sqlite.GetTableNamesAsync();
bool exists = await sqlite.TableExistsAsync("users");

// 事务支持
await sqlite.ExecuteInTransactionAsync(new[]
{
    "INSERT INTO users (name) VALUES ('John')",
    "INSERT INTO logs (action) VALUES ('User created')"
});
```

## 安全特性

```csharp
// ❌ 不安全
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ 安全
var sql = "SELECT * FROM users WHERE name = @userName";
var result = sqlite.Query(sql, new SQLiteParameter("@userName", userName));
```

## 最佳实践

- 始终使用参数化查询
- 对文件数据库使用连接池
- 定期执行 VACUUM 和 ANALYZE 优化性能
- 使用事务进行批量操作
