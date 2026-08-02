# Linger.DataAccess.SqlServer

[2.0 迁移指南](../Linger.DataAccess/MIGRATION.zh-CN.md)

`Linger.DataAccess` 的 SQL Server 实现，并提供可选的 `IBulkInsert` 批量写入能力。

## 安装

```powershell
dotnet add package Linger.DataAccess.SqlServer
```

## 使用

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

`BulkInsert` 会校验并逐段引用带 schema 的表名。`GetMaxId` 只适合单写入者场景；并发下需要保证唯一性时，
请使用 `IDENTITY` 或 `SEQUENCE`。
