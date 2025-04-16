using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Attributes;
using Linger.Helper;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests
{
    /// <summary>
    /// Excel服务测试基类，提供通用测试方法
    /// </summary>
    public abstract class ExcelServiceTestBase
    {
        // 测试用的目录路径
        protected readonly string TestFilesDir;
        protected readonly ILogger Logger;
        protected readonly ExcelOptions Options;

        protected ExcelServiceTestBase()
        {
            // 确保测试目录存在
            TestFilesDir = Path.Combine(Path.GetTempPath(), "LingerExcelTests", GuidCode.NewGuid().ToString());
            if (!Directory.Exists(TestFilesDir))
            {
                Directory.CreateDirectory(TestFilesDir);
            }

            // 创建测试用的日志记录器
            var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().AddDebug());
            Logger = loggerFactory.CreateLogger("ExcelTests");

            // 创建测试用的配置选项
            Options = new ExcelOptions
            {
                AutoFitColumns = true,
                UseBatchWrite = true,
                BatchSize = 100,
                ParallelProcessingThreshold = 500,
                EnablePerformanceMonitoring = true,
                PerformanceThreshold = 100
            };
        }

        /// <summary>
        /// 获取测试用的Excel服务实例
        /// </summary>
        /// <returns>Excel服务实例</returns>
        protected abstract IExcelService GetExcelService();

        /// <summary>
        /// 生成测试用的DataTable
        /// </summary>
        /// <param name="rowCount">行数</param>
        /// <returns>测试用的DataTable</returns>
        protected DataTable GenerateTestDataTable(int rowCount = 100)
        {
            var dt = new DataTable("TestData");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Birthday", typeof(DateTime));
            dt.Columns.Add("Salary", typeof(decimal));
            dt.Columns.Add("IsActive", typeof(bool));

            for (int i = 1; i <= rowCount; i++)
            {
                var row = dt.NewRow();
                row["Id"] = i;
                row["Name"] = $"Person {i}";
                row["Birthday"] = DateTime.Now.AddYears(-20).AddDays(-i);
                row["Salary"] = 1000M + (i * 50.75M);
                row["IsActive"] = i % 2 == 0;
                dt.Rows.Add(row);
            }

            return dt;
        }

        /// <summary>
        /// 生成测试用的对象列表
        /// </summary>
        /// <param name="count">列表项数量</param>
        /// <returns>测试用的对象列表</returns>
        protected List<TestPerson> GenerateTestPersonList(int count = 100)
        {
            var list = new List<TestPerson>(count);

            for (int i = 1; i <= count; i++)
            {
                list.Add(new TestPerson
                {
                    Id = i,
                    Name = $"Person {i}",
                    Birthday = DateTime.Now.AddYears(-20).AddDays(-i),
                    Salary = 1000M + (i * 50.75M),
                    IsActive = i % 2 == 0,
                    Email = $"person{i}@example.com",
                    Department = i % 3 == 0 ? "HR" : (i % 3 == 1 ? "IT" : "Finance")
                });
            }

            return list;
        }

        /// <summary>
        /// 删除测试文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        protected void DeleteTestFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "删除测试文件失败: {FilePath}", filePath);
            }
        }

        /// <summary>
        /// 清理测试目录
        /// </summary>
        protected void CleanupTestDir()
        {
            try
            {
                var files = Directory.GetFiles(TestFilesDir, "*.xlsx");
                foreach (var file in files)
                {
                    DeleteTestFile(file);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "清理测试目录失败: {TestFilesDir}", TestFilesDir);
            }
        }
    }

    /// <summary>
    /// 测试用的人员类
    /// </summary>
    public class TestPerson
    {
        public int Id { get; set; }

        [ExcelColumn(ColumnName = "姓名", Index = 1)]
        public string Name { get; set; }

        [ExcelColumn(ColumnName = "生日", Index = 2)]
        public DateTime Birthday { get; set; }

        [ExcelColumn(ColumnName = "薪资", Index = 3)]
        public decimal Salary { get; set; }

        [ExcelColumn(ColumnName = "是否在职", Index = 4)]
        public bool IsActive { get; set; }

        [ExcelColumn(ColumnName = "电子邮箱", Index = 5)]
        public string Email { get; set; }

        public string Department { get; set; }
    }
}
