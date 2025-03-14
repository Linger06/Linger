using System.Data;
using Linger.Excel.Contracts;
using Linger.Excel.Tests.Helpers;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests;

/// <summary>
/// Excel实现的基础测试类
/// </summary>
/// <typeparam name="T">Excel实现类型</typeparam>
public abstract class BaseExcelTests<T> : IDisposable where T : class, IExcel
{
    protected readonly T Excel;
    protected readonly ILogger<T> Logger;
    protected readonly List<string> TempFiles = new();
    protected readonly ExcelOptions Options;

    protected BaseExcelTests()
    {
        Options = CreateOptions();
        Logger = TestHelper.CreateLogger<T>();
        Excel = CreateExcelInstance(Options, Logger);
    }

    /// <summary>
    /// 创建Excel实例
    /// </summary>
    protected abstract T CreateExcelInstance(ExcelOptions options, ILogger<T> logger);

    /// <summary>
    /// 创建测试选项
    /// </summary>
    protected virtual ExcelOptions CreateOptions() => new ExcelOptions
    {
        ParallelProcessingThreshold = 1000,
        AutoFitColumns = true,
        DefaultDateFormat = "yyyy-MM-dd"
    };

    #region 标准测试方法

    /// <summary>
    /// 测试列表转内存流功能
    /// </summary>
    protected void TestConvertCollectionToMemoryStream()
    {
        // Arrange
        var persons = TestHelper.CreateTestPersonList(10);

        // Act
        using var stream = Excel.ConvertCollectionToMemoryStream(persons, "TestSheet", "测试标题");

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream!.Length > 0);

