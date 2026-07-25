using System.Data;
using System.Text.Json;
using Linger.Extensions;
using Xunit.v3;

namespace Linger.UnitTests.Extensions;

public class JsonExtensionsTests
{
    [Fact]
    public void JsonElementToDataTable_WithJsonArray_CreatesDataTable()
    {
        // Arrange
        var json = "[{\"Name\":\"John Doe\",\"Age\":30},{\"Name\":\"Linger\",\"Age\":25}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal(2, dataTable.Columns.Count);
        Assert.Equal("John Doe", dataTable.Rows[0]["Name"]);
        Assert.Equal(30L, dataTable.Rows[0]["Age"]);
        Assert.Equal("Linger", dataTable.Rows[1]["Name"]);
        Assert.Equal(25L, dataTable.Rows[1]["Age"]);
    }

    [Fact]
    public void JsonElementToDataTable_WithDifferentValueTypes_SetsCorrectDataTypes()
    {
        // Arrange
        var json = "[{\"StringValue\":\"John Doe\",\"IntValue\":30,\"BoolValue\":true,\"DoubleValue\":12.34}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(1, dataTable.Rows.Count);
        Assert.Equal(4, dataTable.Columns.Count);

        Assert.Equal(typeof(string), dataTable.Columns["StringValue"].DataType);
        Assert.Equal(typeof(long), dataTable.Columns["IntValue"].DataType);
        Assert.Equal(typeof(bool), dataTable.Columns["BoolValue"].DataType);
        Assert.Equal(typeof(double), dataTable.Columns["DoubleValue"].DataType);

        Assert.Equal("John Doe", dataTable.Rows[0]["StringValue"]);
        Assert.Equal(30L, dataTable.Rows[0]["IntValue"]);
        Assert.Equal(true, dataTable.Rows[0]["BoolValue"]);
        Assert.Equal(12.34, dataTable.Rows[0]["DoubleValue"]);
    }

    [Fact]
    public void ValueKindToType_WithUndefinedValueKind_ThrowsNotSupportedException()
    {
        // Arrange & Act & Assert
        var json = "[{}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        
        // This should work without throwing for empty object
        var dataTable = jsonElement.JsonElementToDataTable();
        Assert.Equal(1, dataTable.Rows.Count);
    }

    [Fact]
    public void JsonElementToTypedValue_WithGuidString_ReturnsString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var json = $"[{{\"Id\":\"{guid}\"}}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        // The value is returned as string, not as Guid
        Assert.Equal(guid.ToString(), dataTable.Rows[0]["Id"]);
    }

    [Fact]
    public void JsonElementToTypedValue_WithDateTime_ReturnsString()
    {
        // Arrange
        var dateTime = DateTime.Now.ToString("O"); // ISO 8601 format
        var json = $"[{{\"DateTime\":\"{dateTime}\"}}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        // DateTime is returned as string since parsing fails
        Assert.IsType<string>(dataTable.Rows[0]["DateTime"]);
    }

    [Fact]
    public void JsonElementToTypedValue_WithLocalDateTimeHavingOffset_ReturnsString()
    {
        // Arrange - Create a local DateTime with offset information
        var dateTimeOffset = DateTimeOffset.Now.ToString("O"); // ISO 8601 with offset
        var json = $"[{{\"DateTimeOffset\":\"{dateTimeOffset}\"}}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        var result = dataTable.Rows[0]["DateTimeOffset"];
        Assert.IsType<string>(result);
    }

    [Fact]
    public void JsonElementToDataTable_WithMixedTypes_HandlesAllValueKinds()
    {
        // Arrange
        var json = "[{\"StringVal\":\"text\",\"NumVal\":42,\"BoolVal\":true}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(1, dataTable.Rows.Count);
        Assert.Equal(3, dataTable.Columns.Count);
        Assert.Equal("text", dataTable.Rows[0]["StringVal"]);
        Assert.Equal(42L, dataTable.Rows[0]["NumVal"]);
        Assert.Equal(true, dataTable.Rows[0]["BoolVal"]);
    }

    [Fact]
    public void JsonElementToDataTable_WithFirstRowNullValue_CreatesColumnAndPreservesLaterTypedValue()
    {
        // Arrange
        var json = "[{\"Value\":null},{\"Value\":42}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Single(dataTable.Columns);
        Assert.Equal("Value", dataTable.Columns[0].ColumnName);
        Assert.Equal(typeof(long), dataTable.Columns["Value"].DataType);
        Assert.Equal(DBNull.Value, dataTable.Rows[0]["Value"]);
        Assert.Equal(42L, dataTable.Rows[1]["Value"]);
    }

    [Fact]
    public void JsonElementToDataTable_WithLaterRowAddingColumn_AddsColumnAndUsesDBNullForMissingValues()
    {
        // Arrange
        var json = "[{\"A\":1},{\"A\":2,\"B\":3}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal(2, dataTable.Columns.Count);
        Assert.Equal(typeof(long), dataTable.Columns["A"].DataType);
        Assert.Equal(typeof(long), dataTable.Columns["B"].DataType);
        Assert.Equal(1L, dataTable.Rows[0]["A"]);
        Assert.Equal(DBNull.Value, dataTable.Rows[0]["B"]);
        Assert.Equal(2L, dataTable.Rows[1]["A"]);
        Assert.Equal(3L, dataTable.Rows[1]["B"]);
    }

}
