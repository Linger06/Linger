# Linger.Audit

> 破坏性变更与 2.0 迁移说明见 [Linger 迁移指南](../Linger/MIGRATION.zh-CN.md)。

一个轻量级的 .NET 审计库，提供实体审计的基类和接口。

## 📖 目录

- [✨ 功能特点](#-功能特点)
- [📦 安装](#-安装)
- [🚀 快速开始](#-快速开始)
  - [基础实体](#基础实体)
  - [创建审计实体](#创建审计实体)
  - [完整审计实体](#完整审计实体)
- [持久化集成](#持久化集成)
- [🧩 类图概览](#-类图概览)
- [📋 接口和基类参考](#-接口和基类参考)
- [📜 许可证](#-许可证)

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
    // public string CreatorId { get; set; }
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
    // public string CreatorId { get; set; }
    // public DateTimeOffset CreationTime { get; set; }

    // 修改
    // public string? LastModifierId { get; set; }
    // public DateTimeOffset? LastModificationTime { get; set; }

    // 删除
    // public bool? IsDeleted { get; set; }
    // public string? DeleterId { get; set; }
    // public DateTimeOffset? DeletionTime { get; set; }
}
```

## 持久化集成

`Linger.Audit` 只定义审计契约和实体基类，不会接管持久化流程，也不会自动填充审计字段。

EF Core 项目应使用 [Linger.EFCore.Audit](../Linger.EFCore.Audit/README.zh-CN.md)。其中的 `AuditEntitiesSaveChangesInterceptor` 会填充创建、修改、删除和用户字段，并把 `ISoftDelete` 实体的删除操作转换为更新。需要在查询中排除软删除实体时，请参阅 [Linger.EFCore 全局查询过滤器文档](../Linger.EFCore/README.zh-CN.md#全局查询过滤器)。

使用其他持久化技术时，应在对应的保存管道中填充这些契约。

遗留数据库列类型和 `DateTimeOffset` 转换请参阅 [Linger.EFCore.Audit](../Linger.EFCore.Audit/README.zh-CN.md) 的高级配置章节。

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
    bool? IsDeleted { get; set; }
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
    public string CreatorId { get; set; }
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
public abstract class FullAuditEntity : AuditEntity, ISoftDelete
{
    public bool? IsDeleted { get; set; }
    public string? DeleterId { get; set; }
    public DateTimeOffset? DeletionTime { get; set; }
}
```

## 📜 许可证

此项目使用 MIT 许可证。
