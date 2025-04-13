using Xunit.v3;

namespace Linger.UnitTests;

public class ParameterListTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateEmptyParameterList()
    {
        // Arrange & Act
        var parameterList = new ParameterList();
        
        // Assert
        Assert.Empty(parameterList.Parameters);
    }
    
    [Fact]
    public void Constructor_WithKeyAndData_ShouldAddParameter()
    {
        // Arrange & Act
        var parameterList = new ParameterList("key1", "value1");
        
        // Assert
        Assert.Single(parameterList.Parameters);
        Assert.Equal("value1", parameterList.Parameters["key1"]);
    }
    
    [Fact]
    public void Constructor_WithArrays_ShouldAddParameters()
    {
        // Arrange & Act
        var keys = new[] { "key1", "key2", "key3" };
        var values = new object[] { "value1", 42, true };
        var parameterList = new ParameterList(keys, values);
        
        // Assert
        Assert.Equal(3, parameterList.Parameters.Count);
        Assert.Equal("value1", parameterList.Parameters["key1"]);
        Assert.Equal(42, parameterList.Parameters["key2"]);
        Assert.Equal(true, parameterList.Parameters["key3"]);
    }
    
    [Fact]
    public void Constructor_WithArraysOfDifferentLength_ShouldThrowArgumentException()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var values = new object[] { "value1", 42 };
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ParameterList(keys, values));
    }
    
    [Fact]
    public void Add_WithNewKey_ShouldAddParameter()
    {
        // Arrange
        var parameterList = new ParameterList();
        
        // Act
        parameterList.Add("key1", "value1");
        
        // Assert
        Assert.Single(parameterList.Parameters);
        Assert.Equal("value1", parameterList.Parameters["key1"]);
    }
    
    [Fact]
    public void Add_WithExistingKey_ShouldThrowArgumentException()
    {
        // Arrange
        var parameterList = new ParameterList("key1", "value1");
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => parameterList.Add("key1", "value2"));
    }
    
    [Fact]
    public void Get_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var parameterList = new ParameterList("key1", "value1");
        
        // Act
        var value = parameterList.Get<string>("key1");
        
        // Assert
        Assert.Equal("value1", value);
    }
    
    [Fact]
    public void Get_WithNonExistingKey_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var parameterList = new ParameterList();
        
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => parameterList.Get<string>("key1"));
    }
    
    [Fact]
    public void Remove_WithExistingKey_ShouldRemoveParameterAndReturnTrue()
    {
        // Arrange
        var parameterList = new ParameterList("key1", "value1");
        
        // Act
        var result = parameterList.Remove("key1");
        
        // Assert
        Assert.True(result);
        Assert.Empty(parameterList.Parameters);
    }
    
    [Fact]
    public void Remove_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var parameterList = new ParameterList();
        
        // Act
        var result = parameterList.Remove("key1");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Clear_ShouldRemoveAllParameters()
    {
        // Arrange
        var parameterList = new ParameterList();
        parameterList.Add("key1", "value1");
        parameterList.Add("key2", "value2");
        
        // Act
        parameterList.Clear();
        
        // Assert
        Assert.Empty(parameterList.Parameters);
    }
    
    [Fact]
    public void ContainsKey_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var parameterList = new ParameterList("key1", "value1");
        
        // Act
        var result = parameterList.ContainsKey("key1");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ContainsKey_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var parameterList = new ParameterList();
        
        // Act
        var result = parameterList.ContainsKey("key1");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void SetValue_WithExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var parameterList = new ParameterList("key1", "value1");
        
        // Act
        parameterList.SetValue("key1", "updated");
        
        // Assert
        Assert.Equal("updated", parameterList.Parameters["key1"]);
    }
    
    [Fact]
    public void SetValue_WithNonExistingKey_ShouldAddParameter()
    {
        // Arrange
        var parameterList = new ParameterList();
        
        // Act
        parameterList.SetValue("key1", "value1");
        
        // Assert
        Assert.Equal("value1", parameterList.Parameters["key1"]);
    }
    
    [Fact]
    public void GetEnumerator_ShouldEnumerateParameters()
    {
        // Arrange
        var parameterList = new ParameterList();
        parameterList.Add("key1", "value1");
        parameterList.Add("key2", "value2");
        
        // Act
        var count = 0;
        foreach (var parameter in parameterList)
        {
            count++;
        }
        
        // Assert
        Assert.Equal(2, count);
    }
}