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

// Using with specific database implementations
// (Requires database-specific packages like Linger.DataAccess.SqlServer)

// Execute queries
var users = database.FindListBySql<User>("SELECT * FROM Users WHERE Active = 1");
var userTable = await database.FindTableBySqlAsync("SELECT * FROM Users");

// Execute commands
int affected = database.ExecuteBySql("UPDATE Users SET LastLogin = GETDATE()");

// Count operations
int userCount = await database.FindCountBySqlAsync("SELECT COUNT(*) FROM Users");
```

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
- Implement proper disposal patterns with `using` statements
- Use async methods for I/O intensive operations
- Choose appropriate database-specific implementations for optimal performance

## Contributing

This library is part of the Linger framework. Please refer to the main repository for contributing guidelines.