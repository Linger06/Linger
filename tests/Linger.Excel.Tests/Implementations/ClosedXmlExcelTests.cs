using System.Data;
using ClosedXML.Excel;
using Linger.Excel.ClosedXML;
using Linger.Excel.Contracts;
using Linger.Excel.Tests.Helpers;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests.Implementations;

public class ClosedXmlExcelTests : IDisposable
{
    private readonly ClosedXmlExcel _excel;
    private readonly ILogger<ClosedXmlExcel> _logger;
    private readonly List<string> _tempFiles = new();
    private readonly ExcelOptions _options;
    
    public ClosedXmlExcelTests()
    {
        _options = new ExcelOptions 
        { 
            ParallelProcessingThreshold = 1000, 
            AutoFitColumns = true,
            DefaultDateFormat = "yyyy-MM-dd"
        };
        _logger = TestHelper.CreateLogger<ClosedXmlExcel>();
        _excel = new ClosedXmlExcel(_options, _logger);
    }
    
    [Fact]
    public void ConvertCollectionToMemoryStream_ValidList_ReturnsValidStream()
    {
        // Arrange
        var persons = TestHelper.CreateTestPersonList(10);
        
        // Act
        using var stream = _excel.ConvertCollectionToMemoryStream(persons, "TestSheet", "测试标题");
        
        // Assert
        Assert.NotNull(stream);
        Assert.True(stream!.Length > 0);
        
        // 验证Excel内容
        stream.Position = 0;
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("TestSheet");
        
        // 检查标题
        Assert.Equal("测试标题", worksheet.Cell(1, 1).Value.ToString());
        
        // 检查列头
        Assert.Equal("Id", worksheet.Cell(2, 1).Value.ToString());
        Assert.Equal("Name", worksheet.Cell(2, 2).Value.ToString());
        
        // 检查数据
        Assert.Equal(persons[0].Id, worksheet.Cell(3, 1).Value);
        Assert.Equal(persons[0].Name, worksheet.Cell(3, 2).Value);
    }
    
    [Fact]
    public void ConvertDataTableToMemoryStream_ValidDataTable_ReturnsValidStream()
    {
        // Arrange
        var dataTable = TestHelper.CreateTestDataTable(10);
        
        // Act
        using var stream = _excel.ConvertDataTableToMemoryStream(dataTable, "TestSheet", "测试标题");
        
        // Assert
        Assert.NotNull(stream);
        Assert.True(stream!.Length > 0);
        
        // 验证Excel内容
        stream.Position = 0;
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("TestSheet");
        
        // 检查标题
        Assert.Equal("测试标题", worksheet.Cell(1, 1).Value.ToString());
        
        // 检查列头
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            Assert.Equal(dataTable.Columns[i].ColumnName, worksheet.Cell(2, i + 1).Value.ToString());
        }
        
