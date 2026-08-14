using System.Data;
using Linger.Excel.ClosedXML;
using Linger.Excel.Contracts;
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
    public void IExcelService_ExplicitColumnExport_IsPartOfServiceContract()
    {
        Assert.Contains(
            typeof(IExcelService).GetMethods(),
            method => method.Name == nameof(IExcelService.CollectionToExcel) &&
                      method.GetParameters().Any(parameter =>
                          parameter.ParameterType.IsGenericType &&
                          parameter.ParameterType.GetGenericArguments().Any(argument =>
                              argument.IsGenericType &&
                               argument.GetGenericTypeDefinition() == typeof(ExcelExportColumn<>))));
    }

    [Fact]
    public void ExcelContracts_ExposeFocusedApi()
    {
        Assert.Equal(18, typeof(IExcelService).GetMethods().Length);
        Assert.Equal(
            5,
            typeof(IExcel<>).GetMethods(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly).Length);
    }

    [Fact]
    public void IExcelService_StreamImports_UsePublicImplementations()
    {
        var interfaceMap = typeof(NpoiExcel).GetInterfaceMap(typeof(IExcelService));
        var methodNames = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(IExcelService.StreamToDataTable),
            nameof(IExcelService.StreamToList),
            nameof(IExcelService.StreamToDataSet)
        };
        var matchedMethods = 0;

        for (var index = 0; index < interfaceMap.InterfaceMethods.Length; index++)
        {
            if (methodNames.Contains(interfaceMap.InterfaceMethods[index].Name))
            {
                matchedMethods++;
                Assert.True(interfaceMap.TargetMethods[index].IsPublic);
            }
        }

        Assert.Equal(5, matchedMethods);
    }

    [Fact]
    public void CollectionToMemoryStream_WithExplicitColumn_ConvertsValueToDeclaredTypeAcrossProviders()
    {
        var columns = new[]
        {
            new ExcelExportColumn<int>("Value", value => value, typeof(string))
        };

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.CollectionToMemoryStream([42], columns);
            var imported = provider.Service.StreamToDataTable(stream);

            Assert.NotNull(imported);
            Assert.Equal("42", Assert.Single(imported.Rows.Cast<DataRow>())["Value"]);
        }
    }

    [Fact]
    public void DataTableToMemoryStream_WithUInt64Overflow_ThrowsAcrossProviders()
    {
        var sourceData = new DataTable("UnsignedData");
        sourceData.Columns.Add("Value", typeof(ulong));
        sourceData.Rows.Add(ulong.MaxValue);

        foreach (var provider in CreateProviders())
        {
            Assert.Throws<OverflowException>(() =>
                provider.Service.DataTableToMemoryStream(sourceData).Dispose());
        }
    }

    [Fact]
    public void DataTableToMemoryStream_WithWholeFloatingPoint_UsesDecimalFormatAcrossProviders()
    {
        var options = new ExcelOptions
        {
            AutoFitColumns = false,
            StyleOptions = new ExcelStyleOptions
            {
                DataStyle = new DataStyle
                {
                    DecimalFormat = "0.000",
                    IntegerFormat = "0"
                }
            }
        };
        var sourceData = new DataTable("FloatingPointData");
        sourceData.Columns.Add("Value", typeof(double));
        sourceData.Rows.Add(2d);

        foreach (var provider in CreateProviders(options))
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);

            Assert.Equal("0.000", GetFirstDataCellNumberFormat(provider.Name, stream));
        }
    }

    [Fact]
    public void StreamToDataTable_WithTypedLookingText_PreservesTextAcrossProviders()
    {
        var sourceData = new DataTable("TextData");
        sourceData.Columns.Add("BooleanText", typeof(string));
        sourceData.Columns.Add("GuidText", typeof(string));
        sourceData.Rows.Add("true", "6f9619ff-8b86-d011-b42d-00c04fc964ff");

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);

            var imported = provider.Service.StreamToDataTable(stream);

            Assert.NotNull(imported);
            Assert.Equal(typeof(string), imported.Columns["BooleanText"]!.DataType);
            Assert.Equal(typeof(string), imported.Columns["GuidText"]!.DataType);
            var row = Assert.Single(imported.Rows.Cast<DataRow>());
            Assert.Equal("true", row["BooleanText"]);
            Assert.Equal("6f9619ff-8b86-d011-b42d-00c04fc964ff", row["GuidText"]);
        }
    }

    [Fact]
    public void StreamToDataTable_WithDateFormatWithoutYear_ReadsDateAcrossProviders()
    {
        var expected = new DateTime(2026, 8, 10);

        foreach (var provider in CreateProviders())
        {
            using var stream = CreateDateWorkbook(expected, "dd-mmm");

            var imported = provider.Service.StreamToDataTable(stream);

            Assert.NotNull(imported);
            Assert.Equal(typeof(DateTime), imported.Columns["Value"]!.DataType);
            Assert.Equal(expected, Assert.Single(imported.Rows.Cast<DataRow>())["Value"]);
        }
    }

    [Fact]
    public void CollectionToMemoryStream_WithNoExplicitColumns_ThrowsArgumentExceptionAcrossProviders()
    {
        foreach (var provider in CreateProviders())
        {
            Assert.Throws<ArgumentException>(() =>
                provider.Service.CollectionToMemoryStream(Array.Empty<int>(), Array.Empty<ExcelExportColumn<int>>()));
        }
    }

    [Fact]
    public void CollectionToMemoryStream_WithDuplicateExplicitColumns_ThrowsArgumentExceptionAcrossProviders()
    {
        var columns = new[]
        {
            new ExcelExportColumn<int>("Value", value => value),
            new ExcelExportColumn<int>("value", value => value)
        };

        foreach (var provider in CreateProviders())
        {
            Assert.Throws<ArgumentException>(() =>
                provider.Service.CollectionToMemoryStream([42], columns));
        }
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
    public void StreamToDataTable_PreservesNativeCellValueTypesAcrossProviders()
    {
        var sourceData = new DataTable("TypedData");
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Columns.Add("Name", typeof(string));
        sourceData.Columns.Add("CreatedAt", typeof(DateTime));
        sourceData.Columns.Add("Amount", typeof(decimal));
        sourceData.Columns.Add("Enabled", typeof(bool));
        sourceData.Rows.Add(42, "Alice", new DateTime(2024, 1, 15, 15, 4, 5), 123.5m, true);

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);

            var imported = provider.Service.StreamToDataTable(stream);

            Assert.NotNull(imported);
            Assert.Equal(typeof(int), imported.Columns["Id"]!.DataType);
            Assert.Equal(typeof(string), imported.Columns["Name"]!.DataType);
            Assert.Equal(typeof(DateTime), imported.Columns["CreatedAt"]!.DataType);
            Assert.Equal(typeof(double), imported.Columns["Amount"]!.DataType);
            Assert.Equal(typeof(bool), imported.Columns["Enabled"]!.DataType);
            var row = Assert.Single(imported.Rows.Cast<DataRow>());
            Assert.IsType<int>(row["Id"]);
            Assert.IsType<string>(row["Name"]);
            Assert.IsType<DateTime>(row["CreatedAt"]);
            Assert.IsType<double>(row["Amount"]);
            Assert.IsType<bool>(row["Enabled"]);
        }
    }

    [Fact]
    public void StreamToDataTable_WithMixedColumn_FallsBackToObjectAcrossProviders()
    {
        var sourceData = new DataTable("MixedData");
        sourceData.Columns.Add("Value", typeof(object));
        sourceData.Rows.Add(new DateTime(2024, 1, 15, 15, 4, 5));
        sourceData.Rows.Add("text");

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);

            var imported = provider.Service.StreamToDataTable(stream);

            Assert.NotNull(imported);
            Assert.Equal(typeof(object), imported.Columns["Value"]!.DataType);
            Assert.IsType<DateTime>(imported.Rows[0]["Value"]);
            Assert.IsType<string>(imported.Rows[1]["Value"]);
        }
    }

    [Fact]
    public void StreamToDataTable_InfersCommonNumericAndEmptyColumnTypesAcrossProviders()
    {
        var sourceData = new DataTable("InferredData");
        sourceData.Columns.Add("Integral", typeof(object));
        sourceData.Columns.Add("Fractional", typeof(object));
        sourceData.Columns.Add("Empty", typeof(object));
        sourceData.Columns.Add("Marker", typeof(string));
        sourceData.Rows.Add(42, 10, DBNull.Value, "A");
        sourceData.Rows.Add(5_000_000_000L, 1.5, DBNull.Value, "B");

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);

            var imported = provider.Service.StreamToDataTable(stream);

            Assert.NotNull(imported);
            Assert.Equal(typeof(long), imported.Columns["Integral"]!.DataType);
            Assert.Equal(typeof(double), imported.Columns["Fractional"]!.DataType);
            Assert.Equal(typeof(object), imported.Columns["Empty"]!.DataType);
            Assert.Equal(typeof(string), imported.Columns["Marker"]!.DataType);
            Assert.Equal(42L, Assert.IsType<long>(imported.Rows[0]["Integral"]));
            Assert.Equal(1.5, Assert.IsType<double>(imported.Rows[1]["Fractional"]));
            Assert.Equal(DBNull.Value, imported.Rows[0]["Empty"]);
        }
    }

    [Fact]
    public void StreamToList_WithEmptyRow_HonorsAddEmptyRowAcrossProviders()
    {
        var sourceData = new DataTable("Rows");
        sourceData.Columns.Add("Value", typeof(string));
        sourceData.Rows.Add("A");
        sourceData.Rows.Add(DBNull.Value);
        sourceData.Rows.Add("B");

        foreach (var provider in CreateProviders())
        {
            using var streamWithoutEmptyRow = provider.Service.DataTableToMemoryStream(sourceData);
            var rowsWithoutEmptyRow = provider.Service.StreamToList<DirectImportRow>(
                streamWithoutEmptyRow,
                addEmptyRow: false);

            using var streamWithEmptyRow = provider.Service.DataTableToMemoryStream(sourceData);
            var rowsWithEmptyRow = provider.Service.StreamToList<DirectImportRow>(
                streamWithEmptyRow,
                addEmptyRow: true);

            Assert.NotNull(rowsWithoutEmptyRow);
            Assert.Equal(["A", "B"], rowsWithoutEmptyRow.Select(static row => row.Value));
            Assert.NotNull(rowsWithEmptyRow);
            Assert.Equal(["A", null, "B"], rowsWithEmptyRow.Select(static row => row.Value));
        }
    }

    [Fact]
    public void StreamToList_WithMapper_ProvidesCaseInsensitiveColumnAccessAcrossProviders()
    {
        var sourceData = new DataTable("Rows");
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Columns.Add("Name", typeof(string));
        sourceData.Rows.Add(7, "Alice");

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);
            var rows = provider.Service.StreamToList(
                stream,
                row =>
                {
                    Assert.Equal(2, row.ColumnCount);
                    Assert.True(row.ContainsColumn("id"));
                    Assert.Equal(1, row.GetOrdinal("NAME"));
                    Assert.Equal("Alice", row[1]);

                    return row.Get<int>("ID");
                });

            Assert.Equal(7, Assert.Single(rows!));
        }
    }

    [Fact]
    public void StreamToList_WithMapper_NormalizesEmptyCellsAcrossProviders()
    {
        var sourceData = new DataTable("Rows");
        sourceData.Columns.Add("Value", typeof(string));
        sourceData.Columns.Add("Marker", typeof(string));
        sourceData.Rows.Add(DBNull.Value, "A");

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);
            var rows = provider.Service.StreamToList(
                stream,
                row =>
                {
                    Assert.True(row.IsNull("Value"));
                    Assert.Null(row["Value"]);
                    Assert.True(row.TryGet<string>("Value", out var value));

                    return value;
                },
                addEmptyRow: true);

            Assert.Null(Assert.Single(rows!));
        }
    }

    [Fact]
    public void StreamToList_WithMapper_ReportsConversionFailureAcrossProviders()
    {
        var sourceData = new DataTable("Rows");
        sourceData.Columns.Add("Value", typeof(string));
        sourceData.Rows.Add("not-a-number");

        foreach (var provider in CreateProviders())
        {
            using var stream = provider.Service.DataTableToMemoryStream(sourceData);
            var rows = provider.Service.StreamToList(
                stream,
                row =>
                {
                    Assert.False(row.TryGet<int>("Value", out _));
                    Assert.Throws<InvalidCastException>(() => row.Get<int>("Value"));

                    return row["Value"];
                });

            Assert.Equal("not-a-number", Assert.Single(rows!));
        }
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
            ("ClosedXml", new ClosedXmlExcel(options, _loggerFactory.CreateLogger<ClosedXmlExcel>()))
        };

        foreach (var provider in providers)
        {
            var filePath = Path.Combine(TestFilesDir, $"{provider.Name}_InvalidStyle.xlsx");

            Assert.Throws<ArgumentException>(() => provider.Service.DataTableToExcel(dataTable, filePath, title: "Title"));
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
    public void StreamToDataTable_WithSeekableStream_DoesNotRequireBulkCopy()
    {
        var sourceData = new DataTable();
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Rows.Add(1);
        var service = new ClosedXmlExcel(Options, _loggerFactory.CreateLogger<ClosedXmlExcel>());
        using var excelStream = service.DataTableToMemoryStream(sourceData);
        using var stream = new LimitedReadSizeStream(excelStream.ToArray(), 64 * 1024);

        var imported = service.StreamToDataTable(stream);

        Assert.NotNull(imported);
        Assert.Single(imported.Rows);
        Assert.True(stream.CanRead);
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
    public void StreamToList_WithCancellationDuringNonSeekableRead_ObservesCancellationAcrossProviders()
    {
        var sourceData = new DataTable();
        sourceData.Columns.Add("Id", typeof(int));
        sourceData.Rows.Add(1);

        foreach (var provider in CreateProviders())
        {
            using var excelStream = provider.Service.DataTableToMemoryStream(sourceData);
            using var cancellationTokenSource = new CancellationTokenSource();
            using var stream = new CancellationOnFirstReadStream(excelStream, cancellationTokenSource);

            Assert.ThrowsAny<OperationCanceledException>(() =>
                provider.Service.StreamToList(stream, row => row.Get<int>("Id"), cancellationToken: cancellationTokenSource.Token));
        }
    }

    private static MemoryStream CreateInvalidExcelStream()
    {
        return new MemoryStream([0x00, 0x01, 0x02, 0x03]);
    }

    private static string GetFirstDataCellNumberFormat(string providerName, Stream stream)
    {
        switch (providerName)
        {
            case "Npoi":
                using (var workbook = new global::NPOI.XSSF.UserModel.XSSFWorkbook(stream))
                {
                    return workbook.GetSheetAt(0).GetRow(1).GetCell(0).CellStyle.GetDataFormatString();
                }
            case "ClosedXml":
                using (var workbook = new global::ClosedXML.Excel.XLWorkbook(stream))
                {
                    return workbook.Worksheet(1).Cell(2, 1).Style.NumberFormat.Format;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(providerName), providerName, null);
        }
    }

    private static MemoryStream CreateDateWorkbook(DateTime value, string numberFormat)
    {
        using var workbook = new global::NPOI.XSSF.UserModel.XSSFWorkbook();
        var worksheet = workbook.CreateSheet("Sheet1");
        worksheet.CreateRow(0).CreateCell(0).SetCellValue("Value");
        var valueCell = worksheet.CreateRow(1).CreateCell(0);
        valueCell.SetCellValue(value);
        var dateStyle = workbook.CreateCellStyle();
        dateStyle.DataFormat = workbook.CreateDataFormat().GetFormat(numberFormat);
        valueCell.CellStyle = dateStyle;
        var stream = new MemoryStream();
        workbook.Write(stream, true);
        stream.Position = 0;

        return stream;
    }

    private sealed class DirectImportRow
    {
        public string? Value { get; set; }
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

    private sealed class LimitedReadSizeStream(byte[] buffer, int maximumReadSize) : MemoryStream(buffer)
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count > maximumReadSize)
            {
                throw new InvalidOperationException($"Read request exceeded {maximumReadSize} bytes.");
            }

            return base.Read(buffer, offset, count);
        }
    }

    private List<(string Name, IExcelService Service)> CreateProviders(ExcelOptions? options = null)
    {
        return
        [
            ("Npoi", new NpoiExcel(options ?? Options, _loggerFactory.CreateLogger<NpoiExcel>())),
            ("ClosedXml", new ClosedXmlExcel(options ?? Options, _loggerFactory.CreateLogger<ClosedXmlExcel>()))
        ];
    }
}
