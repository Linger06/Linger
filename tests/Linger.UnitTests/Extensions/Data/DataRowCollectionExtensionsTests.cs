namespace Linger.UnitTests.Extensions.Data;

using System.Data;
using Linger.Extensions.Data;
using Xunit;

public class DataRowCollectionExtensionsTests
{
    [Fact]
    public void ForEach_PerformsActionOnEachElement()
    {
        var table = new DataTable();
        table.Rows.Add(table.NewRow());
        table.Rows.Add(table.NewRow());
        DataRowCollection? dataRowCollection = table.Rows;

        var rowCount = 0;
        dataRowCollection.ForEach(_ => rowCount++);

        Assert.Equal(2, rowCount);
    }

    [Fact]
    public void ForEach_PerformsActionWithIndexOnEachElement()
    {
        var table = new DataTable();
        table.Rows.Add(table.NewRow());
        table.Rows.Add(table.NewRow());
        DataRowCollection? dataRowCollection = table.Rows;

        var rowIndices = new List<int>();
        dataRowCollection.ForEach((_, index) => rowIndices.Add(index));

        Assert.Contains(0, rowIndices);
        Assert.Contains(1, rowIndices);
    }

    [Fact]
    public void ForEach_DoesNotThrowOnEmptyCollection()
    {
        var table = new DataTable();
        DataRowCollection? dataRowCollection = table.Rows;

        Exception? exception = Record.Exception(() => dataRowCollection.ForEach(_ => { }));
        Assert.Null(exception);
    }

    [Fact]
    public void ForEach_WithIndexDoesNotThrowOnEmptyCollection()
    {
        var table = new DataTable();
        DataRowCollection? dataRowCollection = table.Rows;

        Exception? exception = Record.Exception(() => dataRowCollection.ForEach((_, _) => { }));
        Assert.Null(exception);
    }
}