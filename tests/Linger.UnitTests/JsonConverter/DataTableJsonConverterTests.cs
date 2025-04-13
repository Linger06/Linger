using System.Data;
using System.Text.Json;
using Linger.JsonConverter;
using Xunit.v3;

namespace Linger.UnitTests.JsonConverter;

public class DataTableJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public DataTableJsonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new DataTableJsonConverter() },
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 禁用 Unicode 转义
        };
    }

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
        table.Columns.Add("TimeSpanCol", typeof(TimeSpan));

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
        row["TimeSpanCol"] = new TimeSpan(10, 22, 10, 15, 100);

        table.Rows.Add(row);

        return table;
    }

    [Fact]
    public void Read_ValidJson_ReturnsDataTable()
    {
        var json = "[{\"Int\":1,\"String\":\"Test\",\"Guid\":\"" + Guid.NewGuid() + "\",\"DateTime\":\"" + DateTime.Now.ToString("o") + "\",\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5,\"Single\":5.6,\"Double\":7.8}]";
        DataTable? result = JsonSerializer.Deserialize<DataTable>(json, _options);

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
    public void Write_DataTable_WritesCorrectJson()
    {
        DataTable? dataTable = CreateTestDataTable();
#if NETCOREAPP3_0_OR_GREATER
#else
        dataTable.Columns.Remove("Single");
        dataTable.Columns.Remove("Double");
#endif

        var json = JsonSerializer.Serialize(dataTable, _options);

#if NETCOREAPP3_0_OR_GREATER
        var expectedJson = "[{\"Int\":1,\"NullableInt\":\"\",\"String\":\"Test\",\"Guid\":\"" + dataTable.Rows[0]["Guid"] + "\",\"NullableGuid\":\"\",\"DateTime\":\"" + "2019-08-01T00:00:00" + "\",\"NullableDateTime\":\"\",\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5,\"Single\":5.6,\"Double\":7.8,\"TimeSpanCol\":\"10.22:10:15.1000000\"}]";
#else
        var expectedJson = "[{\"Int\":1,\"NullableInt\":\"\",\"String\":\"Test\",\"Guid\":\"" + dataTable.Rows[0]["Guid"] + "\",\"NullableGuid\":\"\",\"DateTime\":\"" + "2019-08-01T00:00:00" + "\",\"NullableDateTime\":\"\",\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5,\"TimeSpanCol\":\"10.22:10:15.1000000\"}]";
#endif
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void WriteDataTable_ValidDataTable_WritesCorrectJson()
    {
        DataTable? dataTable = CreateTestDataTable();

#if NETCOREAPP3_0_OR_GREATER
#else
        dataTable.Columns.Remove("Single");
        dataTable.Columns.Remove("Double");
#endif
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        DataTableJsonHelper.WriteDataTable(writer, dataTable);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());

#if NETCOREAPP3_0_OR_GREATER
        var expectedJson = "[{\"Int\":1,\"NullableInt\":\"\",\"String\":\"Test\",\"Guid\":\"" + dataTable.Rows[0]["Guid"] + "\",\"NullableGuid\":\"\",\"DateTime\":\"" + "2019-08-01T00:00:00" + "\",\"NullableDateTime\":\"\",\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5,\"Single\":5.6,\"Double\":7.8,\"TimeSpanCol\":\"10.22:10:15.1000000\"}]";
#else
        var expectedJson = "[{\"Int\":1,\"NullableInt\":\"\",\"String\":\"Test\",\"Guid\":\"" + dataTable.Rows[0]["Guid"] + "\",\"NullableGuid\":\"\",\"DateTime\":\"" + "2019-08-01T00:00:00" + "\",\"NullableDateTime\":\"\",\"Boolean\":true,\"Int16\":2,\"Int64\":3,\"Decimal\":4.5,\"TimeSpanCol\":\"10.22:10:15.1000000\"}]";
#endif
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Read_InvalidJson_ThrowsJsonException()
    {
        var json = "[{\"NullableInt\":null}]";

        Assert.Throws<NotSupportedException>(() => JsonSerializer.Deserialize<DataTable>(json, _options));
    }

    [Fact]
    public void Write_EmptyDataTable_WritesEmptyJsonArray()
    {
        var dataTable = new DataTable();
        var json = JsonSerializer.Serialize(dataTable, _options);

        Assert.Equal("[]", json);
    }

    [Fact]
    public void Read_EmptyJsonArray_ReturnsEmptyDataTable()
    {
        var json = "[]";
        DataTable? result = JsonSerializer.Deserialize<DataTable>(json, _options);

        Assert.NotNull(result);
        Assert.Empty(result.Columns);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void Read_ValidJson_ReturnsDataTable2()
    {
        var json = "[{\"Column1\":\"Value1\",\"Column2\":123}]";
        DataTable? result = JsonSerializer.Deserialize<DataTable>(json, _options);

        Assert.NotNull(result);
        Assert.Equal(2, result.Columns.Count);
        Assert.Equal("Column1", result.Columns[0].ColumnName);
        Assert.Equal("Column2", result.Columns[1].ColumnName);
        Assert.Single(result.Rows);
        Assert.Equal("Value1", result.Rows[0]["Column1"]);
        Assert.Equal(123, result.Rows[0]["Column2"].ToInt());
    }

    [Fact]
    public void Write_DataTable_WritesCorrectJson2()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Column1", typeof(string));
        dataTable.Columns.Add("Column2", typeof(int));
        DataRow? row = dataTable.NewRow();
        row["Column1"] = "Value1";
        row["Column2"] = 123;
        dataTable.Rows.Add(row);

        var json = JsonSerializer.Serialize(dataTable, _options);
        var expectedJson = "[{\"Column1\":\"Value1\",\"Column2\":123}]";

        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void WriteDataTable_ValidDataTable_WritesCorrectJson2()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Column1", typeof(string));
        dataTable.Columns.Add("Column2", typeof(int));
        DataRow? row = dataTable.NewRow();
        row["Column1"] = "Value1";
        row["Column2"] = 123;
        dataTable.Rows.Add(row);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        DataTableJsonHelper.WriteDataTable(writer, dataTable);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());
        var expectedJson = "[{\"Column1\":\"Value1\",\"Column2\":123}]";

        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Serialize_DataTable_WritesJsonArray()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Columns.Add("Age", typeof(int));

        dataTable.Rows.Add("John Doe", 30);
        dataTable.Rows.Add("Linger", 25);

        var json = JsonSerializer.Serialize(dataTable, _options);

        Assert.StartsWith("[", json);
        Assert.EndsWith("]", json);
        Assert.Contains("\"Name\":\"John Doe\"", json);
        Assert.Contains("\"Age\":30", json);
        Assert.Contains("\"Name\":\"Linger\"", json);
        Assert.Contains("\"Age\":25", json);
    }

    [Fact]
    public void Serialize_EmptyDataTable_WritesEmptyJsonArray()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Name", typeof(string));
        dataTable.Columns.Add("Age", typeof(int));

        var json = JsonSerializer.Serialize(dataTable, _options);

        Assert.Equal("[]", json);
    }

    [Fact]
    public void Serialize_WithVariousDataTypes_HandlesTypesCorrectly()
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("StringValue", typeof(string));
        dataTable.Columns.Add("IntValue", typeof(int));
        dataTable.Columns.Add("BoolValue", typeof(bool));
        dataTable.Columns.Add("DoubleValue", typeof(double));
        dataTable.Columns.Add("DateTimeValue", typeof(DateTime));
        dataTable.Columns.Add("GuidValue", typeof(Guid));

        var guid = Guid.NewGuid();
        var dateTime = new DateTime(2025, 4, 11, 14, 30, 0);

        dataTable.Rows.Add("John Doe", 30, true, 12.34, dateTime, guid);

        var json = JsonSerializer.Serialize(dataTable, _options);

        Assert.Contains("\"StringValue\":\"John Doe\"", json);
        Assert.Contains("\"IntValue\":30", json);
        Assert.Contains("\"BoolValue\":true", json);
        Assert.Contains("\"DoubleValue\":12.34", json);
        Assert.Contains("\"DateTimeValue\":\"" + dateTime.ToString("o").Substring(0, 19), json);
        Assert.Contains("\"GuidValue\":\"" + guid.ToString(), json);
    }

    [Fact]
    public void Deserialize_JsonArray_CreatesDataTable()
    {
        var json = "[{\"Name\":\"John Doe\",\"Age\":30},{\"Name\":\"Linger\",\"Age\":25}]";

        var dataTable = JsonSerializer.Deserialize<DataTable>(json, _options);

        Assert.NotNull(dataTable);
        Assert.Equal(2, dataTable.Columns.Count);
        Assert.Equal(2, dataTable.Rows.Count);

        Assert.Equal("Name", dataTable.Columns[0].ColumnName);
        Assert.Equal("Age", dataTable.Columns[1].ColumnName);

        Assert.Equal("John Doe", dataTable.Rows[0]["Name"]);
        Assert.Equal(30L, dataTable.Rows[0]["Age"]);
        Assert.Equal("Linger", dataTable.Rows[1]["Name"]);
        Assert.Equal(25L, dataTable.Rows[1]["Age"]);
    }

    [Fact]
    public void Deserialize_EmptyJsonArray_CreatesEmptyDataTable()
    {
        var json = "[]";

        var dataTable = JsonSerializer.Deserialize<DataTable>(json, _options);

        Assert.NotNull(dataTable);
        Assert.Equal(0, dataTable.Columns.Count);
        Assert.Equal(0, dataTable.Rows.Count);
    }
}