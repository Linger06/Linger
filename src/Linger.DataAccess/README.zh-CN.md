# Linger.DataAccess

`Linger.DataAccess` 是基于 ADO.NET 的轻量数据访问组件，提供 SQL 与存储过程执行、同步/异步查询、对象映射、
事务和参数化分批查询。它不依赖 ORM，所有 SQL 和参数均由调用方明确提供。

## 安装

通常只需安装对应数据库的实现包，它会同时引入核心包：

| 数据库 | NuGet 包 | 入口类型 |
| --- | --- | --- |
| SQL Server | `Linger.DataAccess.SqlServer` | `SqlServerHelper` |
| SQLite | `Linger.DataAccess.Sqlite` | `SqliteHelper` |
| Oracle | `Linger.DataAccess.Oracle` | `OracleHelper` |
| 其他 ADO.NET Provider | `Linger.DataAccess` | `Database` |

以 SQL Server 为例：

```powershell
dotnet add package Linger.DataAccess.SqlServer
```

## 快速开始

```csharp
using System.Data;
using Linger.DataAccess.SqlServer;
using Microsoft.Data.SqlClient;

using var database = new SqlServerHelper(connectionString)
{
    CommandTimeout = 30,
};

DataTable users = database.QueryTable(
    "SELECT Id, Name FROM dbo.Users WHERE Active = @active",
    new SqlParameter("@active", true));
```

使用其他 Provider 时，也可以直接传入标准 `DbProviderFactory`：

```csharp
using Linger.DataAccess;
using Microsoft.Data.SqlClient;

using var database = new Database(SqlClientFactory.Instance, connectionString);
```

所有数据库参数都使用对应驱动提供的 `DbParameter`，不要把值拼接进 SQL。

## 功能概览

| 场景 | 主要 API |
| --- | --- |
| 查询 `DataSet` / `DataTable` | `Query`、`QueryTable` |
| 查询列表或单个对象 | `FindListBySql`、`FindEntityBySql` |
| 判断是否有数据或查询数量 | `HasRows`、`FindCountBySql` |
| 执行 SQL 或存储过程 | `ExecuteBySql`、`ExecuteByProc` |
| 流式读取 | 直接使用数据库 Provider 的 ADO.NET API |
| 执行底层 ADO.NET 命令 | `ExecuteNonQuery`、`ExecuteScalar`、`GetDataSet` |
| 管理环境事务 | `BeginTrans`、`Commit`、`Rollback` |
| 在独立事务中批量执行 SQL | `ExecuteTransaction` |
| 拆分超长 `IN` 参数 | `QueryInBatches` |

列表查询、存在性检查、计数、SQL/存储过程执行和环境事务均提供带 `CancellationToken` 的异步版本。

## 查询结果集

`Query` 返回全部结果集，`QueryTable` 只返回第一个结果集：

```csharp
DataSet allResults = database.Query(
    "SELECT * FROM dbo.Users; SELECT * FROM dbo.Roles");

DataTable users = database.QueryTable(
    "SELECT Id, Name FROM dbo.Users WHERE Active = @active",
    new SqlParameter("@active", true));
```

由于 ADO.NET 的 `DataSet`、`DataTable` 填充 API 是同步的，这两个物化方法只提供同步版本。异步查询可使用
`FindListBySqlAsync`；需要流式读取时直接使用数据库 Provider 的 ADO.NET API。

## 查询对象

按查询列名和公开可写属性名自动映射：

```csharp
List<User> users = await database.FindListBySqlAsync<User>(
    "SELECT Id, Name FROM dbo.Users WHERE Active = @active",
    [new SqlParameter("@active", true)],
    cancellationToken);

User? user = database.FindEntityBySql<User>(
    "SELECT Id, Name FROM dbo.Users WHERE Id = @id",
    new SqlParameter("@id", userId));
```

需要控制字段转换，或项目启用了 Native AOT / trimming 时，使用显式映射函数：

```csharp
List<User> users = await database.FindListBySqlAsync(
    "SELECT Id, Name FROM dbo.Users",
    record => new User(record.GetInt32(0), record.GetString(1)),
    cancellationToken: cancellationToken);
```

## 判断存在和计数

只关心是否存在记录时使用 `HasRows`，它读取第一行后立即停止；需要实际数量时使用 `FindCountBySql`：

```csharp
bool exists = await database.HasRowsAsync(
    "SELECT 1 FROM dbo.Users WHERE Email = @email",
    [new SqlParameter("@email", email)],
    cancellationToken);

int count = await database.FindCountBySqlAsync(
    "SELECT COUNT(*) FROM dbo.Users WHERE Active = @active",
    [new SqlParameter("@active", true)],
    cancellationToken);
```

