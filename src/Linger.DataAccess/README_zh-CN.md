# Linger.DataAccess

核心数据访问库，提供数据库抽象和通用数据库操作。

## 功能特性

- **数据库抽象**: 提供程序无关的数据库访问
- **CRUD 操作**: 完整的创建、读取、更新、删除操作
- **异步支持**: 完整 async/await 支持
- **多种数据类型**: 支持 DataTable、DataSet、实体对象、Hashtable
- **事务支持**: 内置事务管理
- **SQL 构建器**: 动态 SQL 生成助手
- **批量操作**: 高性能批量数据插入接口
- **分批查询**: 拆分超大参数列表（默认 batchSize = 1000），支持参数化与 Raw 版本

## 支持的 .NET 版本

- .NET 9.0
- .NET 8.0
- .NET Framework 4.6.2+

## 安装

通常不直接安装本库，而是通过安装具体数据库实现包间接引用：

```bash
# SQL Server
dotnet add package Linger.DataAccess.SqlServer

# Oracle
dotnet add package Linger.DataAccess.Oracle

# SQLite
dotnet add package Linger.DataAccess.Sqlite
```

## 核心接口

### IDatabase
提供全面数据库操作的主要接口：

```csharp
// 执行操作
int ExecuteBySql(string sql);
int ExecuteByProc(string procName, DbParameter[] parameters);

// 查询操作
List<T> FindListBySql<T>(string sql);
DataTable FindTableBySql(string sql, DbParameter[] parameters);
DataSet FindDataSetBySql(string sql, DbParameter[] parameters);

// 分批查询操作
DataTable QueryInBatches(string sql, List<string> parameters, int batchSize = 1000);
Task<DataTable> QueryInBatchesAsync(string sql, List<string> parameters, int batchSize = 1000, CancellationToken cancellationToken = default);
DataTable QueryInBatchesRaw(string sql, List<string> values, int batchSize = 1000, bool quote = true);
Task<DataTable> QueryInBatchesRawAsync(string sql, List<string> values, int batchSize = 1000, bool quote = true, CancellationToken cancellationToken = default);

// 异步操作
Task<DataTable> FindTableBySqlAsync(string sql);
Task<DataSet> FindDataSetBySqlAsync(string sql, DbParameter[] parameters);
Task<int> FindCountBySqlAsync(string sql);

// 实体操作
T FindEntityBySql<T>(string sql, DbParameter[] parameters);
Hashtable FindHashtableBySql(string sql, DbParameter[] parameters);

// 批量操作
bool BulkInsert(DataTable dt);
```

### IProvider
不同数据库引擎的提供程序抽象。

## 基本用法

```csharp
using Linger.DataAccess;

// 查询列表
var users = database.FindListBySql<User>("SELECT * FROM Users WHERE Active = 1");
var userTable = await database.FindTableBySqlAsync("SELECT * FROM Users");

// 分批查询（默认每批 1000）
var ids = Enumerable.Range(1, 5000).Select(i => i.ToString()).ToList();
var dt = database.QueryInBatches("SELECT * FROM Users WHERE Id IN ({0})", ids);

// 自定义批大小
var dt500 = database.QueryInBatches("SELECT * FROM Users WHERE Id IN ({0})", ids, 500);

// Raw 版本（仅可信数字 ID）
var dtRaw = database.QueryInBatchesRaw("SELECT * FROM Users WHERE Id IN ({0})", ids, 800, quote: false);

// 异步参数化版本
var dtAsync = await database.QueryInBatchesAsync("SELECT * FROM Users WHERE Id IN ({0})", ids, 750);

// 执行命令
int affected = database.ExecuteBySql("UPDATE Users SET LastLogin = GETDATE()");

// 计数操作
int userCount = await database.FindCountBySqlAsync("SELECT COUNT(*) FROM Users");
```

## 分批查询

针对超大 IN 列表（成千上万 ID），单条 SQL 可能过长或性能下降。分批查询助手自动拆分并合并结果。

