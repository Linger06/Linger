# Linger.EFCore.Audit

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

用于自动跟踪数据变更的 Entity Framework Core 审计跟踪库。

## ✨ 功能特点

- 自动记录 Entity Framework Core 操作的审计日志
- 跟踪实体的创建、修改和删除
- 捕获已更改属性的旧值和新值
- 记录每次更改的用户信息
- 支持软删除
- 内置审计数据的 JSON 序列化
- 兼容 EF Core 9.0 和 8.0

## 📦 安装

### 通过 Visual Studio

1. 打开 `解决方案资源管理器`。
2. 右键单击解决方案中的项目。
3. 点击 `管理 NuGet 包...`。
4. 点击 `浏览` 选项卡并搜索 "Linger.EFCore.Audit"。
5. 点击 `Linger.EFCore.Audit` 包，选择适当的版本并点击安装。

### 通过包管理器控制台

```
PM> Install-Package Linger.EFCore.Audit
```

### 通过 .NET CLI 控制台

```
dotnet add package Linger.EFCore.Audit
```

## 🔧 配置

将审计功能集成到您的 EF Core DbContext 中：

```csharp
public class AppDbContext : DbContext
{
    // 您的 DbContext 配置...
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 应用审计配置
        modelBuilder.ApplyAudit();
    }
    
    public DbSet<AuditTrailEntry> AuditTrails { get; set; }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 在保存前捕获审计信息
        var auditEntries = this.CaptureAuditEntries();
        
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // 处理审计条目
        await this.ProcessAuditEntries(auditEntries);
        
        return result;
    }
}
```

## 📋 使用示例

### 基本审计跟踪

自动跟踪对实体的所有更改：

```csharp
// 创建一个新实体
var user = new User
{
    Name = "张三",
    Email = "zhangsan@example.com"
};
dbContext.Users.Add(user);
await dbContext.SaveChangesAsync();  // 这将生成一个"创建"审计记录

// 修改实体
user.Email = "new.email@example.com";
await dbContext.SaveChangesAsync();  // 这将生成一个"修改"审计记录

// 删除实体
dbContext.Users.Remove(user);
await dbContext.SaveChangesAsync();  // 这将生成一个"删除"审计记录
```

### 设置当前用户信息

审计记录可以包括执行操作的用户信息：

```csharp
// 在应用程序中设置当前用户
dbContext.SetAuditUserId("user123");
dbContext.SetAuditUsername("张三");

// 现在所有操作都将包含此用户信息
var product = new Product { Name = "示例产品", Price = 100.00m };
dbContext.Products.Add(product);
await dbContext.SaveChangesAsync();  // 审计记录包含用户ID和用户名
```

### 查询审计记录

```csharp
// 查找与特定实体相关的所有审计记录
var entityAudits = await dbContext.AuditTrails
    .Where(a => a.EntityId == "123" && a.EntityType == "User")
    .OrderBy(a => a.CreatedAt)
    .ToListAsync();

// 显示审计历史
foreach (var audit in entityAudits)
{
    Console.WriteLine($"操作: {audit.AuditType}, 时间: {audit.CreatedAt}, 用户: {audit.Username}");
    
    if (audit.Changes != null)
    {
        Console.WriteLine("变更:");
        foreach (var change in audit.Changes)
        {
            Console.WriteLine($"  {change.PropertyName}: 旧值 = {change.OldValue}, 新值 = {change.NewValue}");
        }
    }
}
```

## 📄 API 参考

### AuditTrailEntry 类

表示单个审计记录的主要类：

```csharp
public class AuditTrailEntry
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public AuditType AuditType { get; set; }  // 创建、更新或删除
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string? TableName { get; set; }
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
    public List<AuditChange>? Changes { get; set; }
    public List<string>? AffectedColumns { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    // ... 其他属性
}
```

## 🔄 与其他 Linger 库集成

`Linger.EFCore.Audit` 可与以下组件完美集成：

- `Linger.EFCore`: 增强的 EF Core 实用工具和扩展
- `Linger.Audit`: 更广泛的审计功能

## 📜 许可证

本项目根据 Linger 项目提供的许可条款授权。
