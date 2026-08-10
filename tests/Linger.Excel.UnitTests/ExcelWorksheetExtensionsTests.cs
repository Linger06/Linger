using System.Data;
using Linger.Excel.Contracts;
using Linger.Excel.EPPlus;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Xunit;

namespace Linger.Excel.Tests;

public class ExcelWorksheetExtensionsTests
{
    [Fact]
    public void DataTableToMemoryStream_AutoFitColumnsDisabled_PreservesDefaultColumnWidth()
    {
        var options = new ExcelOptions
        {
            AutoFitColumns = false
        };
        var dataTable = new DataTable();
        dataTable.Columns.Add("Value", typeof(string));
        dataTable.Rows.Add(new string('x', 100));
        var service = new EPPlusExcel(options);

        using var stream = service.DataTableToMemoryStream(dataTable);
        using var package = new ExcelPackage(stream);

        Assert.True(package.Workbook.Worksheets[0].Column(1).Width < 20);
    }

    [Fact]
    public void IsLastRowEmpty_WithEmptyWorksheet_ReturnsTrue()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");

        Assert.True(worksheet.IsLastRowEmpty());
    }

    [Fact]
    public void TrimLastEmptyRows_WithMultipleTrailingEmptyRows_RemovesAllTrailingRows()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sheet1");
        worksheet.Cells[1, 1].Value = "Value";
        worksheet.Cells[2, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;

        worksheet.TrimLastEmptyRows();

        Assert.Equal(1, worksheet.Dimension.End.Row);
    }
}
