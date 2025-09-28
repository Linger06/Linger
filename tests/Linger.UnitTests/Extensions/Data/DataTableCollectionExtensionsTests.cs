namespace Linger.UnitTests.Extensions.Data;

using System.Data;
using Linger.Extensions.Data;
using Xunit;

public class DataTableCollectionExtensionsTests
{
    [Fact]
    public void ForEach_PerformsActionOnEachElement()
    {
        var dataSet = new DataSet();
        dataSet.Tables.Add(new DataTable("Table1"));
        dataSet.Tables.Add(new DataTable("Table2"));
        DataTableCollection? dataTableCollection = dataSet.Tables;

        var tableNames = new List<string>();
        dataTableCollection.ForEach(table => tableNames.Add(table.TableName));

        Assert.Contains("Table1", tableNames);
        Assert.Contains("Table2", tableNames);
    }

    [Fact]
    public void ForEach_PerformsActionWithIndexOnEachElement()
    {
        var dataSet = new DataSet();
        dataSet.Tables.Add(new DataTable("Table1"));
        dataSet.Tables.Add(new DataTable("Table2"));
        DataTableCollection? dataTableCollection = dataSet.Tables;

        var tableNamesWithIndex = new List<string>();
        dataTableCollection.ForEach((table, index) => tableNamesWithIndex.Add($"{index}:{table.TableName}"));

        Assert.Contains("0:Table1", tableNamesWithIndex);
        Assert.Contains("1:Table2", tableNamesWithIndex);
    }

    [Fact]
    public void ForEach_DoesNotThrowOnEmptyCollection()
    {
        var dataSet = new DataSet();
        DataTableCollection? dataTableCollection = dataSet.Tables;

        Exception? exception = Record.Exception(() => dataTableCollection.ForEach(_ => { }));
        Assert.Null(exception);
    }

    [Fact]
    public void ForEach_WithIndexDoesNotThrowOnEmptyCollection()
    {
        var dataSet = new DataSet();
        DataTableCollection? dataTableCollection = dataSet.Tables;

        Exception? exception = Record.Exception(() => dataTableCollection.ForEach((_, _) => { }));
        Assert.Null(exception);
    }
}