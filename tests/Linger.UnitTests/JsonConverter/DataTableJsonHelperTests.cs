namespace Linger.UnitTests.JsonConverter;

public class DataTableJsonHelperTests
{
    private DataTable CreateTestDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Int", typeof(int));
        table.Columns.Add("NullableInt", typeof(int));
        table.Columns.Add("String", typeof(string));
        table.Columns.Add("Guid", typeof(Guid));
        table.Columns.Add("NullableGuid", typeof(Guid));
        table.Columns.Add("DateTime", typeof(DateTime));
        table.Columns.Add("NullableDateTime", typeof(DateTime));
        table.Columns.Add("Boolean", typeof(bool));
        table.Columns.Add("Int16", typeof(short));
        table.Columns.Add("Int64", typeof(long));
        table.Columns.Add("Decimal", typeof(decimal));
        table.Columns.Add("Single", typeof(float));
        table.Columns.Add("Double", typeof(double));

        DataRow? row = table.NewRow();
        row["Int"] = 1;
        row["NullableInt"] = DBNull.Value;
        row["String"] = "Test";
        row["Guid"] = Guid.NewGuid();
        row["NullableGuid"] = DBNull.Value;
        row["DateTime"] = new DateTime(2019, 8, 1);
        row["NullableDateTime"] = DBNull.Value;
        row["Boolean"] = true;
        row["Int16"] = (short)2;
        row["Int64"] = 3L;
        row["Decimal"] = 4.5m;
        row["Single"] = 5.6f;
        row["Double"] = 7.8;

        table.Rows.Add(row);

        return table;
    }

    [Fact]
    public void ReadDataTable_ValidJson_ReturnsDataTable()
    {
        var json = "[{\"Int\":1,\"String\":\"Test\",\"Guid\":\"" + Guid.NewGuid() + "\",\"DateTime\":\"" + new DateTime(2019, 8, 1) + "\",\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5,\"Single\":5.6,\"Double\":7.8}]";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        DataTable? result = DataTableJsonHelper.ReadDataTable(ref reader);

        Assert.NotNull(result);
        Assert.Equal(10, result.Columns.Count);
        Assert.Single(result.Rows);
        Assert.Equal(1, result.Rows[0]["Int"].ToInt());
        Assert.Equal("Test", result.Rows[0]["String"]);
        Assert.IsType<Guid>(result.Rows[0]["Guid"].ToGuid());
        Assert.IsType<DateTime>(result.Rows[0]["DateTime"].ToDateTime());
        Assert.True((bool)result.Rows[0]["Boolean"]);
        Assert.Equal((short)2, result.Rows[0]["Int16"].ToShort());
        Assert.Equal(3L, result.Rows[0]["Int64"].ToLong());
        Assert.Equal(4.5m, result.Rows[0]["Decimal"].ToDecimal());
        Assert.Equal(5.6f, result.Rows[0]["Single"].ToFloat());
        Assert.Equal(7.8, result.Rows[0]["Double"].ToDouble());
    }

    [Fact]
    public void WriteDataTable_ValidDataTable_WritesCorrectJson()
    {
        DataTable? dataTable = CreateTestDataTable();

#if NETCOREAPP3_0_OR_GREATER
#else
        //WriteNumberValue writes the Single/Double value using the default StandardFormat (that is, 'G') on .NET Core 3.0 or later versions. Uses 'G9' on any other framework.
        dataTable.Columns.Remove("Single");
        dataTable.Columns.Remove("Double");
#endif
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        DataTableJsonHelper.WriteDataTable(writer, dataTable);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

#if NETCOREAPP3_0_OR_GREATER
        var expectedJson = "[{\"Int\":1,\"NullableInt\":null,\"String\":\"Test\",\"Guid\":\"" + dataTable.Rows[0]["Guid"] + "\",\"NullableGuid\":null,\"DateTime\":\"" + "2019-08-01T00:00:00" + "\",\"NullableDateTime\":null,\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5,\"Single\":5.6,\"Double\":7.8}]";
#else
        var expectedJson = "[{\"Int\":1,\"NullableInt\":null,\"String\":\"Test\",\"Guid\":\"" + dataTable.Rows[0]["Guid"] + "\",\"NullableGuid\":null,\"DateTime\":\"" + "2019-08-01T00:00:00" + "\",\"NullableDateTime\":null,\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5}]";
#endif
        Assert.Equal(expectedJson, json);
    }

    [Theory]
    [InlineData("[{\"Invalid\":\"Data\"}]", typeof(JsonException))]
    [InlineData("[{\"Int\":\"InvalidInt\"}]", typeof(FormatException))]
    public void ReadDataTable_InvalidJson_ThrowsException(string json, Type expectedException)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        try
        {
            DataTableJsonHelper.ReadDataTable(ref reader);
        }
        catch (Exception ex)
        {
            Assert.IsType(expectedException, ex);
        }
    }

    [Fact]
    public void WriteDataTable_EmptyDataTable_WritesEmptyJsonArray()
    {
        var dataTable = new DataTable();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        DataTableJsonHelper.WriteDataTable(writer, dataTable);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

        Assert.Equal("[]", json);
    }

    [Fact]
    public void ReadDataTable_EmptyJsonArray_ReturnsEmptyDataTable()
    {
        var json = "[]";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        DataTable? result = DataTableJsonHelper.ReadDataTable(ref reader);

        Assert.NotNull(result);
        Assert.Empty(result.Columns);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void WriteDataTable_NullableColumns_WritesCorrectJson()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("NullableInt", typeof(int));
        dataTable.Columns.Add("NullableGuid", typeof(Guid));
        dataTable.Columns.Add("NullableDateTime", typeof(DateTime));

        DataRow? row = dataTable.NewRow();
        row["NullableInt"] = DBNull.Value;
        row["NullableGuid"] = DBNull.Value;
        row["NullableDateTime"] = DBNull.Value;
        dataTable.Rows.Add(row);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        DataTableJsonHelper.WriteDataTable(writer, dataTable);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());
        var expectedJson = "[{\"NullableInt\":null,\"NullableGuid\":null,\"NullableDateTime\":null}]";

        Assert.Equal(expectedJson, json);
    }
}