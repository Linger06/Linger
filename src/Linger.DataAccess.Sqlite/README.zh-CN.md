# Linger.DataAccess.Sqlite

[2.0 迁移指南](../Linger.DataAccess/MIGRATION.zh-CN.md)

`Linger.DataAccess` 的 SQLite 实现，提供文件数据库创建、表结构查看、文件大小查询和同步数据库备份。

## 安装

```powershell
dotnet add package Linger.DataAccess.Sqlite
```

## 使用

```csharp
using Linger.DataAccess.Sqlite;
using System.Data.SQLite;

using var database = SqliteHelper.CreateFileDatabase("app.db");

DataTable users = database.QueryTable(
    "SELECT * FROM users WHERE age > @age",
    new SQLiteParameter("@age", 18));

List<string> tables = await database.GetTableNamesAsync(cancellationToken);
bool hasUsers = await database.TableExistsAsync("users", cancellationToken);
long bytes = database.GetDatabaseSize();

database.BackupDatabase("app.backup.db");
```

`System.Data.SQLite` 的数据库备份调用是同步 API，因此本包不提供名不副实的 `BackupDatabaseAsync`。
多语句修改请使用核心库的参数化 `ExecuteTransaction` API。
