# Linger.DataAccess.Oracle

[2.0 迁移指南](../Linger.DataAccess/MIGRATION.zh-CN.md)

`Linger.DataAccess` 的 Oracle 实现。通用查询、映射、事务和分批查询由核心 `Database` 提供；本 Helper
负责配置 `OracleClientFactory` 和 `:` 参数前缀。

## 安装

```powershell
dotnet add package Linger.DataAccess.Oracle
```

## 使用

```csharp
using Linger.DataAccess.Oracle;
using Oracle.ManagedDataAccess.Client;

using var database = new OracleHelper(connectionString);

bool hasUser = await database.HasRowsAsync(
    "SELECT 1 FROM USERS WHERE EMAIL = :email",
    [new OracleParameter(":email", email)],
    cancellationToken);

List<User> users = await database.FindListBySqlAsync(
    "SELECT ID, NAME FROM USERS WHERE DEPARTMENT = :department",
    record => new User(record.GetInt32(0), record.GetString(1)),
    [new OracleParameter(":department", "IT")],
    cancellationToken);
```

所有值都应通过参数传入。`QueryInBatches` 会自动使用 Oracle 风格的参数名。