## 执行 SQL 和存储过程

```csharp
int affected = await database.ExecuteBySqlAsync(
    "UPDATE dbo.Users SET Name = @name WHERE Id = @id",
    [
        new SqlParameter("@name", name),
        new SqlParameter("@id", userId),
    ],
    cancellationToken);

int procAffected = await database.ExecuteByProcAsync(
    "dbo.DisableUser",
    [new SqlParameter("@id", userId)],
    cancellationToken);

DataTable procResult = database.FindTableByProc(
    "dbo.GetUsers",
    new SqlParameter("@active", true));
```

需要直接使用 `CommandType` 时，可以调用底层 API：

```csharp
object? value = await database.ExecuteScalarAsync(
    CommandType.Text,
    "SELECT MAX(Id) FROM dbo.Users",
    cancellationToken: cancellationToken);
```

## 流式读取

流式读取需要同时管理 Connection、Command 和 Reader，因此直接使用数据库 Provider 的原生 API，
让三个资源的所有权都在调用方作用域内明确可见：

```csharp
using var connection = new SqlConnection(connectionString);
await connection.OpenAsync(cancellationToken);

using SqlCommand command = connection.CreateCommand();
command.CommandText = "SELECT Name FROM dbo.Users WHERE Active = @active";
command.Parameters.Add(new SqlParameter("@active", true));

using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

var names = new List<string>();
while (await reader.ReadAsync(cancellationToken))
{
    names.Add(reader.GetString(0));
}
```

`Linger.DataAccess` 不公开只返回裸 `DbDataReader` 的方法，因为组件在内部创建 Command 和 Connection 后，
仅返回 Reader 无法向调用方完整表达另外两个资源的所有权。`QueryTable`、`FindListBySql` 等高层方法仍由
组件在内部完整释放相关资源。

## 事务

### 环境事务

调用 `BeginTrans` 后，同一个 `database` 实例上不带事务参数的方法会自动加入该事务：

```csharp
database.BeginTrans();
try
{
    database.ExecuteBySql(
        "UPDATE dbo.Accounts SET Balance = Balance - @amount WHERE Id = @id",
        new SqlParameter("@amount", amount),
        new SqlParameter("@id", sourceId));

    database.ExecuteBySql(
        "UPDATE dbo.Accounts SET Balance = Balance + @amount WHERE Id = @id",
        new SqlParameter("@amount", amount),
        new SqlParameter("@id", targetId));

    database.Commit();
}
catch
{
    database.Rollback();
    throw;
}
```

`BeginTransAsync`、`CommitAsync` 和 `RollbackAsync` 提供对应的异步操作。`Commit`、`Rollback` 后连接由组件释放。

### 参数化批量事务

`ExecuteTransaction` 使用独立连接和事务按顺序执行多条语句；全部成功才提交，任一失败则回滚：

```csharp
var statements = new[]
{
    new SqlStatement(
        "UPDATE dbo.Accounts SET Balance = Balance - @amount WHERE Id = @id",
        new SqlParameter("@amount", amount),
        new SqlParameter("@id", sourceId)),
    new SqlStatement(
        "UPDATE dbo.Accounts SET Balance = Balance + @amount WHERE Id = @id",
        new SqlParameter("@amount", amount),
        new SqlParameter("@id", targetId)),
};

int[] affectedRows = await database.ExecuteTransactionAsync(statements, cancellationToken);
```

环境事务生效期间不要调用 `ExecuteTransaction`，否则会抛出 `InvalidOperationException`。

## 超长 IN 查询

`QueryInBatches` 会把取值列表拆分为多批参数化查询，并合并返回结果：

```csharp
DataTable orders = database.QueryInBatches(
    "SELECT * FROM dbo.Orders WHERE OrderId IN ({0})",
    orderIds,
    batchSize: 500);
```

它仅适合可以直接合并的行查询，不支持依赖全局结果集的排序、分页、聚合或去重语义。

## 数据库特有功能

- [SQL Server](../Linger.DataAccess.SqlServer/README.zh-CN.md)：`IBulkInsert` 批量写入、`GetMaxId`。
- [SQLite](../Linger.DataAccess.Sqlite/README.zh-CN.md)：文件数据库创建、表信息、数据库大小和备份。
- [Oracle](../Linger.DataAccess.Oracle/README.zh-CN.md)：Oracle Provider、命名参数绑定和 `:` 参数前缀。

## 目标框架

- .NET 10
- .NET 9
- .NET 8
- .NET Framework 4.7.2

从旧版本升级时，请参阅 [2.0 迁移指南](MIGRATION.zh-CN.md)。
