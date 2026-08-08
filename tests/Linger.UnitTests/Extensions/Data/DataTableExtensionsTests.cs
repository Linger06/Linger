using LingerDataTableExtensions = Linger.Extensions.Data.DataTableExtensions;

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
    public void ToList_WithMapper_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();

        List<TestClass2>? result = table.ToList(row => new TestClass2
        {
            Int = Convert.ToInt32(row["Int"]),
            Name = row["Name"]?.ToString(),
            NullableInt = row["NullableInt"] == DBNull.Value ? null : Convert.ToInt32(row["NullableInt"])
        });

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Int);
        Assert.Equal("John", result[0].Name);
        Assert.Null(result[0].NullableInt);
        Assert.Equal(2, result[1].Int);
        Assert.Equal("Jane", result[1].Name);
        Assert.Equal(2, result[1].NullableInt);
    }

    [Fact]
    public void ToList_WithMapperAndNullDataTable_ReturnsNull()
    {
        DataTable? table = null;

        List<TestClass2>? result = table.ToList(row => new TestClass2
        {
            Int = Convert.ToInt32(row["Int"]),
            Name = row["Name"]?.ToString()
        });

        Assert.Null(result);
    }

    [Fact]
    public void ToList_WithMapperAndParameterizedConstructor_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();

        List<TestClassWithConstructor>? result = table.ToList(row =>
        {
            return new TestClassWithConstructor(tenantId: 7, source: "mapper")
            {
                Int = Convert.ToInt32(row["Int"]),
                Name = row["Name"]?.ToString()
            };
        });

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(7, result[0].TenantId);
        Assert.Equal("mapper", result[0].Source);
        Assert.Equal(1, result[0].Int);
        Assert.Equal("John", result[0].Name);
        Assert.Equal(2, result[1].Int);
        Assert.Equal("Jane", result[1].Name);
    }

    [Fact]
    public void ToList_WithMapperAndRequiredMemberParameterizedConstructor_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();

        List<TestClassWithRequiredAndConstructor>? result = table.ToList(row =>
        {
            return new TestClassWithRequiredAndConstructor(tenantId: 13, source: "mapper-required")
            {
                Int = Convert.ToInt32(row["Int"]),
                Name = row["Name"]?.ToString()
            };
        });

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(13, result[0].TenantId);
        Assert.Equal("mapper-required", result[0].Source);
        Assert.Equal(1, result[0].Int);
        Assert.Equal("John", result[0].Name);
        Assert.Equal(2, result[1].Int);
        Assert.Equal("Jane", result[1].Name);
    }

    [Fact]
    public void ToList_WithFactoryAndColumnSetters_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();
        var setters = new Dictionary<string, Action<TestClass2, object?>>
        {
            ["Int"] = LingerDataTableExtensions.CreateColumnSetter<TestClass2, int>((x, v) => x.Int = v),
            ["Name"] = LingerDataTableExtensions.CreateColumnSetter<TestClass2, string?>((x, v) => x.Name = v),
            ["NullableInt"] = LingerDataTableExtensions.CreateColumnSetter<TestClass2, int?>((x, v) => x.NullableInt = v)
        };

        List<TestClass2>? result = table.ToList(() => new TestClass2(), setters);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Int);
        Assert.Equal("John", result[0].Name);
        Assert.Null(result[0].NullableInt);
        Assert.Equal(2, result[1].Int);
        Assert.Equal("Jane", result[1].Name);
        Assert.Equal(2, result[1].NullableInt);
    }

    [Fact]
    public void ToList_WithFactoryAndColumnSettersAndParameterizedConstructor_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();
        var setters = new Dictionary<string, Action<TestClassWithConstructor, object?>>
        {
            ["Int"] = LingerDataTableExtensions.CreateColumnSetter<TestClassWithConstructor, int>((x, v) => x.Int = v),
            ["Name"] = LingerDataTableExtensions.CreateColumnSetter<TestClassWithConstructor, string?>((x, v) => x.Name = v)
        };

        List<TestClassWithConstructor>? result = table.ToList(() => new TestClassWithConstructor(tenantId: 9, source: "factory"), setters);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(9, result[0].TenantId);
        Assert.Equal("factory", result[0].Source);
        Assert.Equal(1, result[0].Int);
        Assert.Equal("John", result[0].Name);
        Assert.Equal(2, result[1].Int);
        Assert.Equal("Jane", result[1].Name);
    }

    [Fact]
    public void ToList_WithFactoryAndColumnSettersAndRequiredMemberParameterizedConstructor_ReturnsListOfObjects()
    {
        DataTable? table = CreateTestDataTable();
        var setters = new Dictionary<string, Action<TestClassWithRequiredAndConstructor, object?>>
        {
            ["Int"] = LingerDataTableExtensions.CreateColumnSetter<TestClassWithRequiredAndConstructor, int>((x, v) => x.Int = v),
            ["Name"] = LingerDataTableExtensions.CreateColumnSetter<TestClassWithRequiredAndConstructor, string?>((x, v) => x.Name = v)
        };

        List<TestClassWithRequiredAndConstructor>? result = table.ToList(() => new TestClassWithRequiredAndConstructor(tenantId: 17, source: "factory-required"), setters);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(17, result[0].TenantId);
        Assert.Equal("factory-required", result[0].Source);
        Assert.Equal(1, result[0].Int);
        Assert.Equal("John", result[0].Name);
        Assert.Equal(2, result[1].Int);
        Assert.Equal("Jane", result[1].Name);
    }

    [Fact]
    public void ToList_WithFactoryAndColumnSettersAndNoMatchingColumns_ThrowsArgumentException()
    {
        DataTable? table = CreateTestDataTable();
        var setters = new Dictionary<string, Action<TestClass2, object?>>
        {
            ["NotExists"] = (x, v) => x.Name = v?.ToString()
        };

        var exception = Assert.Throws<ArgumentException>(() => table.ToList(() => new TestClass2(), setters));
        Assert.Equal("columnSetters", exception.ParamName);
    }

    [Fact]
    public void CreateColumnSetter_CanReuseConversionLogicAcrossMappings()
    {
        DataTable? table = CreateTestDataTable();

        var intSetter = LingerDataTableExtensions.CreateColumnSetter<TestClass2, int>((x, v) => x.Int = v);
        var nameSetter = LingerDataTableExtensions.CreateColumnSetter<TestClass2, string?>((x, v) => x.Name = v);
        var nullableIntSetter = LingerDataTableExtensions.CreateColumnSetter<TestClass2, int?>((x, v) => x.NullableInt = v);

        var setters = new Dictionary<string, Action<TestClass2, object?>>
        {
            ["Int"] = intSetter,
            ["Name"] = nameSetter,
            ["NullableInt"] = nullableIntSetter
        };

        List<TestClass2>? result = table.ToList(() => new TestClass2(), setters);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Int);
        Assert.Equal("John", result[0].Name);
        Assert.Null(result[0].NullableInt);
        Assert.Equal(2, result[1].Int);
        Assert.Equal("Jane", result[1].Name);
        Assert.Equal(2, result[1].NullableInt);
    }

    [Fact]
    public void CreateColumnSetter_WithNullValue_AssignsDefaultToNullableTarget()
    {
        var target = new TestClass2 { NullableInt = 42, Name = "existing" };
        var nullableIntSetter = LingerDataTableExtensions.CreateColumnSetter<TestClass2, int?>((x, v) => x.NullableInt = v);
        var nameSetter = LingerDataTableExtensions.CreateColumnSetter<TestClass2, string?>((x, v) => x.Name = v);

        nullableIntSetter(target, null);
        nameSetter(target, DBNull.Value);

        Assert.Null(target.NullableInt);
        Assert.Null(target.Name);
    }

    [Fact]
    public void CreateColumnSetter_WithNullValueAndDefaultAssignmentDisabled_PreservesTargetValue()
    {
        var target = new TestClass2 { NullableInt = 42 };
        var setter = LingerDataTableExtensions.CreateColumnSetter<TestClass2, int?>(
            (x, v) => x.NullableInt = v,
            assignDefaultWhenNull: false);

        setter(target, null);

        Assert.Equal(42, target.NullableInt);
    }

    [Fact]
    public void CreateColumnSetter_WithUnsupportedValue_ThrowsConversionException()
    {
        var original = new Uri("https://original.example");
        var target = new UriTarget { Value = original };
        var setter = LingerDataTableExtensions.CreateColumnSetter<UriTarget, Uri>((x, v) => x.Value = v);

        var exception = Assert.Throws<InvalidOperationException>(() => setter(target, "https://replacement.example"));

        Assert.Same(original, target.Value);
        Assert.Contains(nameof(Uri), exception.Message);
    }

    [Fact]
    public void Paging_WithOverflowingOffset_ReturnsEmptyClone()
    {
        var table = CreateTestDataTable();

        var result = table.Paging(int.MaxValue, 2);

        Assert.NotNull(result);
        Assert.Empty(result.Rows);
        Assert.Equal(table.Columns.Count, result.Columns.Count);
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
    public void Sum_WithInvalidValue_ThrowsInvalidCastException()
    {
        var table = new DataTable();
        table.Columns.Add("Value", typeof(object));
        table.Rows.Add(1.5d);
        table.Rows.Add("invalid");

        var exception = Assert.Throws<InvalidCastException>(() => table.Sum("Value"));

        Assert.Contains("Value", exception.Message);
        Assert.Contains("1", exception.Message);
    }

    [Fact]
    public void Sum_WithDbNull_IgnoresNullValues()
    {
        var table = new DataTable();
        table.Columns.Add("Value", typeof(object));
        table.Rows.Add(1.5d);
        table.Rows.Add(DBNull.Value);

        var result = table.Sum("Value");

        Assert.Equal(1.5d, result);
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
    public void Combine_WithDifferentColumnCounts_ThrowsArgumentException()
    {
        var table1 = new DataTable();
        table1.Columns.Add("Id", typeof(int));
        table1.Columns.Add("Name", typeof(string));
        table1.Rows.Add(1, "existing");

        var table2 = new DataTable();
        table2.Columns.Add("Id", typeof(int));
        table2.Rows.Add(2);

        var exception = Assert.Throws<ArgumentException>(() => table1.Combine(table2));

        Assert.Equal("dataTable2", exception.ParamName);
    }

    [Fact]
    public void Combine_WithDifferentColumnTypes_ThrowsArgumentException()
    {
        var table1 = new DataTable();
        table1.Columns.Add("Id", typeof(int));

        var table2 = new DataTable();
        table2.Columns.Add("Id", typeof(string));

        var exception = Assert.Throws<ArgumentException>(() => table1.Combine(table2));

        Assert.Equal("dataTable2", exception.ParamName);
    }

    [Fact]
    public void Combine_WithUnchangedSourceRows_CreatesAddedRows()
    {
        var table1 = new DataTable();
        table1.Columns.Add("Id", typeof(int));
        table1.Rows.Add(1);
        table1.AcceptChanges();

        var table2 = table1.Clone();

        var result = table1.Combine(table2);

        Assert.Equal(DataRowState.Added, result.Rows[0].RowState);
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

    [Fact]
    public void Join_WithDuplicateRightKeys_ReturnsEveryMatchingPair()
    {
        var left = new DataTable();
        left.Columns.Add("Key", typeof(int));
        left.Columns.Add("LeftValue", typeof(string));
        left.Rows.Add(1, "left");

        var right = new DataTable();
        right.Columns.Add("Key", typeof(int));
        right.Columns.Add("RightValue", typeof(string));
        right.Rows.Add(1, "right-1");
        right.Rows.Add(1, "right-2");

        var result = left.Join(
            right,
            new[] { left.Columns["Key"]! },
            new[] { right.Columns["Key"]! },
            includeLeftJoin: false,
            includeRightJoin: false);

        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("right-1", result.Rows[0]["RightValue"]);
        Assert.Equal("right-2", result.Rows[1]["RightValue"]);
    }

    [Fact]
    public void Join_WithDifferentKeyColumnCounts_ThrowsArgumentException()
    {
        var left = new DataTable();
        left.Columns.Add("Id", typeof(int));
        left.Columns.Add("Code", typeof(string));

        var right = new DataTable();
        right.Columns.Add("Id", typeof(int));

        var exception = Assert.Throws<ArgumentException>(() => left.Join(
            right,
            new[] { left.Columns["Id"]!, left.Columns["Code"]! },
            new[] { right.Columns["Id"]! },
            includeLeftJoin: false,
            includeRightJoin: false));

        Assert.Equal("rightCols", exception.ParamName);
    }

    private sealed class UriTarget
    {
        public required Uri Value { get; set; }
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

    private sealed class TestClassWithConstructor(int tenantId, string source)
    {
        public int TenantId { get; } = tenantId;
        public string Source { get; } = source;

        public int Int { get; set; }
        public string? Name { get; set; }
    }

    private sealed class TestClassWithRequiredAndConstructor
    {
        public int TenantId { get; }
        public required string Source { get; init; }

        public int Int { get; set; }
        public string? Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public TestClassWithRequiredAndConstructor(int tenantId, string source)
        {
            TenantId = tenantId;
            Source = source;
        }
    }
}
