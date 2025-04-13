using System.Linq.Expressions;
using Linger.Extensions;
using Xunit.v3;

namespace Linger.UnitTests.Extensions;

public class ExpressionExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    [Fact]
    public void And_CombinesTwoExpressions_WithAndAlsoOperator()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Age > 18;
        
        // Act
        var combined = expr1.And(expr2);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Age = 20 }));
        Assert.False(func(new TestEntity { Id = 1, Age = 17 }));
        Assert.False(func(new TestEntity { Id = 0, Age = 20 }));
        Assert.False(func(new TestEntity { Id = 0, Age = 17 }));
    }
    
    [Fact]
    public void Or_CombinesTwoExpressions_WithOrElseOperator()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Age > 18;
        
        // Act
        var combined = expr1.Or(expr2);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Age = 20 }));
        Assert.True(func(new TestEntity { Id = 1, Age = 17 }));
        Assert.True(func(new TestEntity { Id = 0, Age = 20 }));
        Assert.False(func(new TestEntity { Id = 0, Age = 17 }));
    }
    
    [Fact]
    public void AndIf_WhenConditionIsTrue_CombinesExpressions()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Age > 18;
        
        // Act
        var combined = expr1.AndIf(true, expr2);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Age = 20 }));
        Assert.False(func(new TestEntity { Id = 1, Age = 17 }));
    }
    
    [Fact]
    public void AndIf_WhenConditionIsFalse_DoesNotCombineExpressions()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Age > 18;
        
        // Act
        var combined = expr1.AndIf(false, expr2);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Age = 17 })); // 只检查Id，忽略Age条件
        Assert.False(func(new TestEntity { Id = 0, Age = 20 }));
    }
    
    [Fact]
    public void OrIf_WhenConditionIsTrue_CombinesExpressions()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Age > 18;
        
        // Act
        var combined = expr1.OrIf(true, expr2);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Age = 17 }));
        Assert.True(func(new TestEntity { Id = 0, Age = 20 }));
        Assert.False(func(new TestEntity { Id = 0, Age = 17 }));
    }
    
    [Fact]
    public void OrIf_WhenConditionIsFalse_DoesNotCombineExpressions()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Age > 18;
        
        // Act
        var combined = expr1.OrIf(false, expr2);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Age = 17 }));
        Assert.False(func(new TestEntity { Id = 0, Age = 20 })); // 只检查Id，忽略Age条件
    }
    
    [Fact]
    public void Compose_WithAndAlso_CombinesExpressions()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Name == "测试";
        
        // Act
        var combined = expr1.Compose(expr2, Expression.AndAlso);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Name = "测试" }));
        Assert.False(func(new TestEntity { Id = 1, Name = "其他" }));
        Assert.False(func(new TestEntity { Id = 0, Name = "测试" }));
    }
    
    [Fact]
    public void Compose_WithOrElse_CombinesExpressions()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> expr1 = e => e.Id > 0;
        Expression<Func<TestEntity, bool>> expr2 = e => e.Name == "测试";
        
        // Act
        var combined = expr1.Compose(expr2, Expression.OrElse);
        var func = combined.Compile();
        
        // Assert
        Assert.True(func(new TestEntity { Id = 1, Name = "测试" }));
        Assert.True(func(new TestEntity { Id = 1, Name = "其他" }));
        Assert.True(func(new TestEntity { Id = 0, Name = "测试" }));
        Assert.False(func(new TestEntity { Id = 0, Name = "其他" }));
    }
    
#if !NETFRAMEWORK || NET40_OR_GREATER
    [Fact]
    public void HasOrderBy_WithOrderedQuery_ReturnsTrue()
    {
        // Arrange
        var query = new List<TestEntity>
        {
            new() { Id = 1, Name = "John Doe", Age = 30 },
            new() { Id = 2, Name = "Linger", Age = 25 }
        }.AsQueryable().OrderBy(e => e.Name);
        
        // Act
        var hasOrderBy = query.HasOrderBy();
        
        // Assert
        Assert.True(hasOrderBy);
    }
    
    [Fact]
    public void HasOrderBy_WithOrderByDescending_ReturnsTrue()
    {
        // Arrange
        var query = new List<TestEntity>
        {
            new() { Id = 1, Name = "John Doe", Age = 30 },
            new() { Id = 2, Name = "Linger", Age = 25 }
        }.AsQueryable().OrderByDescending(e => e.Age);
        
        // Act
        var hasOrderBy = query.HasOrderBy();
        
        // Assert
        Assert.True(hasOrderBy);
    }
    
    [Fact]
    public void HasOrderBy_WithUnorderedQuery_ReturnsFalse()
    {
        // Arrange
        var query = new List<TestEntity>
        {
            new() { Id = 1, Name = "John Doe", Age = 30 },
            new() { Id = 2, Name = "Linger", Age = 25 }
        }.AsQueryable();
        
        // Act
        var hasOrderBy = query.HasOrderBy();
        
        // Assert
        Assert.False(hasOrderBy);
    }
    
    [Fact]
    public void HasOrderBy_WithWhereClause_ReturnsFalse()
    {
        // Arrange
        var query = new List<TestEntity>
        {
            new() { Id = 1, Name = "John Doe", Age = 30 },
            new() { Id = 2, Name = "Linger", Age = 25 }
        }.AsQueryable().Where(e => e.Age > 25);
        
        // Act
        var hasOrderBy = query.HasOrderBy();
        
        // Assert
        Assert.False(hasOrderBy);
    }
#endif
}