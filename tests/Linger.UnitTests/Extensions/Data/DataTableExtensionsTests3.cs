using System.Data;
using Linger.Extensions.Core;
using Linger.Extensions.Data;

namespace Linger.UnitTests;

public partial class DataTableExtensionsTests
{
    [Fact]
    public void TableRowTurnToColumn_ReturnsCorrectTransformedDataTable()
    {
        // 创建测试数据表
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("ProductName", typeof(string));
        sourceTable.Columns.Add("Quantity", typeof(decimal));

        sourceTable.Rows.Add(1, "ProductA", 10m);
        sourceTable.Rows.Add(1, "ProductB", 20m);
        sourceTable.Rows.Add(2, "ProductA", 15m);
        sourceTable.Rows.Add(2, "ProductB", 25m);

        // 定义分组列、标题列和值列
        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["ProductName"] };
        var valueColumn = sourceTable.Columns["Quantity"];

        // 执行行转列
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // 打印调试信息 (仅测试时使用)
        Console.WriteLine($"结果表列数: {result.Columns.Count}");
        foreach (DataColumn col in result.Columns)
        {
            Console.WriteLine($"列名: {col.ColumnName}, 类型: {col.DataType.Name}");
        }
        
        Console.WriteLine($"结果表行数: {result.Rows.Count}");
        foreach (DataRow row in result.Rows)
        {
            Console.WriteLine($"GroupId: {row["GroupId"]}, ProductA: {row["ProductA"]}, ProductB: {row["ProductB"]}");
        }

        // 断言
        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(3, result.Columns.Count); // GroupId、ProductA、ProductB
        
        // 检查列名
        var columnNames = result.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        Assert.Contains("GroupId", columnNames);
        Assert.Contains("ProductA", columnNames);
        Assert.Contains("ProductB", columnNames);
        
        // 验证数据 - 使用更精确的类型转换和比较
        Assert.Equal(1, Convert.ToInt32(result.Rows[0]["GroupId"]));
        Assert.Equal(10m, Convert.ToDecimal(result.Rows[0]["ProductA"]));
        Assert.Equal(20m, Convert.ToDecimal(result.Rows[0]["ProductB"]));
        Assert.Equal(2, Convert.ToInt32(result.Rows[1]["GroupId"]));
        Assert.Equal(15m, Convert.ToDecimal(result.Rows[1]["ProductA"]));
        Assert.Equal(25m, Convert.ToDecimal(result.Rows[1]["ProductB"]));
    }

    [Fact]
    public void Paging_ReturnsCorrectPageOfData()
    {
        // 创建测试数据表
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("Id", typeof(int));
        sourceTable.Columns.Add("Name", typeof(string));

        for (int i = 1; i <= 50; i++)
        {
            sourceTable.Rows.Add(i, $"Item{i}");
        }

        // 获取第 2 页，每页 10 条记录
        var result = sourceTable.Paging(2, 10);

        // 断言
        Assert.NotNull(result);
        Assert.Equal(10, result.Rows.Count);
        Assert.Equal(11, result.Rows[0]["Id"]);
        Assert.Equal("Item11", result.Rows[0]["Name"]);
        Assert.Equal(20, result.Rows[9]["Id"]);
        Assert.Equal("Item20", result.Rows[9]["Name"]);
    }

    [Fact]
    public void Paging_WithNullTable_ReturnsNull()
    {
        DataTable? nullTable = null;
        var result = nullTable.Paging(1, 10);
        Assert.Null(result);
    }

    [Fact]
    public void Paging_WithEmptyTable_ReturnsEmptyTable()
    {
        var emptyTable = new DataTable();
        emptyTable.Columns.Add("Id", typeof(int));

        var result = emptyTable.Paging(1, 10);

        Assert.NotNull(result);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void Paging_WithPageIndexBeyondAvailableData_ReturnsEmptyDataTable()
    {
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("Id", typeof(int));

        for (int i = 1; i <= 10; i++)
        {
            sourceTable.Rows.Add(i);
        }

        var result = sourceTable.Paging(3, 5); // 第三页，但只有10条数据

        Assert.NotNull(result);
        Assert.Empty(result.Rows);
    }

}
