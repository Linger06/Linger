using Xunit.v3;

namespace Linger.UnitTests;

public class ParameterListTests
{
    #region ����������
    
    [Theory]
    [InlineData(true, "value1", null, "value1")]  // ���ڵļ�, ����ʵ��ֵ
    [InlineData(false, null, "default", "default")]  // �����ڵļ�, ����Ĭ��ֵ
    [InlineData(false, null, null, null)]  // �����ڵļ�, ��Ĭ��ֵ, ����null
    public void GetOrDefault_ShouldReturnCorrectValue(bool addKey, string keyValue, string defaultValue, string expected)
    {
        // Arrange
        var parameterList = new ParameterList();
        if (addKey)
        {
            parameterList.Add("key1", keyValue);
        }
        
        // Act
        var value = parameterList.GetOrDefault("key1", defaultValue);
        
        // Assert
        Assert.Equal(expected, value);
    }
    
    [Theory]
    [InlineData(true, 42, 0, 42)]  // ���ڵļ�, ����ʵ��ֵ
    [InlineData(false, 0, 99, 99)]  // �����ڵļ�, ����Ĭ��ֵ
    [InlineData(false, 0, 0, 0)]  // �����ڵļ�, ��������Ĭ��ֵ
    public void GetValueOrDefault_ShouldReturnCorrectValue(bool addKey, int keyValue, int defaultValue, int expected)
    {
        // Arrange
        var parameterList = new ParameterList();
        if (addKey)
        {
            parameterList.Add("key1", keyValue);
        }
        
        // Act
        var value = parameterList.GetValueOrDefault("key1", defaultValue);
        
        // Assert
        Assert.Equal(expected, value);
    }
    
