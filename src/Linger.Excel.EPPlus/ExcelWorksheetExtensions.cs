using OfficeOpenXml;

namespace Linger.Excel.EPPlus;

public static class ExcelWorksheetExtensions
{
    public static void TrimLastEmptyRows(this ExcelWorksheet worksheet)
    {
        if (worksheet.IsLastRowEmpty())
            worksheet.DeleteRow(worksheet.Dimension.End.Row);
    }

    public static bool IsLastRowEmpty(this ExcelWorksheet worksheet)
    {
        var empties = new List<bool>();

        for (var i = 1; i <= worksheet.Dimension.End.Column; i++)
        {
            var rowEmpty = worksheet.Cells[worksheet.Dimension.End.Row, i].Value == null;
            empties.Add(rowEmpty);
        }

        return empties.All(e => e);

        //return Enumerable.Range(1, worksheet.Dimension.End.Column).Select(i => worksheet.Cells[worksheet.Dimension.End.Row, i].Value == null).All(x => x);
    }
}
