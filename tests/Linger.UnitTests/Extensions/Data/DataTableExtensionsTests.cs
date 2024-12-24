namespace Linger.UnitTests;

public partial class DataTableExtensionsTests
{
    private DataTable CreateTestDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Int", typeof(int));
        table.Columns.Add("NullableInt", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Guid", typeof(Guid));
        table.Columns.Add("NullableGuid", typeof(Guid));
        table.Columns.Add("DateTime", typeof(DateTime));
        table.Columns.Add("NullableDateTime", typeof(DateTime));
        table.Columns.Add("Binary", typeof(byte[]));
        table.Columns.Add("Boolean", typeof(bool));
        table.Columns.Add("Int16", typeof(short));
        table.Columns.Add("Int64", typeof(long));
        table.Columns.Add("Decimal", typeof(decimal));
        table.Columns.Add("Single", typeof(float));
        table.Columns.Add("Double", typeof(double));

        table.Columns.Add("NullBool", typeof(bool));
        table.Columns.Add("NullShort", typeof(short));
        table.Columns.Add("NullLong", typeof(long));
        table.Columns.Add("NullDecimal", typeof(decimal));
        table.Columns.Add("NullFloat", typeof(float));
        table.Columns.Add("NullDouble", typeof(double));
        table.Columns.Add("NotIncludeInClass", typeof(string));

        table.Rows.Add(1, DBNull.Value, "John", Guid.NewGuid(), DBNull.Value, DateTime.Now, DBNull.Value, new byte[] { 1, 2, 3 }, true, (short)1, 1L, 1.1m, 1.1f, 1.1);
        table.Rows.Add(2, 2, "Jane", Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now, new byte[] { 4, 5, 6 }, false, (short)2, 2L, 2.2m, 2.2f, 2.2);

        return table;
    }

    [Fact]
    public async Task ToListAsync_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();

