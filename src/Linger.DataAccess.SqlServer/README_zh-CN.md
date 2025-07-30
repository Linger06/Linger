# Linger.DataAccess.SqlServer

一个高性能的 SQL Server 数据访问库，提供批量复制支持的数据库操作功能。

## 特性

- **高性能批量插入** 使用 SqlBulkCopy 技术
- **异步/等待支持** 适用于现代 .NET 应用程序
- **全面的参数验证** 提供有意义的错误消息
- **线程安全操作** 具有适当的资源管理
- **完全兼容** Linger.DataAccess 框架

## 支持的 .NET 版本

本库支持以下 .NET 应用程序：
- .NET 9.0
- .NET 8.0  
- .NET Framework 4.6.2+

## 使用示例

### 基本设置

```csharp
using Linger.DataAccess.SqlServer;

var connectionString = "Data Source=localhost;Initial Catalog=MyDB;User ID=xxxx;Password=xxxx;TrustServerCertificate=true";
var sqlHelper = new SqlServerHelper(connectionString);
```

### 批量插入操作

```csharp
// 同步批量插入
var dataTable = GetDataTable(); // 包含数据的 DataTable
sqlHelper.AddByBulkCopy(dataTable, "Users", batchSize: 2000, timeout: 120);

// 异步批量插入
await sqlHelper.AddByBulkCopyAsync(dataTable, "Users", batchSize: 2000, timeout: 120);
```

### ID 生成

```csharp
// 获取下一个可用的 ID
var nextId = sqlHelper.GetMaxId("UserId", "Users");
if (nextId.HasValue)
{
    Console.WriteLine($"下一个 ID: {nextId.Value}");
}

// 异步版本
var nextIdAsync = await sqlHelper.GetMaxIdAsync("UserId", "Users");
```

### 数据存在性检查

```csharp
// 检查数据是否存在
var hasData = sqlHelper.Exists("SELECT COUNT(*) FROM Users WHERE Age > 18");

// 异步版本
var hasDataAsync = await sqlHelper.ExistsAsync("SELECT COUNT(*) FROM Users WHERE Age > 18");
```

### 继承的数据库操作

由于 `SqlServerHelper` 继承自 `Database`，您可以使用所有标准的数据库操作：

```csharp
// 执行 SQL 命令
var rowsAffected = sqlHelper.ExecuteBySql("UPDATE Users SET LastLogin = GETDATE() WHERE UserId = 1");

// 查询数据
var users = sqlHelper.FindListBySql<User>("SELECT * FROM Users WHERE IsActive = 1");
var userTable = sqlHelper.FindTableBySql("SELECT * FROM Users");

// 异步操作
var usersAsync = await sqlHelper.FindTableBySqlAsync("SELECT * FROM Users WHERE IsActive = 1");
var countAsync = await sqlHelper.FindCountBySqlAsync("SELECT COUNT(*) FROM Users");
```

## 错误处理

本库提供全面的错误处理，包含有意义的异常消息：

```csharp
try
{
    var nextId = sqlHelper.GetMaxId("InvalidField", "NonExistentTable");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"数据库操作失败: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"无效参数: {ex.Message}");
}
```

## 性能考虑

- 对于大数据集插入（> 1000 行）使用 `AddByBulkCopy`
- 根据内存限制调整 `batchSize` 参数
- 对于 I/O 密集型操作使用异步方法
- 对于多个相关操作考虑使用事务

## 线程安全

`SqlServerHelper` 中的所有方法都是线程安全的，可以在并发场景中使用。每个操作都会创建自己的数据库连接并正确释放资源。

## API 参考

### 构造函数

```csharp
public SqlServerHelper(string connectionString)
```

创建一个新的 SqlServerHelper 实例。

**参数:**
- `connectionString`: SQL Server 数据库连接字符串

### 批量插入方法

#### AddByBulkCopy

```csharp
public void AddByBulkCopy(DataTable table, string tableName, int batchSize = 1000, int timeout = 100)
```

使用 SqlBulkCopy 进行高性能批量数据插入。

**参数:**
- `table`: 要插入的数据表
- `tableName`: 目标数据表名称
- `batchSize`: 批处理大小，默认 1000
- `timeout`: 超时时间（秒），默认 100

**异常:**
- `ArgumentNullException`: 当 table 为 null 时
- `ArgumentException`: 当 tableName 为空或无效时

#### AddByBulkCopyAsync

```csharp
public async Task AddByBulkCopyAsync(DataTable table, string tableName, int batchSize = 1000, int timeout = 100, CancellationToken cancellationToken = default)
```

异步版本的批量数据插入方法。

### ID 生成方法

#### GetMaxId

```csharp
public int? GetMaxId(string fieldName, string tableName)
```

获取指定字段的最大值并加1，通常用于生成下一个ID。

**参数:**
- `fieldName`: 字段名称
- `tableName`: 表名称

**返回值:**
- `int?`: 最大值加1，如果没有数据则返回1，如果字段不是数值类型则返回null

#### GetMaxIdAsync

```csharp
public async Task<int?> GetMaxIdAsync(string fieldName, string tableName, CancellationToken cancellationToken = default)
```

异步版本的 ID 生成方法。

### 数据存在性检查

#### Exists

```csharp
public bool Exists(string strSql)
```

检查指定 SQL 查询是否返回数据。

**参数:**
- `strSql`: SQL 查询语句

**返回值:**
- `bool`: 如果有数据返回 true，否则返回 false

#### ExistsAsync

```csharp
public async Task<bool> ExistsAsync(string strSql, CancellationToken cancellationToken = default)
```

异步版本的数据存在性检查方法。

## 最佳实践

### 1. 连接字符串管理

```csharp
// 推荐：从配置文件读取连接字符串
var connectionString = Configuration.GetConnectionString("DefaultConnection");
var sqlHelper = new SqlServerHelper(connectionString);
```

### 2. 批量操作优化

```csharp
// 对于大量数据，调整批处理大小以优化性能
var largeDataTable = GetLargeDataTable();
await sqlHelper.AddByBulkCopyAsync(largeDataTable, "LargeTable", 
    batchSize: 5000, timeout: 300);
```

### 3. 异常处理

```csharp
try
{
    await sqlHelper.AddByBulkCopyAsync(dataTable, "Users");
}
catch (SqlException ex)
{
    // 处理 SQL Server 特定错误
    logger.LogError(ex, "批量插入失败");
}
catch (ArgumentException ex)
{
    // 处理参数验证错误
    logger.LogWarning(ex, "参数验证失败");
}
```

### 4. 资源管理

```csharp
// SqlServerHelper 实现了正确的资源管理
// 不需要手动释放连接
using var sqlHelper = new SqlServerHelper(connectionString);
await sqlHelper.AddByBulkCopyAsync(dataTable, "Users");
// 连接会自动释放
```
