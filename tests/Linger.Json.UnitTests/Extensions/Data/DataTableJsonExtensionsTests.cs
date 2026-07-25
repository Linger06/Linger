namespace Linger.Json.UnitTests.Extensions.Data;

public class DataTableJsonExtensionsTests
{
    [Fact]
    public void ToJsonString_ReturnsJsonString()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "John");

        var result = table.ToJsonString();

        Assert.Equal("[{\"Id\":1,\"Name\":\"John\"}]", result);
    }

    [Fact]
    public void ToJsonString_SerializesDateTimeValues()
    {
        var table = new DataTable();
        table.Columns.Add("Date", typeof(DateTime));
        table.Rows.Add(new DateTime(2023, 4, 15));

        var result = table.ToJsonString();

        Assert.Equal("[{\"Date\":\"2023-04-15T00:00:00\"}]", result);
    }

    [Fact]
    public void ToJsonString_HandlesEmptyDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));

        Assert.Equal("[]", table.ToJsonString());
    }

    [Fact]
    public void ToJsonString_HandlesNullDataTable()
    {
        DataTable? table = null;

        Assert.Equal("null", table.ToJsonString());
    }
}
