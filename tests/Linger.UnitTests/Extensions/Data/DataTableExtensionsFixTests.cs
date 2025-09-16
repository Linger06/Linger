using System;
using System.Data;
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
}
