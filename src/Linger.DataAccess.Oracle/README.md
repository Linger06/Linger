# Linger.DataAccess.Oracle

Oracle database access library with enhanced security and batch processing.

## Features

- Security-first parameterized queries
- Intelligent batch processing (implemented in core Linger.DataAccess)
- Async/await with CancellationToken
- Comprehensive operations & provider-specific helpers

## Supported .NET Versions

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Framework 4.7.2+

## Installation

```powershell
dotnet add package Linger.DataAccess.Oracle
```

## Basic Usage

```csharp
using Linger.DataAccess.Oracle;

var oracle = new OracleHelper("Data Source=localhost:1521/XE;User Id=hr;Password=password;");

// Parameterized query
var users = await oracle.QueryAsync<User>("SELECT * FROM users WHERE department = :dept", new OracleParameter(":dept", "IT"));

// Batch query (delegates to core; default batchSize = 1000)
var ids = Enumerable.Range(1, 6000).Select(i => i.ToString()).ToList();
var dt = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", ids);

// Existence check
bool exists = await oracle.ExistsAsync("SELECT 1 FROM users WHERE email = :email", new OracleParameter(":email", "user@example.com"));
```

## Oracle Notes

- Parameter prefix uses ':' (e.g., :dept)
- For very large IN lists you may tune batchSize (core default 1000)
- Use parameterized versions for safety; Raw variants only for trusted numeric IDs

## Async Implementation ✅

All async methods use **true asynchronous I/O**:

```csharp
// ✅ TRUE ASYNC - Uses ADO.NET async APIs
public async Task<bool> ExistsAsync(string sql, CancellationToken ct = default)
{
    var count = await FindCountBySqlAsync(sql).ConfigureAwait(false);
    return count > 0;
}
```

**Benefits for Oracle database operations:**
- **Network-bound operations** release threads during database calls
- Supports **thousands of concurrent connections** efficiently
- Full cancellation support for long-running queries
- Optimal for distributed and high-traffic applications

## Best Practices

- **Use async methods** for all database operations - True async ensures optimal performance
- **Always pass CancellationToken** for query timeout and cancellation control
- Always prefer parameterized SQL
- Tune batchSize only if statement length limits are hit
- Handle CancellationToken for long-running queries