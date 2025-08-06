# Linger.DataAccess

中文 | [English](README.md)

核心数据访问库，提供数据库抽象和通用数据库操作。

## 功能特性

- **数据库抽象**: 提供程序无关的数据库访问
- **CRUD 操作**: 完整的创建、读取、更新、删除操作
- **异步支持**: 完整的 async/await 支持
- **多种数据类型**: 支持 DataTable、DataSet、实体对象和 Hashtable
- **事务支持**: 内置事务管理
- **SQL 构建器**: 动态 SQL 生成助手
- **批量操作**: 高性能批量数据插入接口

## 支持的 .NET 版本

- .NET 9.0
- .NET 8.0
- .NET Framework 4.6.2+

## 安装

此库通常不直接安装，而是通过安装具体的数据库实现包自动引用：

```bash
# SQL Server 数据库
dotnet add package Linger.DataAccess.SqlServer

# Oracle 数据库  
dotnet add package Linger.DataAccess.Oracle

# SQLite 数据库
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
不同数据库引擎的数据库提供程序抽象。

## 基本用法

```csharp
using Linger.DataAccess;

// 与特定数据库实现一起使用
// (需要特定数据库包，如 Linger.DataAccess.SqlServer)

// 执行查询
var users = database.FindListBySql<User>("SELECT * FROM Users WHERE Active = 1");
var userTable = await database.FindTableBySqlAsync("SELECT * FROM Users");

// 执行命令
int affected = database.ExecuteBySql("UPDATE Users SET LastLogin = GETDATE()");

// 计数操作
int userCount = await database.FindCountBySqlAsync("SELECT COUNT(*) FROM Users");
```

## 架构设计

此库为特定数据库实现提供基础：

- **Linger.DataAccess.SqlServer** - SQL Server 实现
- **Linger.DataAccess.Oracle** - Oracle 数据库实现
- **Linger.DataAccess.Sqlite** - SQLite 实现

## 核心组件

### Database 类
`IDatabase` 接口的基础实现，提供通用数据库操作。

### BaseDatabase 类
核心数据库功能，包括连接管理和参数处理。

### SqlBuilder 类
安全构建动态 SQL 查询的助手工具。

## 最佳实践

- 使用参数化查询防止 SQL 注入
- 使用 `using` 语句实现适当的资源释放模式
- 对 I/O 密集操作使用异步方法
- 选择适当的特定数据库实现以获得最佳性能

## 贡献

此库是 Linger 框架的一部分。请参考主仓库的贡献指南。
