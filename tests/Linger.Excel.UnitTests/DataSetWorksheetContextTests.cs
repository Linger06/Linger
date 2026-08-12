using System.Data;
using Linger.Excel.ClosedXML;
using Linger.Excel.Contracts;
using Linger.Excel.Npoi;
using NPOI.SS.UserModel;
using Xunit;

namespace Linger.Excel.Tests;

public class DataSetWorksheetContextTests : ExcelServiceTestBase, IDisposable
{
    public DataSetWorksheetContextTests()
    {
        CleanupTestDir();
    }

    public void Dispose()
    {
        CleanupTestDir();
        GC.SuppressFinalize(this);
    }

    protected override IExcelService GetExcelService()
    {
        return new NpoiExcel(Options);
    }

    [Fact]
    public void DataSetToExcel_WithZeroColumnTable_InvokesAction()
    {
        var dataSet = new DataSet();
        dataSet.Tables.Add(new DataTable("Empty"));
        var actionInvocations = 0;
        var filePath = Path.Combine(TestFilesDir, "Npoi_EmptyTableAction.xlsx");
        var service = new NpoiExcel(Options);

        service.DataSetToExcel(dataSet, filePath, _ => actionInvocations++);

        Assert.Equal(1, actionInvocations);
    }

    [Fact]
    public void DataSetToExcel_WithWorksheetContext_ProvidesEachWorksheetAndSourceTable()
    {
        var dataSet = new DataSet();
        dataSet.Tables.Add(new DataTable("First"));
        dataSet.Tables.Add(new DataTable("Second"));
        var contexts = new List<IWorksheetExportContext<ISheet>>();
        var filePath = Path.Combine(TestFilesDir, "Npoi_WorksheetContext.xlsx");
        var service = new NpoiExcel(Options);

        service.DataSetToExcel(dataSet, filePath, contexts.Add);

        Assert.Collection(
            contexts,
            context => AssertWorksheetContext(context, dataSet.Tables[0], 0, "First"),
            context => AssertWorksheetContext(context, dataSet.Tables[1], 1, "Second"));
    }

    [Fact]
    public void DataSetToExcel_WithWorksheetContext_InvokesAllProvidersForZeroColumnTable()
    {
        var dataSet = new DataSet();
        dataSet.Tables.Add(new DataTable("Empty"));
        var npoiInvocations = 0;
        var closedXmlInvocations = 0;

        new NpoiExcel(Options).DataSetToExcel(
            dataSet,
            Path.Combine(TestFilesDir, "Npoi_WorksheetContextEmpty.xlsx"),
            context => AssertWorksheetContext(context, dataSet.Tables[0], 0, "Empty", ref npoiInvocations));
        new ClosedXmlExcel(Options).DataSetToExcel(
            dataSet,
            Path.Combine(TestFilesDir, "ClosedXml_WorksheetContextEmpty.xlsx"),
            context => AssertWorksheetContext(context, dataSet.Tables[0], 0, "Empty", ref closedXmlInvocations));
        Assert.Equal(1, npoiInvocations);
        Assert.Equal(1, closedXmlInvocations);
    }

    [Fact]
    public void DataSetToExcel_WithWorksheetContextActionThatThrows_PropagatesExceptionAcrossProviders()
    {
        var dataSet = new DataSet();
        dataSet.Tables.Add(new DataTable("Data"));

        Assert.Throws<InvalidOperationException>(() => new NpoiExcel(Options).DataSetToExcel(
            dataSet,
            Path.Combine(TestFilesDir, "Npoi_WorksheetContextException.xlsx"),
            _ => throw new InvalidOperationException()));
        Assert.Throws<InvalidOperationException>(() => new ClosedXmlExcel(Options).DataSetToExcel(
            dataSet,
            Path.Combine(TestFilesDir, "ClosedXml_WorksheetContextException.xlsx"),
            _ => throw new InvalidOperationException()));
    }

    private static void AssertWorksheetContext(
        IWorksheetExportContext<object> context,
        DataTable expectedDataTable,
        int expectedTableIndex,
        string expectedSheetName)
    {
        Assert.Equal(expectedTableIndex, context.TableIndex);
        Assert.Equal(expectedSheetName, context.SheetName);
        Assert.Same(expectedDataTable, context.DataTable);
        Assert.NotNull(context.Worksheet);
    }

    private static void AssertWorksheetContext(
        IWorksheetExportContext<object> context,
        DataTable expectedDataTable,
        int expectedTableIndex,
        string expectedSheetName,
        ref int invocations)
    {
        AssertWorksheetContext(context, expectedDataTable, expectedTableIndex, expectedSheetName);
        Assert.Empty(context.DataTable.Columns.Cast<DataColumn>());
        invocations++;
    }
}
