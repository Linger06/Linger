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

    private List<(string Name, IExcelService Service)> CreateProviders()
    {
        return
        [
            ("Npoi", new NpoiExcel(Options, _loggerFactory.CreateLogger<NpoiExcel>())),
            ("EPPlus", new EPPlusExcel(Options, _loggerFactory.CreateLogger<EPPlusExcel>())),
            ("ClosedXml", new ClosedXmlExcel(Options, _loggerFactory.CreateLogger<ClosedXmlExcel>()))
        ];
    }
}
