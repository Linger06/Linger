# Linger.EFCore.Audit

> 破坏性变更与 2.0 迁移说明见 [Linger 迁移指南](../Linger/MIGRATION.zh-CN.md)。

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
    public string? GetUser() => "张三";
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

### 审计时间

拦截器默认使用 UTC 时间。若应用需要使用指定时区，注册 `IAuditTimeProvider`，拦截器会在每次保存时获取当前时间：

```csharp
public sealed class ChinaStandardTimeAuditTimeProvider : IAuditTimeProvider
{
    public DateTimeOffset GetNow() =>
        DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8));
}

services.AddSingleton<IAuditTimeProvider, ChinaStandardTimeAuditTimeProvider>();
```

原有的两参数构造函数仍然可用；未注册时间提供器时保持 UTC 行为。

### 保存失败后的重试与自增键

**保存失败会报错，不要求重试。** 审计拦截器不吞掉 `SaveChanges` / `SaveChangesAsync` 的异常，也不会自动再次保存。调用方可以报告错误、结束当前工作单元并释放 `DbContext`；无需为了清理审计记录而再次保存。

只有在确认上次保存未提交、已修正失败原因，且调用方主动用**同一个 `DbContext`** 再次保存时，才涉及待写审计行的清理。对于未实现 `ISoftDelete` 的实体，拦截器按本次变更重新生成自己的待写审计行，保留应用手动添加的审计行。实现了 `ISoftDelete` 的实体不在此去重范围内，即使本次操作只是普通修改。该机制不保证跨上下文去重；若上次提交结果未知，应先核实数据库状态，不要盲目重试。

`EntityId` 在**写库前**采集。只要主键的任一部分仍为临时值（例如插入时才由数据库生成的自增键），审计行就记录 `null`，插入成功后也不会回填。保存前已确定的键则会被记录，包括 InMemory 提供程序提前分配的键。

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
await dbContext.SaveChangesAsync();  // 软删除：生成 Modified 审计记录
```

### 软删除限制

只传主键调用 `Remove` 时，拦截器不会把未加载的业务列当作修改写回数据库。需要同时更改业务数据时，请先加载实体并显式修改。`CurrentValuesSnapshot` 来自跟踪对象，不会为只含主键的对象回查整行；需要完整审计快照时请先加载实体。配置了并发令牌时仍需提供正确的原始令牌。

对实现 `ISoftDelete` 的实体，请使用 `Remove` + `SaveChanges`，让拦截器将删除转换为更新并记录删除信息。**不要对这类实体调用 `ExecuteDelete` / `ExecuteDeleteAsync`**：它们直接执行数据库删除，不经过 ChangeTracker 或 `SaveChanges` 拦截器，会物理删除记录。需要批量软删除时，请显式执行将 `IsDeleted` 等字段更新为删除状态的操作，并自行处理审计字段和审计记录。

软删除后的查询隐藏依赖 EF Core 全局过滤器，需在 `OnModelCreating` 中显式调用 `ApplyGlobalFilters<ISoftDelete>(e => e.IsDeleted != true)`；`ApplyAudit()` 不会自动启用该过滤器。
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
    public Dictionary<string, object>? CurrentValuesSnapshot { get; set; }
    public List<string>? AffectedColumns { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
}
```

`AuditTrailEntry` 捕获：
- 实体名称和 ID
- 变更类型（Added/Modified/Deleted）
- 执行变更的用户名
- 时间戳
- 属性的旧值和新值
- 当前非主键属性的完整快照
- 已修改属性列表
```

## 🔍 自动跟踪

- 创建审计：CreatorId, CreationTime
- 修改审计：LastModifierId, LastModificationTime
- 软删除：IsDeleted, DeleterId, DeletionTime

## 🔧 高级配置

### 处理遗留数据库的 DateTime 类型

当现有数据库使用 `datetime` 而不是 `datetimeoffset` 时，应在 EF Core 实体映射中配置审计字段的类型转换：

```csharp
public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.Property(e => e.CreationTime)
            .HasColumnType("datetime")
            .HasConversion(
                value => value.ToDateTime(),
                value => new DateTimeOffset(value));

        entity.Property(e => e.LastModificationTime)
            .HasColumnType("datetime")
            .HasConversion(
                value => value.HasValue ? value.Value.ToDateTime() : (DateTime?)null,
                value => value.HasValue
                    ? new DateTimeOffset(value.Value, TimeSpan.Zero)
                    : (DateTimeOffset?)null);

        entity.Property(e => e.DeletionTime)
            .HasColumnType("datetime")
            .HasConversion(
                value => value.HasValue ? value.Value.ToDateTime() : (DateTime?)null,
                value => value.HasValue
                    ? new DateTimeOffset(value.Value, TimeSpan.Zero)
                    : (DateTimeOffset?)null);

        entity.Property(e => e.CreatorId).HasMaxLength(30).IsUnicode(false);
        entity.Property(e => e.LastModifierId).HasMaxLength(30).IsUnicode(false);
        entity.Property(e => e.DeleterId).HasMaxLength(30).IsUnicode(false);
    }
}
```

转换会丢失原始时区偏移量。应统一以 UTC 保存和读取这些值；`TimeSpan.Zero` 表示 UTC 偏移量。

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
