using OfficeOpenXml;

namespace Linger.Excel.EPPlus;

[Obsolete("Use NpoiExcel or ClosedXmlExcel instead. The EPPlus provider will be removed in 2.0.0. See the migration guide in the Linger repository.")]
public static class ExcelWorksheetExtensions
{
    /// <summary>
    /// Removes all trailing rows that contain no cell values.
    /// </summary>
    /// <param name="worksheet">The worksheet to trim.</param>
    public static void TrimLastEmptyRows(this ExcelWorksheet worksheet)
    {
        ArgumentNullException.ThrowIfNull(worksheet);

        while (worksheet.Dimension is not null && worksheet.IsLastRowEmpty())
        {
            worksheet.DeleteRow(worksheet.Dimension.End.Row);
        }
    }

    /// <summary>
    /// Determines whether the last used row contains no cell values.
    /// </summary>
    /// <param name="worksheet">The worksheet to inspect.</param>
    /// <returns><see langword="true"/> when the worksheet is empty or its last used row has no values; otherwise, <see langword="false"/>.</returns>
    public static bool IsLastRowEmpty(this ExcelWorksheet worksheet)
    {
        ArgumentNullException.ThrowIfNull(worksheet);

        if (worksheet.Dimension is null)
        {
            return true;
        }

        var lastRow = worksheet.Dimension.End.Row;
        for (var columnIndex = worksheet.Dimension.Start.Column; columnIndex <= worksheet.Dimension.End.Column; columnIndex++)
        {
            if (worksheet.Cells[lastRow, columnIndex].Value is not null)
            {
                return false;
            }
        }

        return true;
    }
}
