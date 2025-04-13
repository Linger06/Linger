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
    public void GetPropertyName_SimpleLambdaExpression_ShouldReturnPropertyName()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Name;
        
        // Act
        var propertyName = expression.GetPropertyName();
        
        // Assert
        Assert.Equal("Name", propertyName);
    }

    [Fact]
    public void GetPropertyName_NestedLambdaExpression_ShouldReturnFullPropertyPath()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Child.Name;
        
        // Act
        var propertyName = expression.GetPropertyName();
        
        // Assert
        Assert.Equal("Child.Name", propertyName);
    }

    [Fact]
    public void GetPropertyName_DeepNestedLambdaExpression_ShouldReturnFullPropertyPath()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Child.GrandChild.Description;
        
        // Act
        var propertyName = expression.GetPropertyName();
        
        // Assert
        Assert.Equal("Child.GrandChild.Description", propertyName);
    }

    [Fact]
    public void GetPropertyName_WithGetAllFalse_ShouldReturnOnlyLastProperty()
    {
        // Arrange
        Expression<Func<TestModel, string>> expression = x => x.Child.Name;
        
        // Act
        var propertyName = expression.GetPropertyName(getAll: false);
        
        // Assert
        Assert.Equal("Name", propertyName);
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
}
