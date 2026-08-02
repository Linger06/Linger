# Linger.DataAccess.Sqlite

[Migration guide for 2.0](../Linger.DataAccess/MIGRATION.md)

SQLite provider for `Linger.DataAccess` with file-database creation, schema inspection, size reporting, and
synchronous database backup.

## Installation

```powershell
dotnet add package Linger.DataAccess.Sqlite
```

## Usage

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

`System.Data.SQLite` exposes database backup as a synchronous operation, so this package intentionally does not
provide `BackupDatabaseAsync`. Use the core parameterized `ExecuteTransaction` APIs for multi-statement changes.
