# Linger.DataAccess.Oracle

[中文](README_zh-CN.md) | English

Oracle database access library with enhanced security and batch processing.

## Features

- Security-first: Parameterized queries prevent SQL injection
- High performance: Intelligent batch processing (1000 items per batch)
- Async support: Full async/await with CancellationToken
- Comprehensive operations: Complete CRUD with advanced features

## Installation

```xml
<PackageReference Include="Linger.DataAccess.Oracle" Version="0.8.0-preview" />
```

## Basic Usage

```csharp
using Linger.DataAccess.Oracle;

var oracle = new OracleHelper("Data Source=localhost:1521/XE;User Id=hr;Password=password;");

// Parameterized query
var users = await oracle.QueryAsync<User>("SELECT * FROM users WHERE department = :dept", 
    new OracleParameter(":dept", "IT"));

// Batch processing
var userIds = new List<string> { "1", "2", "3", ..., "5000" };
var results = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", userIds);

// Existence check
bool exists = await oracle.ExistsAsync("SELECT 1 FROM users WHERE email = :email",
    new OracleParameter(":email", "user@example.com"));
```

## Core Methods

### QueryInBatchesAsync
```csharp
public async Task<DataSet> QueryInBatchesAsync(string sql, IEnumerable<string> parameters, 
    CancellationToken cancellationToken = default)
```
Intelligent batch processing for large parameter lists.

### ExistsAsync
```csharp
public async Task<bool> ExistsAsync(string sql, params OracleParameter[] parameters)
```
Parameterized existence verification.

### QueryAsync
```csharp
public async Task<List<T>> QueryAsync<T>(string sql, params OracleParameter[] parameters)
```
Strongly typed parameterized queries.

## Security Features

```csharp
// ❌ Vulnerable
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ Secure
var sql = "SELECT * FROM users WHERE name = :userName";
var result = oracle.Query<User>(sql, new OracleParameter(":userName", userName));
```

## Best Practices

- Always use parameterized queries
- Use async methods for I/O operations
- Implement proper disposal patterns
- Handle cancellation tokens appropriately