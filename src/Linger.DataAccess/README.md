# Linger.DataAccess

[中文](README_zh-CN.md) | English

Core data access library providing database abstraction and common database operations.

## Features

- **Database Abstraction**: Provider-agnostic database access
- **CRUD Operations**: Complete Create, Read, Update, Delete operations
- **Async Support**: Full async/await support for modern applications
- **Multiple Data Types**: Support for DataTable, DataSet, Entity objects, and Hashtable
- **Transaction Support**: Built-in transaction management
- **SQL Builder**: Helper for dynamic SQL generation
- **Bulk Operations**: Interface for high-performance bulk data insertion
- **Batch Query**: Large parameter list splitting (default batchSize = 1000) with parameterized & raw variants

## Supported .NET Versions

- .NET 9.0
- .NET 8.0
- .NET Framework 4.6.2+

## Installation

This library is typically not installed directly, but is automatically referenced when installing specific database implementation packages:

```bash
# For SQL Server
dotnet add package Linger.DataAccess.SqlServer

# For Oracle Database
dotnet add package Linger.DataAccess.Oracle

# For SQLite
dotnet add package Linger.DataAccess.Sqlite
```

## Core Interfaces

### IDatabase
The main interface providing comprehensive database operations:

```csharp
// Execute operations
int ExecuteBySql(string sql);
int ExecuteByProc(string procName, DbParameter[] parameters);

// Query operations
List<T> FindListBySql<T>(string sql);
DataTable FindTableBySql(string sql, DbParameter[] parameters);
DataSet FindDataSetBySql(string sql, DbParameter[] parameters);

// Batch query operations
DataTable QueryInBatches(string sql, List<string> parameters, int batchSize = 1000);
Task<DataTable> QueryInBatchesAsync(string sql, List<string> parameters, int batchSize = 1000, CancellationToken cancellationToken = default);
DataTable QueryInBatchesRaw(string sql, List<string> values, int batchSize = 1000, bool quote = true);
Task<DataTable> QueryInBatchesRawAsync(string sql, List<string> values, int batchSize = 1000, bool quote = true, CancellationToken cancellationToken = default);

// Async operations
Task<DataTable> FindTableBySqlAsync(string sql);
Task<DataSet> FindDataSetBySqlAsync(string sql, DbParameter[] parameters);
Task<int> FindCountBySqlAsync(string sql);

// Entity operations
T FindEntityBySql<T>(string sql, DbParameter[] parameters);
Hashtable FindHashtableBySql(string sql, DbParameter[] parameters);

// Bulk operations
bool BulkInsert(DataTable dt);
```

### IProvider
Database provider abstraction for different database engines.

## Basic Usage

```csharp
using Linger.DataAccess;

// Execute queries
var users = database.FindListBySql<User>("SELECT * FROM Users WHERE Active = 1");
var userTable = await database.FindTableBySqlAsync("SELECT * FROM Users");

// Batch query (IDs split automatically, default batchSize 1000)
var ids = Enumerable.Range(1, 5000).Select(i => i.ToString()).ToList();
var dt = database.QueryInBatches("SELECT * FROM Users WHERE Id IN ({0})", ids);

// Custom batch size
var dt500 = database.QueryInBatches("SELECT * FROM Users WHERE Id IN ({0})", ids, 500);

// Raw version (trusted numeric IDs only)
var dtRaw = database.QueryInBatchesRaw("SELECT * FROM Users WHERE Id IN ({0})", ids, 800, quote: false);

// Async parameterized version
var dtAsync = await database.QueryInBatchesAsync("SELECT * FROM Users WHERE Id IN ({0})", ids, 750);

// Execute commands
int affected = database.ExecuteBySql("UPDATE Users SET LastLogin = GETDATE()");

// Count operations
int userCount = await database.FindCountBySqlAsync("SELECT COUNT(*) FROM Users");
```

## Batch Query

When dealing with very large IN lists (thousands of IDs) a single SQL statement may exceed length limits or degrade performance. The batch query helpers automatically split the list and concatenate the results.

```csharp
// Parameterized (safe)
var result = database.QueryInBatches(
    "SELECT * FROM Orders WHERE OrderId IN ({0})",
    orderIds); // default batchSize = 1000

// Raw (only for trusted constant values)
var resultRaw = database.QueryInBatchesRaw(
    "SELECT * FROM Orders WHERE OrderId IN ({0})",
    orderIds, 500, quote: false);
```

Guidelines:
- Use {0} in sql where the batch placeholder will be injected.
- Prefer parameterized methods for security (prevents SQL injection).
- Raw methods are only for fully trusted data (e.g., internally generated numeric IDs).
- Adjust batchSize to balance network round-trips and SQL size limits.

Return Type:
- All batch methods merge rows into a single DataTable preserving schema of the first non-empty batch.

## Architecture

This library provides the foundation for database-specific implementations:

- **Linger.DataAccess.SqlServer** - SQL Server implementation
- **Linger.DataAccess.Oracle** - Oracle Database implementation  
- **Linger.DataAccess.Sqlite** - SQLite implementation

## Key Components

### Database Class
Base implementation of `IDatabase` interface providing common database operations.

### BaseDatabase Class
Core database functionality including connection management and parameter handling.

### SqlBuilder Class
Helper utility for building dynamic SQL queries safely.

## Best Practices

- Use parameterized queries to prevent SQL injection
- Use batch query helpers for large IN lists instead of manual concatenation
- Implement proper disposal patterns with `using` statements
- Use async methods for I/O intensive operations
- Choose appropriate database-specific implementations for optimal performance

## Contributing

This library is part of the Linger framework. Please refer to the main repository for contributing guidelines.