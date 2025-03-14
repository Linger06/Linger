using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;

namespace Linger.Excel.EPPlus;

public class EPPlusExcel(ExcelOptions? options = null, ILogger<EPPlusExcel>? logger = null) : ExcelBase(options, logger)
{
    #region Import
    // 添加基类要求的方法实现
    protected override object OpenWorkbook(Stream stream)
    {
        return new ExcelPackage(stream);
    }

    protected override object GetWorksheet(object workbook, string? sheetName)
    {
        var package = (ExcelPackage)workbook;
        var workBook = package.Workbook;

        if (workBook.Worksheets.Count == 0)
            return null!;

        if (!string.IsNullOrEmpty(sheetName))
        {
            return workBook.Worksheets[sheetName] ?? workBook.Worksheets[0];
        }

        return workBook.Worksheets[0];
    }

    protected override string GetSheetName(object worksheet)
    {
        return ((ExcelWorksheet)worksheet).Name;
    }

    protected override bool HasData(object worksheet)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        return excelWorksheet.Dimension != null;
    }

    protected override Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(object worksheet, int headerRowIndex)
    {
        var columnMappings = new Dictionary<int, PropertyInfo>();
        var excelWorksheet = (ExcelWorksheet)worksheet;

        var properties = typeof(T).GetProperties().Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        if (headerRowIndex >= 0)
        {
            int colCount = excelWorksheet.Dimension.End.Column;

            for (int i = 1; i <= colCount; i++)
            {
                string? columnName = excelWorksheet.Cells[headerRowIndex + 1, i].Text?.Trim();
                if (columnName.IsNotNullAndEmpty() && properties.TryGetValue(columnName, out var property))
                {
                    columnMappings[i] = property;
                }
            }
        }

        return columnMappings;
    }

    protected override int GetDataStartRow(object worksheet, int headerRowIndex)
    {
        return headerRowIndex + 2; // EPPlus从1开始计数，加2表示从表头下一行开始
    }

    protected override int GetDataEndRow(object worksheet)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        return excelWorksheet.Dimension.End.Row;
    }

    //protected override bool ProcessRow<T>(object worksheet, int rowNum, Dictionary<int, PropertyInfo> columnMappings, T item)
    //{
    //    var excelWorksheet = (ExcelWorksheet)worksheet;
    //    bool hasData = false;

    //    foreach (var mapping in columnMappings)
    //    {
    //        var cell = excelWorksheet.Cells[rowNum, mapping.Key];
    //        if (cell?.Value != null)
    //        {
    //            var value = GetExcelCellValue(cell);

    //            if (value != DBNull.Value)
    //            {
    //                SetPropertySafely(item, mapping.Value, value);
    //                hasData = true;
    //            }
    //        }
    //    }

    //    return hasData;
    //}

    protected override void CloseWorkbook(object workbook)
    {
        if (workbook is ExcelPackage package)
        {
            try
            {
                package.Dispose();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "关闭EPPlus工作簿时出错");
            }
        }
    }

    protected override int EstimateColumnCount(object worksheet)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        return excelWorksheet.Dimension?.End.Column ?? 0;
    }

    protected override Dictionary<int, string> CreateHeaderMappings(object worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();
        var excelWorksheet = (ExcelWorksheet)worksheet;

        if (headerRowIndex < 0 || excelWorksheet.Dimension == null)
            return result;

        int colCount = excelWorksheet.Dimension.End.Column;

        for (int i = 1; i <= colCount; i++)
        {
            var cell = excelWorksheet.Cells[headerRowIndex + 1, i]; // EPPlus从1开始计数
            string columnName = cell.Text?.Trim() ?? $"Column{i}";
            result[i] = columnName;
        }

        return result;
    }

    protected override object GetCellValue(object worksheet, int rowNum, int colIndex)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        var cell = excelWorksheet.Cells[rowNum, colIndex + 1]; // EPPlus从1开始计数，需要+1

        if (cell?.Value == null)
            return DBNull.Value;

        // 处理日期格式
        bool isDateFormat = cell.Style.Numberformat.Format.Contains("yy");

        // 使用通用转换器
        return GetExcelCellValue(cell.Value, isDateFormat);
    }
    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 获取单元格值并转换为适当类型
    /// </summary>
    private object GetExcelCellValue(ExcelRange cell)
    {
        if (cell.Value == null)
            return DBNull.Value;

        // 处理日期格式
        bool isDateFormat = cell.Style.Numberformat.Format.Contains("yy");

        // 使用通用转换器
        return GetExcelCellValue(cell.Value, isDateFormat);
    }

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
                cell.Style.Numberformat.Format = Options.DefaultDateFormat;
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                cell.Style.Numberformat.Format = DECIMAL_FORMAT;
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                cell.Style.Numberformat.Format = doubleValue % 1 == 0 ? INTEGER_FORMAT : DECIMAL_FORMAT;
                break;
            case float floatValue:
                cell.Value = floatValue;
                cell.Style.Numberformat.Format = floatValue % 1 == 0 ? INTEGER_FORMAT : DECIMAL_FORMAT;
                break;
            case int or long or short or byte or sbyte or ushort or uint or ulong:
                cell.Value = Convert.ToInt64(value);
                cell.Style.Numberformat.Format = INTEGER_FORMAT;
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
        try
        {
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = TITLE_FONT_SIZE;
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // 可以在这里添加更多标题行的样式设置
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "设置标题行样式失败");
        }
    }

    /// <summary>
    /// 设置表头行样式
    /// </summary>
    private void ApplyHeaderRowFormatting(ExcelRange headerCell)
    {
        try
        {
            headerCell.Style.Font.Bold = true;
            headerCell.Style.Font.Size = HEADER_FONT_SIZE;
            headerCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            headerCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerCell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            // 应用边框
            DrawBorder(headerCell);
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "设置表头行样式失败");
        }
    }

    private void ApplyBasicFormatting(ExcelWorksheet worksheet, int rowCount, int columnCount)
    {
        // 设置所有单元格自动适应宽度
        worksheet.Cells.AutoFitColumns();

        // 对标题行进行特殊处理，最小宽度为12
        for (int i = 1; i <= columnCount; i++)
        {
            var column = worksheet.Column(i);
            if (column.Width < 12)
                column.Width = 12;
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
    #endregion

    #region Export

    /// <summary>
    /// 创建空工作簿
    /// </summary>
    protected override object CreateWorkbook()
    {
        return new ExcelPackage();
    }

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected override object CreateWorksheet(object workbook, string sheetName)
    {
        var package = (ExcelPackage)workbook;
        return package.Workbook.Worksheets.Add(sheetName);
    }

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    protected override int ApplyTitle(object worksheet, string title, int columnCount)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        excelWorksheet.Cells[1, 1].Value = title;
        var titleRange = excelWorksheet.Cells[1, 1, 1, columnCount];
        titleRange.Merge = true;
        ApplyTitleRowFormatting(titleRange);
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法 - 处理共通逻辑
    /// </summary>
    protected override void CreateHeaderRowCore(object worksheet, string[] columnNames, int startRowIndex)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
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
    protected override void ProcessDataRows(object worksheet, DataTable dataTable, int startRowIndex)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            logger?.LogDebug("使用并行处理导出 {Count} 行数据", dataTable.Rows.Count);

            // 使用批处理提高性能
            int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;

            // 预计算所有值
            var cellValues = new object?[dataTable.Rows.Count, dataTable.Columns.Count];

            Parallel.For(0, dataTable.Rows.Count, i =>
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    var value = dataTable.Rows[i][j];
                    cellValues[i, j] = value != DBNull.Value ? value : null;
                }
            });

            // 批量写入
            for (int batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, dataTable.Rows.Count);
                for (int i = batchStart; i < batchEnd; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var cell = excelWorksheet.Cells[startRowIndex + i + 2, j + 1];
                        WriteValueToCell(cell, cellValues[i, j]);
                    }
                }
            }
        }
        else
        {
            // 顺序处理小数据集
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    var cell = excelWorksheet.Cells[startRowIndex + i + 2, j + 1];
                    var value = dataTable.Rows[i][j];
                    WriteValueToCell(cell, value != DBNull.Value ? value : null);
                }
            }
        }
    }

    /// <summary>
    /// 处理集合数据行
    /// </summary>
    protected override void ProcessCollectionRows<T>(object worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        
        // 获取有ExcelColumn特性的列，如果没有则使用所有列
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
                    cellValues[i, j] = properties.FirstOrDefault(p => p.Name == columns[j].Name)?.GetValue(list[i]);
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
                        var cell = excelWorksheet.Cells[startRowIndex + i + 2, j + 1];
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
                    var cell = excelWorksheet.Cells[startRowIndex + i + 2, j + 1];
                    var property = properties.FirstOrDefault(p => p.Name == columns[j].Name);
                    WriteValueToCell(cell, property?.GetValue(list[i]));
                }
            }
        }
    }

    /// <summary>
    /// 应用工作表格式化
    /// </summary>
    protected override void ApplyWorksheetFormatting(object worksheet, int rowCount, int columnCount)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        ApplyBasicFormatting(excelWorksheet, rowCount, columnCount);
    }

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected override MemoryStream SaveWorkbookToStream(object workbook)
    {
        var package = (ExcelPackage)workbook;
        var ms = new MemoryStream();
        package.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    #endregion
}