        // 验证Excel内容 - 由子类实现具体验证
        stream.Position = 0;
        VerifyExcelContent(stream, persons, "TestSheet", "测试标题");
    }

    /// <summary>
    /// 验证Excel内容
    /// </summary>
    protected abstract void VerifyExcelContent<TData>(Stream stream, List<TData> data, string sheetName, string title) where TData : class;

    /// <summary>
    /// 测试DataTable转内存流功能
    /// </summary>
    protected void TestConvertDataTableToMemoryStream()
    {
        // Arrange
        var dataTable = TestHelper.CreateTestDataTable(10);

        // Act
        using var stream = Excel.ConvertDataTableToMemoryStream(dataTable, "TestSheet", "测试标题");

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream!.Length > 0);

        // 验证内容 - 由子类实现具体验证
        stream.Position = 0;
        VerifyExcelContent(stream, dataTable, "TestSheet", "测试标题");
    }

    /// <summary>
    /// 验证Excel内容
    /// </summary>
    protected abstract void VerifyExcelContent(Stream stream, DataTable dataTable, string sheetName, string title);

    /// <summary>
    /// 测试流转DataTable功能
    /// </summary>
    protected void TestConvertStreamToDataTable()
    {
        // Arrange - 创建测试Excel文件
        var originalData = TestHelper.CreateTestDataTable(5);
        var filePath = Path.GetTempFileName() + ".xlsx";
        TempFiles.Add(filePath);

        using (var stream = Excel.ConvertDataTableToMemoryStream(originalData, "TestSheet"))
        {
            using var fs = File.Create(filePath);
            stream!.Position = 0;
            stream.CopyTo(fs);
        }

        // Act
        DataTable? result;
        using (var fs = File.OpenRead(filePath))
        {
            result = Excel.ConvertStreamToDataTable(fs);
        }

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalData.Columns.Count, result!.Columns.Count);
        Assert.Equal(originalData.Rows.Count, result.Rows.Count);

        // 验证数据
        VerifyDataTableContent(originalData, result);
    }

    /// <summary>
    /// 验证DataTable内容
    /// </summary>
    protected virtual void VerifyDataTableContent(DataTable original, DataTable result)
    {
        for (int colIndex = 0; colIndex < original.Columns.Count; colIndex++)
        {
            for (int rowIndex = 0; rowIndex < original.Rows.Count; rowIndex++)
            {
                var originalValue = original.Rows[rowIndex][colIndex];
                var resultValue = result.Rows[rowIndex][colIndex];

                if (originalValue is DateTime dateTime)
                {
                    // 日期只比较日期部分
                    Assert.Equal(dateTime.Date, ((DateTime)resultValue).Date);
                }
                else if (originalValue != DBNull.Value)
                {
                    // 使用字符串比较以处理可能的类型差异
                    Assert.Equal(Convert.ToString(originalValue), Convert.ToString(resultValue));
                }
            }
        }
    }

    /// <summary>
    /// 测试模板创建功能
    /// </summary>
    protected void TestCreateExcelTemplate()
    {
        // Act
        using var templateStream = Excel.CreateExcelTemplate<TestPerson>();

        // Assert
        Assert.NotNull(templateStream);
        Assert.True(templateStream.Length > 0);

        // 验证模板
        templateStream.Position = 0;
        VerifyExcelTemplate(templateStream);
    }

    /// <summary>
    /// 验证Excel模板
    /// </summary>
    protected abstract void VerifyExcelTemplate(Stream templateStream);

    /// <summary>
    /// 测试列表转文件功能
    /// </summary>
    protected void TestListToFile()
    {
        // Arrange
        var persons = TestHelper.CreateTestPersonList(5);
        var filePath = Path.GetTempFileName() + ".xlsx";
        TempFiles.Add(filePath);

        // Act
        var result = Excel.ListToFile(persons, filePath);

        // Assert
        Assert.Equal(filePath, result);
        Assert.True(File.Exists(filePath));

        // 验证文件内容
        VerifyExcelFile(filePath, persons.Count + 1); // +1表示表头行
    }

    /// <summary>
    /// 验证Excel文件
    /// </summary>
    protected abstract void VerifyExcelFile(string filePath, int expectedRowCount);

    /// <summary>
    /// 测试DataTable转文件功能
    /// </summary>
    protected void TestDataTableToFile()
    {
        // Arrange
        var dt = TestHelper.CreateTestDataTable(5);
        var filePath = Path.GetTempFileName() + ".xlsx";
        TempFiles.Add(filePath);

        // Act
        var result = Excel.DataTableToFile(dt, filePath);

        // Assert
        Assert.Equal(filePath, result);
        Assert.True(File.Exists(filePath));

        // 验证文件内容
        VerifyExcelFile(filePath, dt.Rows.Count + 1); // +1表示表头行
    }

    /// <summary>
    /// 测试性能监控功能
    /// </summary>
    protected void TestPerformanceMonitoring()
    {
        // Arrange
        var (mockLogger, logMessages) = PerformanceTestHelper.CreateMockLogger<T>();
        var options = PerformanceTestHelper.CreatePerformanceEnabledOptions();
        var excel = CreateExcelInstance(options, mockLogger.Object);
        var largeDataTable = PerformanceTestHelper.GenerateLargeDataTable(200);

        // Act
        using var stream = excel.ConvertDataTableToMemoryStream(largeDataTable);

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream!.Length > 0);
        Assert.Contains(logMessages, m => m.Contains("导出到Excel"));
    }

    /// <summary>
    /// 测试边界条件
    /// </summary>
    protected void TestEdgeCases()
    {
        // 空列表
        using var emptyResult = Excel.ConvertCollectionToMemoryStream(new List<TestPerson>(), "EmptySheet");
        Assert.NotNull(emptyResult);

        // 空DataTable
        var emptyDt = new DataTable();
        emptyDt.Columns.Add("Test", typeof(string));
        using var emptyDtResult = Excel.ConvertDataTableToMemoryStream(emptyDt, "EmptyDtSheet");
        Assert.NotNull(emptyDtResult);

        // 不存在的文件
        var nonExistentResult = Excel.ExcelToDataTable("non_existent_file.xlsx");
        Assert.Null(nonExistentResult);

        // 空流
        var emptyStreamResult = Excel.ConvertStreamToDataTable(new MemoryStream());
        Assert.Null(emptyStreamResult);
    }

    // 添加更多通用测试方法
    protected void TestExcelToList()
    {
        // Arrange - 创建测试Excel文件
        var originalPersons = TestHelper.CreateTestPersonList(10);
        var filePath = Path.GetTempFileName() + ".xlsx";
        TempFiles.Add(filePath);

        Excel.ListToFile(originalPersons, filePath);

        // Act
        var importedPersons = Excel.ExcelToList<TestPerson>(filePath);

        // Assert
        Assert.NotNull(importedPersons);
        Assert.Equal(originalPersons.Count, importedPersons!.Count);

        // 比较数据
        for (int i = 0; i < originalPersons.Count; i++)
        {
            Assert.Equal(originalPersons[i].Id, importedPersons[i].Id);
            Assert.Equal(originalPersons[i].Name, importedPersons[i].Name);
            Assert.Equal(originalPersons[i].Birthday.Date, importedPersons[i].Birthday.Date);
        }
    }

    protected void TestExcelToDataTable()
    {
        // Arrange - 创建测试Excel文件
        var originalData = TestHelper.CreateTestDataTable(5);
        var filePath = Path.GetTempFileName() + ".xlsx";
        TempFiles.Add(filePath);

        Excel.DataTableToFile(originalData, filePath);

        // Act
        var importedData = Excel.ExcelToDataTable(filePath);

        // Assert
        Assert.NotNull(importedData);
        Assert.Equal(originalData.Columns.Count, importedData!.Columns.Count);
        Assert.Equal(originalData.Rows.Count, importedData.Rows.Count);

        // 比较数据
        VerifyDataTableContent(originalData, importedData);
    }

    protected void TestParallelPerformance()
    {
        // Arrange
        var largePersonList = TestHelper.CreateTestPersonList(3000);

        // 确保启用并行处理
        var originalThreshold = Options.ParallelProcessingThreshold;
        Options.ParallelProcessingThreshold = 1000;

        // Act - 测量执行时间
        var sw = System.Diagnostics.Stopwatch.StartNew();
        using var stream = Excel.ConvertCollectionToMemoryStream(largePersonList);
        sw.Stop();

        // 恢复原始设置
        Options.ParallelProcessingThreshold = originalThreshold;

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream!.Length > 0);

        // 记录性能数据 - 不做硬性断言，因为依赖于硬件
        System.Diagnostics.Debug.WriteLine($"处理3000条数据耗时: {sw.ElapsedMilliseconds}ms");
    }

    protected void TestCustomFormatting()
    {
        // Arrange
        var persons = TestHelper.CreateTestPersonList(5);

        // 添加自定义行为
        bool customActionExecuted = false;
        void CustomAction(object worksheet, System.Reflection.PropertyInfo[] properties)
        {
            customActionExecuted = true;
        }

        // Act
        using var stream = Excel.ConvertCollectionToMemoryStream(persons, "TestSheet", action: CustomAction);

        // Assert
        Assert.NotNull(stream);
        Assert.True(stream!.Length > 0);
        Assert.True(customActionExecuted, "自定义操作应该被执行");
    }

    #endregion

    public virtual void Dispose()
    {
        // 清理测试文件
        foreach (var file in TempFiles)
        {
            TestHelper.DeleteFileIfExists(file);
        }
    }
}
