# Linger.DataAccess.Oracle

[English](README.md) | 中文

一个安全且功能丰富的 Oracle 数据访问库，适配企业级场景。

## 特性

- 参数化查询防止 SQL 注入
- 分批查询能力由核心库提供（默认每批 1000，可自定义）
- 完整异步支持 (CancellationToken)
- 兼容多框架

## 基本用法

```csharp
using Linger.DataAccess.Oracle;

var oracle = new OracleHelper("Data Source=localhost:1521/XE;User Id=hr;Password=password;");

// 参数化查询
var users = await oracle.QueryAsync<User>("SELECT * FROM users WHERE department = :dept", new OracleParameter(":dept", "IT"));

// 分批查询（委托核心实现）
var ids = Enumerable.Range(1, 6000).Select(i => i.ToString()).ToList();
var dt = oracle.QueryInBatches("SELECT * FROM users WHERE id IN ({0})", ids);

// 存在性检查
bool exists = await oracle.ExistsAsync("SELECT 1 FROM users WHERE email = :email", new OracleParameter(":email", "user@example.com"));
```

## Oracle 说明

- 参数前缀使用 ':'
- 极大 IN 列表可调整 batchSize（核心默认 1000）
- 优先使用参数化方法；Raw 版本仅用于可信数字 ID

## 最佳实践

- 始终使用参数化防注入
- 仅在需要时调小或调大 batchSize
- 对网络/IO 操作使用异步
- 合理传递 CancellationToken
