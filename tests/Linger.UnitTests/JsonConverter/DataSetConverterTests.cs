using System.Data;
using System.Text.Json;
using Linger.JsonConverter;
using Xunit.v3;

namespace Linger.UnitTests.JsonConverter;

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

    [Fact]
    public void Serialize_DataSet_WritesJsonArray()
    {
        // Arrange
        var dataSet = new DataSet();

        // 第一个表：用户表
        var usersTable = new DataTable("Users");
        usersTable.Columns.Add("Id", typeof(int));
        usersTable.Columns.Add("Name", typeof(string));
        usersTable.Columns.Add("Age", typeof(int));
        usersTable.Rows.Add(1, "John Doe", 30);
        usersTable.Rows.Add(2, "Linger", 25);
        dataSet.Tables.Add(usersTable);

        // 第二个表：订单表
        var ordersTable = new DataTable("Orders");
        ordersTable.Columns.Add("OrderId", typeof(int));
        ordersTable.Columns.Add("UserId", typeof(int));
        ordersTable.Columns.Add("Amount", typeof(decimal));
        ordersTable.Rows.Add(101, 1, 199.99m);
        ordersTable.Rows.Add(102, 2, 299.99m);
        ordersTable.Rows.Add(103, 1, 99.99m);
        dataSet.Tables.Add(ordersTable);

        // Act
        var json = JsonSerializer.Serialize(dataSet, _options);

        // Assert
        Assert.StartsWith("[[", json); // 数组的数组
        Assert.EndsWith("]]", json);

        // 验证用户表数据
        Assert.Contains("\"Id\":1", json);
        Assert.Contains("\"Name\":\"John Doe\"", json);
        Assert.Contains("\"Age\":30", json);

        // 验证订单表数据
        Assert.Contains("\"OrderId\":101", json);
        Assert.Contains("\"UserId\":1", json);
        Assert.Contains("\"Amount\":199.99", json);
    }

    [Fact]
    public void Serialize_EmptyDataSet_WritesEmptyJsonArray()
    {
        // Arrange
        var dataSet = new DataSet();

        // Act
        var json = JsonSerializer.Serialize(dataSet, _options);

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void Serialize_DataSetWithEmptyTables_WritesArrayOfEmptyArrays()
    {
        // Arrange
        var dataSet = new DataSet();

        // 添加空表
        var emptyTable1 = new DataTable("EmptyTable1");
        emptyTable1.Columns.Add("Id", typeof(int));
        emptyTable1.Columns.Add("Name", typeof(string));
        dataSet.Tables.Add(emptyTable1);

        var emptyTable2 = new DataTable("EmptyTable2");
        emptyTable2.Columns.Add("OrderId", typeof(int));
        emptyTable2.Columns.Add("Amount", typeof(decimal));
        dataSet.Tables.Add(emptyTable2);

        // Act
        var json = JsonSerializer.Serialize(dataSet, _options);

        // Assert
        Assert.Equal("[[],[]]", json);
    }

    [Fact]
    public void Deserialize_JsonArray_CreatesDataSet()
    {
        // Arrange
        var json = "[[{\"Id\":1,\"Name\":\"John Doe\",\"Age\":30},{\"Id\":2,\"Name\":\"Linger\",\"Age\":25}]," +
                   "[{\"OrderId\":101,\"UserId\":1,\"Amount\":199.99},{\"OrderId\":102,\"UserId\":2,\"Amount\":299.99}]]";

        // Act
        var dataSet = JsonSerializer.Deserialize<DataSet>(json, _options);

        // Assert
        Assert.NotNull(dataSet);
        Assert.Equal(2, dataSet.Tables.Count);

        // 验证第一个表 (Users)
        var usersTable = dataSet.Tables[0];
        Assert.Equal(3, usersTable.Columns.Count);
        Assert.Equal(2, usersTable.Rows.Count);
        Assert.Equal(1L, usersTable.Rows[0]["Id"]);
        Assert.Equal("John Doe", usersTable.Rows[0]["Name"]);
        Assert.Equal(30L, usersTable.Rows[0]["Age"]);

        // 验证第二个表 (Orders)
        var ordersTable = dataSet.Tables[1];
        Assert.Equal(3, ordersTable.Columns.Count);
        Assert.Equal(2, ordersTable.Rows.Count);
        Assert.Equal(101L, ordersTable.Rows[0]["OrderId"]);
        Assert.Equal(1L, ordersTable.Rows[0]["UserId"]);
        // Use precision parameter to handle floating-point comparison
        Assert.Equal(199.99m, Convert.ToDecimal(ordersTable.Rows[0]["Amount"]), precision: 2);
    }

    [Fact]
    public void Deserialize_EmptyJsonArray_CreatesEmptyDataSet()
    {
        // Arrange
        var json = "[]";

        // Act
        var dataSet = JsonSerializer.Deserialize<DataSet>(json, _options);

        // Assert
        Assert.NotNull(dataSet);
        Assert.Equal(0, dataSet.Tables.Count);
    }

    [Fact]
    public void Deserialize_ArrayOfEmptyArrays_CreatesDataSetWithEmptyTables()
    {
        // Arrange
        var json = "[[],[]]";

        // Act
        var dataSet = JsonSerializer.Deserialize<DataSet>(json, _options);

        // Assert
        Assert.NotNull(dataSet);
        Assert.Equal(2, dataSet.Tables.Count);
        Assert.Equal(0, dataSet.Tables[0].Columns.Count);
        Assert.Equal(0, dataSet.Tables[0].Rows.Count);
        Assert.Equal(0, dataSet.Tables[1].Columns.Count);
        Assert.Equal(0, dataSet.Tables[1].Rows.Count);
    }
}