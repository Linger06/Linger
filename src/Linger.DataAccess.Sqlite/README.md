# Linger.DataAccess.Sqlite

[中文](README_zh-CN.md) | English

## Overview

Linger.DataAccess.Sqlite is a comprehensive SQLite database access library that leverages SQLite-specific features while providing secure, high-performance database operations. It offers factory methods, advanced SQLite capabilities, and full async support for modern .NET applications.

## Features

- **🏭 Factory Methods**: Easy creation of in-memory, file-based, and temporary databases
- **🔒 Security First**: All queries use parameterized statements to prevent SQL injection
- **⚡ SQLite Optimizations**: WAL mode, VACUUM, ANALYZE, and SQLite-specific features
- **🔄 Async Support**: Full async/await support with CancellationToken
- **🎯 Multi-Framework**: Supports .NET 9.0, .NET 8.0, and .NET Framework 4.6.2
- **📊 Database Management**: Backup, restore, table operations, and schema management
- **🧪 Well Tested**: Comprehensive unit test coverage (45+ test methods)

## Installation

```xml
<PackageReference Include="Linger.DataAccess.Sqlite" Version="0.8.0-preview" />
```

## Quick Start

```csharp
using Linger.DataAccess.Sqlite;

// Create different types of databases
var inMemoryDb = SqliteHelper.CreateInMemory();
var fileDb = SqliteHelper.CreateFileDatabase("myapp.db");
var tempDb = SqliteHelper.CreateTemporary();

// Secure parameterized queries
var users = await fileDb.QueryAsync<User>("SELECT * FROM users WHERE age > @age", 
    new SQLiteParameter("@age", 18));

// Batch processing with automatic pagination
var userIds = new List<string> { "1", "2", "3", ..., "5000" };
var results = fileDb.Page("SELECT * FROM users WHERE id IN ({0})", userIds);

// SQLite-specific operations
await fileDb.EnableWalModeAsync();
await fileDb.VacuumAsync();
await fileDb.AnalyzeAsync();
```

## Factory Methods

### Database Creation
```csharp
// In-memory database (fastest, temporary)
var memDb = SqliteHelper.CreateInMemory();

// File-based database (persistent)
var fileDb = SqliteHelper.CreateFileDatabase("app.db", createIfNotExists: true);

// Temporary database (automatic cleanup)
var tempDb = SqliteHelper.CreateTemporary();
```

## Core Methods

### Batch Operations
- `Page(sql, parameters)` - Intelligent batch processing for large parameter lists
- `PageAsync(sql, parameters, cancellationToken)` - Async batch processing

### Existence Checks
- `Exists(sql, parameters)` - Parameterized existence verification
- `ExistsAsync(sql, parameters, cancellationToken)` - Async existence checks

### Query Operations
- `Query<T>(sql, parameters)` - Strongly typed parameterized queries
- `QueryAsync<T>(sql, parameters, cancellationToken)` - Async typed queries

## SQLite-Specific Features

### Performance Optimizations
```csharp
// Enable WAL (Write-Ahead Logging) mode for better concurrency
await sqlite.EnableWalModeAsync();

// Optimize database performance
await sqlite.VacuumAsync();    // Reclaim space and defragment
await sqlite.AnalyzeAsync();   // Update query planner statistics

// Set performance pragmas
await sqlite.SetPragmaAsync("cache_size", "-64000");  // 64MB cache
await sqlite.SetPragmaAsync("temp_store", "MEMORY");   // Temp tables in memory
```

### Database Management
```csharp
// Backup and restore
await sqlite.BackupAsync("backup.db");
await sqlite.RestoreAsync("backup.db");

// Table operations
var tables = await sqlite.GetTableNamesAsync();
bool exists = await sqlite.TableExistsAsync("users");
await sqlite.DropTableAsync("temp_table");

// Schema information
var columns = await sqlite.GetTableSchemaAsync("users");
var indexes = await sqlite.GetIndexesAsync("users");
```

### Transaction Support
```csharp
// Automatic transaction management
await sqlite.ExecuteInTransactionAsync(async (transaction) =>
{
    await sqlite.ExecuteNonQueryAsync("INSERT INTO users (name) VALUES (@name)", 
        new SQLiteParameter("@name", "John"), transaction);
    await sqlite.ExecuteNonQueryAsync("INSERT INTO logs (action) VALUES (@action)", 
        new SQLiteParameter("@action", "User created"), transaction);
});
```

## Security Features

### SQL Injection Prevention
```csharp
// ❌ Vulnerable (old approach)
var sql = $"SELECT * FROM users WHERE name = '{userName}'";

// ✅ Secure (parameterized)
var sql = "SELECT * FROM users WHERE name = @userName";
var result = sqlite.Query<User>(sql, new SQLiteParameter("@userName", userName));
```

### Parameter Validation
All methods include comprehensive parameter validation:
- Null reference checks
- Empty string validation
- Type safety enforcement
- Connection state verification

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
await sqlite.ExecuteInTransactionAsync(async (transaction) =>
{
    foreach (var user in users)
    {
        await sqlite.ExecuteNonQueryAsync(
            "INSERT INTO users (name, email) VALUES (@name, @email)",
            new[] { 
                new SQLiteParameter("@name", user.Name),
                new SQLiteParameter("@email", user.Email) 
            }, transaction);
    }
});
```

## Dependencies

- **System.Data.SQLite.Core** 1.0.119 (All frameworks)
- **Linger.DataAccess** (Core abstractions)

## Testing

Comprehensive unit test suite with 45+ test methods covering:
- Factory method creation
- SQL injection prevention
- SQLite-specific features
- Performance optimizations
- Transaction management
- Error handling

```bash
dotnet test Linger.DataAccess.Sqlite.UnitTests
```

## Advanced Features

### Custom Functions
```csharp
// Register custom SQLite functions
sqlite.RegisterFunction("REGEXP", (pattern, input) => 
    Regex.IsMatch(input.ToString(), pattern.ToString()));

// Use in queries
var results = await sqlite.QueryAsync<User>(
    "SELECT * FROM users WHERE REGEXP(@pattern, email)",
    new SQLiteParameter("@pattern", @".*@company\.com$"));
```

### Full-Text Search
```csharp
// Create FTS table
await sqlite.ExecuteNonQueryAsync(@"
    CREATE VIRTUAL TABLE documents_fts USING fts5(title, content);
");

// Search with FTS
var documents = await sqlite.QueryAsync<Document>(
    "SELECT * FROM documents_fts WHERE documents_fts MATCH @query",
    new SQLiteParameter("@query", "database AND sqlite"));
```

## Best Practices

1. **Use appropriate database type for your needs**
   - In-memory: Testing, caching, temporary data
   - File-based: Persistent application data
   - Temporary: Session-specific data

2. **Enable WAL mode for concurrent access**
3. **Use transactions for batch operations**
4. **Regular VACUUM and ANALYZE for maintenance**
5. **Implement proper disposal patterns**
6. **Use async methods for I/O operations**

## License

This project is part of the Linger framework ecosystem.