# Linger.DataAccess.Sqlite

一个全面的 SQLite 数据库访问库，提供安全、高性能的数据库操作。

## 特性

- 🏭 工厂方法：轻松创建内存、文件和临时数据库
- 🔒 安全优先：参数化查询防止 SQL 注入
- ⚡ SQLite 优化：VACUUM / ANALYZE 等特性
- 🔄 异步支持：完整 async/await 支持
- 🎯 多框架：支持 .NET 9.0 / 8.0 / .NET Framework 4.6.2
- ✅ 支持核心库提供的分批查询方法（详见 Linger.DataAccess README）

## 基本用法

```csharp
using Linger.DataAccess.Sqlite;

var fileDb = SqliteHelper.CreateFileDatabase("myapp.db");

// 参数化查询
var users = fileDb.Query("SELECT * FROM users WHERE age > @age", new SQLiteParameter("@age", 18));

// 分批查询（调用核心实现）
var ids = Enumerable.Range(1, 3000).Select(i => i.ToString()).ToList();
var dt = fileDb.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", ids); // 默认 batchSize=1000

// SQLite 优化
await fileDb.VacuumDatabaseAsync();
await fileDb.AnalyzeDatabaseAsync();
```

## SQLite 特有功能

```csharp
await sqlite.VacuumDatabaseAsync();
await sqlite.AnalyzeDatabaseAsync();
await sqlite.BackupDatabaseAsync("backup.db");
var tables = await sqlite.GetTableNamesAsync();
bool exists = await sqlite.TableExistsAsync("users");
```

## 最佳实践

- 优先使用参数化查询
- 定期执行 VACUUM / ANALYZE
- 仅在极大 IN 列表时考虑调整 batchSize
- 多语句写操作使用事务
