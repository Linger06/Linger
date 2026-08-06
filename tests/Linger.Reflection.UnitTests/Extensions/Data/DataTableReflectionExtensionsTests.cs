using System.Data;
using System.Globalization;
using Linger.Extensions.Data;

namespace Linger.UnitTests.Extensions.Data;

public class DataTableReflectionExtensionsTests
{
    [Fact]
    public void ToList_MapsPublicWritableProperties()
    {
        DataTable table = CreateTable();

        List<RowModel>? result = table.ToList<RowModel>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Null(result[0].OptionalCount);
        Assert.Equal("John", result[0].Name);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(2, result[1].OptionalCount);
    }

    [Fact]
    public void ToList_MapsSpecialTypes()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Enabled", typeof(bool));
        table.Columns.Add("Status", typeof(int));
        table.Columns.Add("CreatedAt", typeof(double));

        var createdAt = new DateTime(2023, 4, 15);
        table.Rows.Add(1, true, (int)RowStatus.Active, createdAt.ToOADate());

        List<SpecialTypesModel>? result = table.ToList<SpecialTypesModel>();

        Assert.NotNull(result);
        SpecialTypesModel item = Assert.Single(result);
        Assert.True(item.Enabled);
        Assert.Equal(RowStatus.Active, item.Status);
        Assert.Equal(createdAt.Date, item.CreatedAt.Date);
    }

    [Fact]
    public void ToList_WithZhCnDateTimeString_UsesCurrentCultureFallback()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("zh-CN");
            var table = new DataTable();
            table.Columns.Add("CreatedAt", typeof(string));
            table.Rows.Add("2024/1/15 下午 3:04:05");

            List<SpecialTypesModel>? result = table.ToList<SpecialTypesModel>();

            SpecialTypesModel item = Assert.Single(result!);
            Assert.Equal(new DateTime(2024, 1, 15, 15, 4, 5), item.CreatedAt);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ToList_WithEmptyDataTable_ReturnsEmptyList()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));

        List<RowModel>? result = table.ToList<RowModel>();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToList_WithNullDataTable_ReturnsNull()
    {
        DataTable? table = null;

        List<RowModel>? result = table.ToList<RowModel>();

        Assert.Null(result);
    }

    [Fact]
    public void ToList_WithNoMatchingColumns_ReturnsEmptyList()
    {
        var table = new DataTable();
        table.Columns.Add("Unknown", typeof(string));
        table.Rows.Add("value");

        List<RowModel>? result = table.ToList<RowModel>();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToList_WithInvalidValue_ThrowsInvalidCastException()
    {
        var table = new DataTable();
        table.Columns.Add("CreatedAt", typeof(string));
        table.Rows.Add("not a date");

        InvalidCastException exception = Assert.Throws<InvalidCastException>(() => table.ToList<SpecialTypesModel>());

        Assert.Contains("CreatedAt", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ToList_OverParallelThreshold_PreservesRowOrder()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));

        for (var i = 1; i <= 2_000; i++)
        {
            table.Rows.Add(i, $"Name{i}");
        }

        List<RowModel>? result = table.ToList<RowModel>(parallelProcessingThreshold: 1_000);

        Assert.NotNull(result);
        Assert.Equal(2_000, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Name1000", result[999].Name);
        Assert.Equal(2_000, result[1999].Id);
    }

    [Fact]
    public void ToList_WhenTargetHasNoWritableProperties_ReturnsEmptyList()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Rows.Add(1);

        List<ReadOnlyModel>? result = table.ToList<ReadOnlyModel>();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private static DataTable CreateTable()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("OptionalCount", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, DBNull.Value, "John");
        table.Rows.Add(2, 2, "Jane");

        return table;
    }

    private sealed class RowModel
    {
        public int Id { get; set; }
        public int? OptionalCount { get; set; }
        public string? Name { get; set; }
    }

    private sealed class SpecialTypesModel
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public RowStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class ReadOnlyModel
    {
        public int Id { get; }
    }

    private enum RowStatus
    {
        Active = 1
    }
}
