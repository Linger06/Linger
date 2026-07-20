namespace Linger.Excel.Contracts;

/// <summary>
/// Provides read-only context for a worksheet created during a <see cref="DataSet"/> export.
/// </summary>
/// <typeparam name="TWorksheet">The provider-specific worksheet type.</typeparam>
public interface IWorksheetExportContext<out TWorksheet>
    where TWorksheet : class
{
    /// <summary>
    /// Gets the created worksheet.
    /// </summary>
    TWorksheet Worksheet { get; }

    /// <summary>
    /// Gets the source data table.
    /// </summary>
    DataTable DataTable { get; }

    /// <summary>
    /// Gets the zero-based index of the source table.
    /// </summary>
    int TableIndex { get; }

    /// <summary>
    /// Gets the created worksheet name.
    /// </summary>
    string SheetName { get; }
}

/// <summary>
/// Provides context for a worksheet created during a <see cref="DataSet"/> export.
/// </summary>
/// <typeparam name="TWorksheet">The provider-specific worksheet type.</typeparam>
public sealed class WorksheetExportContext<TWorksheet>
    : IWorksheetExportContext<TWorksheet>
    where TWorksheet : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorksheetExportContext{TWorksheet}"/> class.
    /// </summary>
    /// <param name="worksheet">The created worksheet.</param>
    /// <param name="dataTable">The source data table.</param>
    /// <param name="tableIndex">The zero-based index of the source table.</param>
    /// <param name="sheetName">The created worksheet name.</param>
    public WorksheetExportContext(TWorksheet worksheet, DataTable dataTable, int tableIndex, string sheetName)
    {
        ArgumentNullException.ThrowIfNull(worksheet);
        ArgumentNullException.ThrowIfNull(dataTable);
        ArgumentNullException.ThrowIfNull(sheetName);

        Worksheet = worksheet;
        DataTable = dataTable;
        TableIndex = tableIndex;
        SheetName = sheetName;
    }

    /// <summary>
    /// Gets the created worksheet.
    /// </summary>
    public TWorksheet Worksheet { get; }

    /// <summary>
    /// Gets the source data table.
    /// </summary>
    public DataTable DataTable { get; }

    /// <summary>
    /// Gets the zero-based index of the source table.
    /// </summary>
    public int TableIndex { get; }

    /// <summary>
    /// Gets the created worksheet name.
    /// </summary>
    public string SheetName { get; }
}
