using System.Reflection;
using Linger.Excel.Contracts.Attributes;
using Linger.Excel.Tests.Models;
using Xunit;

namespace Linger.Excel.Tests.Attributes;

public class ExcelColumnAttributeTests
{
    [Fact]
    public void ExcelColumnAttribute_Properties_SetAndGetCorrectly()
    {
        // Arrange
        var attribute = new ExcelColumnAttribute
        {
            ColumnName = "测试列名",
            Index = 42
        };
        
        // Assert
        Assert.Equal("测试列名", attribute.ColumnName);
        Assert.Equal(42, attribute.Index);
    }
    
    [Fact]
    public void ExcelColumnAttribute_CanBeAppliedToProperty()
    {
        // Arrange & Act
        var propInfo = typeof(TestPerson).GetProperty("Name");
        var attribute = propInfo?.GetCustomAttribute<ExcelColumnAttribute>();
        
        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("姓名", attribute!.ColumnName);
        Assert.Equal(1, attribute.Index);
    }
    
    [Fact]
    public void ExcelColumnAttribute_MultiplePropertiesHaveAttributes()
    {
        // Arrange & Act
        var properties = typeof(TestPerson).GetProperties()
            .Where(p => p.GetCustomAttribute<ExcelColumnAttribute>() != null)
            .ToArray();
        
        // Assert
        Assert.Equal(7, properties.Length); // 7个属性有ExcelColumn特性
    }
    
    [Fact]
    public void ExcelColumnAttribute_NotAppliedToRemarkProperty()
    {
        // Arrange & Act
        var propInfo = typeof(TestPerson).GetProperty("Remark");
        var attribute = propInfo?.GetCustomAttribute<ExcelColumnAttribute>();
        
        // Assert
        Assert.Null(attribute); // Remark属性没有ExcelColumn特性
    }
}
