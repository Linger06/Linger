# Linger.DataAccess.Sqlite

[中文](README_zh-CN.md) | English

## Overview

Linger.DataAccess.Sqlite is a comprehensive SQLite database access library that leverages SQLite-specific features while providing secure, high-performance database operations. It offers factory methods, advanced SQLite capabilities, and full async support for modern .NET applications.

## Features

- **🏭 Factory Methods**: Easy creation of in-memory, file-based, and temporary databases
- **🔒 Security First**: All queries use parameterized statements to prevent SQL injection
- **⚡ SQLite Optimizations**: SQLite-specific features
- **🔄 Async Support**: Full async/await support with CancellationToken
- **🎯 Multi-Framework**: Supports .NET 9.0, .NET 8.0, and .NET Framework 4.6.2
- **📊 Database Management**: Backup, restore, table operations, and schema management

## Quick Start

```csharp
using Linger.DataAccess.Sqlite;

// Create file database
var fileDb = SqliteHelper.CreateFileDatabase("myapp.db");

// Secure parameterized queries
var users = fileDb.Query("SELECT * FROM users WHERE age > @age", 
    new SQLiteParameter("@age", 18));

// Batch processing with automatic pagination
var userIds = new List<string> { "1", "2", "3", ..., "5000" };
var results = fileDb.Page("SELECT * FROM users WHERE id IN ({0})", userIds);

// SQLite-specific operations
await fileDb.VacuumDatabaseAsync();
await fileDb.AnalyzeDatabaseAsync();
```

## Factory Methods

### Database Creation
```csharp
// File-based database (persistent)
var fileDb = SqliteHelper.CreateFileDatabase("app.db", createIfNotExists: true);
```

## Core Methods

### Batch Operations
- `Page(sql, parameters)` - Intelligent batch processing for large parameter lists
- `PageAsync(sql, parameters, cancellationToken)` - Async batch processing

### Existence Checks
- `Exists(sql, parameters)` - Parameterized existence verification
- `ExistsAsync(sql, parameters, cancellationToken)` - Async existence checks

### Query Operations
- `Query(sql, parameters)` - Parameterized query returning DataSet
- `QueryAsync(sql, parameters, cancellationToken)` - Async query returning DataSet

## SQLite-Specific Features

### Performance Optimizations
```csharp
// Optimize database performance
await sqlite.VacuumDatabaseAsync();   // Reclaim space and defragment
await sqlite.AnalyzeDatabaseAsync();  // Update query planner statistics

// Get database size
var size = await sqlite.GetDatabaseSizeAsync();
```

### Database Management
```csharp
// Backup database
await sqlite.BackupDatabaseAsync("backup.db");

// Table operations
var tables = await sqlite.GetTableNamesAsync();
bool exists = await sqlite.TableExistsAsync("users");

// Integrity check
var integrityResult = await sqlite.CheckIntegrityAsync();
```

### Transaction Support
```csharp
// Automatic transaction management
await sqlite.ExecuteInTransactionAsync(new[]
{
    "INSERT INTO users (name) VALUES ('John')",
    "INSERT INTO logs (action) VALUES ('User created')"
});
```

## Security Features

### SQL Injection Prevention
```csharp
// ❌ Vulnerable (old approach)
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ Secure (parameterized)
var sql = "SELECT * FROM users WHERE name = @userName";
var result = sqlite.Query(sql, new SQLiteParameter("@userName", userName));
```

## Performance Best Practices

### Connection Pooling
```csharp
// Use connection pooling for file databases
var connectionString = "Data Source=app.db;Pooling=true;Max Pool Size=100;";
var sqlite = new SqliteHelper(connectionString);
```

### Batch Insertions
```csharp
// Efficient batch insertions
var users = GenerateTestUsers(10000);
await sqlite.ExecuteInTransactionAsync(users.Select(user => 
    $"INSERT INTO users (name, email) VALUES ('{user.Name}', '{user.Email}')"));
```

## Advanced Features

### Database Management
```csharp
// Get database size
var size = await sqlite.GetDatabaseSizeAsync();

// Check database integrity
var integrityResult = await sqlite.CheckIntegrityAsync();

// Get all table names
var tableNames = await sqlite.GetTableNamesAsync();
```