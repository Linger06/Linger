using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Linger.Excel;
using Linger.Excel.Contracts;
using Linger.Excel.Npoi;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests.BackwardCompatibilityTests;

/// <summary>
/// 测试 Excel 导出/导入方法的功能性
/// 确保行为一致性
/// </summary>
public class LegacyMethodCompatibilityTests : IDisposable
{
    private readonly string _testFilesDir;
    private readonly ILogger<NpoiExcel> _logger;
    private readonly NpoiExcel _excelService;

    public LegacyMethodCompatibilityTests()
    {
        _testFilesDir = Path.Combine(Path.GetTempPath(), "LingerExcelTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testFilesDir);

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
        _logger = loggerFactory.CreateLogger<NpoiExcel>();
        _excelService = new NpoiExcel(null, _logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFilesDir))
        {
            Directory.Delete(_testFilesDir, true);
        }
    }

    /// <summary>
    /// 创建测试用的Excel文件(多个Sheet)
    /// 包含复杂场景:空行、日期、各种数据类型、null值等
    /// </summary>
    private string CreateMultiSheetTestFile()
    {
        var filePath = Path.Combine(_testFilesDir, $"MultiSheet_{Guid.NewGuid():N}.xlsx");
        var dataSet = new DataSet();

        // Sheet1 - Users (包含空行、null值、日期)
        var dt1 = new DataTable("Users");
        dt1.Columns.Add("ID", typeof(int));
        dt1.Columns.Add("Name", typeof(string));
        dt1.Columns.Add("BirthDate", typeof(DateTime));
        dt1.Columns.Add("Salary", typeof(decimal));
        dt1.Columns.Add("IsActive", typeof(bool));
        dt1.Columns.Add("Email", typeof(string));

        // 正常数据行
        dt1.Rows.Add(1, "张三", new DateTime(1990, 5, 15), 8500.50m, true, "zhangsan@example.com");
        dt1.Rows.Add(2, "李四", new DateTime(1985, 8, 20), 12000.00m, true, "lisi@example.com");
        
        // 包含空值的行
        dt1.Rows.Add(3, "王五", DBNull.Value, 9500.75m, false, DBNull.Value);
        
        // 正常数据
        dt1.Rows.Add(4, "赵六", new DateTime(1992, 12, 1), 7800.00m, true, "zhaoliu@example.com");
        
        // 部分空值
        dt1.Rows.Add(5, DBNull.Value, new DateTime(1988, 3, 25), DBNull.Value, true, "unknown@example.com");
        
        dataSet.Tables.Add(dt1);

        // Sheet2 - Orders (包含更多数据类型和边界值)
        var dt2 = new DataTable("Orders");
        dt2.Columns.Add("OrderNo", typeof(string));
        dt2.Columns.Add("OrderDate", typeof(DateTime));
        dt2.Columns.Add("Amount", typeof(decimal));
        dt2.Columns.Add("Quantity", typeof(int));
        dt2.Columns.Add("Discount", typeof(double));
        dt2.Columns.Add("Status", typeof(string));

        // 正常订单
        dt2.Rows.Add("ORD001", new DateTime(2024, 1, 15, 10, 30, 0), 1500.50m, 3, 0.1, "已完成");
        dt2.Rows.Add("ORD002", new DateTime(2024, 2, 20, 14, 45, 0), 2800.75m, 5, 0.15, "处理中");
        
        // 包含空值
        dt2.Rows.Add("ORD003", new DateTime(2024, 3, 10, 9, 0, 0), DBNull.Value, 0, DBNull.Value, "已取消");
        
        // 大数值
        dt2.Rows.Add("ORD004", new DateTime(2024, 4, 5, 16, 20, 0), 99999.99m, 100, 0.25, "已完成");
        
        // 特殊字符
        dt2.Rows.Add("ORD-005/A", new DateTime(2024, 5, 1, 8, 15, 0), 450.00m, 1, 0.0, "待确认");

        dataSet.Tables.Add(dt2);

        // Sheet3 - Products (包含更多边界情况)
        var dt3 = new DataTable("Products");
        dt3.Columns.Add("ProductID", typeof(int));
        dt3.Columns.Add("ProductName", typeof(string));
        dt3.Columns.Add("Price", typeof(decimal));
        dt3.Columns.Add("Stock", typeof(int));
        dt3.Columns.Add("LastUpdate", typeof(DateTime));

        dt3.Rows.Add(101, "笔记本电脑", 5999.99m, 50, new DateTime(2024, 1, 1));
        dt3.Rows.Add(102, "无线鼠标", 89.90m, 200, new DateTime(2024, 2, 15));
        
        // 空库存
        dt3.Rows.Add(103, "机械键盘", 299.00m, 0, new DateTime(2024, 3, 20));
        
        // 负数库存(特殊情况)
        dt3.Rows.Add(104, "显示器", 1299.00m, -5, new DateTime(2024, 4, 10));
        
        // null价格
        dt3.Rows.Add(105, "待定产品", DBNull.Value, 0, DBNull.Value);

        dataSet.Tables.Add(dt3);

        _excelService.DataSetToExcel(dataSet, filePath);
        return filePath;
    }

