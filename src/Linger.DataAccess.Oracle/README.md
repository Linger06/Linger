# Linger.DataAccess.Oracle

[中文](README_zh-CN.md) | English

## Overview

Linger.DataAccess.Oracle is a secure and feature-rich Oracle database access library designed for enterprise applications. It provides enhanced security through parameterized queries, preventing SQL injection attacks, and offers comprehensive database operations with async support.

## Features

- **🔒 Security First**: All queries use parameterized statements to prevent SQL injection
- **⚡ High Performance**: Batch processing with intelligent pagination (1000 items per batch)
- **🔄 Async Support**: Full async/await support with CancellationToken
- **🎯 Multi-Framework**: Supports .NET 9.0, .NET 8.0, and .NET Framework 4.6.2
- **📊 Comprehensive Operations**: Complete CRUD operations with advanced features
- **🧪 Well Tested**: Comprehensive unit test coverage (28+ test methods)

## Installation

```xml
<PackageReference Include="Linger.DataAccess.Oracle" Version="0.8.0-preview" />
```

## Quick Start

```csharp
using Linger.DataAccess.Oracle;

// Initialize Oracle helper
var oracle = new OracleHelper("Data Source=localhost:1521/XE;User Id=hr;Password=password;");

// Secure parameterized query
var users = await oracle.QueryAsync<User>("SELECT * FROM users WHERE department = :dept", 
    new OracleParameter(":dept", "IT"));

// Batch processing with automatic pagination
var userIds = new List<string> { "1", "2", "3", ..., "5000" }; // Large list
var results = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", userIds);

// Check existence safely
bool exists = await oracle.ExistsAsync("SELECT 1 FROM users WHERE email = :email",
    new OracleParameter(":email", "user@example.com"));
```

## Core Methods

### Batch Operations
- `QueryInBatches(sql, parameters)` - Intelligent batch processing for large parameter lists
- `QueryInBatchesAsync(sql, parameters, cancellationToken)` - Async batch processing

### Existence Checks
- `Exists(sql, parameters)` - Parameterized existence verification
- `ExistsAsync(sql, parameters, cancellationToken)` - Async existence checks

### Query Operations
- `Query<T>(sql, parameters)` - Strongly typed parameterized queries
- `QueryAsync<T>(sql, parameters, cancellationToken)` - Async typed queries

## Security Features

### SQL Injection Prevention
```csharp
// ❌ Vulnerable (old approach)
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ Secure (parameterized)
var sql = "SELECT * FROM users WHERE name = :userName";
var result = oracle.Query<User>(sql, new OracleParameter(":userName", userName));
```

### Parameter Validation
All methods include comprehensive parameter validation:
- Null reference checks
- Empty string validation
- Type safety enforcement

## Performance Optimizations

### Automatic Batching
Large parameter lists are automatically split into batches of 1000 items:

```csharp
// Handles 10,000+ IDs efficiently
var massiveIdList = Enumerable.Range(1, 10000).Select(i => i.ToString()).ToList();
var results = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", massiveIdList);
// Automatically processes in 10 batches of 1000 items each
```

### Async Operations
All database operations support cancellation:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var users = await oracle.QueryAsync<User>("SELECT * FROM users", cancellationToken: cts.Token);
```

## Dependencies

- **Oracle.ManagedDataAccess.Core** 23.9.1 (.NET 8.0, .NET 9.0)
- **Oracle.ManagedDataAccess** 21.19.0 (.NET Framework 4.6.2)
- **Linger.DataAccess** (Core abstractions)

## Testing

Comprehensive unit test suite with 28+ test methods covering:
- SQL injection prevention
- Parameter validation
- Batch processing
- Async operations
- Error handling

```bash
dotnet test Linger.DataAccess.Oracle.UnitTests
```

## Best Practices

1. **Always use parameterized queries**
2. **Implement proper disposal patterns**
3. **Use async methods for I/O operations**
4. **Handle cancellation tokens appropriately**
5. **Validate inputs before database calls**

## License

This project is part of the Linger framework ecosystem.