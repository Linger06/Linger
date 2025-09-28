# Linger.DataAccess.SqlServer

SQL Server data access library with high-performance bulk operations.

## Features

- High-performance bulk insert using SqlBulkCopy
- Async/await support
- Parameter validation with meaningful error messages
- Thread-safe operations
- Full compatibility with Linger.DataAccess framework

## Installation

```powershell
dotnet add package Linger.DataAccess.SqlServer
```

## Basic Usage

```csharp
using Linger.DataAccess.SqlServer;

var connectionString = "Data Source=localhost;Initial Catalog=MyDB;User ID=xxxx;Password=xxxx;TrustServerCertificate=true";
var sqlHelper = new SqlServerHelper(connectionString);

// Bulk insert
var dataTable = GetDataTable();
await sqlHelper.AddByBulkCopyAsync(dataTable, "Users", batchSize: 2000, timeout: 120);

// ID generation
var nextId = await sqlHelper.GetMaxIdAsync("UserId", "Users");

// Data existence check
var hasData = await sqlHelper.ExistsAsync("SELECT COUNT(*) FROM Users WHERE Age > 18");
```

## Core Methods

### AddByBulkCopyAsync
```csharp
public async Task AddByBulkCopyAsync(DataTable table, string tableName, 
    int batchSize = 1000, int timeout = 100, CancellationToken cancellationToken = default)
```
High-performance bulk data insertion.

### GetMaxIdAsync
```csharp
public async Task<int?> GetMaxIdAsync(string fieldName, string tableName, 
    CancellationToken cancellationToken = default)
```
Gets the maximum value of specified field plus 1 for ID generation.

### ExistsAsync
```csharp
public async Task<bool> ExistsAsync(string strSql, CancellationToken cancellationToken = default)
```
Checks if specified SQL query returns data.

## Best Practices

- Use `AddByBulkCopy` for large datasets (> 1000 rows)
- Adjust `batchSize` based on memory constraints
- Use async methods for I/O intensive operations
- All methods are thread-safe
