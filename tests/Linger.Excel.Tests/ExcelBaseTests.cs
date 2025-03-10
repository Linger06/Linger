using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Excel.Tests.Helpers;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests;

public class ExcelBaseTests
{
    private readonly ILogger<TestExcelImplementation> _logger;
    private readonly TestExcelImplementation _excel;
    
    public ExcelBaseTests()
    {
        _logger = TestHelper.CreateLogger<TestExcelImplementation>();
        _excel = new TestExcelImplementation(new ExcelOptions(), _logger);
    }
    
    [Fact]
    public void ConvertDataTableToList_ValidData_ReturnsCorrectList()
    {
        // Arrange
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Birthday", typeof(DateTime));
        
        var today = DateTime.Today;
        dt.Rows.Add(1, "张三", today);
        dt.Rows.Add(2, "李四", today.AddDays(-10));
        
        // Act
        var result = _excel.TestConvertDataTableToList<TestPerson>(dt);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("张三", result[0].Name);
        Assert.Equal(today, result[0].Birthday);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("李四", result[1].Name);
        Assert.Equal(today.AddDays(-10), result[1].Birthday);
    }
    
    [Fact]
    public void ConvertDataTableToList_EmptyTable_ReturnsEmptyList()
    {
        // Arrange
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        
        // Act
        var result = _excel.TestConvertDataTableToList<TestPerson>(dt);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public void ConvertDataTableToList_MismatchedColumns_MatchesAvailableColumns()
    {
        // Arrange
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("NotName", typeof(string)); // 与TestPerson.Name不匹配
        dt.Rows.Add(123, "Test");
        
        // Act
        var result = _excel.TestConvertDataTableToList<TestPerson>(dt);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(123, result[0].Id);
        Assert.Equal(string.Empty, result[0].Name); // Name应该是默认值
    }
    
    [Fact]
    public void SetPropertySafely_ValidValues_SetsPropertiesCorrectly()
    {
        // Arrange
        var person = new TestPerson();
        var idProp = typeof(TestPerson).GetProperty("Id")!;
        var nameProp = typeof(TestPerson).GetProperty("Name")!;
        var birthdayProp = typeof(TestPerson).GetProperty("Birthday")!;
        var isActiveProp = typeof(TestPerson).GetProperty("IsActive")!;
        
        // Act
        _excel.TestSetPropertySafely(person, idProp, 123);
        _excel.TestSetPropertySafely(person, nameProp, "测试姓名");
        _excel.TestSetPropertySafely(person, birthdayProp, DateTime.Today);
        _excel.TestSetPropertySafely(person, isActiveProp, true);
        
        // Assert
        Assert.Equal(123, person.Id);
        Assert.Equal("测试姓名", person.Name);
        Assert.Equal(DateTime.Today, person.Birthday);
        Assert.True(person.IsActive);
    }
    
    [Fact]
    public void SetPropertySafely_NullableProperty_HandlesNullValue()
    {
        // Arrange
        var person = new TestPerson { Department = "测试部门" };
        var deptProp = typeof(TestPerson).GetProperty("Department")!;
        
        // Act
        _excel.TestSetPropertySafely(person, deptProp, null);
        
        // Assert
        Assert.Equal("测试部门", person.Department); // 值应保持不变
    }
    
    [Fact]
    public void SetPropertySafely_SpecialTypes_HandlesCorrectly()
    {
        // Arrange
        var person = new TestPerson();
        var birthdayProp = typeof(TestPerson).GetProperty("Birthday")!;
        var isActiveProp = typeof(TestPerson).GetProperty("IsActive")!;
        
        // Act - 从字符串转换
        _excel.TestSetPropertySafely(person, birthdayProp, "2023-01-01");
        _excel.TestSetPropertySafely(person, isActiveProp, "true");
        
        // Assert
        Assert.Equal(new DateTime(2023, 1, 1), person.Birthday);
        Assert.True(person.IsActive);
    }
    
    [Fact]
    public void ProcessInBatches_AllItemsProcessed()
    {
        // Arrange
        int totalCount = 100;
        var processedItems = new List<int>();
        
        // Act
        _excel.TestProcessInBatches<int, string>(
            totalCount,
            i => i,                        // 创建"行"
            i => new[] { $"Item {i}" },    // 获取值
            (row, index, values, _) =>     // 处理行
            {
                processedItems.Add(index);
            });
        
        // Assert
        Assert.Equal(totalCount, processedItems.Count);
        for (int i = 0; i < totalCount; i++)
        {
            Assert.Contains(i, processedItems);
        }
    }
    
    [Fact]
    public void GetExcelCellValue_TypeConversion_HandlesCorrectly()
    {
        // Act & Assert
        
        // 数字类型
        Assert.Equal(42, _excel.TestGetExcelCellValue(42.0));
        Assert.Equal(42.5, _excel.TestGetExcelCellValue(42.5));
        
        // 字符串
        Assert.Equal("test", _excel.TestGetExcelCellValue("test"));
        
        // 日期
        var date = new DateTime(2023, 1, 1);
        Assert.Equal(date, _excel.TestGetExcelCellValue(date, true));
        
        // 从Excel数字日期
        var oaDate = 44927.0; // 2023-01-01
        Assert.Equal(DateTime.FromOADate(oaDate), _excel.TestGetExcelCellValue(oaDate, true));
        
        // 空值
        Assert.Equal(DBNull.Value, _excel.TestGetExcelCellValue(null));
        Assert.Equal(DBNull.Value, _excel.TestGetExcelCellValue(""));
    }
    
    [Fact]
    public void CreateExcelTemplate_CreatesValidTemplate()
    {
        // Act
        using var template = _excel.CreateExcelTemplate<TestPerson>();
        
        // Assert
        Assert.NotNull(template);
        Assert.True(template.Length > 0);
    }
    
    [Fact]
    public void ObjectToDataRow_ConvertsProperly()
    {
        // Arrange
        var person = new TestPerson 
        { 
            Id = 1, 
            Name = "Test",
            Birthday = new DateTime(2000, 1, 1),
            IsActive = true
        };
        
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Birthday", typeof(DateTime));
        dt.Columns.Add("IsActive", typeof(bool));
        dt.Columns.Add("InvalidColumn", typeof(string)); // 不匹配的列
        
        // Act
        var row = _excel.TestObjectToDataRow(person, dt);
        
        // Assert
        Assert.NotNull(row);
        Assert.Equal(1, row["Id"]);
        Assert.Equal("Test", row["Name"]);
        Assert.Equal(new DateTime(2000, 1, 1), row["Birthday"]);
        Assert.Equal(true, row["IsActive"]);
        Assert.Equal(DBNull.Value, row["InvalidColumn"]); // 不匹配列应为DBNull
    }
}

/// <summary>
/// 用于测试的Excel基类实现
/// </summary>
public class TestExcelImplementation : ExcelBase
{
    public TestExcelImplementation(ExcelOptions? options = null, ILogger<TestExcelImplementation>? logger = null)
        : base(options, logger)
    {
    }
    
    // 暴露protected方法以便测试
    public List<T> TestConvertDataTableToList<T>(DataTable dataTable) where T : class, new()
    {
        return ConvertDataTableToList<T>(dataTable);
    }
    
    public void TestSetPropertySafely<T>(T obj, PropertyInfo property, object? value) where T : class
    {
        SetPropertySafely(obj, property, value);
    }
    
    public void TestProcessInBatches<TRow, TValue>(
        int totalCount,
        Func<int, TRow> createRowFunc,
        Func<int, TValue[]> getValuesFunc,
        Action<TRow, int, TValue[], object?> processRowFunc,
        object? additionalParam = null)
    {
        ProcessInBatches(totalCount, createRowFunc, getValuesFunc, processRowFunc, additionalParam);
    }
    
    public object TestGetExcelCellValue(object? cellValue, bool isDateFormat = false)
    {
        return GetExcelCellValue(cellValue, isDateFormat);
    }
    
    public DataRow TestObjectToDataRow<T>(T obj, DataTable dt) where T : class
    {
        return ObjectToDataRow(obj, dt);
    }
    
    // 实现抽象方法
    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        return new DataTable("Test");
    }
    
    public override MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        return new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
    }
    
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    {
        return new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
    }
}
