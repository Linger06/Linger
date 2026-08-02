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
