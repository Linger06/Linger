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
        Assert.Equal(1.1m, result.Rows[0]["John"].ToDecimal());
        Assert.Equal(2.2m, result.Rows[1]["Jane"].ToDecimal());
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
        Assert.Equal(1.1m, result.Rows[0]["John"].ToDecimal());
        Assert.Equal(2.2m, result.Rows[1]["Jane"].ToDecimal());
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

        DBNull? dfd = DBNull.Value;

        DataColumn[]? groupColumns = new[] { table.Columns["Int"]! };
        DataColumn[]? captionColumns = new[] { table.Columns["Name"]! };
        DataColumn? valueColumn = table.Columns["Decimal"]!;

        DataTable? result = table.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(3, result.Columns.Count);
        Assert.Equal(0m, result.Rows[0]["John"].ToDecimal());
        Assert.Equal(2.2m, result.Rows[1]["Jane"].ToDecimal());
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
        Assert.Equal(2.2m, result.Rows[0]["John"].ToDecimal());
    }

    [Fact]
    public void Paging_ReturnsPaginatedDataTable()
    {
        DataTable? table = CreateTestDataTable();

        DataTable? result = table.Paging(1, 1);

        Assert.NotNull(result);
        Assert.Single(result.Rows);
        Assert.Equal(1, result.Rows[0]["Int"].ToInt());
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

    [Fact]
    public void ToList_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();

        var result = table.ToList<TestClass2>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Int);
        Assert.Null(result[0].NullableInt);
        Assert.Equal("John", result[0].Name);
        Assert.Equal(2, result[1].Int);
        Assert.Equal(2, result[1].NullableInt);
        Assert.Equal("Jane", result[1].Name);
        Assert.Null(result[0].NullBool);
        Assert.Null(result[0].NullShort);
        Assert.Null(result[0].NullLong);
        Assert.Null(result[1].NullDecimal);
        Assert.Null(result[1].NullFloat);
        Assert.Null(result[1].NullDouble);
    }

    [Fact]
    public void ToList_HandlesEmptyDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Int", typeof(int));

        var result = table.ToList<TestClass2>();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToList_HandlesNullDataTable()
    {
        DataTable? table = null;

        var result = table.ToList<TestClass2>();

        Assert.Null(result);
    }
}