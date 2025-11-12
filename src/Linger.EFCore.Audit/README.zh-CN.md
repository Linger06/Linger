# Linger.EFCore.Audit

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
// 1. 在你的 DbContext 中添加审计跟踪
public class AppDbContext : DbContext
{
    public DbSet<AuditTrailEntry> AuditTrails { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 应用审计配置
        modelBuilder.ApplyAudit();
    }
}

// 2. 实现审计用户提供程序
public class CurrentUserProvider : IAuditUserProvider 
{ 
    // 可以从当前认证系统获取用户信息
    public string? UserName => "张三"; 
    
    public string GetUser() => UserName ?? "匿名用户"; 
}

// 3. 注册服务和拦截器
services.AddScoped<IAuditUserProvider, CurrentUserProvider>();

services.AddDbContext<AppDbContext>((serviceProvider, options) => 
{
    options.UseSqlServer(connectionString);
    
    // 不带日志
    options.AddInterceptors(
        new AuditEntitiesSaveChangesInterceptor(
            serviceProvider.GetRequiredService<IAuditUserProvider>()
        )
    );
    
    // 或 带日志 (推荐 - 用于监控和调试)
    options.AddInterceptors(
        new AuditEntitiesSaveChangesInterceptor(
            serviceProvider.GetRequiredService<IAuditUserProvider>(),
            serviceProvider.GetRequiredService<ILogger<AuditEntitiesSaveChangesInterceptor>>()
        )
    );
});
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
await dbContext.SaveChangesAsync();  // 自动生成"创建"审计记录,包含用户信息

// 修改实体
user.Email = "new.email@example.com";
await dbContext.SaveChangesAsync();  // 自动生成"修改"审计记录

// 删除实体
dbContext.Users.Remove(user);
await dbContext.SaveChangesAsync();  // 自动生成"删除"审计记录(软删除)
```

### 查询审计记录

审计记录保存在 `AuditTrails` DbSet 中，可以通过多种方式查询：

```csharp
// 查找与特定实体相关的所有审计记录
var entityAudits = await dbContext.AuditTrails
    .Where(a => a.EntityId == "123" && a.EntityType == "User")
    .OrderBy(a => a.TimeStamp)
    .ToListAsync();

// 显示审计历史
foreach (var audit in entityAudits)
{
    Console.WriteLine($"操作: {audit.AuditType}, 时间: {audit.TimeStamp}, 用户: {audit.Username}");
    
    // 显示所有更改的属性
    if (audit.AffectedColumns != null)
    {
        Console.WriteLine("变更的属性:");
        foreach (var column in audit.AffectedColumns)
        {
            var oldValue = audit.OldValues?[column];
            var newValue = audit.NewValues?[column];
            Console.WriteLine($"  {column}: 旧值 = {oldValue}, 新值 = {newValue}");
        }
    }
}

// 按用户查询审计记录
var userAudits = await dbContext.AuditTrails
    .Where(a => a.Username == "张三")
    .OrderByDescending(a => a.TimeStamp)
    .Take(10)
    .ToListAsync();

// 查询特定时间范围内的审计记录
var startDate = DateTimeOffset.Now.AddDays(-7);
var endDate = DateTimeOffset.Now;

var recentAudits = await dbContext.AuditTrails
    .Where(a => a.TimeStamp >= startDate && a.TimeStamp <= endDate)
    .OrderBy(a => a.TimeStamp)
    .ToListAsync();
```
```

## 📄 审计跟踪数据

`AuditTrailEntry` 类是表示单个审计记录的主要类：

```csharp
public class AuditTrailEntry
{
    public long Id { get; set; }
    public string? Username { get; set; }
    public AuditType AuditType { get; set; }  // Added、Modified或Deleted
    public string EntityName { get; set; }
    public string? EntityId { get; set; }
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
    public List<string>? AffectedColumns { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public Dictionary<string, object>? Changes { get; set; }
    public IEnumerable<PropertyEntry>? TempProperties { get; set; }
}
```

`AuditTrailEntry` 捕获：
- 实体名称和 ID
- 变更类型（Added/Modified/Deleted）
- 执行变更的用户名
- 时间戳
- 属性的旧值和新值
- 已修改属性列表
```

## 🔍 自动跟踪

- 创建审计：CreatorId, CreationTime
- 修改审计：LastModifierId, LastModificationTime
- 软删除：IsDeleted, DeleterId, DeletionTime

## 📊 日志记录

拦截器支持可选的日志记录功能,帮助监控审计操作和排查问题。

### 配置日志级别

在 `appsettings.json` 中配置:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Linger.EFCore.Audit.Interceptors": "Debug"
    }
  }
}
```