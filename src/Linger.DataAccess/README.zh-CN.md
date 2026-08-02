# Linger.DataAccess

[2.0 迁移指南](MIGRATION.zh-CN.md)

提供与数据库驱动无关的 ADO.NET 命令执行、连接/事务生命周期管理和通用查询语义。

## 目标框架

- .NET 10
- .NET 9
- .NET 8
- .NET Framework 4.7.2

## 架构

- `BaseDatabase`：命令、连接、事务、取消和参数归属。
- `Database`：DataSet/DataTable、实体与列表映射、计数、分批查询和参数化批量事务。
- 各数据库 Helper：只提供数据库特有能力。

`Database` 直接接收标准 `DbProviderFactory`，不再定义自有 Provider 抽象。

```csharp
using Linger.DataAccess;
using Microsoft.Data.SqlClient;

using var database = new Database(SqlClientFactory.Instance, connectionString);
```

## 查询

```csharp
DataSet allResults = database.Query("SELECT * FROM Users; SELECT * FROM Roles");
DataTable users = database.QueryTable(
    "SELECT * FROM Users WHERE Active = @active",
    new SqlParameter("@active", true));

List<User> mapped = database.FindListBySql<User>("SELECT Id, Name FROM Users");
List<User> asyncMapped = await database.FindListBySqlAsync(
    "SELECT Id, Name FROM Users",
    record => new User(record.GetInt32(0), record.GetString(1)),
    cancellationToken: cancellationToken);
```

由于 BCL 的填充 API 是同步的，`DataSet` 和 `DataTable` 物化只提供同步版本。异步场景请使用
`FindListBySqlAsync`，或通过 `ExecuteReaderAsync` 直接读取。

## 存在性与计数

```csharp
bool hasRow = database.HasRows(
    "SELECT 1 FROM Users WHERE Email = @email",
    new SqlParameter("@email", email));

int count = await database.FindCountBySqlAsync(
    "SELECT COUNT(*) FROM Users WHERE Active = @active",
    [new SqlParameter("@active", true)],
    cancellationToken);
```

判断是否有行使用 `HasRows`；确实需要数值时使用 `FindCountBySql`。

## 参数化批量事务

```csharp
var statements = new[]
{
    new SqlStatement("UPDATE Accounts SET Balance = Balance - @amount WHERE Id = @id",
        new SqlParameter("@amount", 100), new SqlParameter("@id", 1)),
    new SqlStatement("UPDATE Accounts SET Balance = Balance + @amount WHERE Id = @id",
        new SqlParameter("@amount", 100), new SqlParameter("@id", 2)),
};

int[] affected = await database.ExecuteTransactionAsync(statements, cancellationToken);
```

全部语句成功后才提交；任一语句失败会回滚并重新抛出原异常。`BeginTrans` 开启的环境事务生效期间，
不要调用此 API。

## 超大 `IN` 查询

```csharp
DataTable result = database.QueryInBatches(
    "SELECT * FROM Orders WHERE OrderId IN ({0})",
    orderIds,
    batchSize: 500);
```

所有值都会参数化并按批次执行，首批结果决定返回 `DataTable` 的结构。

## 可选能力

数据库特有功能通过能力接口暴露。SQL Server 实现了 `IBulkInsert`：

```csharp
if (database is IBulkInsert bulkInsert)
{
    await bulkInsert.BulkInsertAsync(table, "dbo.Users", cancellationToken: cancellationToken);
}
```
