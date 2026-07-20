using System.Data;
using Linger.Excel.ClosedXML;
using Linger.Excel.Contracts;
using Linger.Excel.EPPlus;
using Linger.Excel.Npoi;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Linger.Excel.Tests;

public class ProviderConsistencyTests : ExcelServiceTestBase, IDisposable
{
    private readonly ILoggerFactory _loggerFactory;

    public ProviderConsistencyTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());
        CleanupTestDir();
    }

    public void Dispose()
    {
        CleanupTestDir();
        _loggerFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override IExcelService GetExcelService()
    {
        return new NpoiExcel(Options, _loggerFactory.CreateLogger<NpoiExcel>());
    }

    [Fact]
    public void ExcelToDataTable_WithTitleAndHeaderRowIndexZero_UsesUniformSchemaAcrossProviders()
    {
        var sourceData = new DataTable("ConsistencyData");
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Columns.Add("Name", typeof(string));
        sourceData.Rows.Add(1, "Alice");
        sourceData.Rows.Add(2, "Bob");

        var providers = CreateProviders();
        var importedTables = new List<DataTable>();

        foreach (var provider in providers)
        {
            var filePath = Path.Combine(TestFilesDir, $"{provider.Name}_TitleHeaderZero.xlsx");
            provider.Service.DataTableToExcel(sourceData, filePath, "Sheet1", "导出标题");

            var imported = provider.Service.ExcelToDataTable(filePath, "Sheet1", headerRowIndex: 0);

            Assert.NotNull(imported);
            importedTables.Add(imported);
        }

        var baseline = importedTables[0];
        for (var providerIndex = 1; providerIndex < importedTables.Count; providerIndex++)
        {
            var table = importedTables[providerIndex];

            Assert.Equal(baseline.Columns.Count, table.Columns.Count);
            Assert.Equal(baseline.Rows.Count, table.Rows.Count);

            for (var columnIndex = 0; columnIndex < baseline.Columns.Count; columnIndex++)
            {
                Assert.Equal(baseline.Columns[columnIndex].ColumnName, table.Columns[columnIndex].ColumnName);
            }

            for (var rowIndex = 0; rowIndex < baseline.Rows.Count; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < baseline.Columns.Count; columnIndex++)
                {
                    Assert.Equal(
                        baseline.Rows[rowIndex][columnIndex]?.ToString(),
                        table.Rows[rowIndex][columnIndex]?.ToString());
                }
            }
        }

        Assert.Equal(2, baseline.Columns.Count);
        Assert.Equal("导出标题", baseline.Columns[0].ColumnName);
        Assert.Equal("Column2", baseline.Columns[1].ColumnName);
        Assert.Equal(3, baseline.Rows.Count);
    }

    [Fact]
    public void StreamConversions_WithInvalidExcelContent_Throw()
    {
        foreach (var provider in CreateProviders())
        {
            Assert.ThrowsAny<Exception>(() => provider.Service.StreamToDataTable(CreateInvalidExcelStream()));
            Assert.ThrowsAny<Exception>(() => provider.Service.StreamToDataSet(CreateInvalidExcelStream()));
            Assert.ThrowsAny<Exception>(() => provider.Service.StreamToDataSet(CreateInvalidExcelStream(), _ => 0));
        }
    }

    [Fact]
    public void DataTableToExcel_WithInvalidStyleColor_ThrowsArgumentException()
    {
        var options = new ExcelOptions
        {
            StyleOptions = new ExcelStyleOptions
            {
                TitleStyle = new TitleStyle
                {
                    BackgroundColor = "not-a-hex-color"
                }
            }
        };
        var dataTable = new DataTable();
        dataTable.Columns.Add("Value", typeof(string));
        dataTable.Rows.Add("Value");
        var providers = new List<(string Name, IExcelService Service)>
        {
            ("Npoi", new NpoiExcel(options, _loggerFactory.CreateLogger<NpoiExcel>())),
            ("EPPlus", new EPPlusExcel(options, _loggerFactory.CreateLogger<EPPlusExcel>())),
            ("ClosedXml", new ClosedXmlExcel(options, _loggerFactory.CreateLogger<ClosedXmlExcel>()))
        };

        foreach (var provider in providers)
        {
            var filePath = Path.Combine(TestFilesDir, $"{provider.Name}_InvalidStyle.xlsx");

            Assert.Throws<ArgumentException>(() => provider.Service.DataTableToExcel(dataTable, filePath, title: "Title"));
        }
    }

    [Fact]
    public void DataTableToMemoryStream_WithNonPositiveBatchSize_ThrowsArgumentOutOfRangeException()
    {
        var options = new ExcelOptions
        {
            UseBatchWrite = true,
            BatchSize = 0,
            ParallelProcessingThreshold = 0
        };
        var dataTable = new DataTable();
        dataTable.Columns.Add("Value", typeof(string));
        dataTable.Rows.Add("Value");

        foreach (var provider in CreateProviders(options))
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => provider.Service.DataTableToMemoryStream(dataTable));
        }
    }

    [Fact]
    public void ExcelToDataSet_WithDuplicateRequestedSheetNames_ImportsEachWorksheetOnce()
    {
        var dataSet = new DataSet();
        var firstTable = new DataTable("First");
        firstTable.Columns.Add("Value", typeof(string));
        firstTable.Rows.Add("First value");
        dataSet.Tables.Add(firstTable);
        var secondTable = new DataTable("Second");
        secondTable.Columns.Add("Value", typeof(string));
        secondTable.Rows.Add("Second value");
        dataSet.Tables.Add(secondTable);

        foreach (var provider in CreateProviders())
        {
            var filePath = Path.Combine(TestFilesDir, $"{provider.Name}_DuplicateSheetSelection.xlsx");
            provider.Service.DataSetToExcel(dataSet, filePath);

            var imported = provider.Service.ExcelToDataSet(filePath, ["First", "first", "Second", "Second"]);

            Assert.NotNull(imported);
            Assert.Equal(2, imported.Tables.Count);
            Assert.Equal("First", imported.Tables[0].TableName);
            Assert.Equal("Second", imported.Tables[1].TableName);
        }
    }

    [Fact]
    public void StreamToDataTable_WithNonSeekableStream_ReadsAcrossProviders()
    {
        var sourceData = new DataTable();
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Rows.Add(1);

        foreach (var provider in CreateProviders())
        {
            using var excelStream = provider.Service.DataTableToMemoryStream(sourceData);
            using var nonSeekableStream = new NonSeekableReadStream(excelStream);

            var imported = provider.Service.StreamToDataTable(nonSeekableStream);

            Assert.NotNull(imported);
            Assert.Single(imported.Rows);
        }
    }

    [Fact]
    public async Task StreamToDataTableAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var sourceData = new DataTable();
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Rows.Add(1);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                provider.Service.StreamToDataTableAsync(stream, cancellationToken: cancellationTokenSource.Token));
        }
    }

    [Fact]
    public void StreamToDataTable_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var sourceData = new DataTable();
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Rows.Add(1);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);

            Assert.ThrowsAny<OperationCanceledException>(() =>
                provider.Service.StreamToDataTable(stream, cancellationToken: cancellationTokenSource.Token));
        }
    }

    [Fact]
    public async Task StreamToListAsync_WithCancellationDuringNonSeekableRead_ObservesCancellationAcrossProviders()
    {
        var sourceData = new DataTable();
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Rows.Add(1);

        foreach (var provider in CreateProviders())
        {
            using var excelStream = provider.Service.DataTableToMemoryStream(sourceData);
            using var cancellationTokenSource = new CancellationTokenSource();
            using var stream = new CancellationOnFirstReadStream(excelStream, cancellationTokenSource);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                provider.Service.StreamToListAsync(stream, row => row.Field<int>("Id"), cancellationToken: cancellationTokenSource.Token));
        }
    }

    private static MemoryStream CreateInvalidExcelStream()
    {
        return new MemoryStream([0x00, 0x01, 0x02, 0x03]);
    }

    private class NonSeekableReadStream(Stream stream) : Stream
    {
        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class CancellationOnFirstReadStream(Stream stream, CancellationTokenSource cancellationTokenSource) : NonSeekableReadStream(stream)
    {
        private bool _hasRead;

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = base.Read(buffer, offset, count);
            if (!_hasRead && bytesRead > 0)
            {
                _hasRead = true;
                cancellationTokenSource.Cancel();
            }

            return bytesRead;
        }
    }

    private List<(string Name, IExcelService Service)> CreateProviders(ExcelOptions? options = null)
    {
        return
        [
            ("Npoi", new NpoiExcel(options ?? Options, _loggerFactory.CreateLogger<NpoiExcel>())),
            ("EPPlus", new EPPlusExcel(options ?? Options, _loggerFactory.CreateLogger<EPPlusExcel>())),
            ("ClosedXml", new ClosedXmlExcel(options ?? Options, _loggerFactory.CreateLogger<ClosedXmlExcel>()))
        ];
    }
}