        List<TestClass2>? result = await table.ToListAsync<TestClass2>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Int);
        Assert.Null(result[0].NullableInt);
        Assert.Equal("John", result[0].Name);
        Assert.Equal(2, result[1].Int);
        Assert.Equal(2, result[1].NullableInt);
        Assert.Equal("Jane", result[1].Name);
    }

    [Fact]
    public void ClearEmptyRow_ReturnNullIfNull()
    {
        DataTable? table = null;
        DataTable? result = table.ClearEmptyRow();
        Assert.Null(result);
    }

    [Fact]
    public void ClearEmptyRow_RemovesEmptyRows()
    {
        DataTable? table = CreateTestDataTable();
        table.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

        DataTable? result = table.ClearEmptyRow();

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
    }

    [Fact]
    public void Find_ReturnsFilteredDataTable()
    {
        DataTable? table = CreateTestDataTable();

        DataTable? result = table.Find("Int = 1");

        Assert.NotNull(result);
        Assert.Single(result.Rows);
        Assert.Equal(1, result.Rows[0]["Int"]);
    }

    [Fact]
    public void Sort_ReturnsSortedDataTable()
    {
        DataTable? table = CreateTestDataTable();

        DataTable? result = table.Sort("Int DESC");

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows[0]["Int"]);
        Assert.Equal(1, result.Rows[1]["Int"]);
    }

    [Fact]
    public void Distinct_ReturnsDistinctDataTable()
    {
        DataTable? table = CreateTestDataTable();
        table.Rows.Add(1, DBNull.Value, "John", Guid.NewGuid(), DBNull.Value, DateTime.Now, DBNull.Value, new byte[] { 1, 2, 3 }, true, (short)1, 1L, 1.1m, 1.1f, 1.1);

        DataTable? result = table.Distinct(new[] { "Int", "Name" });

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
    }

    [Fact]
    public void Sum_ReturnsSumOfColumn()
    {
        DataTable? table = CreateTestDataTable();

        var result = table.Sum("Int");

        Assert.Equal(3, result);
    }

    [Fact]
    public void Combine_ReturnsCombinedDataTable()
    {
        DataTable? table1 = CreateTestDataTable();
        DataTable? table2 = CreateTestDataTable();

        DataTable? result = table1.Combine(table2);

        Assert.NotNull(result);
        Assert.Equal(4, result.Rows.Count);
    }

    [Fact]
    public void ContainAllColumns_ReturnsTrueIfAllColumnsExist()
    {
        DataTable? table = CreateTestDataTable();

        var result = table.ContainAllColumns("Int,Name");

        Assert.True(result);
    }

    [Fact]
    public void ContainAllColumns_ReturnsFalseIfAnyColumnDoesNotExist()
    {
        DataTable? table = CreateTestDataTable();

        var result = table.ContainAllColumns("Int,NonExistentColumn");

        Assert.False(result);
    }

    [Fact]
    public void Join_ReturnsJoinedDataTable()
    {
        DataTable? table1 = CreateTestDataTable();
        DataTable? table2 = CreateTestDataTable();

        DataTable? result = table1.Join(table2, new[] { table1.Columns["Int"]! }, new[] { table2.Columns["Int"]! }, true, false);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(1, result.Rows[0]["Int"]);
        Assert.Equal("John", result.Rows[0]["Name"]);
        Assert.Equal(2, result.Rows[1]["Int"]);
        Assert.Equal("Jane", result.Rows[1]["Name"]);
    }

    [Fact]
    public void Join_LeftJoin_ReturnsAllRowsFromLeftTable()
    {
        DataTable? table1 = CreateTestDataTable();
        DataTable? table2 = CreateTestDataTable();
        table2.Rows.Clear();

        DataTable? result = table1.Join(table2, new[] { table1.Columns["Int"]! }, new[] { table2.Columns["Int"]! }, true, false);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(1, result.Rows[0]["Int"]);
        Assert.Equal("John", result.Rows[0]["Name"]);
        Assert.Equal(2, result.Rows[1]["Int"]);
        Assert.Equal("Jane", result.Rows[1]["Name"]);
    }

    [Fact]
    public void Join_RightJoin_ReturnsAllRowsFromRightTable()
    {
        DataTable? table1 = CreateTestDataTable();
        table1.Rows.Clear();
        DataTable? table2 = CreateTestDataTable();

        DataTable? result = table1.Join(table2, new[] { table1.Columns["Int"]! }, new[] { table2.Columns["Int"]! }, false, true);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(1, result.Rows[0]["Int_2"]);
        Assert.Equal("John", result.Rows[0]["Name_2"]);
        Assert.Equal(2, result.Rows[1]["Int_2"]);
        Assert.Equal("Jane", result.Rows[1]["Name_2"]);
    }

    [Fact]
    public void Join_FullOuterJoin_ReturnsAllRowsFromBothTables()
    {
        DataTable? table1 = CreateTestDataTable();
        DataTable? table2 = CreateTestDataTable();
        table2.Rows.Add(3, 3, "Doe", Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now, new byte[] { 7, 8, 9 }, true, (short)3, 3L, 3.3m, 3.3f, 3.3);

        DataTable? result = table1.Join(table2, new[] { table1.Columns["Int"]! }, new[] { table2.Columns["Int"]! }, true, true);

        Assert.NotNull(result);
        Assert.Equal(3, result.Rows.Count);
        Assert.Equal(1, result.Rows[0]["Int"]);
        Assert.Equal("John", result.Rows[0]["Name"]);
        Assert.Equal(2, result.Rows[1]["Int"]);
        Assert.Equal("Jane", result.Rows[1]["Name"]);
        Assert.Equal(3, result.Rows[2]["Int_2"]);
        Assert.Equal("Doe", result.Rows[2]["Name_2"]);
    }

    private class TestClass2
    {
        public int Int { get; set; }
        public int? NullableInt { get; set; }
        public string? Name { get; set; }
        public Guid Guid { get; set; }
        public Guid? NullableGuid { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public bool Boolean { get; set; }
        public short Int16 { get; set; }
        public long Int64 { get; set; }
        public decimal Decimal { get; set; }
        public float Single { get; set; }
        public double Double { get; set; }
        public bool? NullBool { get; set; }
        public short? NullShort { get; set; }
        public long? NullLong { get; set; }
        public decimal? NullDecimal { get; set; }
        public float? NullFloat { get; set; }
        public double? NullDouble { get; set; }
        public string NotIncludeInDataTable { get; set; }
    }
}