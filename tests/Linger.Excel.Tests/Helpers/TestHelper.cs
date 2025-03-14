using System.Data;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;

namespace Linger.Excel.Tests.Helpers;

public static class TestHelper
{
    private static readonly Random Random = new Random(42); // 固定种子以确保可重复的测试数据
    
    /// <summary>
    /// 创建简单的测试日志记录器
    /// </summary>
    public static ILogger<T> CreateLogger<T>()
    {
        return new Microsoft.Extensions.Logging.LoggerFactory().CreateLogger<T>();
    }
    
    /// <summary>
    /// 创建测试人员列表
    /// </summary>
    public static List<TestPerson> CreateTestPersonList(int count)
    {
        var result = new List<TestPerson>();
        for (int i = 0; i < count; i++)
        {
            result.Add(new TestPerson
            {
                Id = i + 1,
                Name = $"Person {i + 1}",
                Birthday = DateTime.Today.AddDays(-i * 30),
                Salary = 5000 + i * 100,
                IsActive = i % 2 == 0,
                Department = i % 3 == 0 ? "IT" : (i % 3 == 1 ? "HR" : "Finance"),
                JoinDate = DateTime.Today.AddYears(-i % 5)
            });
        }
        return result;
    }
    
    /// <summary>
    /// 创建测试数据表
    /// </summary>
    public static DataTable CreateTestDataTable(int rowCount)
    {
        var dt = new DataTable("TestTable");
        dt.Columns.Add("ID", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Date", typeof(DateTime));
        dt.Columns.Add("Value", typeof(decimal));
        dt.Columns.Add("IsActive", typeof(bool));

        for (int i = 0; i < rowCount; i++)
        {
            dt.Rows.Add(
                i + 1,
                $"Item {i + 1}",
                DateTime.Today.AddDays(-i),
                100.50m + i * 10.25m,
                i % 2 == 0
            );
        }
        
        return dt;
    }
    
    /// <summary>
    /// 删除文件
    /// </summary>
    public static void DeleteFileIfExists(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // 忽略删除失败的情况
        }
    }
    
    /// <summary>
    /// 创建测试Excel选项
    /// </summary>
    public static Contracts.ExcelOptions CreateExcelOptions(bool enablePerformanceMonitoring = false)
    {
        var options = new Contracts.ExcelOptions
        {
            ParallelProcessingThreshold = 1000,
            AutoFitColumns = true,
            EnablePerformanceMonitoring = enablePerformanceMonitoring,
            PerformanceThreshold = 50,
            UseBatchWrite = true,
            BatchSize = 1000,
            UseMemoryOptimization = true,
            MemoryBufferSize = 500
        };
        
        // 设置StyleOptions中的日期格式
        options.StyleOptions.DefaultDateFormat = "yyyy-MM-dd";
        
        return options;
    }
}
