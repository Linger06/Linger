# Linger.DataAccess.Sqlite

[中文](README_zh-CN.md) | English

SQLite database access library with SQLite-specific optimizations and factory methods.

## Features

- Factory methods for easy database creation
- Security-first: Parameterized queries prevent SQL injection
- SQLite optimizations: VACUUM, ANALYZE, and performance features
- Async support: Full async/await with CancellationToken
- Database management: Backup, restore, and schema operations
- Supports core batch query helpers from Linger.DataAccess (see core README)

## Basic Usage

```csharp
using Linger.DataAccess.Sqlite;

var fileDb = SqliteHelper.CreateFileDatabase("myapp.db");

// Parameterized query
var users = fileDb.Query("SELECT * FROM users WHERE age > @age", new SQLiteParameter("@age", 18));

// Batch query (delegates to core implementation)
var ids = Enumerable.Range(1, 3000).Select(i => i.ToString()).ToList();
var dt = fileDb.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", ids); // default batchSize=1000

// SQLite specific maintenance
await fileDb.VacuumDatabaseAsync();
await fileDb.AnalyzeDatabaseAsync();
```

## SQLite Specific APIs

```csharp
await sqlite.VacuumDatabaseAsync();
await sqlite.AnalyzeDatabaseAsync();
await sqlite.BackupDatabaseAsync("backup.db");
var tables = await sqlite.GetTableNamesAsync();
bool exists = await sqlite.TableExistsAsync("users");
```

## Best Practices

- Prefer parameterized queries
- Run VACUUM / ANALYZE periodically
- Adjust batchSize only if needed for extremely large IN lists
- Use transactions for multi-statement changes
