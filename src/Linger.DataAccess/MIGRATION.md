# Migrating Linger.DataAccess from 1.x to 2.0

2.0 is a deliberate API reset. It removes provider wrappers and pseudo-async materialization APIs, makes
transactions parameterized by default, and moves provider-specific capabilities out of `IDatabase`.

## Provider construction

`IProvider` and the three custom provider classes were removed. Pass the driver's standard `DbProviderFactory`.

```csharp
// 1.x
var database = new Database(new SqlServerProvider(), connectionString);

// 2.0
var database = new Database(SqlClientFactory.Instance, connectionString);
```

`BaseDatabase` is now abstract. Application code should instantiate `Database` or a provider helper.

## Queries

`Query` returns all result sets as `DataSet`; `QueryTable` returns the first result set as `DataTable`.
The old asynchronous `DataSet`/`DataTable` APIs were removed because the BCL fill APIs are synchronous.

```csharp
DataTable table = database.QueryTable(sql, parameters);

List<User> users = await database.FindListBySqlAsync(
    sql,
    record => new User(record.GetInt32(0), record.GetString(1)),
    parameters,
    cancellationToken);
```

`ExecuteReader` / `ExecuteReaderAsync` were removed. They created a connection and command internally but returned
only the reader, so the API could not express ownership of all three resources. For streaming, use the provider's
native ADO.NET API and dispose the connection, command, and reader explicitly. Higher-level query methods continue
to manage all resources internally.

The `protected ExecuteReader<TResult>` / `ExecuteReaderAsync<TResult>` extension points previously available to
external derived classes were also removed. This is a breaking change. Derived classes should use the existing
higher-level query APIs, or use the provider's native ADO.NET API when streaming is required.

Native provider streaming cannot join the ambient transaction created by `database.BeginTrans()`, because that
transaction and its connection are managed internally by the `database` instance. For transactional streaming,
the caller must create, associate, and dispose the provider connection, transaction, command, and reader together.

`StringBuilder` overloads of `ExecuteBySql` / `ExecuteByProc` were removed; use the `string` overloads (call
`sql.ToString()` first when needed). `FindTableBySql` / `FindDataSetBySql` (synchronous) and
`FindMaxBySql` / `FindMaxBySqlAsync` were removed; use `QueryTable` / `Query` / `FindListBySql`, or write SQL directly.

`IBaseDatabase` no longer exposes `params DbParameter[]` overloads for asynchronous methods.
`ExecuteNonQueryAsync` / `ExecuteScalarAsync` now take a single `DbParameter[]?` parameter array
(the cancellation token remains an optional parameter):

```csharp
await database.ExecuteNonQueryAsync(
    CommandType.Text,
    "UPDATE Accounts SET Balance = @amount WHERE Id = @id",
    new SqlParameter[] { new("@amount", 100), new("@id", 1) },
    cancellationToken);
```

## Existence checks

`Exists` and `ExistsAsync` were removed because they duplicated count operations while overlapping semantically
with row-existence checks.

```csharp
// Does the query return a row?
bool hasRow = database.HasRows("SELECT 1 FROM T WHERE Id = @id", idParameter);

// Is the numeric count greater than zero?
bool hasCount = database.FindCountBySql("SELECT COUNT(*) FROM T WHERE Id = @id", idParameter) > 0;
```

## Parameterized batch transactions

`ExecuteTransaction` no longer accepts raw SQL strings. Every item carries its own parameter array.

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

On .NET Framework 4.7.2, `BeginTransAsync`, `CommitAsync`, `RollbackAsync`, and
`ExecuteTransactionAsync` are unavailable. Use `BeginTrans`, `Commit`, `Rollback`, and
`ExecuteTransaction` directly; do not wrap synchronous transaction work in `Task.Run`.

For a parameterless statement, call `new SqlStatement(sql)` and omit the parameter arguments. An explicitly null
parameter array is rejected.

## SQL Server

- `ExecuteSql`, `GetSingle`, and the provider-specific `Query(string, SqlParameter[])` transition overload were removed.
- `AddByBulkCopy` / `AddByBulkCopyAsync` were replaced by `BulkInsert` / `BulkInsertAsync`.
- `SqlServerHelper` implements `IBulkInsert`; `IDatabase` does not expose bulk insertion.
- `BulkInsert` accepts `batchSize` and `timeout` and returns the number of rows written.
- `GetMaxId` and `GetMaxIdAsync` return `int`, quote qualified names as `[schema].[table]`, and return `1` for an empty table.
- `TableExists("schema.Table")` now matches both schema and table name.

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

The provider helper now contains only SQLite-specific operations that have clear synchronous or genuinely
asynchronous behavior:

- `CreateFileDatabase`
- `GetDatabaseSize`
- `GetTableNames` / `GetTableNamesAsync`
- `TableExists` / `TableExistsAsync`
- `BackupDatabase` (synchronous)

`VacuumDatabase`, `AnalyzeDatabase`, `CheckIntegrity`, `BackupDatabaseAsync`, and provider-specific transaction
wrappers were removed. Execute SQL such as `VACUUM`, `ANALYZE`, or `PRAGMA integrity_check` through the core APIs
when needed. Use the core parameterized transaction methods for multi-statement changes.

## Other removals

- `SqlBuilder` was removed.
- Raw batch-query variants and asynchronous `DataTable` batch-query variants were removed.
- `CommandTimeout` now rejects negative values when assigned.
- `OracleHelper`'s own `Query` / `QueryAsync` / `Exists` / `ExistsAsync` overloads were removed; use the `Database` base-class APIs instead.
