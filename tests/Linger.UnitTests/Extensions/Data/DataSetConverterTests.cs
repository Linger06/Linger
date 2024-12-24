namespace Linger.UnitTests.Extensions.Data;

public class DataSetConverterTests
{
    private readonly JsonSerializerOptions _options;

    public DataSetConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new DataSetConverter() }
        };
    }

    private DataSet CreateTestDataSet()
    {
        var dataSet = new DataSet();
        var table1 = new DataTable("Table1");
        table1.Columns.Add("Id", typeof(int));
        table1.Columns.Add("Name", typeof(string));
        table1.Rows.Add(1, "John");
        table1.Rows.Add(2, "Jane");

        var table2 = new DataTable("Table2");
        table2.Columns.Add("ProductId", typeof(int));
        table2.Columns.Add("ProductName", typeof(string));
        table2.Rows.Add(1, "Product1");
        table2.Rows.Add(2, "Product2");

        dataSet.Tables.Add(table1);
        dataSet.Tables.Add(table2);

        return dataSet;
    }

    [Fact]
    public void Read_ValidJson_ReturnsDataSet()
    {
        var json = "[[{\"Id\":1,\"Name\":\"John\"},{\"Id\":2,\"Name\":\"Jane\"}],[{\"ProductId\":1,\"ProductName\":\"Product1\"},{\"ProductId\":2,\"ProductName\":\"Product2\"}]]";
        DataSet? result = JsonSerializer.Deserialize<DataSet>(json, _options);

        Assert.NotNull(result);
        Assert.Equal(2, result.Tables.Count);

        DataTable? table1 = result.Tables["Table1"];
        Assert.NotNull(table1);
        Assert.Equal(2, table1.Rows.Count);
        Assert.Equal(1, table1.Rows[0]["Id"].ToInt());
        Assert.Equal("John", table1.Rows[0]["Name"]);
        Assert.Equal(2, table1.Rows[1]["Id"].ToInt());
        Assert.Equal("Jane", table1.Rows[1]["Name"]);

        DataTable? table2 = result.Tables["Table2"];
        Assert.NotNull(table2);
        Assert.Equal(2, table2.Rows.Count);
        Assert.Equal(1, table2.Rows[0]["ProductId"].ToInt());
        Assert.Equal("Product1", table2.Rows[0]["ProductName"]);
        Assert.Equal(2, table2.Rows[1]["ProductId"].ToInt());
        Assert.Equal("Product2", table2.Rows[1]["ProductName"]);
    }

    [Fact]
    public void Write_DataSet_WritesCorrectJson()
    {
        DataSet? dataSet = CreateTestDataSet();
        var json = JsonSerializer.Serialize(dataSet, _options);

        var expectedJson = "[[{\"Id\":1,\"Name\":\"John\"},{\"Id\":2,\"Name\":\"Jane\"}],[{\"ProductId\":1,\"ProductName\":\"Product1\"},{\"ProductId\":2,\"ProductName\":\"Product2\"}]]";
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Write_EmptyDataSet_WritesEmptyJsonArray()
    {
        var dataSet = new DataSet();
        var json = JsonSerializer.Serialize(dataSet, _options);

        Assert.Equal("[]", json);
    }

    [Fact]
    public void Read_EmptyJsonArray_ReturnsEmptyDataSet()
    {
        var json = "[]";
        DataSet? result = JsonSerializer.Deserialize<DataSet>(json, _options);

        Assert.NotNull(result);
        Assert.Empty(result.Tables);
    }

    [Fact]
    public void Read_InvalidJson_ThrowsJsonException()
    {
        var json = "[{\"Invalid\":\"Data\"}]";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DataSet>(json, _options));
    }
}