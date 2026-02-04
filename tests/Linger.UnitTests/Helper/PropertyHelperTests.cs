using System.Linq.Expressions;
using Linger.Helper;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class PropertyHelperTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public TestChild Child { get; set; } = new();
        public TestChild[] Children { get; set; } = Array.Empty<TestChild>();
        public List<TestChild> ChildList { get; set; } = new();
    }

    private class TestChild
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public TestGrandChild GrandChild { get; set; } = new();
    }

    private class TestGrandChild
    {
        public string Description { get; set; } = string.Empty;
    }

    private class TestModelWithAttribute
    {
        [TestAttribute]
        public string IgnoredProperty { get; set; } = string.Empty;
        
        public string NormalProperty { get; set; } = string.Empty;
    }

    [AttributeUsage(AttributeTargets.Property)]
    private class TestAttribute : Attribute
    {
    }

    [Fact]
    public void GetMemberExp_SimpleProperty_ShouldReturnCorrectExpression()
    {
        // Arrange
        var paramExp = Expression.Parameter(typeof(TestModel), "x");
        
        // Act
        var memberExp = paramExp.GetMemberExp("Name");
        
        // Assert
        Assert.NotNull(memberExp);
        Assert.Equal(typeof(string), memberExp.Type);
        Assert.Equal("x.Name", memberExp.ToString());
    }

    [Fact]
    public void GetMemberExp_NestedProperty_ShouldReturnCorrectExpression()
    {
        // Arrange
        var paramExp = Expression.Parameter(typeof(TestModel), "x");
        
        // Act
        var memberExp = paramExp.GetMemberExp("Child,Name");
        
        // Assert
        Assert.NotNull(memberExp);
        Assert.Equal(typeof(string), memberExp.Type);
        Assert.Equal("x.Child.Name", memberExp.ToString());
    }

    [Fact]
    public void GetPropertyInfo_SimpleLambdaExpression_ShouldReturnPropertyInfo()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Name;
        
        // Act
        var propertyInfo = expression.GetPropertyInfo();
        
        // Assert
        Assert.NotNull(propertyInfo);
        Assert.Equal("Name", propertyInfo.Name);
        Assert.Equal(typeof(string), propertyInfo.PropertyType);
    }

    [Fact]
    public void GetPropertyInfo_GenericMethod_ShouldReturnPropertyInfo()
    {
        // Arrange & Act
        var propertyInfo = PropertyHelper.GetPropertyInfo((TestModel x) => x.Name);
        
        // Assert
        Assert.NotNull(propertyInfo);
        Assert.Equal("Name", propertyInfo.Name);
        Assert.Equal(typeof(string), propertyInfo.PropertyType);
    }

    [Fact]
    public void TrySetProperty_WithValueFactory_ShouldSetProperty()
    {
        // Arrange
        var model = new TestModel { Name = "Original" };
        
        // Act
        PropertyHelper.TrySetProperty(model, x => x.Name, () => "Updated");
        
        // Assert
        Assert.Equal("Updated", model.Name);
    }

    [Fact]
    public void TrySetProperty_WithInstanceValueFactory_ShouldSetProperty()
    {
        // Arrange
        var model = new TestModel { Name = "Original", Age = 25 };
        
        // Act
        PropertyHelper.TrySetProperty(model, x => x.Name, obj => $"Age: {obj.Age}");
        
        // Assert
        Assert.Equal("Age: 25", model.Name);
    }

    [Fact]
    public void TrySetProperty_WithIgnoredAttribute_ShouldNotSetProperty()
    {
        // Arrange
        var model = new TestModelWithAttribute { IgnoredProperty = "Original" };
        
        // Act
        PropertyHelper.TrySetProperty(model, x => x.IgnoredProperty, () => "Updated", typeof(TestAttribute));
        
        // Assert
        Assert.Equal("Original", model.IgnoredProperty);
    }

    [Fact]
    public void TrySetProperty_WithoutIgnoredAttribute_ShouldSetProperty()
    {
        // Arrange
        var model = new TestModelWithAttribute { NormalProperty = "Original" };
        
        // Act
        PropertyHelper.TrySetProperty(model, x => x.NormalProperty, () => "Updated", typeof(TestAttribute));
        
        // Assert
        Assert.Equal("Updated", model.NormalProperty);
    }

    [Fact]
    public void GetMemberExp_DeeplyNestedProperty_ShouldReturnCorrectExpression()
    {
        // Arrange
        var paramExp = Expression.Parameter(typeof(TestModel), "x");
        
        // Act
        var memberExp = paramExp.GetMemberExp("Child,GrandChild,Description");
        
        // Assert
        Assert.NotNull(memberExp);
        Assert.Equal(typeof(string), memberExp.Type);
        Assert.Equal("x.Child.GrandChild.Description", memberExp.ToString());
    }

    [Fact]
    public void GetPropertyName_NullExpression_ShouldReturnEmptyString()
    {
        // Arrange
        Expression? expression = null;
        
        // Act
        var result = expression!.GetPropertyName();
        
        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetPropertyName_SimpleMemberExpression_ShouldReturnPropertyName()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Name;
        
        // Act
        var result = expression.GetPropertyName();
        
        // Assert
        Assert.Equal("Name", result);
    }

    [Fact]
    public void GetPropertyName_NestedMemberExpression_ShouldReturnFullPath()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Child.Name;
        
        // Act
        var result = expression.GetPropertyName(getAll: true);
        
        // Assert
        Assert.Equal("Child.Name", result);
    }

    [Fact]
    public void GetPropertyName_NestedMemberExpression_WithGetAllFalse_ShouldReturnOnlyPropertyName()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Child.Name;
        
        // Act
        var result = expression.GetPropertyName(getAll: false);
        
        // Assert
        Assert.Equal("Name", result);
    }

    [Fact]
    public void GetPropertyName_WithUnaryExpression_ShouldReturnPropertyName()
    {
        // Arrange - object 会导致 UnaryExpression 包装
        Expression<Func<TestModel, object>> expression = x => x.Age;
        
        // Act
        var result = expression.GetPropertyName();
        
        // Assert
        Assert.Equal("Age", result);
    }

    [Fact]
    public void GetPropertyInfo_WithUnaryExpression_ShouldReturnPropertyInfo()
    {
        // Arrange - object 会导致 UnaryExpression 包装
        Expression<Func<TestModel, object>> expression = x => x.Age;
        
        // Act
        var propertyInfo = expression.GetPropertyInfo();
        
        // Assert
        Assert.NotNull(propertyInfo);
        Assert.Equal("Age", propertyInfo.Name);
        Assert.Equal(typeof(int), propertyInfo.PropertyType);
    }

    [Fact]
    public void GetPropertyInfo_DirectMemberExpression_ShouldReturnPropertyInfo()
    {
        // Arrange
        var paramExp = Expression.Parameter(typeof(TestModel), "x");
        var memberExp = Expression.Property(paramExp, "Name");
        
        // Act
        var propertyInfo = memberExp.GetPropertyInfo();
        
        // Assert
        Assert.NotNull(propertyInfo);
        Assert.Equal("Name", propertyInfo.Name);
    }

    [Fact]
    public void TrySetProperty_WithNonMemberAccessExpression_ShouldNotThrow()
    {
        // Arrange
        var model = new TestModel { Name = "Original" };
        
        // Act - 尝试用非属性访问表达式
        PropertyHelper.TrySetProperty(model, x => x.ToString(), () => "Updated");
        
        // Assert - 不应抛出异常，属性应该保持不变
        Assert.Equal("Original", model.Name);
    }

    [Fact]
    public void TrySetProperty_WithMultipleIgnoredAttributes_ShouldRespectAll()
    {
        // Arrange
        var model = new TestModelWithAttribute { IgnoredProperty = "Original" };
        
        // Act
        PropertyHelper.TrySetProperty(model, x => x.IgnoredProperty, () => "Updated", typeof(TestAttribute));
        
        // Assert
        Assert.Equal("Original", model.IgnoredProperty);
    }

    [Fact]
    public void GetPropertyName_WithArrayIndexer_ShouldReturnPropertyName()
    {
        // Arrange
        var index = 0;
        Expression<Func<TestModel, string>> expression = x => x.Children[index].Name;
        
        // Act
        var result = expression.GetPropertyName();
        
        // Assert - GetPropertyName 返回包含索引的路径
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetPropertyName_DeeplyNestedPath_ShouldReturnFullPath()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Child.GrandChild.Description;
        
        // Act
        var result = expression.GetPropertyName(getAll: true);
        
        // Assert
        Assert.Equal("Child.GrandChild.Description", result);
    }
}