    [Fact]
    public void ExcelToDataSet_CompatibleSignature_AcceptsCommaSeparatedString()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - use comma-separated string (compatible with old method)
        var result = _excelService.ExcelToDataSet(filePath, "Users,Orders", 0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tables.Count);
        Assert.Contains(result.Tables.Cast<DataTable>(), t => t.TableName == "Users");
        Assert.Contains(result.Tables.Cast<DataTable>(), t => t.TableName == "Orders");
    }

    [Fact]
    public void ExcelToDataSet_CompatibleSignature_NullSheetNames_ReadsAllSheets()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - null means all sheets (compatible with old method)
        var result = _excelService.ExcelToDataSet(filePath, null, 0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Tables.Count); // 现在有3个工作表
        Assert.Contains(result.Tables.Cast<DataTable>(), t => t.TableName == "Users");
        Assert.Contains(result.Tables.Cast<DataTable>(), t => t.TableName == "Orders");
        Assert.Contains(result.Tables.Cast<DataTable>(), t => t.TableName == "Products");
    }

    [Fact]
    public void ExcelToDataSet_CompatibleSignature_HandlesWhitespace()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - with whitespace
        var result = _excelService.ExcelToDataSet(filePath, " Users , Orders ", 0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tables.Count);
    }

    [Fact]
    public void ExcelToDataSet_CompatibleSignature_SingleSheet()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - single sheet
        var result = _excelService.ExcelToDataSet(filePath, "Users", 0);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tables);
        Assert.Equal("Users", result.Tables[0].TableName);
    }

    [Fact]
    public async Task ExcelToDataSetAsync_CompatibleSignature_WorksCorrectly()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - async compatible version
        var result = await _excelService.ExcelToDataSetAsync(filePath, "Users,Orders", 0);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tables.Count);
    }

    [Fact]
    public void ExcelToDataSet_CompatibleMethod_ReadsDataCorrectly()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act
        var result = _excelService.ExcelToDataSet(filePath, "Users", 0);

        // Assert
        Assert.NotNull(result);
        var userTable = result.Tables[0];
        Assert.Equal("Users", userTable.TableName);
        Assert.Equal(6, userTable.Columns.Count); // ID, Name, BirthDate, Salary, IsActive, Email
        Assert.Equal(5, userTable.Rows.Count);
        
        // 验证第一行数据 (Excel可能将数字读取为double)
        Assert.Equal(1.0, Convert.ToDouble(userTable.Rows[0]["ID"]));
        Assert.Equal("张三", userTable.Rows[0]["Name"]);
        Assert.Equal(new DateTime(1990, 5, 15), Convert.ToDateTime(userTable.Rows[0]["BirthDate"]));
        Assert.Equal(8500.50m, Convert.ToDecimal(userTable.Rows[0]["Salary"]));
        Assert.Equal(true, Convert.ToBoolean(userTable.Rows[0]["IsActive"]));
        
        // 验证包含DBNull的行
        Assert.Equal(3.0, Convert.ToDouble(userTable.Rows[2]["ID"]));
        Assert.Equal(DBNull.Value, userTable.Rows[2]["BirthDate"]);
        Assert.Equal(DBNull.Value, userTable.Rows[2]["Email"]);
    }

    [Fact]
    public void CompareResults_OldStyleVsNewStyle_ProduceSameOutput()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - old style (comma-separated string)
        var resultOldStyle = _excelService.ExcelToDataSet(filePath, "Users,Orders", 0);

        // Act - new style (strong-typed collection)
        var resultNewStyle = _excelService.ExcelToDataSet(
            filePath,
            new[] { "Users", "Orders" },
            headerRowIndex: 0,
            addEmptyRow: false
        );

        // Assert - both should produce same results
        Assert.NotNull(resultOldStyle);
        Assert.NotNull(resultNewStyle);
        Assert.Equal(resultOldStyle.Tables.Count, resultNewStyle.Tables.Count);

        for (int i = 0; i < resultOldStyle.Tables.Count; i++)
        {
            var oldTable = resultOldStyle.Tables[i];
            var newTable = resultNewStyle.Tables[i];

            Assert.Equal(oldTable.TableName, newTable.TableName);
            Assert.Equal(oldTable.Columns.Count, newTable.Columns.Count);
            Assert.Equal(oldTable.Rows.Count, newTable.Rows.Count);
        }
    }

    [Fact]
    public void CompareResults_NPOIHelperVsNewMethod_ProduceSameOutput()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - 使用旧的 NPOIHelper.ImportExcelToDs 方法
        var resultOldNPOIHelper = NpoiHelper.ImportExcelToDs(filePath, "Users,Orders", 0);

        // Act - 使用新的兼容方法
        var resultNewMethod = _excelService.ExcelToDataSet(filePath, "Users,Orders", 0);

        // Assert - 两者应该产生相同的结果
        Assert.NotNull(resultOldNPOIHelper);
        Assert.NotNull(resultNewMethod);
        Assert.Equal(resultOldNPOIHelper.Tables.Count, resultNewMethod.Tables.Count);

        for (int i = 0; i < resultOldNPOIHelper.Tables.Count; i++)
        {
            var oldTable = resultOldNPOIHelper.Tables[i];
            var newTable = resultNewMethod.Tables[i];

            Assert.Equal(oldTable.TableName, newTable.TableName);
            Assert.Equal(oldTable.Columns.Count, newTable.Columns.Count);
            Assert.Equal(oldTable.Rows.Count, newTable.Rows.Count);

            // 验证数据内容一致
            for (int row = 0; row < oldTable.Rows.Count; row++)
            {
                for (int col = 0; col < oldTable.Columns.Count; col++)
                {
                    var oldValue = oldTable.Rows[row][col];
                    var newValue = newTable.Rows[row][col];
                    Assert.Equal(oldValue, newValue);
                }
            }
        }
    }

    [Fact]
    public void CompareResults_NPOIHelperWithNull_ReadsAllSheets()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - 使用旧的 NPOIHelper.ImportExcelToDs 方法 (null = 所有工作表)
        var resultOldNPOIHelper = NpoiHelper.ImportExcelToDs(filePath, null, 0);

        // Act - 使用新的兼容方法 (null = 所有工作表)
        var resultNewMethod = _excelService.ExcelToDataSet(filePath, null, 0);

        // Assert - 两者应该产生相同的结果
        Assert.NotNull(resultOldNPOIHelper);
        Assert.NotNull(resultNewMethod);
        Assert.Equal(resultOldNPOIHelper.Tables.Count, resultNewMethod.Tables.Count);
        Assert.Equal(3, resultOldNPOIHelper.Tables.Count); // 应该读取所有3个工作表
    }

    [Fact]
    public void ComplexData_WithDatesAndNulls_HandledCorrectly()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - 使用旧的 NPOIHelper 方法读取 Orders 表
        var resultOld = NpoiHelper.ImportExcelToDs(filePath, "Orders", 0);

        // Act - 使用新的兼容方法读取 Orders 表
        var resultNew = _excelService.ExcelToDataSet(filePath, "Orders", 0);

        // Assert
        Assert.NotNull(resultOld);
        Assert.NotNull(resultNew);
        
        var oldTable = resultOld.Tables[0];
        var newTable = resultNew.Tables[0];
        
        Assert.Equal(oldTable.Rows.Count, newTable.Rows.Count);
        Assert.Equal(5, oldTable.Rows.Count); // 5个订单
        
        // 验证日期数据
        Assert.Equal(oldTable.Rows[0]["OrderDate"], newTable.Rows[0]["OrderDate"]);
        
        // 验证包含DBNull的行(ORD003)
        Assert.Equal(oldTable.Rows[2]["Amount"], newTable.Rows[2]["Amount"]);
        Assert.Equal(oldTable.Rows[2]["Discount"], newTable.Rows[2]["Discount"]);
    }

    [Fact]
    public void ComplexData_MultipleSheets_ProduceSameResults()
    {
        // Arrange
        var filePath = CreateMultiSheetTestFile();

        // Act - 读取所有3个工作表
        var resultOld = NpoiHelper.ImportExcelToDs(filePath, "Users,Orders,Products", 0);
        var resultNew = _excelService.ExcelToDataSet(filePath, "Users,Orders,Products", 0);

        // Assert
        Assert.NotNull(resultOld);
        Assert.NotNull(resultNew);
        Assert.Equal(3, resultOld.Tables.Count);
        Assert.Equal(3, resultNew.Tables.Count);

        // 验证每个工作表
        for (int i = 0; i < resultOld.Tables.Count; i++)
        {
            var oldTable = resultOld.Tables[i];
            var newTable = resultNew.Tables[i];

            Assert.Equal(oldTable.TableName, newTable.TableName);
            Assert.Equal(oldTable.Columns.Count, newTable.Columns.Count);
            Assert.Equal(oldTable.Rows.Count, newTable.Rows.Count);

            // 验证数据内容一致(包括DBNull)
            for (int row = 0; row < oldTable.Rows.Count; row++)
            {
                for (int col = 0; col < oldTable.Columns.Count; col++)
                {
                    var oldValue = oldTable.Rows[row][col];
                    var newValue = newTable.Rows[row][col];
                    
                    // 特殊处理DBNull比较
                    if (oldValue == DBNull.Value && newValue == DBNull.Value)
                    {
                        continue;
                    }
                    
                    Assert.Equal(oldValue, newValue);
                }
            }
        }
    }
}
