# Linger.DataAccess.SqlServer

一个高性能的 SQL Server 数据访问库，提供批量复制支持的数据库操作功能。

## 特性

- **高性能批量插入** 使用 SqlBulkCopy 技术
- **异步/等待支持** 适用于现代 .NET 应用程序  
- **线程安全操作** 具有适当的资源管理
- **完全兼容** Linger.DataAccess 框架

## 支持的 .NET 版本

- .NET 10.0
- .NET 9.0
- .NET 8.0  
- .NET Framework 4.7.2+

## 安装

```powershell
dotnet add package Linger.DataAccess.SqlServer
```

## 基本用法

```csharp
using Linger.DataAccess.SqlServer;

var connectionString = "Data Source=localhost;Initial Catalog=MyDB;User ID=xxxx;Password=xxxx;TrustServerCertificate=true";
var sqlHelper = new SqlServerHelper(connectionString);

// 批量插入
var dataTable = GetDataTable();
await sqlHelper.AddByBulkCopyAsync(dataTable, "Users", batchSize: 2000, timeout: 120);

// ID 生成
var nextId = await sqlHelper.GetMaxIdAsync("UserId", "Users");

// 数据存在性检查
var hasData = await sqlHelper.ExistsAsync("SELECT COUNT(*) FROM Users WHERE Age > 18");

// 继承的数据库操作
var users = await sqlHelper.FindListBySqlAsync<User>("SELECT * FROM Users WHERE IsActive = 1");
var rowsAffected = sqlHelper.ExecuteBySql("UPDATE Users SET LastLogin = GETDATE() WHERE UserId = 1");
```

## 核心方法

### AddByBulkCopyAsync
```csharp
public async Task AddByBulkCopyAsync(DataTable table, string tableName, 
    int batchSize = 1000, int timeout = 100, CancellationToken cancellationToken = default)
```
使用 SqlBulkCopy 进行高性能批量数据插入。

### GetMaxIdAsync
```csharp
public async Task<int?> GetMaxIdAsync(string fieldName, string tableName, 
    CancellationToken cancellationToken = default)
```
获取指定字段的最大值并加1，用于生成下一个ID。

### ExistsAsync
```csharp
public async Task<bool> ExistsAsync(string strSql, CancellationToken cancellationToken = default)
```
检查指定 SQL 查询是否返回数据。

## 异步实现 ✅

所有异步方法均使用**真正的异步 I/O**，包括 SQL Server 专用功能：

```csharp
// ✅ 真正异步 - 使用 ADO.NET 异步 API
public async Task<bool> ExistsAsync(string sql, CancellationToken ct = default)
{
    var count = await FindCountBySqlAsync(sql).ConfigureAwait(false);
    return count > 0;
}

// ✅ SqlBulkCopy 异步操作
public async Task AddByBulkCopyAsync(DataTable table, string tableName, ...)
{
    using var bulk = new SqlBulkCopy(connection);
    await bulk.WriteToServerAsync(table, cancellationToken).ConfigureAwait(false);
}
```

**性能优势：**
- **BulkCopy 操作**可高效处理数百万行数据
- 支持**高吞吐量场景**且线程使用最小化
- 长时间运行的批量操作的完整取消支持
- 最适合数据仓库和 ETL 场景

## 最佳实践

- **所有操作使用异步方法** - 真正的异步确保最佳可扩展性
- **始终传递 CancellationToken** 以支持正确的取消操作
- 对于大数据集插入（> 1000 行）使用 `AddByBulkCopyAsync`
- 根据内存限制调整 `batchSize` 参数
- 从配置文件读取连接字符串
- 使用适当的异常处理
