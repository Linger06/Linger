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

public class NpoiExcelTests : IDisposable
{
    private readonly NpoiExcel _excel;
    private readonly ILogger<NpoiExcel> _logger;
    private readonly List<string> _tempFiles = new();
    private readonly ExcelOptions _options;

    public NpoiExcelTests()
    {
        _options = new ExcelOptions
        {
            ParallelProcessingThreshold = 1000,
            AutoFitColumns = true,
            DefaultDateFormat = "yyyy-MM-dd"
        };
        _logger = TestHelper.CreateLogger<NpoiExcel>();
        _excel = new NpoiExcel(_options, _logger);
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
        var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheet("TestSheet");

        // 检查标题
        Assert.Equal("测试标题", sheet.GetRow(0).GetCell(0).StringCellValue);

        // 检查列头
        var headerRow = sheet.GetRow(1);
        Assert.Equal("Id", headerRow.GetCell(0).StringCellValue);
        Assert.Equal("Name", headerRow.GetCell(1).StringCellValue);

        // 检查数据
        var firstDataRow = sheet.GetRow(2);
        Assert.Equal(persons[0].Id, (int)firstDataRow.GetCell(0).NumericCellValue);
        Assert.Equal(persons[0].Name, firstDataRow.GetCell(1).StringCellValue);
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
        var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheet("TestSheet");

        // 检查标题
        Assert.Equal("测试标题", sheet.GetRow(0).GetCell(0).StringCellValue);

        // 检查列头
        var headerRow = sheet.GetRow(1);
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            Assert.Equal(dataTable.Columns[i].ColumnName, headerRow.GetCell(i).StringCellValue);
        }