```csharp
// 参数化（安全）
var result = database.QueryInBatches(
    "SELECT * FROM Orders WHERE OrderId IN ({0})",
    orderIds); // 默认 batchSize = 1000

// Raw（仅可信常量值）
var resultRaw = database.QueryInBatchesRaw(
    "SELECT * FROM Orders WHERE OrderId IN ({0})",
    orderIds, 500, quote: false);
```

指南：
- 在 sql 中使用 {0} 作为占位符注入当前批次。
- 优先使用参数化方法以防 SQL 注入。
- Raw 仅用于完全可信的内部生成值（尤其是数字 ID）。
- 调整 batchSize 以权衡网络往返与 SQL 长度限制。

返回值：
- 所有分批方法合并为一个 DataTable，结构取首个非空批次的列模式。

## 架构

此库为特定数据库实现提供基础：

- **Linger.DataAccess.SqlServer** - SQL Server 实现
- **Linger.DataAccess.Oracle** - Oracle 实现
- **Linger.DataAccess.Sqlite** - SQLite 实现

## 核心组件

### Database 类
`IDatabase` 接口的基础实现，提供通用数据库操作。

### BaseDatabase 类
连接管理与参数处理等核心功能。

### SqlBuilder 类
用于安全构建动态 SQL。

## 异步/等待最佳实践

### 真正的异步实现 ✅

本库中的所有异步方法均使用**真正的异步 I/O** 操作，而非 `Task.Run` 包装：

```csharp
// ✅ 真正异步 - I/O 期间释放线程
public async Task<bool> ExistsAsync(string sql, CancellationToken ct = default)
{
    var count = await FindCountBySqlAsync(sql).ConfigureAwait(false);
    return count > 0;
}

// ❌ 伪异步（本库不使用）
// Task.Run 只是包装同步阻塞代码
public Task<bool> BadExistsAsync(string sql)
{
    return Task.Run(() => FindCountBySql(sql)); // 浪费线程池线程
}
```

### 性能优势

| 指标 | 同步方法 | 伪异步 (Task.Run) | 真正异步 ✅ |
|------|---------|------------------|-------------|
| **线程使用** | 阻塞 1 个线程 | 占用 1 个线程池线程 | I/O 期间不占用线程 |
| **并发能力** | ~数千 | ~数千 | ~数万 |
| **内存占用** | ~1MB/线程 | ~1MB/线程 | ~几KB/任务 |
| **可扩展性** | 受限 | 受限 | 优秀 |
| **取消支持** | 不支持 | 仅启动前 | I/O 操作期间可取消 |

### 使用建议

```csharp
// ✅ 推荐：I/O 操作使用异步方法
var users = await database.FindTableBySqlAsync("SELECT * FROM Users");
var count = await database.FindCountBySqlAsync("SELECT COUNT(*) FROM Orders");

// ✅ 推荐：库代码中使用 ConfigureAwait(false)（内部已实现）
var result = await database.QueryAsync(sql).ConfigureAwait(false);

// ✅ 推荐：支持取消令牌
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var data = await database.QueryTableAsync(sql, null, cts.Token);

// ❌ 避免：混用同步和异步（选其一）
var badResult = database.FindTableBySqlAsync(sql).Result; // 可能死锁！
```

### 高并发场景

对于需要处理数千个并发请求的应用程序：

```csharp
// ✅ 可扩展至数万个并发操作
var tasks = Enumerable.Range(1, 10000)
    .Select(id => database.FindTableBySqlAsync($"SELECT * FROM Orders WHERE Id = {id}"))
    .ToList();

var results = await Task.WhenAll(tasks); // 最小化线程使用
```

## 最佳实践

- **使用参数化查询**防止 SQL 注入
- **使用分批查询**处理极大 IN 列表，避免手工拼接
- **使用 `using` / 异步释放模式**管理资源
- **I/O 密集任务使用异步方法** - 所有异步方法均使用真正的异步 I/O
- **始终传递 CancellationToken** 到异步方法以支持正确的取消操作
- **库代码中使用 ConfigureAwait(false)**（内部已实现）
- **依据场景选择合适的具体数据库实现**以获得最佳性能