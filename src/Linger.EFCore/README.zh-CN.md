# Linger.EFCore

一个为 .NET 9.0 和 .NET 8.0 提供增强查询过滤功能和属性转换扩展的 C# Entity Framework Core 辅助库。

## 介绍

Linger.EFCore 通过强大的功能扩展了 Entity Framework Core，包括全局查询过滤器和属性类型转换，使处理复杂数据类型和过滤场景变得更加容易。

## 功能特点

### 全局查询过滤器
- 根据特定接口自动应用过滤器
- 基于属性值应用过滤器
- 类型安全的过滤表达式
- 支持所有 Entity Framework Core 查询场景

### 属性转换
- 支持复杂类型的 JSON 序列化
- 字符串集合的转换
- 自定义值比较器
- 灵活的配置选项

## 安装

### 通过 NuGet

```bash
dotnet add package Linger.EFCore
```

### 通过 Package Manager

```powershell
Install-Package Linger.EFCore
```

## 使用示例

### JSON 属性转换

轻松地将复杂对象序列化为 JSON 并存储在数据库中：

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public UserPreferences Preferences { get; set; }
}

public class UserPreferences
{
    public bool DarkMode { get; set; }
    public string[] FavoriteTags { get; set; }
    public Dictionary<string, string> CustomSettings { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 配置 Preferences 属性使用 JSON 转换
    modelBuilder.Entity<User>()
        .Property(u => u.Preferences)
        .HasJsonConversion();
}
```

### 集合属性转换

轻松处理集合类型：

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> Tags { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 将字符串集合转换为以分隔符分隔的字符串
    modelBuilder.Entity<Product>()
        .Property(p => p.Tags)
        .HasStringCollectionConversion();
}
```

### 分页扩展

简化分页操作：

```csharp
// 在控制器中
public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    var query = _dbContext.Products.AsQueryable();
    
    // 应用过滤条件
    if (!string.IsNullOrEmpty(searchTerm))
    {
        query = query.Where(p => p.Name.Contains(searchTerm));
    }
    
    // 应用排序
    query = query.OrderBy(p => p.Name);
    
    // 应用分页并执行查询
    var pagedResult = await query.ToPagedResultAsync(page, pageSize);
    
    // 映射到 DTO
    var mappedResult = pagedResult.Map(p => _mapper.Map<ProductDto>(p));
    
    return Ok(mappedResult);
}
```

### 基于接口的过滤

自动应用软删除过滤器：

```csharp
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}

public class Customer : ISoftDelete
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 为所有实现 ISoftDelete 的实体应用全局过滤器
    modelBuilder.ApplyGlobalFilters<ISoftDelete>(e => !e.IsDeleted);
}
```

### 基于属性的过滤

```csharp
// 多租户过滤示例
public class ApplicationDbContext : DbContext 
{
    private readonly int _currentTenantId;
    public ApplicationDbContext(int currentTenantId)
    {
        _currentTenantId = currentTenantId;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 自动按租户过滤实体
        modelBuilder.ApplyGlobalFilters("TenantId", _currentTenantId);
    }
}
```

## 高级用法

### 自定义 JSON 转换选项

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    modelBuilder.Entity<User>()
        .Property(u => u.Preferences)
        .HasJsonConversion(jsonOptions);
}
```

### 组合过滤器

```csharp
public interface IMultiTenant
{
    string TenantId { get; set; }
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 应用软删除过滤器
    modelBuilder.ApplyGlobalFilters<ISoftDelete>(e => !e.IsDeleted);
    
    // 应用多租户过滤器
    var tenantId = _tenantService.GetCurrentTenantId();
    modelBuilder.ApplyGlobalFilters<IMultiTenant>(e => e.TenantId == tenantId);
}
```

## 依赖项

- Microsoft.EntityFrameworkCore
- System.Text.Json

## 贡献

欢迎贡献！请随时提交 Pull Request。
