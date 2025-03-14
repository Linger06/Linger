using System.Data;
using Linger.Excel.Contracts;
using Linger.Excel.EPPlus;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Xunit;

namespace Linger.Excel.Tests.Implementations;

public class EPPlusExcelTests : BaseExcelTests<EPPlusExcel>
{
    static EPPlusExcelTests()
    {
        // 设置EPPlus的LicenseContext避免许可警告
        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    protected override EPPlusExcel CreateExcelInstance(ExcelOptions options, ILogger<EPPlusExcel> logger)
    {
        return new EPPlusExcel(options, logger);
    }

    // 定义公共测试方法，以便XUnit能够找到它们

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


    // EPPlus特有的测试
    [Fact]
    public void GetExcelColumns_WithExcelColumnAttributes_ReturnsCorrectColumns()
    {
        // Arrange
        Type type = typeof(TestPerson);
        var properties = type.GetProperties().Where(p => p.CanRead).ToArray();

        // 通过反射调用私有方法
        var methodInfo = typeof(EPPlusExcel).GetMethod("GetExcelColumns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<Tuple<string, string, int>>)methodInfo!.Invoke(Excel, new object[] { properties });

        // Assert
        Assert.Equal(7, result.Count); // 应该有7个属性带有ExcelColumn特性
        Assert.Equal("Id", result.First(c => c.Item3 == 0).Item1);
        Assert.Equal("编号", result.First(c => c.Item3 == 0).Item2);
    }

    #region 实现抽象方法

    protected override void VerifyExcelContent<TData>(Stream stream, List<TData> data, string sheetName, string title)
    {
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[sheetName];

        // 检查标题
        Assert.Equal(title, worksheet.Cells[1, 1].Value.ToString());

        // 检查数据行数
        Assert.True(worksheet.Dimension.End.Row >= data.Count + 1); // +1表示表头行
    }

    protected override void VerifyExcelContent(Stream stream, DataTable dataTable, string sheetName, string title)
    {
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[sheetName];

        // 检查标题
        Assert.Equal(title, worksheet.Cells[1, 1].Value.ToString());

        // 检查列头
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            Assert.Equal(dataTable.Columns[i].ColumnName, worksheet.Cells[2, i + 1].Value.ToString());
        }
    }

    protected override void VerifyExcelTemplate(Stream templateStream)
    {
        using var package = new ExcelPackage(templateStream);
        var worksheet = package.Workbook.Worksheets.First();

        // 检查列头是否存在
        var expectedColumns = new[] { "Id", "Name", "Birthday", "Salary", "IsActive", "Department", "JoinDate" };

        for (int i = 0; i < expectedColumns.Length; i++)
        {
            bool found = false;
            for (int j = 1; j <= worksheet.Dimension.End.Column; j++)
            {
                var cellValue = worksheet.Cells[1, j].Value?.ToString();
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
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.First();
        Assert.Equal(expectedRowCount, worksheet.Dimension.End.Row);
    }

    #endregion
}
