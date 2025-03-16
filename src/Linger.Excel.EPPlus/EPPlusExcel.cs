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

public class EPPlusExcel(ExcelOptions? options = null, ILogger<EPPlusExcel>? logger = null) : ExcelBase<ExcelPackage, ExcelWorksheet>(options, logger)
{
    #region Import
    // 添加基类要求的方法实现
    protected override ExcelPackage OpenWorkbook(Stream stream)
    {
        return new ExcelPackage(stream);
    }

    protected override ExcelWorksheet GetWorksheet(ExcelPackage workbook, string? sheetName)
    {
        var workBook = workbook.Workbook;

        if (workBook.Worksheets.Count == 0)
            return null!;

        if (!string.IsNullOrEmpty(sheetName))
        {
            return workBook.Worksheets[sheetName] ?? workBook.Worksheets[0];
        }

        return workBook.Worksheets[0];
    }

    protected override string GetSheetName(ExcelWorksheet worksheet)
    {
        return worksheet.Name;
    }

    protected override bool HasData(ExcelWorksheet worksheet)
    {
        return worksheet.Dimension != null;
    }

    protected override Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(ExcelWorksheet worksheet, int headerRowIndex)
    {
        var columnMappings = new Dictionary<int, PropertyInfo>();

        var properties = typeof(T).GetProperties().Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        if (headerRowIndex >= 0)
        {
            int colCount = worksheet.Dimension.End.Column;

            for (int i = 1; i <= colCount; i++)
            {
                string? columnName = worksheet.Cells[headerRowIndex + 1, i].Text?.Trim();
                if (columnName.IsNotNullAndEmpty() && properties.TryGetValue(columnName, out var property))
                {
                    columnMappings[i] = property;
                }
            }
        }

        return columnMappings;
    }

