# Linger.DataAccess.Oracle

[中文](README_zh-CN.md) | English

Oracle database access library with enhanced security and batch processing.

## Features

- Security-first parameterized queries
- Intelligent batch processing (implemented in core Linger.DataAccess)
- Async/await with CancellationToken
- Comprehensive operations & provider-specific helpers

## Installation

```xml
<PackageReference Include="Linger.DataAccess.Oracle" Version="0.8.0-preview" />
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

## Best Practices

- Always prefer parameterized SQL
- Tune batchSize only if statement length limits are hit
- Use async methods for network-bound operations
- Handle CancellationToken for long-running queries