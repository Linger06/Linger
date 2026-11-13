using System;
using System.Data;
using System.Globalization;
using Linger.Extensions.Data;
using Xunit;

namespace Linger.UnitTests.Extensions.Data;

/// <summary>
/// Tests for the fixes made to DataTableExtensions, specifically for the TableRowTurnToColumn method.
/// This test focuses on verifying that the fix for accessing DataColumn properties correctly works.
/// </summary>
public class DataTableExtensionsFixTests
{
    [Fact]
    public void TableRowTurnToColumn_WithEmptyCaption_PreservesAllValues()
    {
        using var table = CreatePivotTable();
        table.Rows.Add("East", "Jan", 5m);
        table.Rows.Add("East", "", 7m);
        table.Rows.Add("East", DBNull.Value, 11m);

        using var result = table.TableRowTurnToColumn(
            new[] { table.Columns[0] }, new[] { table.Columns[1] }, table.Columns[2]);

        Assert.Equal(new object[] { "East", 5m, 7m, 11m }, result.Rows[0].ItemArray);
        Assert.True(result.Columns.Contains("EMPTY"));
    }

    [Fact]
    public void TableRowTurnToColumn_WithEmptyCaptionPlaceholderCollision_ThrowsDuplicateNameException()
    {
        using var table = CreatePivotTable();
        table.Rows.Add("East", "", 7m);
        table.Rows.Add("East", "EMPTY", 11m);

        Assert.Throws<DuplicateNameException>(() => table.TableRowTurnToColumn(
            new[] { table.Columns[0] }, new[] { table.Columns[1] }, table.Columns[2]));
    }

    [Theory]
    [InlineData("de-DE", false)]
    [InlineData("en-US", false)]
    [InlineData("de-DE", true)]
    [InlineData("en-US", true)]
    public void TableRowTurnToColumn_WithNonStringCaption_UsesInvariantCulture(string cultureName, bool useDate)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            using var table = CreatePivotTable(useDate ? typeof(DateTime) : typeof(decimal));
            object caption = useDate ? new DateTime(2026, 11, 7) : 1.5m;
            table.Rows.Add("East", caption, 7m);

            using var result = table.TableRowTurnToColumn(
                new[] { table.Columns[0] }, new[] { table.Columns[1] }, table.Columns[2]);

            Assert.Equal(useDate ? "11_07_2026_00_00_00" : "1.5", result.Columns[1].ColumnName);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void TableRowTurnToColumn_WithDeletedRow_UsesOnlyCurrentRows()
    {
        using var table = CreatePivotTable();
        table.Rows.Add("East", "Jan", 5m);
        table.Rows.Add("East", "Feb", 7m);
        table.AcceptChanges();
        table.Rows[0].Delete();

        using var result = table.TableRowTurnToColumn(
            new[] { table.Columns[0] }, new[] { table.Columns[1] }, table.Columns[2]);

        Assert.Equal(new[] { "Region", "Feb" }, result.Columns.Cast<DataColumn>().Select(column => column.ColumnName));
        Assert.Equal(new object[] { "East", 7m }, result.Rows[0].ItemArray);
    }

    [Fact]
    public void ClearEmptyRow_WithDeletedRow_PreservesDeletionState()
    {
        using var table = new DataTable();
        table.Columns.Add("Value");
        table.Rows.Add("deleted");
        table.Rows.Add("");
        table.Rows.Add("keep");
        table.AcceptChanges();
        table.Rows[0].Delete();

        table.ClearEmptyRow();

        Assert.Equal(new[] { DataRowState.Deleted, DataRowState.Unchanged },
            table.Rows.Cast<DataRow>().Select(row => row.RowState));
        Assert.Equal("keep", table.Rows[1][0]);
    }

    [Fact]
    public void Sum_WithDeletedRow_SumsOnlyCurrentRows()
    {
        using var table = CreateTableWithDeletedRow();

        Assert.Equal(5d, table.Sum("Value"));
    }

    [Fact]
    public void Sum_WithMissingColumnOnEmptyTable_ThrowsArgumentException()
    {
        using var table = new DataTable();
        table.Columns.Add("Value", typeof(int));

        Assert.Throws<ArgumentException>("columnName", () => table.Sum("Missing"));
    }

