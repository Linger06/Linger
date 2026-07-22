namespace Linger.UnitTests;

public partial class DataTableExtensionsTests
{
    [Fact]
    public void TableRowTurnToColumn_ConvertsRowsToColumns()
    {
        DataTable? table = CreateTestDataTable();
        DataColumn[]? groupColumns = new[] { table.Columns["Int"]! };
        DataColumn[]? captionColumns = new[] { table.Columns["Name"]! };
        DataColumn? valueColumn = table.Columns["Decimal"]!;

        DataTable? result = table.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(3, result.Columns.Count);
        Assert.Equal(1.1m, result.Rows[0]["John"].ToDecimalOrDefault());
        Assert.Equal(2.2m, result.Rows[1]["Jane"].ToDecimalOrDefault());
    }

    [Fact]
    public void TableRowTurnToColumn_HandlesMultipleGroupColumns()
    {
        DataTable? table = CreateTestDataTable();
        table.Columns.Add("Category", typeof(string));
        table.Rows[0]["Category"] = "A";
        table.Rows[1]["Category"] = "B";

        DataColumn[]? groupColumns = new[] { table.Columns["Int"]!, table.Columns["Category"]! };
        DataColumn[]? captionColumns = new[] { table.Columns["Name"]! };
        DataColumn? valueColumn = table.Columns["Decimal"]!;

        DataTable? result = table.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(4, result.Columns.Count);
        Assert.Equal(1.1m, result.Rows[0]["John"].ToDecimalOrDefault());
        Assert.Equal(2.2m, result.Rows[1]["Jane"].ToDecimalOrDefault());
    }

    [Fact]
    public void TableRowTurnToColumn_HandlesEmptyDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Int", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Decimal", typeof(decimal));

        DataColumn[]? groupColumns = new[] { table.Columns["Int"]! };
        DataColumn[]? captionColumns = new[] { table.Columns["Name"]! };
        DataColumn? valueColumn = table.Columns["Decimal"]!;

        DataTable? result = table.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        Assert.NotNull(result);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void TableRowTurnToColumn_HandlesNullValues()
    {
        DataTable? table = CreateTestDataTable();
        table.Rows[0]["Decimal"] = DBNull.Value;

        DataColumn[]? groupColumns = new[] { table.Columns["Int"]! };
        DataColumn[]? captionColumns = new[] { table.Columns["Name"]! };
        DataColumn? valueColumn = table.Columns["Decimal"]!;

        DataTable? result = table.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(3, result.Columns.Count);
        Assert.Equal(0m, result.Rows[0]["John"].ToDecimalOrDefault());
        Assert.Equal(2.2m, result.Rows[1]["Jane"].ToDecimalOrDefault());
    }

    [Fact]
    public void TableRowTurnToColumn_HandlesDuplicateCaptions()
    {
        DataTable? table = CreateTestDataTable();
        table.Rows.Add(1, DBNull.Value, "John", Guid.NewGuid(), DBNull.Value, DateTime.Now, DBNull.Value, new byte[] { 1, 2, 3 }, true, (short)1, 1L, 1.1m, 1.1f, 1.1);

        DataColumn[]? groupColumns = new[] { table.Columns["Int"]! };
        DataColumn[]? captionColumns = new[] { table.Columns["Name"]! };
        DataColumn? valueColumn = table.Columns["Decimal"]!;

        DataTable? result = table.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(3, result.Columns.Count);
        Assert.Equal(2.2m, result.Rows[0]["John"].ToDecimalOrDefault());
    }

    [Fact]
    public void Paging_ReturnsPaginatedDataTable()
    {
        DataTable? table = CreateTestDataTable();

        DataTable? result = table.Paging(1, 1);

        Assert.NotNull(result);
        Assert.Single(result.Rows);
        Assert.Equal(1, result.Rows[0]["Int"].ToIntOrDefault());
    }

    [Fact]
    public void Paging_HandlesEmptyDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Int", typeof(int));

        DataTable? result = table.Paging(1, 1);

        Assert.NotNull(result);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void Paging_HandlesNullDataTable()
    {
        DataTable? table = null;
        DataTable? result = table.Paging(1, 1);
        Assert.Null(result);
    }

    [Fact]
    public void Paging_HandlesPageIndexOutOfRange()
    {
        DataTable? table = CreateTestDataTable();

        DataTable? result = table.Paging(3, 1);

        Assert.NotNull(result);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void ToJsonString_ReturnsJsonString()
    {
        DataTable? table = CreateTestDataTable();

        var result = table.ToJsonString();

        Assert.NotNull(result);
        Assert.Contains("\"Int\":1", result);
        Assert.Contains("\"Name\":\"John\"", result);
    }

    [Fact]
    public void ToJsonString_HandlesEmptyDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Int", typeof(int));

        var result = table.ToJsonString();

        Assert.Equal("[]", result);
    }

    [Fact]
    public void ToJsonString_HandlesNullDataTable()
    {
        DataTable? table = null;

        var result = table.ToJsonString();

        Assert.Equal("null", result);
    }

}
