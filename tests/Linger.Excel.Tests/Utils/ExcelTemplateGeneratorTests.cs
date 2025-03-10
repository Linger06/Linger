using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Linger.Excel.Contracts.Attributes;
using Linger.Excel.Contracts.Utils;
using Linger.Excel.Tests.Models;
using Xunit;

namespace Linger.Excel.Tests.Utils;

public class ExcelTemplateGeneratorTests
{
    [Fact]
    public void CreateColumnInfos_TestPerson_ReturnsCorrectColumnInfos()
    {
        // Act
        var columns = ExcelTemplateGenerator.CreateColumnInfos<TestPerson>();
        
        // Assert
        Assert.NotNull(columns);
        Assert.Equal(7, columns.Count); // 不包括Remark属性
        
        // 检查列是否按正确顺序排序
        Assert.Equal("编号", columns[0].DisplayName);
        Assert.Equal("姓名", columns[1].DisplayName);
        Assert.Equal("出生日期", columns[2].DisplayName); // 使用DisplayName特性值
        Assert.Equal("薪资", columns[3].DisplayName);
        Assert.Equal("是否在职", columns[4].DisplayName);
        Assert.Equal("部门", columns[5].DisplayName);
        Assert.Equal("入职日期", columns[6].DisplayName);
    }
    
    [Fact]
    public void CreateColumnInfos_TestPerson_IdentifiesRequiredProperties()
    {
        // Act
        var columns = ExcelTemplateGenerator.CreateColumnInfos<TestPerson>();
        
        // Assert
        var idColumn = columns.Find(c => c.PropertyName == "Id");
        var nameColumn = columns.Find(c => c.PropertyName == "Name");
        var birthdayColumn = columns.Find(c => c.PropertyName == "Birthday");
        
        Assert.NotNull(idColumn);
        Assert.NotNull(nameColumn);
        Assert.NotNull(birthdayColumn);
        
        Assert.True(idColumn.Required);    // Id标记为Required
        Assert.True(nameColumn.Required);  // Name标记为Required
        Assert.False(birthdayColumn.Required); // Birthday未标记Required
    }
    
    [Fact]
    public void CreateColumnInfos_TestPerson_ReturnsCorrectPropertyTypes()
    {
        // Act
        var columns = ExcelTemplateGenerator.CreateColumnInfos<TestPerson>();
        
        // Assert
        Assert.Equal(typeof(int), columns.Find(c => c.PropertyName == "Id").PropertyType);
        Assert.Equal(typeof(string), columns.Find(c => c.PropertyName == "Name").PropertyType);
        Assert.Equal(typeof(DateTime), columns.Find(c => c.PropertyName == "Birthday").PropertyType);
        Assert.Equal(typeof(decimal), columns.Find(c => c.PropertyName == "Salary").PropertyType);
        Assert.Equal(typeof(bool), columns.Find(c => c.PropertyName == "IsActive").PropertyType);
        Assert.Equal(typeof(string), columns.Find(c => c.PropertyName == "Department").PropertyType);
        Assert.Equal(typeof(DateTime?), columns.Find(c => c.PropertyName == "JoinDate").PropertyType);
    }
    
    [Fact]
    public void CreateColumnInfos_EmptyClass_ReturnsEmptyList()
    {
        // Act
        var columns = ExcelTemplateGenerator.CreateColumnInfos<EmptyClass>();
        
        // Assert
        Assert.Empty(columns);
    }
    
    [Fact]
    public void CreateColumnInfos_WithMultipleAttributes_UsesCorrectPriority()
    {
        // Act
        var columns = ExcelTemplateGenerator.CreateColumnInfos<MultiAttributeClass>();
        
        // Assert
        var column = columns.Find(c => c.PropertyName == "MultiAttributeProperty");
        Assert.NotNull(column);
        Assert.Equal("Display名称优先", column.DisplayName); // Display特性优先于其他特性
    }
    
    // 测试用空类
    private class EmptyClass
    {
    }
    
    // 多特性测试类
    private class MultiAttributeClass
    {
        [Display(Name = "Display名称优先")]
        [DisplayName("DisplayName名称")]
        [ExcelColumn(ColumnName = "Excel列名称", Index = 0)]
        public string MultiAttributeProperty { get; set; }
    }
}