    [Theory]
    [InlineData(true, "value1", true, "value1")]  // ���ڵļ�, ����true����ȷ��ֵ
    [InlineData(false, null, false, null)]  // �����ڵļ�, ����false��null
    public void TryGet_ShouldReturnCorrectResult(bool addKey, string keyValue, bool expectedResult, string expectedValue)
    {
        // Arrange
        var parameterList = new ParameterList();
        if (addKey)
        {
            parameterList.Add("key1", keyValue);
        }
        
        // Act
        var result = parameterList.TryGet("key1", out string? value);
        
        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedValue, value);
    }
    
    [Theory]
    [InlineData(true, 42, true, 42)]  // ���ڵļ�, ����true����ȷ��ֵ
    [InlineData(false, 0, false, 0)]  // �����ڵļ�, ����false��Ĭ��ֵ0
    public void TryGetValue_ShouldReturnCorrectResult(bool addKey, int keyValue, bool expectedResult, int expectedValue)
    {
        // Arrange
        var parameterList = new ParameterList();
        if (addKey)
        {
            parameterList.Add("key1", keyValue);
        }
        
        // Act
        var result = parameterList.TryGetValue("key1", out int value);
        
        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedValue, value);
    }
    
    [Theory]
    [InlineData("key1", "value1", true)]  // ���ڵļ�
    [InlineData("key2", "value2", false)]  // �����ڵļ�
    public void ContainsKey_ShouldReturnCorrectResult(string addKey, string value, bool expectedResult)
    {
        // Arrange
        var parameterList = new ParameterList(addKey, value);
        
        // Act
        var result = parameterList.ContainsKey("key1");
        
        // Assert
        Assert.Equal(expectedResult, result);
    }
    
    [Theory]
    [InlineData("key1", "value1", "key1", true, 0)]  // �Ƴ����ڵļ�
    [InlineData("key1", "value1", "key2", false, 1)]  // �Ƴ������ڵļ�
    public void Remove_ShouldReturnCorrectResult(string addKey, string value, string removeKey, 
        bool expectedResult, int expectedCount)
    {
        // Arrange
        var parameterList = new ParameterList(addKey, value);
        
        // Act
        var result = parameterList.Remove(removeKey);
        
        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(expectedCount, parameterList.Parameters.Count);
    }
    
    [Theory]
    [InlineData("key1", "initial", "updated")]  // �������м�
    [InlineData("key2", "initial", "updated")]  // ����¼�
    public void SetValue_ShouldUpdateOrAddParameter(string initialKey, string initialValue, string newValue)
    {
        // Arrange
        var parameterList = new ParameterList(initialKey, initialValue);
        
        // Act
        parameterList.SetValue(initialKey, newValue);
        
        // Assert
        Assert.Equal(newValue, parameterList.Parameters[initialKey]);
    }
    
    [Theory]
    [InlineData(0)]  // �ղ����б�
    [InlineData(1)]  // 1������
    [InlineData(3)]  // �������
    public void GetEnumerator_ShouldEnumerateCorrectNumberOfParameters(int parameterCount)
    {
        // Arrange
        var parameterList = new ParameterList();
        for (int i = 0; i < parameterCount; i++)
        {
            parameterList.Add($"key{i}", $"value{i}");
        }
        
        // Act
        var count = 0;
        foreach (var parameter in parameterList)
        {
            count++;
        }
        
        // Assert
        Assert.Equal(parameterCount, count);
    }
    
    [Theory]
    [InlineData(new[] {"key1"}, new object[] {"value1"})]
    [InlineData(new[] {"key1", "key2", "key3"}, new object[] {"value1", 42, true})]
    public void Constructor_WithArrays_ShouldAddParametersCorrectly(string[] keys, object[] values)
    {
        // Arrange & Act
        var parameterList = new ParameterList(keys, values);
        
        // Assert
        Assert.Equal(keys.Length, parameterList.Parameters.Count);
        for (int i = 0; i < keys.Length; i++)
        {
            Assert.Equal(values[i], parameterList.Parameters[keys[i]]);
        }
    }
    
    [Theory]
    [InlineData("key1", "value1")]
    public void Constructor_WithKeyAndData_ShouldAddParameter(string key, string value)
    {
        // Arrange & Act
        var parameterList = new ParameterList(key, value);
        
        // Assert
        Assert.Single(parameterList.Parameters);
        Assert.Equal(value, parameterList.Parameters[key]);
    }
    
    [Fact]
    public void Constructor_WithArraysOfDifferentLength_ShouldThrowArgumentException()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var values = new object[] { "value1", 42 };
        
        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => new ParameterList(keys, values));
    }
    
    [Fact]
    public void Add_WithExistingKey_ShouldThrowArgumentException()
    {
        // Arrange
        var parameterList = new ParameterList("key1", "value1");
        
        // Act & Assert
        Assert.Throws<System.ArgumentException>(() => parameterList.Add("key1", "value2"));
    }
    
    [Fact]
    public void Get_WithNonExistingKey_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var parameterList = new ParameterList();
        
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => parameterList.Get<string>("key1"));
    }
    
    [Theory]
    [InlineData("key1", "value1", "key1", "value1")]
    public void Get_WithExistingKey_ShouldReturnValue(string key, string value, string getKey, string expected)
    {
        // Arrange
        var parameterList = new ParameterList(key, value);
        
        // Act
        var result = parameterList.Get<string>(getKey);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void DefaultConstructor_ShouldCreateEmptyParameterList()
    {
        // Arrange & Act
        var parameterList = new ParameterList();
        
        // Assert
        Assert.Empty(parameterList.Parameters);
    }
    
    [Theory]
    [InlineData(2)]
    public void Clear_ShouldRemoveAllParameters(int parameterCount)
    {
        // Arrange
        var parameterList = new ParameterList();
        for (int i = 0; i < parameterCount; i++)
        {
            parameterList.Add($"key{i}", $"value{i}");
        }
        
        // Act
        parameterList.Clear();
        
        // Assert
        Assert.Empty(parameterList.Parameters);
    }
    
    #endregion
}
