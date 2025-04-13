using System.Data;
using System.Text.Json;
using Linger.Extensions.Core;
using Linger.Extensions.Data;

namespace Linger.UnitTests;

public partial class DataTableExtensionsTests
{
    [Fact]
    public void TableRowTurnToColumn_WithEmptyTable_ReturnsTableWithGroupColumnsOnly()
    {
        // 创建空数据表
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("ProductName", typeof(string));
        sourceTable.Columns.Add("Quantity", typeof(decimal));

        // 不添加任何行

        // 定义分组列、标题列和值列
        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["ProductName"] };
        var valueColumn = sourceTable.Columns["Quantity"];

        // 执行行转列
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // 断言
        Assert.NotNull(result);
        Assert.Empty(result.Rows);
        Assert.Single(result.Columns); // 只有GroupId列
        Assert.Equal("GroupId", result.Columns[0].ColumnName);
    }

    [Fact]
    public void TableRowTurnToColumn_WithSpecialCharactersInCaptionValues_HandlesThemCorrectly()
    {
        // 创建测试数据表
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("ProductName", typeof(string));
        sourceTable.Columns.Add("Quantity", typeof(decimal));

        // 添加包含特殊字符的产品名
        sourceTable.Rows.Add(1, "Product/A", 10m);
        sourceTable.Rows.Add(1, "Product*B", 20m);
        sourceTable.Rows.Add(2, "Product:C", 15m);
        sourceTable.Rows.Add(2, "Product?D", 25m);

        // 定义分组列、标题列和值列
        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["ProductName"] };
        var valueColumn = sourceTable.Columns["Quantity"];

        // 执行行转列
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // 断言
        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(5, result.Columns.Count); // GroupId 和 4个产品列
        
        // 检查列名 - 应该已经被净化
        var columnNames = result.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        Assert.Contains("GroupId", columnNames);
        Assert.Contains("Product_A", columnNames); // / 替换为 _
        Assert.Contains("Product_B", columnNames); // * 替换为 _
        Assert.Contains("Product_C", columnNames); // : 替换为 _
        Assert.Contains("Product_D", columnNames); // ? 替换为 _
        
        // 验证数据
        Assert.Equal(1, Convert.ToInt32(result.Rows[0]["GroupId"]));
        Assert.Equal(10m, Convert.ToDecimal(result.Rows[0]["Product_A"]));
        Assert.Equal(20m, Convert.ToDecimal(result.Rows[0]["Product_B"]));
        Assert.Equal(2, Convert.ToInt32(result.Rows[1]["GroupId"]));
        Assert.Equal(15m, Convert.ToDecimal(result.Rows[1]["Product_C"]));
        Assert.Equal(25m, Convert.ToDecimal(result.Rows[1]["Product_D"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithNullValues_HandlesThemCorrectly()
    {
        // 创建测试数据表
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("ProductName", typeof(string));
        sourceTable.Columns.Add("Quantity", typeof(decimal));

        // 添加包含NULL值的行
        sourceTable.Rows.Add(1, "ProductA", 10m);
        sourceTable.Rows.Add(1, DBNull.Value, 20m); // NULL产品名
        sourceTable.Rows.Add(2, "ProductA", DBNull.Value); // NULL数量
        sourceTable.Rows.Add(2, "ProductB", 25m);

        // 定义分组列、标题列和值列
        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["ProductName"] };
        var valueColumn = sourceTable.Columns["Quantity"];

        // 执行行转列
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // 断言
        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        
        // 检查列名
        var columnNames = result.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        Assert.Contains("GroupId", columnNames);
        Assert.Contains("ProductA", columnNames);
        Assert.Contains("ProductB", columnNames);
        Assert.Contains("NULL", columnNames); // 代表NULL产品名
        
        // 验证数据
        Assert.Equal(1, Convert.ToInt32(result.Rows[0]["GroupId"]));
        Assert.Equal(10m, Convert.ToDecimal(result.Rows[0]["ProductA"]));
        Assert.Equal(20m, Convert.ToDecimal(result.Rows[0]["NULL"]));
        Assert.Equal(2, Convert.ToInt32(result.Rows[1]["GroupId"]));
        // ProductA值应为0或DBNull
        if (result.Rows[1]["ProductA"] != DBNull.Value)
        {
            Assert.Equal(0m, Convert.ToDecimal(result.Rows[1]["ProductA"]));
        }
        Assert.Equal(25m, Convert.ToDecimal(result.Rows[1]["ProductB"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithSimilarGroupValues_GroupsThemCorrectly()
    {
        // 创建测试数据表
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(string)); // 使用字符串类型，测试"1"和"10"不会被合并
        sourceTable.Columns.Add("ProductName", typeof(string));
        sourceTable.Columns.Add("Quantity", typeof(decimal));

        // 添加容易混淆的分组值
        sourceTable.Rows.Add("1", "ProductA", 10m);
        sourceTable.Rows.Add("1", "ProductB", 20m);
        sourceTable.Rows.Add("10", "ProductA", 15m); // 如果直接拼接，"1"和"10"会生成相同的键
        sourceTable.Rows.Add("10", "ProductB", 25m);

        // 定义分组列、标题列和值列
        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["ProductName"] };
        var valueColumn = sourceTable.Columns["Quantity"];

        // 执行行转列
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // 断言
        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count); // 应该有两个分组："1"和"10"
        Assert.Equal(3, result.Columns.Count); // GroupId、ProductA、ProductB
        
        // 验证数据
        Assert.Equal("1", result.Rows[0]["GroupId"]);
        Assert.Equal(10m, Convert.ToDecimal(result.Rows[0]["ProductA"]));
        Assert.Equal(20m, Convert.ToDecimal(result.Rows[0]["ProductB"]));
        Assert.Equal("10", result.Rows[1]["GroupId"]);
        Assert.Equal(15m, Convert.ToDecimal(result.Rows[1]["ProductA"]));
        Assert.Equal(25m, Convert.ToDecimal(result.Rows[1]["ProductB"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithInvalidColumnName_ThrowsArgumentException()
    {
        // 创建测试数据表
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("ProductName", typeof(string));
        sourceTable.Columns.Add("Quantity", typeof(decimal));

        sourceTable.Rows.Add(1, "ProductA", 10m);

        // 定义分组列、标题列和值列，但使用不存在的列
        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var nonExistentColumn = new DataColumn("NonExistentColumn");
        var valueColumn = sourceTable.Columns["Quantity"];

        // 断言抛出异常
        Assert.Throws<ArgumentException>(() => 
            sourceTable.TableRowTurnToColumn(groupColumns, new[] { nonExistentColumn }, valueColumn));
    }
}