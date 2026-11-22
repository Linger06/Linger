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
`BulkInsert` 按 `DataTable` 列名映射目标列，并且不允许在 `BeginTrans` 开启的环境事务中执行。

`BulkInsert` 和 `BulkInsertAsync` 使用独立连接，默认每批发送 1000 行。成功返回的行数不含 `Deleted` 行；全为删除行时无需打开连接，直接返回 0。批量插入不保证整次导入全部成功或全部回滚。如果后续批次失败，前面批次可能已提交。重试前请按业务键核对目标表；直接重跑全部数据可能产生重复记录。如果需要整次回滚，请直接使用 `SqlBulkCopy`，并传入同一连接上的显式 `SqlTransaction`。
