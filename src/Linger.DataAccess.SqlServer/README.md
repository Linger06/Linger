# Linger.DataAccess.SqlServer

A SQL Server data access library that provides high-performance database operations with bulk copy support.

## Features

- **High-performance bulk insert** using SqlBulkCopy
- **Async/await support** for modern .NET applications
- **Comprehensive parameter validation** with meaningful error messages
- **Thread-safe operations** with proper resource management
- **Full compatibility** with the Linger.DataAccess framework

## Supported .NET Versions

This library supports .NET applications that utilize:
- .NET 9.0
- .NET 8.0  
- .NET Framework 4.6.2+

## Installation

```xml
<PackageReference Include="Linger.DataAccess.SqlServer" Version="0.8.0-preview" />
```

## Usage Examples

### Basic Setup

```csharp
using Linger.DataAccess.SqlServer;

var connectionString = "Data Source=localhost;Initial Catalog=MyDB;User ID=xxxx;Password=xxxx;TrustServerCertificate=true";
var sqlHelper = new SqlServerHelper(connectionString);
```

### Bulk Insert Operations

```csharp
// Synchronous bulk insert
var dataTable = GetDataTable(); // Your DataTable with data
sqlHelper.AddByBulkCopy(dataTable, "Users", batchSize: 2000, timeout: 120);

// Asynchronous bulk insert
await sqlHelper.AddByBulkCopyAsync(dataTable, "Users", batchSize: 2000, timeout: 120);
```

### ID Generation

```csharp
// Get next available ID
var nextId = sqlHelper.GetMaxId("UserId", "Users");
if (nextId.HasValue)
{
    Console.WriteLine($"Next ID: {nextId.Value}");
}

// Async version
var nextIdAsync = await sqlHelper.GetMaxIdAsync("UserId", "Users");
```

### Data Existence Check

```csharp
// Check if data exists
var hasData = sqlHelper.Exists("SELECT COUNT(*) FROM Users WHERE Age > 18");

// Async version
var hasDataAsync = await sqlHelper.ExistsAsync("SELECT COUNT(*) FROM Users WHERE Age > 18");
```

### Inherited Database Operations

Since `SqlServerHelper` inherits from `Database`, you can use all standard database operations:

```csharp
// Execute SQL commands
var rowsAffected = sqlHelper.ExecuteBySql("UPDATE Users SET LastLogin = GETDATE() WHERE UserId = 1");

// Query data
var users = sqlHelper.FindListBySql<User>("SELECT * FROM Users WHERE IsActive = 1");
var userTable = sqlHelper.FindTableBySql("SELECT * FROM Users");

// Async operations
var usersAsync = await sqlHelper.FindTableBySqlAsync("SELECT * FROM Users WHERE IsActive = 1");
var countAsync = await sqlHelper.FindCountBySqlAsync("SELECT COUNT(*) FROM Users");
```

## Error Handling

The library provides comprehensive error handling with meaningful exception messages:

```csharp
try
{
    var nextId = sqlHelper.GetMaxId("InvalidField", "NonExistentTable");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Database operation failed: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Invalid parameter: {ex.Message}");
}
```

## Performance Considerations

- Use `AddByBulkCopy` for inserting large datasets (> 1000 rows)
- Adjust `batchSize` parameter based on your memory constraints
- Use async methods for I/O intensive operations
- Consider using transactions for multiple related operations

## Thread Safety

All methods in `SqlServerHelper` are thread-safe and can be used in concurrent scenarios. Each operation creates its own database connection and properly disposes of resources.

## API Reference

### Constructor

```csharp
public SqlServerHelper(string connectionString)
```

Creates a new SqlServerHelper instance.

**Parameters:**
- `connectionString`: SQL Server database connection string

### Bulk Insert Methods

#### AddByBulkCopy

```csharp
public void AddByBulkCopy(DataTable table, string tableName, int batchSize = 1000, int timeout = 100)
```

Performs high-performance bulk data insertion using SqlBulkCopy.

**Parameters:**
- `table`: DataTable to insert
- `tableName`: Target table name
- `batchSize`: Batch size, default 1000
- `timeout`: Timeout in seconds, default 100

**Exceptions:**
- `ArgumentNullException`: When table is null
- `ArgumentException`: When tableName is empty or invalid

#### AddByBulkCopyAsync

```csharp
public async Task AddByBulkCopyAsync(DataTable table, string tableName, int batchSize = 1000, int timeout = 100, CancellationToken cancellationToken = default)
```

Async version of bulk data insertion method.

### ID Generation Methods

#### GetMaxId

```csharp
public int? GetMaxId(string fieldName, string tableName)
```

Gets the maximum value of specified field plus 1, commonly used for generating next ID.

**Parameters:**
- `fieldName`: Field name
- `tableName`: Table name

**Returns:**
- `int?`: Maximum value plus 1, returns 1 if no data, returns null if field is not numeric

#### GetMaxIdAsync

```csharp
public async Task<int?> GetMaxIdAsync(string fieldName, string tableName, CancellationToken cancellationToken = default)
```

Async version of ID generation method.

### Data Existence Check

#### Exists

```csharp
public bool Exists(string strSql)
```

Checks if specified SQL query returns data.

**Parameters:**
- `strSql`: SQL query statement

**Returns:**
- `bool`: Returns true if data exists, otherwise false

#### ExistsAsync

```csharp
public async Task<bool> ExistsAsync(string strSql, CancellationToken cancellationToken = default)
```

Async version of data existence check method.

## Best Practices

### 1. Connection String Management

```csharp
// Recommended: Read connection string from configuration
var connectionString = Configuration.GetConnectionString("DefaultConnection");
var sqlHelper = new SqlServerHelper(connectionString);
```

### 2. Bulk Operation Optimization

```csharp
// For large datasets, adjust batch size to optimize performance
var largeDataTable = GetLargeDataTable();
await sqlHelper.AddByBulkCopyAsync(largeDataTable, "LargeTable", 
    batchSize: 5000, timeout: 300);
```

### 3. Exception Handling

```csharp
try
{
    await sqlHelper.AddByBulkCopyAsync(dataTable, "Users");
}
catch (SqlException ex)
{
    // Handle SQL Server specific errors
    logger.LogError(ex, "Bulk insert failed");
}
catch (ArgumentException ex)
{
    // Handle parameter validation errors
    logger.LogWarning(ex, "Parameter validation failed");
}
```

### 4. Resource Management

```csharp
// SqlServerHelper implements proper resource management
// No need to manually dispose connections
using var sqlHelper = new SqlServerHelper(connectionString);
await sqlHelper.AddByBulkCopyAsync(dataTable, "Users");
// Connection will be automatically disposed
```

## Unit Testing

The project includes comprehensive unit test suite:

- **56 test cases** covering all public methods
- **100% test pass rate**
- **Parameter validation tests** ensuring input safety
- **Async method tests** verifying concurrency safety
- **Boundary condition tests** improving code robustness

Run tests:

```bash
dotnet test Linger.DataAccess.SqlServer.UnitTests
```

## Contributing

This library is part of the Linger framework. Please refer to the main repository for contributing guidelines.

## License

This project follows the license terms of the Linger framework.

## Version History

### v0.8.0-preview
- Initial preview release
- High-performance bulk insert support
- Complete async method support
- Comprehensive parameter validation
- Thread-safe operations
