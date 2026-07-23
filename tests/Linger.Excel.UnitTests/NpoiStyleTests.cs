using System.Data;
using Linger.Excel.Contracts;
using Linger.Excel.Npoi;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Xunit;

namespace Linger.Excel.Tests;

public class NpoiStyleTests : ExcelServiceTestBase, IDisposable
{
    public NpoiStyleTests()
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
    public void DataTableToMemoryStream_AppliesConfiguredRgbTitleAndHeaderColors()
    {
        var options = new ExcelOptions
        {
            StyleOptions = new ExcelStyleOptions
            {
                TitleStyle = new TitleStyle
                {
                    BackgroundColor = "#4472C4",
                    FontColor = "#F2F2F2"
                },
                HeaderStyle = new HeaderStyle
                {
                    BackgroundColor = "#70AD47",
                    FontColor = "#FFFFFF"
                }
            }
        };
        var dataTable = new DataTable();
        dataTable.Columns.Add("Value", typeof(string));
        dataTable.Rows.Add("Data");
        var service = new NpoiExcel(options);

        using var stream = service.DataTableToMemoryStream(dataTable, title: "Report");
        using var workbook = new XSSFWorkbook(stream);
        var worksheet = workbook.GetSheetAt(0);

        AssertCellColors(workbook, worksheet.GetRow(0).GetCell(0), [0x44, 0x72, 0xC4], [0xF2, 0xF2, 0xF2]);
        AssertCellColors(workbook, worksheet.GetRow(1).GetCell(0), [0x70, 0xAD, 0x47], [0xFF, 0xFF, 0xFF]);
    }

    private static void AssertCellColors(XSSFWorkbook workbook, ICell cell, byte[] expectedFillColor, byte[] expectedFontColor)
    {
        var style = Assert.IsType<XSSFCellStyle>(cell.CellStyle);
        var fillColor = Assert.IsType<XSSFColor>(style.FillForegroundXSSFColor);
        var font = Assert.IsType<XSSFFont>(workbook.GetFontAt(style.FontIndex));
        var fontColor = Assert.IsType<XSSFColor>(font.GetXSSFColor());

        Assert.Equal(CreateArgbHex(expectedFillColor), fillColor.ARGBHex);
        Assert.Equal(CreateArgbHex(expectedFontColor), fontColor.ARGBHex);
    }

    private static string CreateArgbHex(byte[] rgb)
    {
        return $"FF{rgb[0]:X2}{rgb[1]:X2}{rgb[2]:X2}";
    }
}