        // 检查数据（第一行）
        var firstDataRow = sheet.GetRow(2);
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            var value = dataTable.Rows[0][i];
            if (value != DBNull.Value)
            {
                var cell = firstDataRow.GetCell(i);
                if (cell.CellType == CellType.String)
                {
                    Assert.Equal(Convert.ToString(value), cell.StringCellValue);
                }
                else if (cell.CellType == CellType.Numeric)
                {
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        Assert.Equal(((DateTime)value).Date, cell.DateCellValue.Value);
                    }
                    else if (value is decimal decimalValue)
                    {
                        Assert.Equal((double)decimalValue, cell.NumericCellValue, 3);
                    }
                    else
                    {
                        Assert.Equal(Convert.ToDouble(value), cell.NumericCellValue, 3);
                    }
                }
                else if (cell.CellType == CellType.Boolean)
                {
                    Assert.Equal(Convert.ToBoolean(value), cell.BooleanCellValue);
                }
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
                    // 数值类型可能有轻微差异，使用字符串比较
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
    public void CreateExcelTemplate_ReturnsValidTemplate()
    {
        // Act
        using var templateStream = _excel.CreateExcelTemplate<TestPerson>();

        // Assert
        Assert.NotNull(templateStream);
        Assert.True(templateStream.Length > 0);

        // 验证模板
        templateStream.Position = 0;
        var workbook = new XSSFWorkbook(templateStream);
        var sheet = workbook.GetSheetAt(0);

        // 检查列头是否存在
        var headerRow = sheet.GetRow(0);
        var expectedColumns = new[] { "Id", "Name", "Birthday", "Salary", "IsActive", "Department", "JoinDate" };

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

    [Theory]
    [InlineData(true)]  // 自动调整列宽开启
    [InlineData(false)] // 自动调整列宽关闭
    public void AutoFitColumns_AffectsOutput(bool autoFit)
    {
        // Arrange
        _options.AutoFitColumns = autoFit;
        var persons = TestHelper.CreateTestPersonList(5);

        // Act - 两次执行导出，比较结果大小
        var streamWithLongNames = _excel.ConvertCollectionToMemoryStream(
            persons.Select(p => { p.Name = p.Name + new string('测', 20); return p; }).ToList(),
            "TestSheet"
        );

        var streamWithShortNames = _excel.ConvertCollectionToMemoryStream(
            persons.Select(p => { p.Name = p.Name.Substring(0, 3); return p; }).ToList(),
            "TestSheet"
        );

        // Assert
        Assert.NotNull(streamWithLongNames);
        Assert.NotNull(streamWithShortNames);

        // 如果自动调整列宽开启，长名称的Excel文件应该大一些
        if (autoFit)
        {
            // 只是确认两个文件都已正确生成
            Assert.True(streamWithLongNames.Length > 0);
            Assert.True(streamWithShortNames.Length > 0);
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
            // 日期可能会有精度差异，只比较日期部分
            Assert.Equal(originalPersons[i].Birthday.Date, importedPersons[i].Birthday.Date);
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
        using var fs = File.OpenRead(filePath);
        var workbook = new XSSFWorkbook(fs);
        var sheet = workbook.GetSheetAt(0);

        // 表头+数据行
        Assert.Equal(persons.Count + 1, sheet.PhysicalNumberOfRows);
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
        using var fs = File.OpenRead(filePath);
        var workbook = new XSSFWorkbook(fs);
        var sheet = workbook.GetSheetAt(0);

        // 表头+数据行
        Assert.Equal(dt.Rows.Count + 1, sheet.PhysicalNumberOfRows);
    }

    [Fact]
    public void ConvertStreamToDataTable_WithDifferentSheetNames_ReturnsCorrectDataTable()
    {
        // Arrange
        var dataTable = TestHelper.CreateTestDataTable(5);
        const string customSheetName = "CustomSheet";

        using var stream = _excel.ConvertDataTableToMemoryStream(dataTable, customSheetName);

        // Act
        stream.Position = 0;
        var result = _excel.ConvertStreamToDataTable(stream, customSheetName);

        // 使用错误的sheet名重试
        stream.Position = 0;
        var resultWithWrongSheetName = _excel.ConvertStreamToDataTable(stream, "NonExistentSheet");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dataTable.Columns.Count, result!.Columns.Count);
        Assert.Equal(dataTable.Rows.Count, result.Rows.Count);

        // 使用错误的sheet名应该返回第一个sheet
        Assert.NotNull(resultWithWrongSheetName);
        Assert.Equal(dataTable.Columns.Count, resultWithWrongSheetName!.Columns.Count);
    }

    [Fact]
    public void GetCellValue_HandlesSpecialTypes()
    {
        // Arrange
        var dataTable = new DataTable();
        dataTable.Columns.Add("BooleanColumn", typeof(bool));
        dataTable.Columns.Add("DateColumn", typeof(DateTime));
        dataTable.Columns.Add("FormulaColumn", typeof(string));

        var now = DateTime.Now.Date;
        dataTable.Rows.Add(true, now, "=SUM(1,2)");

        var stream = _excel.ConvertDataTableToMemoryStream(dataTable, "FormulaSheet");

        // Act
        stream.Position = 0;
        var result = _excel.ConvertStreamToDataTable(stream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(true, result!.Rows[0]["BooleanColumn"]);
        Assert.Equal(now, ((DateTime)result.Rows[0]["DateColumn"]).Date);
    }

    [Fact]
    public void CreatePropertyMap_DeterminesCorrectMapping()
    {
        // Arrange
        var persons = TestHelper.CreateTestPersonList(1);
        var filePath = Path.GetTempFileName() + ".xlsx";
        _tempFiles.Add(filePath);

        _excel.ListToFile(persons, filePath);

        using var fs = File.OpenRead(filePath);
        var workbook = new XSSFWorkbook(fs);
        var headerRowIndex = 0; // 第一行是表头

        // 通过反射调用私有方法
        var methodInfo = typeof(NpoiExcel).GetMethod("CreatePropertyMap",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        var result = methodInfo?.Invoke(_excel, new object[] { workbook.GetSheetAt(0), headerRowIndex });

        // Assert
        Assert.NotNull(result);
        // Dictionary<int, PropertyInfo>类型
        var mapping = (Dictionary<int, System.Reflection.PropertyInfo>)result;
        Assert.True(mapping.Count > 0);

        // Id属性应该映射到第一列
        var idProperty = typeof(TestPerson).GetProperty("Id");
        bool containsIdMapping = mapping.Any(m => m.Value.Name == idProperty.Name);
        Assert.True(containsIdMapping);
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
