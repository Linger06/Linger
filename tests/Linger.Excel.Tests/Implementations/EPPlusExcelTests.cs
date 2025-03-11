using System.Data;
using Linger.Excel.Contracts;
using Linger.Excel.EPPlus;
using Linger.Excel.Tests.Helpers;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Xunit;

namespace Linger.Excel.Tests.Implementations;

public class EPPlusExcelTests : IDisposable
{
    private readonly EPPlusExcel _excel;
    private readonly ILogger<EPPlusExcel> _logger;
    private readonly List<string> _tempFiles = new();
    private readonly ExcelOptions _options;

    static EPPlusExcelTests()
    {
        // 设置EPPlus的LicenseContext避免许可警告
        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public EPPlusExcelTests()
    {
        _options = new ExcelOptions
        {
            ParallelProcessingThreshold = 1000,
            AutoFitColumns = true,
            DefaultDateFormat = "yyyy-MM-dd"
        };
        _logger = TestHelper.CreateLogger<EPPlusExcel>();
        _excel = new EPPlusExcel(_options, _logger);
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
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets["TestSheet"];

        // 检查标题
        Assert.Equal("测试标题", worksheet.Cells[1, 1].Value.ToString());

        // 检查列头
        Assert.Equal("Id", worksheet.Cells[2, 1].Value.ToString());
        Assert.Equal("Name", worksheet.Cells[2, 2].Value.ToString());

        // 检查数据
        Assert.Equal(persons[0].Id, worksheet.Cells[3, 1].Value);
        Assert.Equal(persons[0].Name, worksheet.Cells[3, 2].Value);
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
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets["TestSheet"];

        // 检查标题
        Assert.Equal("测试标题", worksheet.Cells[1, 1].Value.ToString());

        // 检查列头
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            Assert.Equal(dataTable.Columns[i].ColumnName, worksheet.Cells[2, i + 1].Value.ToString());
        }

        // 检查数据（检查第一行）
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            // 处理DBNull值
            var cellValue = dataTable.Rows[0][i];
            if (cellValue != DBNull.Value)
            {
                Assert.Equal(Convert.ToString(cellValue), Convert.ToString(worksheet.Cells[3, i + 1].Value));
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
        List<TestPerson> result = new();
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
    public void GetExcelColumns_WithExcelColumnAttributes_ReturnsCorrectColumns()
    {
        // Arrange
        Type type = typeof(TestPerson);
        var properties = type.GetProperties().Where(p => p.CanRead).ToArray();

        // 通过反射调用私有方法
        var methodInfo = typeof(EPPlusExcel).GetMethod("GetExcelColumns",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = (List<Tuple<string, string, int>>)methodInfo!.Invoke(_excel, new object[] { properties });

        // Assert
        Assert.Equal(7, result.Count); // 应该有7个属性带有ExcelColumn特性
        Assert.Equal("Id", result.First(c => c.Item3 == 0).Item1);
        Assert.Equal("编号", result.First(c => c.Item3 == 0).Item2);
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
        using var package = new ExcelPackage(templateStream);
        var worksheet = package.Workbook.Worksheets.First();

        // 检查列头是否存在
        var expectedColumns = new[] { "Id", "Name", "Birthday", "Salary", "IsActive", "Department", "JoinDate" };

        // 由于不同的导出方式可能会有不同的列顺序，所以只检查是否包含所有列名
        for (int i = 1; i <= expectedColumns.Length; i++)
        {
            var columnName = worksheet.Cells[1, i].Value?.ToString();
            Assert.Contains(columnName, expectedColumns);
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
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.First();
        Assert.Equal(persons.Count + 1, worksheet.Dimension.End.Row); // +1 表头行
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
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.First();
        Assert.Equal(dt.Rows.Count + 1, worksheet.Dimension.End.Row); // +1 表头行
    }

    [Fact]
    public void ConvertStreamToListAsync_ValidExcelFile_ReturnsValidList()
    {
        // Arrange
        var persons = TestHelper.CreateTestPersonList(5);
        var filePath = Path.GetTempFileName() + ".xlsx";
        _tempFiles.Add(filePath);

        _excel.ListToFile(persons, filePath);

        // Act
        List<TestPerson>? result;
        using (var fs = File.OpenRead(filePath))
        {
            result = _excel.ConvertStreamToList<TestPerson>(fs);
        }

        // Assert
        Assert.NotNull(result);
        Assert.Equal(persons.Count, result.Count);

        for (int i = 0; i < persons.Count; i++)
        {
            Assert.Equal(persons[i].Id, result[i].Id);
            Assert.Equal(persons[i].Name, result[i].Name);
        }
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