    [Fact]
    public void Combine_WithDeletedRows_CombinesOnlyCurrentRows()
    {
        using var first = CreateTableWithDeletedRow();
        using var second = CreateTableWithDeletedRow();

        using var result = first.Combine(second);

        Assert.Equal(new[] { 2, 3, 2, 3 }, result.Rows.Cast<DataRow>().Select(row => (int)row[0]));
    }

    [Fact]
    public void Join_WithDeletedRows_JoinsOnlyCurrentRows()
    {
        using var left = CreateTableWithDeletedRow();
        using var right = CreateTableWithDeletedRow();

        using var result = left.Join(right, new[] { left.Columns[0] }, new[] { right.Columns[0] }, true, true);

        Assert.Equal(new[] { 2, 3 }, result.Rows.Cast<DataRow>().Select(row => (int)row[0]));
    }

    [Theory]
    [InlineData(1, new[] { 2, 3 })]
    [InlineData(2, new int[0])]
    public void Paging_WithDeletedRow_CountsOnlyCurrentRows(int pageIndex, int[] expected)
    {
        using var table = CreateTableWithDeletedRow();

        using var result = table.Paging(pageIndex, 2);

        Assert.NotNull(result);
        Assert.Equal(expected, result.Rows.Cast<DataRow>().Select(row => (int)row[0]));
    }

    [Fact]
    public void ToList_WithDeletedRow_MapsOnlyCurrentRows()
    {
        using var table = CreateTableWithDeletedRow();

        var result = table.ToList(row => (int)row[0]);

        Assert.Equal(new[] { 2, 3 }, result);
    }

    [Fact]
    public void ToList_WithSettersAndDeletedRow_MapsOnlyCurrentRows()
    {
        using var table = CreateTableWithDeletedRow();

        var result = table.ToList(() => new List<int>(), new Dictionary<string, Action<List<int>, object?>>
        {
            ["Value"] = (values, value) => values.Add(Convert.ToInt32(value, CultureInfo.InvariantCulture))
        });

        Assert.Equal(new[] { 2, 3 }, result!.SelectMany(values => values));
    }

    [Theory]
    [InlineData("Value, Name")]
    [InlineData(" Value , Name ")]
    public void ContainAllColumns_WithWhitespaceAroundNames_ReturnsTrue(string columns)
    {
        using var table = new DataTable();
        table.Columns.Add("Value");
        table.Columns.Add("Name");

        Assert.True(table.ContainAllColumns(columns));
    }

    [Fact]
    public void ContainAllColumns_WithNullColumnString_ThrowsArgumentNullException()
    {
        using var table = new DataTable();

        Assert.Throws<ArgumentNullException>("columnString", () => table.ContainAllColumns(null!));
    }

    private static DataTable CreatePivotTable(Type? captionType = null)
    {
        var table = new DataTable();
        table.Columns.Add("Region", typeof(string));
        table.Columns.Add("Month", captionType ?? typeof(string));
        table.Columns.Add("Amount", typeof(decimal));

        return table;
    }

    private static DataTable CreateTableWithDeletedRow()
    {
        var table = new DataTable();
        table.Columns.Add("Value", typeof(int));
        table.Rows.Add(1);
        table.Rows.Add(2);
        table.Rows.Add(3);
        table.AcceptChanges();
        table.Rows[0].Delete();

        return table;
    }

