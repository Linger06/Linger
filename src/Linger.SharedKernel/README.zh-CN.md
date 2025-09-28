# Linger.SharedKernel

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

## 概述

Linger.SharedKernel 提供了使用 Linger 框架构建应用程序的核心领域原语和共享抽象。它作为跨多个项目实现领域驱动设计 (DDD) 模式的基础。

## 功能特点

- 用于构建丰富查询功能的基础搜索模型
- 基于表达式的过滤系统
- 带排序和过滤的分页支持
- 可重用的领域抽象
- 搜索和分页等横切关注点

## 安装

```shell
dotnet add package Linger.SharedKernel
```

## 使用方法

### 基本搜索实现

```csharp
// 创建特定领域的搜索模型
public class UserSearch : BaseSearch
{
    public string? Username { get; set; }
    public DateTime? RegistrationDateFrom { get; set; }
    public DateTime? RegistrationDateTo { get; set; }
    
    // 可选：重写方法添加自定义过滤逻辑
    public override Expression<Func<User, bool>> GetSearchModelExpression<User>()
    {
        var expression = base.GetSearchModelExpression<User>();
        
        // 添加自定义条件
        if (!Username.IsNullOrEmpty())
        {
            this.SearchParameters.Add(new Condition
            {
                Field = nameof(Username),
                Operator = Operators.Contains,
                Value = Username
            });
        }
        
        if (RegistrationDateFrom.HasValue)
        {
            this.SearchParameters.Add(new Condition
            {
                Field = nameof(RegistrationDate),
                Operator = Operators.GreaterThanOrEqual,
                Value = RegistrationDateFrom
            });
        }
        
        return expression;
    }
}
```

### 实现分页搜索

```csharp
// 创建分页搜索模型
public class ProductPagedSearch : BaseSearchPageList
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Category { get; set; }
}

// 在存储库或服务中使用搜索模型
public class ProductService
{
    private readonly IRepository<Product> _repository;
    
    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }
    
    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductPagedSearch search)
    {
        // BaseSearchPageList 属性将处理分页
        var expression = search.GetSearchModelExpression<Product>();
        
        // 应用搜索表达式
        var query = _repository.Query().Where(expression);
        
        // 如果指定了排序，则应用排序
        if (!string.IsNullOrEmpty(search.Sorting))
        {
            query = query.ApplySorting(search.Sorting);
        }
        
        // 应用分页
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((search.PageIndex - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync();
            
        return new PagedResult<ProductDto>(
            items.Select(p => new ProductDto(p)),
            totalCount,
            search.PageIndex,
            search.PageSize
        );
    }
}
```

### 使用条件进行高级过滤

```csharp
var search = new BaseSearch();

// 以编程方式添加条件
search.SearchParameters.Add(new Condition 
{
    Field = "Name",
    Operator = Operators.Contains,
    Value = "张三"
});

search.SearchParameters.Add(new Condition 
{
    Field = "Age",
    Operator = Operators.GreaterThan,
    Value = 18
});

// 添加 OR 条件
search.SearchOrParameters.Add(new Condition 
{
    Field = "Email",
    Operator = Operators.EndsWith,
    Value = "@qq.com"
});

search.SearchOrParameters.Add(new Condition 
{
    Field = "Email",
    Operator = Operators.EndsWith,
    Value = "@163.com"
});

// 应用到 LINQ 查询
var expression = search.GetSearchModelExpression<Customer>();
var results = dbContext.Customers.Where(expression).ToList();
```

## 依赖项

- Linger (核心包)

## 目标框架

- .NET Framework 4.7.2+
- .NET Standard 2.0+
- .NET 8.0+
- .NET 9.0+