    /// <summary>
    /// 获取数据开始行索引
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)，-1表示没有表头行</param>
    /// <returns>数据开始行索引(1-based)</returns>
    /// <remarks>
    /// EPPlus使用1-based索引系统。
    /// 如果headerRowIndex为-1(无表头)，则从第1行开始读取数据。
    /// 否则从表头行的下一行(headerRowIndex+2，因为表头行已转换为1-based)开始读取数据。
    /// </remarks>
    protected override int GetDataStartRow(ExcelWorksheet worksheet, int headerRowIndex)
    {
        // 如果headerRowIndex为负值(无表头)，则从第1行开始(EPPlus从1开始计数)
        if (headerRowIndex < 0)
            return 1;

        // 否则从表头行下一行开始：headerRowIndex(0-based)+1(转为1-based)+1(下一行)
        return headerRowIndex + 2;
    }

    /// <summary>
    /// 获取数据结束行索引
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <returns>数据结束行索引(1-based)</returns>
    /// <remarks>
    /// EPPlus使用1-based索引系统。
    /// Dimension.End.Row返回的是工作表最后一行的行号(已是1-based)。
    /// </remarks>
    protected override int GetDataEndRow(ExcelWorksheet worksheet)
    {
        return worksheet.Dimension.End.Row;
    }

    protected override void CloseWorkbook(ExcelPackage workbook)
    {
        try
        {
            workbook.Dispose();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "关闭EPPlus工作簿时出错");
        }
    }

    protected override int EstimateColumnCount(ExcelWorksheet worksheet)
    {
        return worksheet.Dimension?.End.Column ?? 0;
    }

    /// <summary>
    /// 创建表头映射关系(列索引到列名的映射)
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)，-1表示没有表头行</param>
    /// <returns>列索引到列名的字典(键为0-based索引)</returns>
    /// <remarks>
    /// EPPlus使用1-based索引系统。
    /// 如果headerRowIndex为-1(无表头)或工作表为空，则返回空字典。
    /// 注意：虽然EPPlus使用1-based列索引，但返回的字典键是0-based，以保持API一致性。
    /// </remarks>
    protected override Dictionary<int, string> CreateHeaderMappings(ExcelWorksheet worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();

        // 如果不存在表头行或工作表为空，返回空字典
        if (headerRowIndex < 0 || worksheet.Dimension == null)
            return result;

        int colCount = worksheet.Dimension.End.Column;

        // 重要: 这里i从1开始，因为EPPlus列索引从1开始
        // 但保存到字典时直接使用i-1作为键，确保在GetCellValue中能正确+1
        for (int i = 1; i <= colCount; i++)
        {
            var cell = worksheet.Cells[headerRowIndex + 1, i];
            string columnName;
            if (cell.Text.IsNotNullAndWhiteSpace())
            {
                columnName = cell.Text?.Trim() ?? $"Column{i}";
            }
            else
            {
                columnName = $"Column{i}";
            }

            result[i - 1] = columnName;  // 存储0-based索引作为键
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
        // EPPlus从1开始计数，rowNum应已经是1-based (通过GetDataStartRow调整)
        // 但colIndex仍然是0-based，需要+1
        var cell = worksheet.Cells[rowNum, colIndex + 1];

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
                cell.Style.Numberformat.Format = Options.StyleOptions.DataStyle.DateFormat; // 更新为新路径
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                cell.Style.Numberformat.Format = Options.StyleOptions.DataStyle.DecimalFormat; // 更新为新路径
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                cell.Style.Numberformat.Format = doubleValue % 1 == 0 ? Options.StyleOptions.DataStyle.IntegerFormat : Options.StyleOptions.DataStyle.DecimalFormat; // 更新为新路径
                break;
            case float floatValue:
                cell.Value = floatValue;
                cell.Style.Numberformat.Format = floatValue % 1 == 0 ? Options.StyleOptions.DataStyle.IntegerFormat : Options.StyleOptions.DataStyle.DecimalFormat; // 更新为新路径
                break;
            case int or long or short or byte or sbyte or ushort or uint or ulong:
                cell.Value = Convert.ToInt64(value);
                cell.Style.Numberformat.Format = Options.StyleOptions.DataStyle.IntegerFormat; // 更新为新路径
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
            titleRange.Style.Font.Bold = Options.StyleOptions.TitleStyle.Bold; // 更新为新路径
            titleRange.Style.Font.Size = Options.StyleOptions.TitleStyle.FontSize; // 更新为新路径
            titleRange.Style.Font.Name = Options.StyleOptions.TitleStyle.FontName; // 更新为新路径
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // 设置背景色
            if (!string.IsNullOrEmpty(Options.StyleOptions.TitleStyle.BackgroundColor)) // 更新为新路径
            {
                titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                var color = Color.Parse(Options.StyleOptions.TitleStyle.BackgroundColor); // 更新为新路径
                titleRange.Style.Fill.BackgroundColor.SetColor(color);
            }

            // 设置文字颜色
            if (!string.IsNullOrEmpty(Options.StyleOptions.TitleStyle.FontColor)) // 更新为新路径
            {
                var fontColor = Color.Parse(Options.StyleOptions.TitleStyle.FontColor); // 更新为新路径
                titleRange.Style.Font.Color.SetColor(fontColor);
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
    private void ApplyHeaderRowFormatting(ExcelRange headerCell)
    {
        try
        {
            headerCell.Style.Font.Bold = Options.StyleOptions.HeaderStyle.Bold; // 更新为新路径
            headerCell.Style.Font.Size = Options.StyleOptions.HeaderStyle.FontSize; // 更新为新路径
            headerCell.Style.Font.Name = Options.StyleOptions.HeaderStyle.FontName; // 更新为新路径
            headerCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // 设置背景色
            if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.BackgroundColor)) // 更新为新路径
            {
                headerCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                var color = Color.Parse(Options.StyleOptions.HeaderStyle.BackgroundColor); // 更新为新路径
                headerCell.Style.Fill.BackgroundColor.SetColor(color);
            }

            // 设置文字颜色
            if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.FontColor)) // 更新为新路径
            {
                var fontColor = Color.Parse(Options.StyleOptions.HeaderStyle.FontColor); // 更新为新路径
                headerCell.Style.Font.Color.SetColor(fontColor);
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
    protected override ExcelPackage CreateWorkbook()
    {
        return new ExcelPackage();
    }

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected override ExcelWorksheet CreateWorksheet(ExcelPackage workbook, string sheetName)
    {
        return workbook.Workbook.Worksheets.Add(sheetName);
    }

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    protected override int ApplyTitle(ExcelWorksheet worksheet, string title, int columnCount)
    {
        worksheet.Cells[1, 1].Value = title;
        var titleRange = worksheet.Cells[1, 1, 1, columnCount];
        titleRange.Merge = true;
        ApplyTitleRowFormatting(titleRange);
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法 - 处理共通逻辑
    /// </summary>
    protected override void CreateHeaderRowCore(ExcelWorksheet worksheet, string[] columnNames, int startRowIndex)
    {
        for (int i = 0; i < columnNames.Length; i++)
        {
            var cell = worksheet.Cells[startRowIndex + 1, i + 1];
            cell.Value = columnNames[i];
            ApplyHeaderRowFormatting(cell);
        }
    }

    /// <summary>
    /// 处理数据行
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="dataTable">数据表</param>
    /// <param name="startRowIndex">起始行索引(0-based)</param>
    /// <remarks>
    /// EPPlus使用1-based索引系统。
    /// 行索引需要进行转换：startRowIndex+i+2，其中:
    /// - startRowIndex是表头行索引(0-based)
    /// - +2是因为要跳过表头行，且转换为1-based
    /// 列索引也需要+1转换为EPPlus的1-based索引。
    /// </remarks>
    protected override void ProcessDataRows(ExcelWorksheet worksheet, DataTable dataTable, int startRowIndex)
    {
        var excelWorksheet = worksheet;
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
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="worksheet">工作表</param>
    /// <param name="list">数据列表</param>
    /// <param name="properties">属性数组</param>
    /// <param name="startRowIndex">起始行索引(0-based)</param>
    /// <remarks>
    /// EPPlus使用1-based索引系统。
    /// 行索引需要进行转换：startRowIndex+i+2，其中:
    /// - startRowIndex是表头行索引(0-based)
    /// - +2是因为要跳过表头行，且转换为1-based
    /// 列索引也需要+1转换为EPPlus的1-based索引。
    /// </remarks>
    protected override void ProcessCollectionRows<T>(ExcelWorksheet worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        var excelWorksheet = worksheet;

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
    protected override void ApplyWorksheetFormatting(ExcelWorksheet worksheet, int rowCount, int columnCount)
    {
        // 设置所有单元格自动适应宽度
        if (Options.AutoFitColumns)
        {
            // 设置所有单元格自动适应宽度
            worksheet.Cells.AutoFitColumns();

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
            worksheet.Cells[1, 1, rowCount, columnCount].Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }
    }

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected override MemoryStream SaveWorkbookToStream(ExcelPackage workbook)
    {
        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    #endregion
}