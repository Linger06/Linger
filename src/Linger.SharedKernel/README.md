# Linger.SharedKernel

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

## Overview

Linger.SharedKernel provides the core domain primitives and shared abstractions for building applications with the Linger framework. It serves as a foundation for implementing Domain-Driven Design (DDD) patterns across multiple projects.

## Features

- Base search models for building rich query capabilities
- Expression-based filtering system
- Pagination support with sorting and filtering
- Reusable domain abstractions
- Cross-cutting concerns like searching and pagination

## Installation

```shell
dotnet add package Linger.SharedKernel
```

## Usage

### Basic Search Implementation

```csharp
// Create a domain-specific search model
public class UserSearch : BaseSearch
{
    public string? Username { get; set; }
    public DateTime? RegistrationDateFrom { get; set; }
    public DateTime? RegistrationDateTo { get; set; }
    
    // Optional: Override methods to add custom filtering logic
    public override Expression<Func<User, bool>> GetSearchModelExpression<User>()
    {
        var expression = base.GetSearchModelExpression<User>();
        
        // Add custom conditions
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

### Implementing Paged Searches

```csharp
// Create a paged search model
public class ProductPagedSearch : BaseSearchPageList
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Category { get; set; }
}

// Using the search model in a repository or service
public class ProductService
{
    private readonly IRepository<Product> _repository;
    
    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }
    
    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductPagedSearch search)
    {
        // The BaseSearchPageList properties will handle pagination
        var expression = search.GetSearchModelExpression<Product>();
        
        // Apply the search expression
        var query = _repository.Query().Where(expression);
        
        // Apply sorting if specified
        if (!string.IsNullOrEmpty(search.Sorting))
        {
            query = query.ApplySorting(search.Sorting);
        }
        
        // Apply pagination
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

### Advanced Filtering with Conditions

```csharp
var search = new BaseSearch();

// Add conditions programmatically
search.SearchParameters.Add(new Condition 
{
    Field = "Name",
    Operator = Operators.Contains,
    Value = "John"
});

search.SearchParameters.Add(new Condition 
{
    Field = "Age",
    Operator = Operators.GreaterThan,
    Value = 18
});

// Add OR conditions
search.SearchOrParameters.Add(new Condition 
{
    Field = "Email",
    Operator = Operators.EndsWith,
    Value = "@gmail.com"
});

search.SearchOrParameters.Add(new Condition 
{
    Field = "Email",
    Operator = Operators.EndsWith,
    Value = "@outlook.com"
});

// Apply to a LINQ query
var expression = search.GetSearchModelExpression<Customer>();
var results = dbContext.Customers.Where(expression).ToList();
```

## Dependencies

- Linger (core package)

## Target Frameworks

- .NET Framework 4.7.2+
- .NET Standard 2.0+
- .NET 8.0+
- .NET 9.0+