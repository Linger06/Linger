using System.Data;
using Linger.Excel.Tests.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Linger.Excel.Tests.Helpers;

/// <summary>
/// 测试辅助类
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// 创建测试用日志记录器
    /// </summary>
    public static ILogger<T> CreateLogger<T>()
    {
        return new Mock<ILogger<T>>().Object;
    }

    /// <summary>
    /// 创建测试用DataTable
    /// </summary>
    public static DataTable CreateTestDataTable(int rowCount = 10)
    {
        var dt = new DataTable("TestTable");
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Birthday", typeof(DateTime));
        dt.Columns.Add("Salary", typeof(decimal));
        dt.Columns.Add("IsActive", typeof(bool));
        dt.Columns.Add("Department", typeof(string));

        var random = new Random(42); // 固定种子以确保结果可重现

        for (int i = 0; i < rowCount; i++)
        {
            var row = dt.NewRow();
            row["Id"] = i + 1;
            row["Name"] = $"测试{i + 1}";
            row["Birthday"] = DateTime.Now.AddDays(-random.Next(1000, 10000));
            row["Salary"] = Math.Round((decimal)(random.NextDouble() * 10000 + 5000), 2);
            row["IsActive"] = random.Next(2) == 1;
            row["Department"] = random.Next(5) == 0 ? DBNull.Value : (object)$"部门{random.Next(1, 6)}";
            dt.Rows.Add(row);
        }

        return dt;
    }

    /// <summary>
    /// 创建测试用Person列表
    /// </summary>
    public static List<TestPerson> CreateTestPersonList(int count = 10)
    {
        var result = new List<TestPerson>();
        var random = new Random(42); // 固定种子以确保结果可重现

        for (int i = 0; i < count; i++)
        {
            result.Add(new TestPerson
            {
                Id = i + 1,
                Name = $"测试人员{i + 1}",
                Birthday = DateTime.Today.AddYears(-random.Next(20, 60)).AddDays(-random.Next(0, 365)),
                Salary = Math.Round((decimal)(random.NextDouble() * 10000 + 5000), 2),
                IsActive = random.Next(2) == 1,
                Remark = $"备注{i + 1}",
                Department = random.Next(5) == 0 ? null : $"部门{random.Next(1, 6)}",
                JoinDate = random.Next(5) == 0 ? null : (DateTime?)DateTime.Today.AddYears(-random.Next(1, 10))
            });
        }

        return result;
    }

    /// <summary>
    /// 获取临时文件路径
    /// </summary>
    public static string GetTempFilePath(string extension = ".xlsx")
    {
        return Path.Combine(Path.GetTempPath(), $"ExcelTest_{Guid.NewGuid():N}{extension}");
    }

    /// <summary>
    /// 创建测试用的Excel文件
    /// </summary>
    public static string CreateTestExcelFile(int rowCount = 10)
    {
        var filePath = GetTempFilePath();
        var dt = CreateTestDataTable(rowCount);

        using (var package = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");

            // 填充表头
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = dt.Columns[i].ColumnName;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // 填充数据
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    var value = dt.Rows[row][col];
                    if (value != DBNull.Value)
                        worksheet.Cells[row + 2, col + 1].Value = value;
                }
            }

            package.SaveAs(new FileInfo(filePath));
        }

        return filePath;
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    public static void DeleteFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // 忽略删除失败的异常
            }
        }
    }

    /// <summary>
    /// 比较两个DataTable是否相等
    /// </summary>
    public static bool AreDataTablesEqual(DataTable dt1, DataTable dt2)
    {
        if (dt1.Rows.Count != dt2.Rows.Count || dt1.Columns.Count != dt2.Columns.Count)
            return false;

        // 比较列名
        for (int i = 0; i < dt1.Columns.Count; i++)
        {
            if (dt1.Columns[i].ColumnName != dt2.Columns[i].ColumnName)
                return false;
        }

        // 比较数据
        for (int i = 0; i < dt1.Rows.Count; i++)
        {
            for (int j = 0; j < dt1.Columns.Count; j++)
            {
                var val1 = dt1.Rows[i][j];
                var val2 = dt2.Rows[i][j];

                if (val1 is DBNull && val2 is DBNull)
                    continue;

                if (val1 is DBNull || val2 is DBNull)
                    return false;

                if (!val1.Equals(val2))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 比较两个对象列表是否相等
    /// </summary>
    public static bool AreListsEqual<T>(List<T> list1, List<T> list2, params string[] propertiesToCompare) where T : class
    {
        if (list1.Count != list2.Count)
            return false;

        var props = typeof(T).GetProperties()
            .Where(p => propertiesToCompare.Length == 0 || propertiesToCompare.Contains(p.Name))
            .ToList();

        for (int i = 0; i < list1.Count; i++)
        {
            foreach (var prop in props)
            {
                var value1 = prop.GetValue(list1[i]);
                var value2 = prop.GetValue(list2[i]);

                if (value1 == null && value2 == null)
                    continue;

                if (value1 == null || value2 == null)
                    return false;

                if (!value1.Equals(value2))
                    return false;
            }
        }

        return true;
    }
}
