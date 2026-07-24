using Linger.Extensions.Collection;

namespace Linger.UnitTests.Extensions.Collection;

public class DataTableEnumerableExtensionsAdditionalTests
{
    [Fact]
    public void ToDataTable_MapsPublicProperties()
    {
        var source = new List<RowModel>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 2, Name = "B" }
        };

        DataTable table = source.ToDataTable();

        Assert.Equal(2, table.Rows.Count);
        Assert.Equal(1, table.Rows[0][nameof(RowModel.Id)]);
        Assert.Equal("A", table.Rows[0][nameof(RowModel.Name)]);
    }

    [Fact]
    public void ToDataTable_AppliesColumnAndRowCallbacks()
    {
        var source = new List<RowModel>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 2, Name = "B" }
        };

        DataTable table = source.ToDataTable(
            (column, columnInfo) => column.ColumnName = columnInfo.PropertyName.ToUpperInvariant(),
            (row, columnInfo, item) => row[columnInfo.PropertyName.ToUpperInvariant()] = columnInfo.Property.GetValue(item));

        Assert.Equal(2, table.Rows.Count);
        Assert.Equal(1, table.Rows[0][nameof(RowModel.Id).ToUpperInvariant()]);
        Assert.Equal("A", table.Rows[0][nameof(RowModel.Name).ToUpperInvariant()]);
    }

    [Fact]
    public void ToDataTable_ExcludesIndexerProperties()
    {
        var source = new List<RowModel> { new() { Id = 1, Name = "A" } };

        DataTable table = source.ToDataTable();

        Assert.Equal(2, table.Columns.Count);
        Assert.False(table.Columns.Contains("Item"));
    }

    private sealed class RowModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string this[int index] => index.ToString();
    }
}
