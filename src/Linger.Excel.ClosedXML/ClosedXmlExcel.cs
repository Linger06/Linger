using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;
using Microsoft.Extensions.Logging;

namespace Linger.Excel.ClosedXML;

public class ClosedXmlExcel(ExcelOptions? options = null, ILogger<ClosedXmlExcel>? logger = null)
    : ExcelBase<XLWorkbook, IXLWorksheet>(options, logger)
{
    // 添加基类要求的方法实现
    protected override XLWorkbook OpenWorkbook(Stream stream)
    {
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return new XLWorkbook(memoryStream);
    }

    protected override IXLWorksheet GetWorksheet(XLWorkbook workbook, string? sheetName)
    {
        if (sheetName.IsNullOrEmpty())
        {
            return workbook.Worksheet(1);
        }
        else if (workbook.Worksheets.TryGetWorksheet(sheetName, out var namedSheet))
        {
            return namedSheet;
        }
        else
        {
            return workbook.Worksheet(1);
        }
    }

    protected override string GetSheetName(IXLWorksheet worksheet)
    {
        return worksheet.Name;
    }

    protected override bool HasData(IXLWorksheet worksheet)
    {
        return worksheet.RangeUsed() != null;
    }

    protected override Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(IXLWorksheet worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, PropertyInfo>();

        if (headerRowIndex < 0) return result;

        var headerRow = worksheet.Row(headerRowIndex + 1);
        var properties = typeof(T).GetProperties()
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            var columnName = cell.Value.ToString();
            if (string.IsNullOrEmpty(columnName)) continue;

            if (properties.TryGetValue(columnName, out var property))
            {
                result[cell.Address.ColumnNumber] = property;
            }
        }

        return result;
    }

    protected override int GetDataStartRow(IXLWorksheet worksheet, int headerRowIndex)
    {
        return headerRowIndex < 0 ? 1 : headerRowIndex + 2;
    }

    protected override int GetDataEndRow(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        return usedRange?.LastRow()?.RowNumber() ?? 0;
    }

    protected override void CloseWorkbook(XLWorkbook workbook)
    {
        try
        {
            workbook.Dispose();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "关闭ClosedXML工作簿时出错");
        }
    }

    protected override int EstimateColumnCount(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        return usedRange?.LastColumn()?.ColumnNumber() ?? 0;
    }

    protected override Dictionary<int, string> CreateHeaderMappings(IXLWorksheet worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();

        if (headerRowIndex < 0)
            return result;

        var headerRow = worksheet.Row(headerRowIndex + 1); // ClosedXML从1开始计数

        foreach (var cell in headerRow.CellsUsed())
        {
            int colIndex = cell.Address.ColumnNumber;
            string columnName = cell.Value.ToString() ?? $"Column{colIndex}";
            result[colIndex] = columnName;
        }

        return result;
    }

    protected override object GetCellValue(IXLWorksheet worksheet, int rowNum, int colIndex)
    {
        // ClosedXML从1开始计数
        var row = worksheet.Row(rowNum);
        var cell = row.Cell(colIndex);

        if (cell.IsEmpty())
            return DBNull.Value;

        return GetExcelCellValue(
            cell.DataType == XLDataType.DateTime ? cell.GetDateTime() :
            cell.DataType == XLDataType.Number ? cell.GetDouble() :
            cell.DataType == XLDataType.Boolean ? cell.GetBoolean() :
            cell.DataType == XLDataType.Text ? cell.GetString() :
            cell.Value,
            cell.DataType == XLDataType.DateTime);
    }

    #region 私有辅助方法

    /// <summary>
    /// 从Excel单元格获取适当类型的值
    /// </summary>
    private object GetExcelCellValue(IXLCell cell)
    {
        switch (cell.DataType)
        {
            case XLDataType.Text:
                return GetExcelCellValue(cell.GetString());
            case XLDataType.Number:
                return GetExcelCellValue(cell.GetDouble());
            case XLDataType.Boolean:
                return GetExcelCellValue(cell.GetBoolean());
            case XLDataType.DateTime:
                return GetExcelCellValue(cell.GetDateTime(), true);
            case XLDataType.TimeSpan:
                return GetExcelCellValue(cell.GetTimeSpan(), true);
            case XLDataType.Error:
            case XLDataType.Blank:
            default:
                return cell.Value;
        }
    }

    /// <summary>
    /// 将值写入Excel单元格并设置适当的格式
    /// </summary>
    private void WriteValueToCell(IXLCell cell, object? value)
    {
        if (value == null || value is DBNull)
        {
            cell.Value = string.Empty;
            return;
        }

        // 根据值类型进行特殊处理
        switch (value)
        {
            case DateTime dateTime:
                cell.Value = dateTime;
                cell.Style.DateFormat.Format = Options.StyleOptions.DefaultDateFormat; // 从StyleOptions中获取
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                cell.Style.NumberFormat.Format = Options.StyleOptions.DecimalFormat;
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                cell.Style.NumberFormat.Format = doubleValue % 1 == 0 ? Options.StyleOptions.IntegerFormat : Options.StyleOptions.DecimalFormat;
                break;
            case float floatValue:
                cell.Value = floatValue;
                cell.Style.NumberFormat.Format = floatValue % 1 == 0 ? Options.StyleOptions.IntegerFormat : Options.StyleOptions.DecimalFormat;
                break;
            case int or long or short or byte or sbyte or ushort or uint or ulong:
                cell.Value = Convert.ToInt64(value);
                cell.Style.NumberFormat.Format = Options.StyleOptions.IntegerFormat;
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
    private void ApplyTitleRowFormatting(IXLRange titleRange)
    {
        try
        {
            titleRange.Style.Font.Bold = Options.StyleOptions.TitleBold;
            titleRange.Style.Font.FontSize = Options.StyleOptions.TitleFontSize;
            titleRange.Style.Font.FontName = Options.StyleOptions.TitleFontName;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // 设置背景色
            if (!string.IsNullOrEmpty(Options.StyleOptions.TitleBackgroundColor))
            {
                try
                {
                    var color = XLColor.FromHtml(Options.StyleOptions.TitleBackgroundColor);
                    titleRange.Style.Fill.BackgroundColor = color;
                }
                catch (Exception ex)
                {
                    logger?.LogDebug(ex, "设置标题背景色失败");
                    titleRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                }
            }

            // 设置文字颜色
            if (!string.IsNullOrEmpty(Options.StyleOptions.TitleFontColor))
            {
                try
                {
                    var fontColor = XLColor.FromHtml(Options.StyleOptions.TitleFontColor);
                    titleRange.Style.Font.FontColor = fontColor;
                }
                catch (Exception ex)
                {
                    logger?.LogDebug(ex, "设置标题文字颜色失败");
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "设置标题行样式失败");
        }
    }

    /// <summary>
    /// 设置表头行样式
    /// </summary>
    private void ApplyHeaderRowFormatting(IXLCell headerCell)
    {
        try
        {
            headerCell.Style.Font.Bold = Options.StyleOptions.HeaderBold;
            headerCell.Style.Font.FontSize = Options.StyleOptions.HeaderFontSize;
            headerCell.Style.Font.FontName = Options.StyleOptions.HeaderFontName;
            headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // 设置背景色
            if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderBackgroundColor))
            {
                try
                {
                    var color = XLColor.FromHtml(Options.StyleOptions.HeaderBackgroundColor);
                    headerCell.Style.Fill.BackgroundColor = color;
                }
                catch (Exception ex)
                {
                    logger?.LogDebug(ex, "设置表头背景色失败");
                    headerCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }
            }

            // 设置文字颜色
            if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderFontColor))
            {
                try
                {
                    var fontColor = XLColor.FromHtml(Options.StyleOptions.HeaderFontColor);
                    headerCell.Style.Font.FontColor = fontColor;
                }
                catch (Exception ex)
                {
                    logger?.LogDebug(ex, "设置表头文字颜色失败");
                }
            }

            // 应用边框
            DrawBorder(headerCell);
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "设置表头行样式失败");
        }
    }

    /// <summary>
    /// 为单元格添加边框
    /// </summary>
    private static void DrawBorder(IXLCell cell)
    {
        cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
    }

    #endregion

    /// <summary>
    /// 创建空工作簿
    /// </summary>
    protected override XLWorkbook CreateWorkbook()
    {
        return new XLWorkbook();
    }

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected override IXLWorksheet CreateWorksheet(XLWorkbook workbook, string sheetName)
    {
        return workbook.Worksheets.Add(sheetName);
    }

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    protected override int ApplyTitle(IXLWorksheet worksheet, string title, int columnCount)
    {
        var cell = worksheet.Cell(1, 1);
        cell.Value = title;
        var titleRange = worksheet.Range(1, 1, 1, columnCount);
        titleRange.Merge();
        ApplyTitleRowFormatting(titleRange);
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法 - 处理共通逻辑
    /// </summary>
    protected override void CreateHeaderRowCore(IXLWorksheet worksheet, string[] columnNames, int startRowIndex)
    {
        for (int i = 0; i < columnNames.Length; i++)
        {
            var cell = worksheet.Cell(startRowIndex + 1, i + 1);
            cell.Value = columnNames[i];
            ApplyHeaderRowFormatting(cell);
        }
    }

    /// <summary>
    /// 处理数据行
    /// </summary>
    protected override void ProcessDataRows(IXLWorksheet worksheet, DataTable dataTable, int startRowIndex)
    {
        bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            logger?.LogDebug("使用并行处理导出 {Count} 行数据", dataTable.Rows.Count);

            // 使用批处理提高性能
            int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;

            // 预先计算所有值以避免在多线程中重复计算
            var cellValues = new object?[dataTable.Rows.Count, dataTable.Columns.Count];

            Parallel.For(0, dataTable.Rows.Count, i =>
            {
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    cellValues[i, j] = dataTable.Rows[i][j];
                }
            });

            // 批量写入
            for (int batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, dataTable.Rows.Count);

                for (int i = batchStart; i < batchEnd; i++)
                {
                    for (var j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var cell = worksheet.Cell(startRowIndex + i + 2, j + 1);
                        WriteValueToCell(cell, cellValues[i, j]);
                    }
                }
            }
        }
        else
        {
            // 顺序处理小数据集
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var cell = worksheet.Cell(startRowIndex + i + 2, j + 1);
                    WriteValueToCell(cell, dataTable.Rows[i][j]);
                }
            }
        }
    }

    /// <summary>
    /// 处理集合数据行
    /// </summary>
    protected override void ProcessCollectionRows<T>(IXLWorksheet worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        // 优先查找带有ExcelColumn特性的属性
        var columns = GetExcelColumns(properties);
        if (columns.Count == 0)
        {
            columns = properties.Select((p, i) => (Name: p.Name, ColumnName: p.Name, Index: i)).ToList();
        }
        columns = columns.OrderBy(c => c.Index).ToList();

        // 判断是否需要并行处理
        bool useParallelProcessing = list.Count > Options.ParallelProcessingThreshold;

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            logger?.LogDebug("使用并行处理导出 {Count} 条记录", list.Count);

            // 使用批处理提高性能
            int batchSize = Options.UseBatchWrite ? Options.BatchSize : list.Count;

            // 预计算所有值
            var cellValues = new object?[list.Count, columns.Count];

            Parallel.For(0, list.Count, i =>
            {
                for (int j = 0; j < columns.Count; j++)
                {
                    // 查找对应的属性
                    var property = properties.FirstOrDefault(p => p.Name == columns[j].Name);
                    cellValues[i, j] = property?.GetValue(list[i]);
                }
            });

            // 批量写入
            for (int batchStart = 0; batchStart < list.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, list.Count);
                for (int i = batchStart; i < batchEnd; i++)
                {
                    for (int j = 0; j < columns.Count; j++)
                    {
                        var cell = worksheet.Cell(startRowIndex + i + 2, j + 1);
                        WriteValueToCell(cell, cellValues[i, j]);
                    }
                }
            }
        }
        else
        {
            // 顺序处理小数据集
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < columns.Count; j++)
                {
                    var cell = worksheet.Cell(startRowIndex + i + 2, j + 1);
                    var property = properties.FirstOrDefault(p => p.Name == columns[j].Name);
                    WriteValueToCell(cell, property?.GetValue(list[i]));
                }
            }
        }
    }

    /// <summary>
    /// 应用工作表格式化
    /// </summary>
    protected override void ApplyWorksheetFormatting(IXLWorksheet worksheet, int rowCount, int columnCount)
    {
        // 设置所有单元格自动适应宽度
        if (Options.AutoFitColumns)
        {
            // 设置所有单元格自动适应宽度
            worksheet.Columns().AdjustToContents();

            // 对所有列设置最小宽度 - 修正注释
            for (int i = 1; i <= columnCount; i++)
            {
                var column = worksheet.Column(i);
                if (column.Width < 12)
                    column.Width = 12;
            }
        }

        // 设置表格边框
        if (rowCount > 1 && columnCount > 0)
        {
            worksheet.Range(1, 1, rowCount, columnCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(1, 1, rowCount, columnCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }
    }

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected override MemoryStream SaveWorkbookToStream(XLWorkbook workbook)
    {
        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }
}
