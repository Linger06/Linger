# Linger.DataAccess 1.x → 2.0 迁移指南

2.0 是一次有意的 API 收敛：删除自定义 Provider 包装和伪异步物化 API，批量事务默认参数化，并将
数据库特有能力移出 `IDatabase`。

## Provider 构造方式

`IProvider` 与三个自定义 Provider 类已删除，改为直接传入驱动标准的 `DbProviderFactory`。

```csharp
// 1.x
var database = new Database(new SqlServerProvider(), connectionString);

// 2.0
var database = new Database(SqlClientFactory.Instance, connectionString);
```

`BaseDatabase` 已改为抽象类。业务代码应实例化 `Database` 或具体数据库 Helper。

## 查询 API

`Query` 返回包含全部结果集的 `DataSet`；`QueryTable` 返回首个结果集的 `DataTable`。由于 BCL 的填充
API 是同步的，旧的异步 `DataSet`/`DataTable` API 已删除。

```csharp
DataTable table = database.QueryTable(sql, parameters);

List<User> users = await database.FindListBySqlAsync(
    sql,
    record => new User(record.GetInt32(0), record.GetString(1)),
    parameters,
    cancellationToken);
```

`ExecuteReader` / `ExecuteReaderAsync` 已删除。这两个方法在内部创建 Connection 和 Command，却只向调用方
返回 Reader，无法完整表达资源所有权。需要流式读取时，请直接使用数据库 Provider 的 ADO.NET API，并由
调用方分别释放 Connection、Command 和 Reader。高层查询方法仍在内部完成全部资源管理。

原来供外部派生类使用的 `protected ExecuteReader<TResult>` / `ExecuteReaderAsync<TResult>` 扩展点也已删除，
这是破坏性调整。派生类应改用现有高层查询 API；确实需要流式读取时，直接使用 Provider 的 ADO.NET API。

Provider 原生流式读取无法加入由 `database.BeginTrans()` 创建的环境事务，因为该事务及其 Connection 由
`database` 实例在内部管理。需要在事务中流式读取时，调用方必须统一创建、关联并释放 Provider 的
Connection、Transaction、Command 和 Reader。

## 存在性检查

`Exists` 和 `ExistsAsync` 已删除。它们既重复了计数方法，又容易与“查询是否返回行”混淆。

```csharp
// 查询是否返回行
bool hasRow = database.HasRows("SELECT 1 FROM T WHERE Id = @id", idParameter);

// 数值计数是否大于 0
bool hasCount = database.FindCountBySql("SELECT COUNT(*) FROM T WHERE Id = @id", idParameter) > 0;
```

## 参数化批量事务

`ExecuteTransaction` 不再接受裸 SQL 字符串，每条语句必须携带自己的参数数组。

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

无参数语句直接调用 `new SqlStatement(sql)` 并省略参数即可；显式传入 null 参数数组会被拒绝。

## SQL Server

- 删除 `ExecuteSql`、`GetSingle` 和 Provider 特有的 `Query(string, SqlParameter[])` 过渡重载。
- `AddByBulkCopy` / `AddByBulkCopyAsync` 替换为 `BulkInsert` / `BulkInsertAsync`。
- `SqlServerHelper` 实现 `IBulkInsert`；`IDatabase` 不承诺批量复制能力。
- `BulkInsert` 接收 `batchSize`、`timeout` 并返回实际写入行数。
- `GetMaxId` / `GetMaxIdAsync` 返回 `int`，限定名会正确引用为 `[schema].[table]`，空表返回 `1`。
- `TableExists("schema.Table")` 会同时匹配 schema 与表名。

```csharp
if (database is IBulkInsert bulkInsert)
{
    int written = await bulkInsert.BulkInsertAsync(
        table,
        "dbo.Users",
        batchSize: 2000,
        timeout: 120,
        cancellationToken);
}
```

## SQLite

Provider Helper 只保留行为明确的 SQLite 特有操作：

- `CreateFileDatabase`
- `GetDatabaseSize`
- `GetTableNames` / `GetTableNamesAsync`
- `TableExists` / `TableExistsAsync`
- `BackupDatabase`（同步）

`VacuumDatabase`、`AnalyzeDatabase`、`CheckIntegrity`、`BackupDatabaseAsync` 和 Provider 特有事务包装已删除。
需要时可通过核心执行 API 运行 `VACUUM`、`ANALYZE` 或 `PRAGMA integrity_check`；多语句修改使用核心的
参数化事务 API。

## 其他删除与调整

- 删除 `SqlBuilder`。
- 删除原始字符串分批查询和异步 `DataTable` 分批查询 API。
- `CommandTimeout` 设置为负数时会立即抛出异常。
