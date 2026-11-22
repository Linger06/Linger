# Linger.DataAccess.SqlServer

[Migration guide for 2.0](../Linger.DataAccess/MIGRATION.md)

SQL Server provider for `Linger.DataAccess`, including the optional `IBulkInsert` capability.

## Installation

```powershell
dotnet add package Linger.DataAccess.SqlServer
```

## Usage

```csharp
using Linger.DataAccess;
using Linger.DataAccess.SqlServer;
using Microsoft.Data.SqlClient;

using var database = new SqlServerHelper(connectionString);

bool hasUser = await database.HasRowsAsync(
    "SELECT 1 FROM dbo.Users WHERE Email = @email",
    [new SqlParameter("@email", email)],
    cancellationToken);

int nextId = await database.GetMaxIdAsync("UserId", "dbo.Users", cancellationToken);

IBulkInsert bulkInsert = database;
int written = await bulkInsert.BulkInsertAsync(
    dataTable,
    "dbo.Users",
    batchSize: 2000,
    timeout: 120,
    cancellationToken);
```

`BulkInsert` validates and quotes each part of a qualified table name. `GetMaxId` is intended only for
single-writer scenarios; use `IDENTITY` or `SEQUENCE` when uniqueness must be guaranteed concurrently.
`BulkInsert` maps destination columns by `DataTable` column name and rejects execution inside a transaction opened
with `BeginTrans`.

`BulkInsert` and `BulkInsertAsync` use a separate connection and send batches of 1,000 rows by default. The success count excludes `Deleted` rows; an all-deleted input returns zero without opening a connection. They do not guarantee that the whole import is atomic: if a later batch fails, earlier batches may already be committed. Reconcile the destination by business key before retrying; replaying the entire input can duplicate rows. For an all-or-nothing import, use `SqlBulkCopy` with an explicit `SqlTransaction` on the same connection.
