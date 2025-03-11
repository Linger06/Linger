using System.Data;
using Linger.Excel.Contracts;
using Linger.Excel.Npoi;
using Linger.Excel.Tests.Helpers;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Xunit;

namespace Linger.Excel.Tests.Implementations;

[Collection("Sequential")] // NPOI可能不是线程安全的
public class NpoiExcelTests : BaseExcelTests<NpoiExcel>
{
    protected override NpoiExcel CreateExcelInstance(ExcelOptions options, ILogger<NpoiExcel> logger)
    {
        return new NpoiExcel(options, logger);
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

    [Fact(Skip = "NPOI较慢，优化后再测试")]
    public void ParallelProcessing_ImprovePerformance()
    {
        TestParallelPerformance();
    }

    // NPOI特有的一些测试
    [Theory]
    [InlineData(50)]
    [InlineData(150)]
    public void ConvertDataTableToMemoryStream_ShouldHandleDifferentSizes(int rowCount)
    {
        // Arrange
        var dataTable = PerformanceTestHelper.GenerateLargeDataTable(rowCount);

        // Act & Assert
        var elapsed = PerformanceTestHelper.MeasureExecutionTime(() =>
        {
            using var result = Excel.ConvertDataTableToMemoryStream(dataTable);
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        });

        // 输出性能指标，但不验证具体时间（硬件相关）
        System.Diagnostics.Debug.WriteLine($"转换 {rowCount} 行数据耗时: {elapsed.TotalMilliseconds}ms");
    }

    #region 实现抽象方法

    protected override void VerifyExcelContent<TData>(Stream stream, List<TData> data, string sheetName, string title)
    {
        stream.Position = 0;
        var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheet(sheetName);

        // 检查标题
        Assert.Equal(title, sheet.GetRow(0).GetCell(0).StringCellValue);

        // 检查列头和数据行
        Assert.True(sheet.LastRowNum >= data.Count);
    }

    protected override void VerifyExcelContent(Stream stream, DataTable dataTable, string sheetName, string title)
    {
        stream.Position = 0;
        var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheet(sheetName);

        // 检查标题
        Assert.Equal(title, sheet.GetRow(0).GetCell(0).StringCellValue);

        // 检查列头
        var headerRow = sheet.GetRow(1);
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            Assert.Equal(dataTable.Columns[i].ColumnName, headerRow.GetCell(i).StringCellValue);
        }
    }

    protected override void VerifyExcelTemplate(Stream templateStream)
    {
        templateStream.Position = 0;
        var workbook = new XSSFWorkbook(templateStream);
        var sheet = workbook.GetSheetAt(0);

        // 检查列头是否存在
        var expectedColumns = new[] { "Id", "Name", "Birthday", "Salary", "IsActive", "Department", "JoinDate" };

        var headerRow = sheet.GetRow(0);
        for (int i = 0; i < expectedColumns.Length; i++)
        {
            bool found = false;
            for (int j = 0; j < headerRow.LastCellNum; j++)
            {
                var cell = headerRow.GetCell(j);
                if (cell != null && cell.StringCellValue == expectedColumns[i])
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
        using var fs = File.OpenRead(filePath);
        var workbook = new XSSFWorkbook(fs);
        var sheet = workbook.GetSheetAt(0);
        Assert.Equal(expectedRowCount, sheet.LastRowNum + 1); // NPOI是0基索引，需要加1
    }

    #endregion
}