    [Fact]
    public void TableRowTurnToColumn_WithValidColumns_ShouldAccessColumnsByName()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));

        // Add test data
        sourceTable.Rows.Add(1, "Electronics", 100m);
        sourceTable.Rows.Add(1, "Books", 50m);
        sourceTable.Rows.Add(2, "Electronics", 150m);
        sourceTable.Rows.Add(2, "Clothing", 75m);

        // Define columns for the transformation
        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        // Act
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(4, result.Columns.Count); // GroupId + 3 categories

        // Verify column names
        var columnNames = result.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        Assert.Contains("GroupId", columnNames);
        Assert.Contains("Electronics", columnNames);
        Assert.Contains("Books", columnNames);
        Assert.Contains("Clothing", columnNames);

        // Verify data correctness
        Assert.Equal(1, Convert.ToInt32(result.Rows[0]["GroupId"]));
        Assert.Equal(100m, Convert.ToDecimal(result.Rows[0]["Electronics"]));
        Assert.Equal(50m, Convert.ToDecimal(result.Rows[0]["Books"]));
        
        Assert.Equal(2, Convert.ToInt32(result.Rows[1]["GroupId"]));
        Assert.Equal(150m, Convert.ToDecimal(result.Rows[1]["Electronics"]));
        Assert.Equal(75m, Convert.ToDecimal(result.Rows[1]["Clothing"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithMultipleGroupColumns_ShouldWorkCorrectly()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("Year", typeof(int));
        sourceTable.Columns.Add("Quarter", typeof(string));
        sourceTable.Columns.Add("Product", typeof(string));
        sourceTable.Columns.Add("Sales", typeof(decimal));

        // Add test data
        sourceTable.Rows.Add(2023, "Q1", "ProductA", 1000m);
        sourceTable.Rows.Add(2023, "Q1", "ProductB", 800m);
        sourceTable.Rows.Add(2023, "Q2", "ProductA", 1200m);
        sourceTable.Rows.Add(2024, "Q1", "ProductA", 1100m);

        // Define columns for the transformation (multiple group columns)
        var groupColumns = new[] { sourceTable.Columns["Year"], sourceTable.Columns["Quarter"] };
        var captionColumns = new[] { sourceTable.Columns["Product"] };
        var valueColumn = sourceTable.Columns["Sales"];

        // Act
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Rows.Count); // (2023,Q1), (2023,Q2), (2024,Q1)
        Assert.Equal(4, result.Columns.Count); // Year, Quarter, ProductA, ProductB

        // Verify column names
        var columnNames = result.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        Assert.Contains("Year", columnNames);
        Assert.Contains("Quarter", columnNames);
        Assert.Contains("ProductA", columnNames);
        Assert.Contains("ProductB", columnNames);

        // Verify first row (2023, Q1)
        var firstRow = result.Rows[0];
        Assert.Equal(2023, Convert.ToInt32(firstRow["Year"]));
        Assert.Equal("Q1", firstRow["Quarter"].ToString());
        Assert.Equal(1000m, Convert.ToDecimal(firstRow["ProductA"]));
        Assert.Equal(800m, Convert.ToDecimal(firstRow["ProductB"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithColumnReferenceNotInTable_ShouldThrowException()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));

        sourceTable.Rows.Add(1, "Electronics", 100m);

        // Create a DataColumn that doesn't belong to this table
        var otherTable = new DataTable();
        otherTable.Columns.Add("OtherColumn", typeof(string));
        var invalidColumn = otherTable.Columns["OtherColumn"];

        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { invalidColumn }; // This column is not in sourceTable
        var valueColumn = sourceTable.Columns["Amount"];

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => 
            sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn));
    }

    [Fact]
    public void TableRowTurnToColumn_WithNullArgumentValidation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));

        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        // Act & Assert
        Assert.ThrowsAny<System.ArgumentNullException>(() => sourceTable.TableRowTurnToColumn(null!, captionColumns, valueColumn));

        Assert.ThrowsAny<System.ArgumentNullException>(() => sourceTable.TableRowTurnToColumn(groupColumns, null!, valueColumn));

        Assert.ThrowsAny<System.ArgumentNullException>(() => sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, null!));
    }

    [Fact]
    public void TableRowTurnToColumn_WithDuplicateCaptionValues_ShouldAccumulateValues()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));

        // Add duplicate categories within the same group
        sourceTable.Rows.Add(1, "Electronics", 100m);
        sourceTable.Rows.Add(1, "Electronics", 50m); // Duplicate category
        sourceTable.Rows.Add(1, "Books", 30m);

        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        // Act
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Rows);
        Assert.Equal(3, result.Columns.Count); // GroupId, Electronics, Books

        // Verify that Electronics values were accumulated (100 + 50 = 150)
        Assert.Equal(1, Convert.ToInt32(result.Rows[0]["GroupId"]));
        Assert.Equal(150m, Convert.ToDecimal(result.Rows[0]["Electronics"]));
        Assert.Equal(30m, Convert.ToDecimal(result.Rows[0]["Books"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithComplexDataTypes_ShouldPreserveDataTypes()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(Guid));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));

        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        sourceTable.Rows.Add(guid1, "Electronics", 100.50m);
        sourceTable.Rows.Add(guid1, "Books", 50.25m);
        sourceTable.Rows.Add(guid2, "Electronics", 150.75m);

        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        // Act
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        
        // Verify data types are preserved
        Assert.Equal(typeof(Guid), result.Columns["GroupId"].DataType);
        Assert.Equal(typeof(decimal), result.Columns["Electronics"].DataType);
        Assert.Equal(typeof(decimal), result.Columns["Books"].DataType);

        // Verify values
        Assert.Equal(guid1, (Guid)result.Rows[0]["GroupId"]);
        Assert.Equal(100.50m, Convert.ToDecimal(result.Rows[0]["Electronics"]));
        Assert.Equal(50.25m, Convert.ToDecimal(result.Rows[0]["Books"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithNumericStringValues_ShouldConvertToDecimal()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(string));

        sourceTable.Rows.Add(1, "Electronics", "100.50");
        sourceTable.Rows.Add(1, "Books", "50");

        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        // Act
        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        // Assert
        Assert.Single(result.Rows);
        Assert.Equal(100.50m, Convert.ToDecimal(result.Rows[0]["Electronics"]));
        Assert.Equal(50m, Convert.ToDecimal(result.Rows[0]["Books"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithInvalidNumericText_ShouldThrowFormatException()
    {
        // Arrange
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(string));

        sourceTable.Rows.Add(1, "Electronics", "invalid");

        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        // Act & Assert
        Assert.Throws<FormatException>(() =>
            sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn));
    }

    [Fact]
    public void TableRowTurnToColumn_WithDelimiterInCompositeGroupValues_KeepsGroupsSeparate()
    {
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("Group1", typeof(string));
        sourceTable.Columns.Add("Group2", typeof(string));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));
        sourceTable.Rows.Add("a||:||b", "c", "ProductA", 10m);
        sourceTable.Rows.Add("a", "b||:||c", "ProductA", 20m);

        var groupColumns = new[] { sourceTable.Columns["Group1"], sourceTable.Columns["Group2"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        var result = sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn);

        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(10m, Convert.ToDecimal(result.Rows[0]["ProductA"]));
        Assert.Equal(20m, Convert.ToDecimal(result.Rows[1]["ProductA"]));
    }

    [Fact]
    public void TableRowTurnToColumn_WithCaptionNameCollision_ShouldThrowDuplicateNameException()
    {
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));
        sourceTable.Rows.Add(1, "Product/A", 10m);
        sourceTable.Rows.Add(1, "Product\\A", 20m);

        var groupColumns = new[] { sourceTable.Columns["GroupId"] };
        var captionColumns = new[] { sourceTable.Columns["Category"] };
        var valueColumn = sourceTable.Columns["Amount"];

        Assert.Throws<DuplicateNameException>(() =>
            sourceTable.TableRowTurnToColumn(groupColumns, captionColumns, valueColumn));
    }

    [Fact]
    public void TableRowTurnToColumn_WithColumnMissingFromSource_ShouldThrowEnglishMessage()
    {
        var sourceTable = new DataTable();
        sourceTable.Columns.Add("GroupId", typeof(int));
        sourceTable.Columns.Add("Category", typeof(string));
        sourceTable.Columns.Add("Amount", typeof(decimal));
        var otherTable = new DataTable();
        otherTable.Columns.Add("Other", typeof(decimal));

        var exception = Assert.Throws<ArgumentException>(() => sourceTable.TableRowTurnToColumn(
            new[] { sourceTable.Columns["GroupId"]! },
            new[] { sourceTable.Columns["Category"]! },
            otherTable.Columns["Other"]!));

        Assert.Contains("does not exist in the source table", exception.Message);
        Assert.Contains("Other", exception.Message);
    }
}
