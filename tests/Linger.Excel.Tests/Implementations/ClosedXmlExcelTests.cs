using System.Data;
using System.IO;
using ClosedXML.Excel;
using Linger.Excel.ClosedXML;
using Linger.Excel.Contracts;
using Linger.Excel.Tests.Helpers;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests.Implementations;

public class ClosedXmlExcelTests : BaseExcelTests<ClosedXmlExcel>
{
    protected override ClosedXmlExcel CreateExcelInstance(ExcelOptions options, ILogger<ClosedXmlExcel> logger)
    {
        return new ClosedXmlExcel(options, logger);
    }
    
    // 必须定义公共测试方法，以便XUnit能够找到它们
    
    [Fact]
    public void ConvertCollectionToMemoryStream_ValidList_ReturnsValidStream()
    {
        TestConvertCollectionToMemoryStream();
    }
    
    [Fact]
    public void ConvertDataTableToMemoryStream_ValidDataTable_ReturnsValidStream()
    {
        TestConvertDataTableToMemoryStream();
    }
    
    [Fact]
    public void ConvertStreamToDataTable_ValidExcel_ReturnsValidDataTable()
    {
        TestConvertStreamToDataTable();
    }
    
    [Fact]
    public void StreamReadExcel_ValidExcel_ReturnsCorrectObjects()
    {
        TestStreamReadExcel();
    }
    
    [Fact]
    public void CreateExcelTemplate_ReturnsValidTemplate()
    {
        TestCreateExcelTemplate();
    }
    
    [Fact]
    public void ListToFile_FileCreatedSuccessfully()
    {
        TestListToFile();
    }
    
    [Fact]
    public void DataTableToFile_FileCreatedSuccessfully()
    {
        TestDataTableToFile();
    }
    
    [Fact]
    public void ConvertDataTableToMemoryStream_ShouldUsePerformanceMonitoring()
    {
        TestPerformanceMonitoring();
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AutoFitColumns_SetsCorrectWidths(bool autoFit)
    {
        // Arrange
        Options.AutoFitColumns = autoFit;
        var persons = TestHelper.CreateTestPersonList(5);
        
        // Act
        using var stream = Excel.ConvertCollectionToMemoryStream(persons, "TestSheet");
        
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
    public void EdgeCases_HandleGracefully()
    {
        TestEdgeCases();
    }

    [Fact]
    public void ExceptionHandling_HandlesErrors()
    {
        TestExceptionHandling();
    }

    [Fact]
    public void ExcelToList_ShouldConvertCorrectly()
    {
        TestExcelToList();
    }

    [Fact]
    public void ExcelToDataTable_ShouldConvertCorrectly()
    {
        TestExcelToDataTable();
    }

    [Fact]
    public void ParallelProcessing_ImprovePerformance()
    {
        TestParallelPerformance();
    }

    [Fact]
    public void CustomFormatting_ShouldBeApplied()
    {
        TestCustomFormatting();
    }
    
    #region 实现抽象方法
    
    protected override void VerifyExcelContent<TData>(Stream stream, List<TData> data, string sheetName, string title)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(sheetName);
        
        // 检查标题
        Assert.Equal(title, worksheet.Cell(1, 1).Value.ToString());
        
        // 检查列头和数据可以根据TData类型进行更详细的验证
        Assert.True(worksheet.RowsUsed().Count() > data.Count); // 至少有表头+数据行
    }
    
    protected override void VerifyExcelContent(Stream stream, DataTable dataTable, string sheetName, string title)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(sheetName);
        
        // 检查标题
        Assert.Equal(title, worksheet.Cell(1, 1).Value.ToString());
        
        // 检查列头
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            Assert.Equal(dataTable.Columns[i].ColumnName, worksheet.Cell(2, i + 1).Value.ToString());
        }
    }
    
    protected override void VerifyExcelTemplate(Stream templateStream)
    {
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
    
    protected override void VerifyExcelFile(string filePath, int expectedRowCount)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1);
        Assert.Equal(expectedRowCount, worksheet.RowsUsed().Count());
    }
    
    #endregion
}
