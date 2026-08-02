# Linger.DataAccess.Oracle

[Migration guide for 2.0](../Linger.DataAccess/MIGRATION.md)

Oracle provider for `Linger.DataAccess`. Common query, mapping, transaction, and batch-query behavior comes from
the core `Database` class; this helper configures `OracleClientFactory` and the `:` parameter prefix.

## Installation

```powershell
dotnet add package Linger.DataAccess.Oracle
```

## Usage

```csharp
using Linger.DataAccess.Oracle;
using Oracle.ManagedDataAccess.Client;

using var database = new OracleHelper(connectionString);

bool hasUser = await database.HasRowsAsync(
    "SELECT 1 FROM USERS WHERE EMAIL = :email",
    [new OracleParameter(":email", email)],
    cancellationToken);

List<User> users = await database.FindListBySqlAsync(
    "SELECT ID, NAME FROM USERS WHERE DEPARTMENT = :department",
    record => new User(record.GetInt32(0), record.GetString(1)),
    [new OracleParameter(":department", "IT")],
    cancellationToken);
```

Use parameterized SQL for all values. `QueryInBatches` automatically uses Oracle-style parameter names.
