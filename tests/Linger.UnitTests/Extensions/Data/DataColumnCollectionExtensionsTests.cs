namespace Linger.UnitTests.Extensions.Data;

using System.Data;
using Linger.Extensions.Data;
using Xunit;

public class DataColumnCollectionExtensionsTests
{
    [Fact]
    public void ForEach_PerformsActionOnEachElement()
    {
        var table = new DataTable();
        table.Columns.Add("Column1");
        table.Columns.Add("Column2");
        DataColumnCollection? dataColumnCollection = table.Columns;

        var columnNames = new List<string>();
        dataColumnCollection.ForEach(column => columnNames.Add(column.ColumnName));

        Assert.Contains("Column1", columnNames);
        Assert.Contains("Column2", columnNames);
    }

    [Fact]
    public void ForEach_PerformsActionWithIndexOnEachElement()
    {
        var table = new DataTable();
        table.Columns.Add("Column1");
        table.Columns.Add("Column2");
        DataColumnCollection? dataColumnCollection = table.Columns;

        var columnNamesWithIndex = new List<string>();
        dataColumnCollection.ForEach((column, index) => columnNamesWithIndex.Add($"{index}:{column.ColumnName}"));

        Assert.Contains("0:Column1", columnNamesWithIndex);
        Assert.Contains("1:Column2", columnNamesWithIndex);
    }

    [Fact]
    public void ForEach_DoesNotThrowOnEmptyCollection()
    {
        var table = new DataTable();
        DataColumnCollection? dataColumnCollection = table.Columns;

        Exception? exception = Record.Exception(() => dataColumnCollection.ForEach(_ => { }));
        Assert.Null(exception);
    }

    [Fact]
    public void ForEach_WithIndexDoesNotThrowOnEmptyCollection()
    {
        var table = new DataTable();
        DataColumnCollection? dataColumnCollection = table.Columns;

        Exception? exception = Record.Exception(() => dataColumnCollection.ForEach((_, _) => { }));
        Assert.Null(exception);
    }
}