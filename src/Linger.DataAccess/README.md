# Linger.DataAccess

[Migration guide for 2.0](MIGRATION.md)

Provider-neutral ADO.NET execution, connection/transaction lifetime management, and common query semantics.

## Target frameworks

- .NET 10
- .NET 9
- .NET 8
- .NET Framework 4.7.2

## Architecture

- `BaseDatabase`: commands, connections, transactions, cancellation, and parameter ownership.
- `Database`: datasets, tables, entity/list mapping, counts, batch queries, and parameterized batch transactions.
- Provider helpers: only provider-specific capabilities.

`Database` accepts a standard `DbProviderFactory`; there is no custom provider abstraction.

```csharp
using Linger.DataAccess;
using Microsoft.Data.SqlClient;

using var database = new Database(SqlClientFactory.Instance, connectionString);
```

## Queries

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

`DataSet` and `DataTable` materialization is intentionally synchronous because the BCL fill APIs are synchronous.
For asynchronous work, use `FindListBySqlAsync` or `ExecuteReaderAsync` and consume the reader directly.

## Existence and counts

```csharp
bool hasRow = database.HasRows(
    "SELECT 1 FROM Users WHERE Email = @email",
    new SqlParameter("@email", email));

int count = await database.FindCountBySqlAsync(
    "SELECT COUNT(*) FROM Users WHERE Active = @active",
    [new SqlParameter("@active", true)],
    cancellationToken);
```

Use `HasRows` for row existence and `FindCountBySql` when the numeric count is needed.

## Parameterized batch transactions

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

The transaction is committed only when every statement succeeds. Failure rolls back and rethrows the original
error. Do not call this API while an ambient transaction started by `BeginTrans` is active.

## Large `IN` queries

```csharp
DataTable result = database.QueryInBatches(
    "SELECT * FROM Orders WHERE OrderId IN ({0})",
    orderIds,
    batchSize: 500);
```

Values are parameterized and split into batches. The first batch defines the returned `DataTable` schema.

## Optional capabilities

Provider-specific features are exposed as capability interfaces. SQL Server implements `IBulkInsert`:

```csharp
if (database is IBulkInsert bulkInsert)
{
    await bulkInsert.BulkInsertAsync(table, "dbo.Users", cancellationToken: cancellationToken);
}
```
