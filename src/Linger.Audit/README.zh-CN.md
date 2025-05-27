# Linger.Audit

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

[![NuGet](https://img.shields.io/nuget/v/Linger.Audit.svg)](https://www.nuget.org/packages/Linger.Audit/)
[![Downloads](https://img.shields.io/nuget/dt/Linger.Audit.svg)](https://www.nuget.org/packages/Linger.Audit/)
[![License](https://img.shields.io/github/license/lingershub/linger.audit)](LICENSE)

一个轻量级的 .NET 审计库，提供实体审计的基类和接口。

## ✨ 功能特点

- 支持多目标框架 (.NET 9.0/.NET 8.0/.NET 6.0/NetStandard 2.0)
- 完整的审计跟踪（创建、修改、删除）
- 支持泛型实体和类型安全的 ID
- 软删除功能
- 内置审计时间戳和用户跟踪
- 启用可空引用类型
- MIT 许可证

## 📦 安装

### 通过 Visual Studio
1. 打开`解决方案资源管理器`
2. 右键点击您的项目
3. 选择`管理 NuGet 包...`
4. 点击`浏览`选项卡并搜索"Linger.Audit"
5. 点击`安装`

### 通过 Package Manager Console
```powershell
Install-Package Linger.Audit
```

### 通过 .NET CLI
```bash
dotnet add package Linger.Audit
```

## 🚀 快速开始

### 基础实体

继承基础实体类以获取 ID 属性：

```csharp
// 具有 Guid 类型 ID 的简单实体
public class Product : BaseEntity<Guid>
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string Description { get; set; } = null!;
}

// 具有 int 类型 ID 的实体
public class Category : BaseEntity<int>
{
    public string Name { get; set; } = null!;
}

// 具有字符串类型 ID 的实体
public class Tag : BaseEntity<string>
{
    public string Value { get; set; } = null!;
}
```

### 创建审计实体

跟踪创建时间和创建者：

```csharp
// 记录何时何人创建了评论
public class Comment : CreationAuditEntity<Guid>
{
    public string Text { get; set; } = null!;
    public Guid ProductId { get; set; }
    
    // 继承的属性:
    // public string? CreatorId { get; set; }
    // public DateTimeOffset CreationTime { get; set; }
}
```

### 完整审计实体

跟踪创建、修改和删除信息：

```csharp
// 完整审计跟踪的用户实体
public class User : FullAuditEntity<Guid>
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    
    // 继承的属性:
    // 创建
    // public string? CreatorId { get; set; }
    // public DateTimeOffset CreationTime { get; set; }
    
    // 修改
    // public string? LastModifierId { get; set; }
    // public DateTimeOffset? LastModificationTime { get; set; }
    
    // 删除
    // public bool IsDeleted { get; set; }
    // public string? DeleterId { get; set; }
    // public DateTimeOffset? DeletionTime { get; set; }
}
```

## 💡 使用示例

### 设置当前用户上下文

在您的应用程序中设置当前用户信息，以便自动填充审计字段：

```csharp
// 在您的应用服务中
public class ProductService : IProductService
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IAuditUserProvider _auditUserProvider;
    
    public ProductService(IRepository<Product, Guid> productRepository, IAuditUserProvider auditUserProvider)
    {
        _productRepository = productRepository;
        _auditUserProvider = auditUserProvider;
    }
    
    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            // ID, CreatorId 和 CreationTime 将在保存时自动设置
        };
        
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();
        
        return product;
    }
}
```

### 与 EF Core 集成

```csharp
// EF Core 中处理审计字段的示例
public class AppDbContext : DbContext
{
    private readonly IAuditUserProvider _auditUserProvider;
    
    public AppDbContext(DbContextOptions options, IAuditUserProvider auditUserProvider) 
        : base(options)
    {
        _auditUserProvider = auditUserProvider;
    }
    
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateAuditFields()
    {
        var userId = _auditUserProvider.GetUser();
        var now = DateTimeOffset.UtcNow;
        
        foreach (var entry in ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is ICreationAuditEntity creationAuditEntity)
                {
                    creationAuditEntity.CreationTime = now;
                    creationAuditEntity.CreatorId = userId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is IModificationAuditEntity modificationAuditEntity)
                {
                    modificationAuditEntity.LastModificationTime = now;
                    modificationAuditEntity.LastModifierId = userId;
                }
            }
            else if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDeleteEntity)
            {
                // 转换为软删除
                entry.State = EntityState.Modified;
                softDeleteEntity.IsDeleted = true;
                
                if (entry.Entity is IDeletionAuditEntity deletionAuditEntity)
                {
                    deletionAuditEntity.DeletionTime = now;
                    deletionAuditEntity.DeleterId = userId;
                }
            }
        }
    }
}
```

### 软删除过滤

使用全局查询过滤器自动过滤软删除的实体：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 为所有实现 ISoftDelete 的实体应用软删除过滤器
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.PropertyOrField(parameter, nameof(ISoftDelete.IsDeleted));
            var condition = Expression.Not(property);
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
```

## 🧩 类图概览

主要类和接口之间的关系：

```
BaseEntity<T>
    |
    ├─ CreationAuditEntity<T>
    |      |
    |      ├─ AuditEntity<T>
    |      |      |
    |      |      └─ FullAuditEntity<T>
    |      |
    |      └─ [Custom Entity]
    |
    └─ [Custom Entity]
```

## 📋 接口和基类参考

### IEntity\<T\> 接口

定义具有类型化 ID 的实体：

```csharp
public interface IEntity<T> : IEntity
{
    T Id { get; set; }
}
```

### ISoftDelete 接口

启用软删除功能：

```csharp
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
```

### BaseEntity 类

实体的基本基类：

```csharp
public abstract class BaseEntity<T> : IEntity<T>
{
    public T Id { get; set; } = default!;
}
```

### CreationAuditEntity 类

跟踪创建信息的基类：

```csharp
public abstract class CreationAuditEntity : ICreationAuditEntity
{
    public string? CreatorId { get; set; }
    public DateTimeOffset CreationTime { get; set; }
}
```

### AuditEntity 类

跟踪创建和修改信息的基类：

```csharp
public abstract class AuditEntity : CreationAuditEntity, IModificationAuditEntity
{
    public string? LastModifierId { get; set; }
    public DateTimeOffset? LastModificationTime { get; set; }
}
```

### FullAuditEntity 类

跟踪创建、修改和删除信息的基类：

```csharp
public abstract class FullAuditEntity : AuditEntity, IDeletionAuditEntity, ISoftDelete
{
    public bool IsDeleted { get; set; }
    public string? DeleterId { get; set; }
    public DateTimeOffset? DeletionTime { get; set; }
}
```

## 📜 许可证

此项目使用 MIT 许可证 - 有关详细信息，请参阅 [LICENSE](LICENSE) 文件。
