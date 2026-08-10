using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;

namespace Linger.Excel.EPPlus;

public class EPPlusExcel(ExcelOptions? options = null, ILogger<EPPlusExcel>? logger = null)
    : ExcelBase<ExcelPackage, ExcelWorksheet>(options, logger)
{
    // 添加基类要求的方法实现
    protected override ExcelPackage OpenWorkbook(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
        {
            var seekableStream = CopyToMemoryStream(stream, cancellationToken);
            return new ExcelPackage(seekableStream);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new ExcelPackage(stream);
    }

    protected override ExcelWorksheet? GetWorksheet(ExcelPackage workbook, string? sheetName)
    {
        var package = workbook;
        var workBook = package.Workbook;

        if (workBook.Worksheets.Count == 0)
            return null;

        if (!string.IsNullOrEmpty(sheetName))
        {
            // 尝试按名称获取工作表，如果不存在则返回null而不是回退到第一个工作表
            return workBook.Worksheets[sheetName];
        }

        return workBook.Worksheets[0];
    }

    protected override string GetSheetName(ExcelWorksheet worksheet)
    {
        return worksheet.Name;
    }

    protected override List<string> GetAllSheetNames(ExcelPackage workbook)
    {
        var sheetNames = new List<string>();
        foreach (var worksheet in workbook.Workbook.Worksheets)
        {
            if (!string.IsNullOrWhiteSpace(worksheet.Name))
            {
                sheetNames.Add(worksheet.Name);
            }
        }

        return sheetNames;
    }

    protected override bool HasData(ExcelWorksheet worksheet)
    {
        var excelWorksheet = worksheet;
        return excelWorksheet.Dimension != null;
    }

    protected override int GetDataStartRow(ExcelWorksheet worksheet, int headerRowIndex)
    {
        return headerRowIndex + 2; // EPPlus从1开始计数，加2表示从表头下一行开始
    }

    protected override int GetDataEndRow(ExcelWorksheet worksheet)
    {
        var excelWorksheet = worksheet;
        return excelWorksheet.Dimension.End.Row;
    }

    protected override void CloseWorkbook(ExcelPackage workbook)
    {
        workbook.Dispose();
    }

    protected override int EstimateColumnCount(ExcelWorksheet worksheet)
    {
        var excelWorksheet = worksheet;
        return excelWorksheet.Dimension?.End.Column ?? 0;
    }

    /// <summary>
    /// 创建表头映射关系(列索引到列名的映射)
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)，-1表示没有表头行</param>
    /// <returns>列索引到列名的字典(键为0-based索引)</returns>
    /// <remarks>
    /// EPPlus使用1-based索引系统。
    /// 如果headerRowIndex为-1(无表头)，则返回空字典。
    /// 注意：虽然EPPlus使用1-based列索引，但返回的字典键是0-based，以保持API一致性。
    /// </remarks>
    protected override Dictionary<int, string> ExtractRawHeaderMappings(ExcelWorksheet worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();
        var excelWorksheet = worksheet;

        if (headerRowIndex < 0 || excelWorksheet.Dimension == null)
            return result;

        int colCount = excelWorksheet.Dimension.End.Column;

        for (int i = 1; i <= colCount; i++)
        {
            var cell = excelWorksheet.Cells[headerRowIndex + 1, i]; // EPPlus从1开始计数
            string columnName = cell.Text?.Trim() ?? $"Column{i}";
            // 存储0-based索引作为键，确保在GetCellValue中能正确+1
            result[i - 1] = columnName;
        }

        return result;
    }

    /// <summary>
    /// 获取单元格的值
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="rowNum">行索引(1-based)</param>
    /// <param name="colIndex">列索引(0-based)</param>
    /// <returns>单元格值</returns>
    /// <remarks>
    /// EPPlus使用1-based索引系统。
    /// rowNum已经是EPPlus的1-based行号(由GetDataStartRow方法确保)。
    /// colIndex是基类传入的0-based索引，需要+1转换为EPPlus需要的列号。
    /// </remarks>
    protected override object GetCellValue(ExcelWorksheet worksheet, int rowNum, int colIndex)
    {
        var excelWorksheet = worksheet;
        var cell = excelWorksheet.Cells[rowNum, colIndex + 1]; // EPPlus从1开始计数，需要+1

        if (cell?.Value == null)
            return DBNull.Value;

        // 处理日期格式
        bool isDateFormat = cell.Style.Numberformat.Format.Contains("yy");

        // 使用通用转换器
        return GetExcelCellValue(cell.Value, isDateFormat);
    }

    #region 私有辅助方法

    /// <summary>
    /// 将值写入Excel单元格并设置适当的格式
    /// </summary>
    private void WriteValueToCell(ExcelRange cell, object? value)
    {
        if (value == null || value is DBNull)
        {
            cell.Value = null;
            return;
        }

        // 根据值类型进行特殊处理
        switch (value)
        {
            case DateTime dateTime:
                cell.Value = dateTime;
                cell.Style.Numberformat.Format = Options.StyleOptions.DataStyle.DateFormat;
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                cell.Style.Numberformat.Format = Options.StyleOptions.DataStyle.DecimalFormat;
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                cell.Style.Numberformat.Format = doubleValue % 1 == 0 ? Options.StyleOptions.DataStyle.IntegerFormat : Options.StyleOptions.DataStyle.DecimalFormat;
                break;
            case float floatValue:
                cell.Value = floatValue;
                cell.Style.Numberformat.Format = floatValue % 1 == 0 ? Options.StyleOptions.DataStyle.IntegerFormat : Options.StyleOptions.DataStyle.DecimalFormat;
                break;
            case int or long or short or byte or sbyte or ushort or uint or ulong:
                cell.Value = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
                cell.Style.Numberformat.Format = Options.StyleOptions.DataStyle.IntegerFormat;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }

        // 应用边框
        DrawBorder(cell);
    }

    /// <summary>
    /// 设置标题行样式
    /// </summary>
    private void ApplyTitleRowFormatting(ExcelRange titleRange)
    {
        titleRange.Style.Font.Bold = Options.StyleOptions.TitleStyle.Bold;
        titleRange.Style.Font.Size = Options.StyleOptions.TitleStyle.FontSize;
        titleRange.Style.Font.Name = Options.StyleOptions.TitleStyle.FontName;
        titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        if (!string.IsNullOrEmpty(Options.StyleOptions.TitleStyle.BackgroundColor))
        {
            titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            titleRange.Style.Fill.BackgroundColor.SetColor(Color.ParseHex(Options.StyleOptions.TitleStyle.BackgroundColor.TrimStart('#')));
        }

        if (!string.IsNullOrEmpty(Options.StyleOptions.TitleStyle.FontColor))
        {
            titleRange.Style.Font.Color.SetColor(Color.ParseHex(Options.StyleOptions.TitleStyle.FontColor.TrimStart('#')));
        }
    }

    /// <summary>
    /// 设置表头行样式
    /// </summary>
    private void ApplyHeaderRowFormatting(ExcelRange headerCell)
    {
        headerCell.Style.Font.Bold = Options.StyleOptions.HeaderStyle.Bold;
        headerCell.Style.Font.Size = Options.StyleOptions.HeaderStyle.FontSize;
        headerCell.Style.Font.Name = Options.StyleOptions.HeaderStyle.FontName;
        headerCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        headerCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.BackgroundColor))
        {
            headerCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerCell.Style.Fill.BackgroundColor.SetColor(Color.ParseHex(Options.StyleOptions.HeaderStyle.BackgroundColor.TrimStart('#')));
        }

        if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.FontColor))
        {
            headerCell.Style.Font.Color.SetColor(Color.ParseHex(Options.StyleOptions.HeaderStyle.FontColor.TrimStart('#')));
        }

        // 应用边框
        DrawBorder(headerCell);
    }

    private void ApplyBasicFormatting(ExcelWorksheet worksheet, int rowCount, int columnCount)
    {
        if (Options.AutoFitColumns)
        {
            worksheet.Cells.AutoFitColumns();

            for (var columnIndex = 1; columnIndex <= columnCount; columnIndex++)
            {
                var column = worksheet.Column(columnIndex);
                if (column.Width < 12)
                {
                    column.Width = 12;
                }
            }
        }

        // 设置表格边框
        if (rowCount > 1 && columnCount > 0)
        {
            worksheet.Cells[1, 1, rowCount, columnCount].Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }
    }

    /// <summary>
    /// 为单元格添加边框
    /// </summary>
    private static void DrawBorder(ExcelRange cell)
    {
        cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }

    /// <summary>
    /// 创建空工作簿
    /// </summary>
    protected override ExcelPackage CreateWorkbook()
    {
        return new ExcelPackage();
    }

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected override ExcelWorksheet CreateWorksheet(ExcelPackage workbook, string sheetName)
    {
        var package = workbook;
        return package.Workbook.Worksheets.Add(sheetName);
    }

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    protected override int ApplyTitle(ExcelWorksheet worksheet, string title, int columnCount)
    {
        var excelWorksheet = worksheet;
        excelWorksheet.Cells[1, 1].Value = title;
        var titleRange = excelWorksheet.Cells[1, 1, 1, columnCount];
        titleRange.Merge = true;
        ApplyTitleRowFormatting(titleRange);
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法
    /// </summary>
    protected override void CreateHeaderRowCore(ExcelWorksheet worksheet, string[] columnNames, int startRowIndex)
    {
        var excelWorksheet = worksheet;
        for (int i = 0; i < columnNames.Length; i++)
        {
            var cell = excelWorksheet.Cells[startRowIndex + 1, i + 1];
            cell.Value = columnNames[i];
            ApplyHeaderRowFormatting(cell);
        }
    }

    /// <summary>
    /// 处理数据行
    /// </summary>
    protected override void ProcessDataRows(ExcelWorksheet worksheet, DataTable dataTable, int startRowIndex)
    {
        var excelWorksheet = worksheet;

        for (var rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
            {
                var cell = excelWorksheet.Cells[startRowIndex + rowIndex + 2, columnIndex + 1];
                var value = dataTable.Rows[rowIndex][columnIndex];
                WriteValueToCell(cell, value != DBNull.Value ? value : null);
            }
        }
    }

    /// <summary>
    /// 处理集合数据行
    /// </summary>
    protected override void ProcessCollectionRows<T>(ExcelWorksheet worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        var exportProperties = GetExportProperties(properties);

        for (var rowIndex = 0; rowIndex < list.Count; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < exportProperties.Length; columnIndex++)
            {
                var cell = worksheet.Cells[startRowIndex + rowIndex + 2, columnIndex + 1];
                WriteValueToCell(cell, exportProperties[columnIndex].GetValue(list[rowIndex]));
            }
        }
    }

    /// <summary>
    /// 使用显式列定义直接写入集合数据行。
    /// </summary>
    protected override void ProcessCollectionRows<T>(ExcelWorksheet worksheet, IReadOnlyList<T> items,
        IReadOnlyList<ExcelExportColumn<T>> columns, int startRowIndex)
    {
        for (var rowIndex = 0; rowIndex < items.Count; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var cell = worksheet.Cells[startRowIndex + rowIndex + 2, columnIndex + 1];
                WriteValueToCell(cell, GetExportValue(columns[columnIndex], items[rowIndex]));
            }
        }
    }

    /// <summary>
    /// 应用工作表格式化
    /// </summary>
    protected override void ApplyWorksheetFormatting(ExcelWorksheet worksheet, int rowCount, int columnCount)
    {
        var excelWorksheet = worksheet;
        ApplyBasicFormatting(excelWorksheet, rowCount, columnCount);
    }

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected override MemoryStream SaveWorkbookToStream(ExcelPackage workbook)
    {
        var package = workbook;
        var ms = new MemoryStream();
        package.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    #endregion
}