        // 检查数据（检查第一行）
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            // 处理DBNull值
            var cellValue = dataTable.Rows[0][i];
            if (cellValue != DBNull.Value)
            {
                Assert.Equal(Convert.ToString(cellValue), Convert.ToString(worksheet.Cell(3, i + 1).Value));
            }
        }
    }
    
    [Fact]
    public void ConvertStreamToDataTable_ValidExcel_ReturnsValidDataTable()
    {
        // Arrange - 创建一个测试Excel文件并读取
        var filePath = Path.GetTempFileName() + ".xlsx";
        _tempFiles.Add(filePath);
        
        var originalData = TestHelper.CreateTestDataTable(5);
        
        using (var stream = _excel.ConvertDataTableToMemoryStream(originalData, "TestSheet"))
        {
            using var fs = File.Create(filePath);
            stream.Position = 0;
            stream.CopyTo(fs);
        }
        
        // Act
        DataTable? result;
        using (var fs = File.OpenRead(filePath))
        {
            result = _excel.ConvertStreamToDataTable(fs);
        }
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalData.Columns.Count, result!.Columns.Count);
        Assert.Equal(originalData.Rows.Count, result.Rows.Count);
        
        // 检查数据类型和值是否保持一致
        for (int colIndex = 0; colIndex < originalData.Columns.Count; colIndex++)
        {
            // 列名可能略有不同，因此我们按位置比较而不是按列名
            for (int rowIndex = 0; rowIndex < originalData.Rows.Count; rowIndex++)
            {
                var originalValue = originalData.Rows[rowIndex][colIndex];
                var resultValue = result.Rows[rowIndex][colIndex];
                
                if (originalValue is DateTime dateTime) 
                {
                    // 日期只比较日期部分
                    Assert.Equal(dateTime.Date, ((DateTime)resultValue).Date);
                }
                else if (originalValue != DBNull.Value)
                {
                    Assert.Equal(Convert.ToString(originalValue), Convert.ToString(resultValue));
                }
            }
        }
    }
    
    [Fact]
    public void StreamReadExcel_ValidExcel_ReturnsCorrectObjects()
    {
        // Arrange - 创建一个测试Excel文件
        var persons = TestHelper.CreateTestPersonList(20);
        var filePath = Path.GetTempFileName() + ".xlsx";
        _tempFiles.Add(filePath);
        
        _excel.ListToFile(persons, filePath);
        
        // Act
        List<TestPerson> result = new List<TestPerson>();
        using (var fs = File.OpenRead(filePath))
        {
            result = _excel.StreamReadExcel<TestPerson>(fs).ToList();
        }
        
        // Assert
        Assert.Equal(persons.Count, result.Count);
        
        // 比较关键属性
        for (int i = 0; i < persons.Count; i++)
        {
            Assert.Equal(persons[i].Id, result[i].Id);
            Assert.Equal(persons[i].Name, result[i].Name);
            // 日期类型比较日期部分
            Assert.Equal(persons[i].Birthday.Date, result[i].Birthday.Date);
            Assert.Equal(persons[i].IsActive, result[i].IsActive);
        }
    }
    
    [Fact]
    public void CreateExcelTemplate_ReturnsValidTemplate()
    {
        // Act
        using var templateStream = _excel.CreateExcelTemplate<TestPerson>();
        
        // Assert
        Assert.NotNull(templateStream);
        Assert.True(templateStream.Length > 0);
        
        // 验证模板
        templateStream.Position = 0;
        using var workbook = new XLWorkbook(templateStream);
        var worksheet = workbook.Worksheet(1);
        
        // 检查列头是否存在
        var expectedColumns = new[] { "Id", "Name", "Birthday", "Salary", "IsActive", "Department", "JoinDate" };
        
        for (int i = 0; i < expectedColumns.Length; i++)
        {
            bool found = false;
            for (int j = 1; j <= worksheet.ColumnsUsed().Count(); j++)
            {
                var cellValue = worksheet.Cell(1, j).Value.ToString();
                if (cellValue == expectedColumns[i])
                {
                    found = true;
                    break;
                }
            }
            Assert.True(found, $"列头 '{expectedColumns[i]}' 未在模板中找到");
        }
    }
    
    [Theory]
    [InlineData(true)]  // 自动调整列宽开启
    [InlineData(false)] // 自动调整列宽关闭
    public void AutoFitColumns_SetsCorrectWidths(bool autoFit)
    {
        // Arrange
        _options.AutoFitColumns = autoFit;
        var persons = TestHelper.CreateTestPersonList(5);
        
        // Act
        using var stream = _excel.ConvertCollectionToMemoryStream(persons, "TestSheet");
        
        // Assert
        stream.Position = 0;
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("TestSheet");
        
        // 检查是否所有列都有宽度
        for (int i = 1; i <= 7; i++) // TestPerson有7个导出属性
        {
            var column = worksheet.Column(i);
            if (autoFit)
            {
                Assert.True(column.Width > 0, $"Column {i} width should be > 0 when AutoFit is {autoFit}");
            }
        }
    }
    
    [Fact]
    public void ExcelToList_ValidFile_ReturnsCorrectList()
    {
        // Arrange - 创建测试Excel文件
        var originalPersons = TestHelper.CreateTestPersonList(10);
        var filePath = Path.GetTempFileName() + ".xlsx";
        _tempFiles.Add(filePath);
        
        _excel.ListToFile(originalPersons, filePath);
        
        // Act
        var importedPersons = _excel.ExcelToList<TestPerson>(filePath);
        
        // Assert
        Assert.NotNull(importedPersons);
        Assert.Equal(originalPersons.Count, importedPersons!.Count);
        
        // 比较数据
        for (int i = 0; i < originalPersons.Count; i++)
        {
            Assert.Equal(originalPersons[i].Id, importedPersons[i].Id);
            Assert.Equal(originalPersons[i].Name, importedPersons[i].Name);
        }
    }
    
    [Fact]
    public void ListToFile_FileCreatedSuccessfully()
    {
        // Arrange
        var persons = TestHelper.CreateTestPersonList(5);
        var filePath = Path.GetTempFileName() + ".xlsx";
        _tempFiles.Add(filePath);
        
        // Act
        var result = _excel.ListToFile(persons, filePath);
        
        // Assert
        Assert.Equal(filePath, result);
        Assert.True(File.Exists(filePath));
        
        // 检查文件内容
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        Assert.Equal(persons.Count + 1, worksheet.RowsUsed().Count()); // +1 表头行
    }
    
    [Fact]
    public void DataTableToFile_FileCreatedSuccessfully()
    {
        // Arrange
        var dt = TestHelper.CreateTestDataTable(5);
        var filePath = Path.GetTempFileName() + ".xlsx";
        _tempFiles.Add(filePath);
        
        // Act
        var result = _excel.DataTableToFile(dt, filePath);
        
        // Assert
        Assert.Equal(filePath, result);
        Assert.True(File.Exists(filePath));
        
        // 检查文件内容
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        Assert.Equal(dt.Rows.Count + 1, worksheet.RowsUsed().Count()); // +1 表头行
    }
    
    public void Dispose()
    {
        // 清理测试文件
        foreach (var file in _tempFiles)
        {
            TestHelper.DeleteFileIfExists(file);
        }
    }
}
