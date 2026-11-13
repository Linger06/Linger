namespace Linger.UnitTests.Extensions.Data;

using System.Data;
using Linger.Extensions.Data;
using Xunit;

public class DataRowCollectionExtensionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForEach_WithNullCollection_ThrowsArgumentNullException(bool indexed)
    {
        DataRowCollection collection = null!;
        Action operation = indexed ? () => collection.ForEach((_, _) => { }) : () => collection.ForEach(_ => { });

        Assert.Throws<ArgumentNullException>("dataRowCollection", operation);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ForEach_WithNullAction_ThrowsArgumentNullException(bool indexed, bool populated)
    {
        using var table = new DataTable();
        if (populated)
        {
            table.Rows.Add(table.NewRow());
        }
        Action operation = indexed
            ? () => table.Rows.ForEach((Action<DataRow, int>)null!)
            : () => table.Rows.ForEach((Action<DataRow>)null!);

        Assert.Throws<ArgumentNullException>("action", operation);
    }

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
