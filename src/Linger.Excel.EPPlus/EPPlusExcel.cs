using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Attributes;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;

namespace Linger.Excel.EPPlus;

public class EPPlusExcel(ExcelOptions? options = null, ILogger<EPPlusExcel>? logger = null) : ExcelBase(options, logger)
{
    /// <summary>
    /// 将对象集合转换为内存流
    /// </summary>
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = "Sheet1",
        string title = "",
        Action<object, PropertyInfo[]>? action = null)
    {
        if (list == null || list.Count == 0)
        {
            logger?.LogWarning("要转换的集合为空");
            return null;
        }

        return ExecuteSafely(() =>
        {
            var memoryStream = new MemoryStream();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetsName);

            // 获取属性信息
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .ToArray();

            if (properties.Length == 0)
            {
                logger?.LogWarning("类型 {Type} 没有可读属性", typeof(T).Name);
                return new MemoryStream();
            }

            // 获取有ExcelColumn特性的列，如果没有则使用所有列
            var columns = GetExcelColumns(properties);
            if (columns.Count == 0)
            {
                columns = properties.Select((p, i) => new Tuple<string, string, int>(
                    p.Name, p.Name, i)).ToList();
            }
            columns = columns.OrderBy(a => a.Item3).ToList();

            // 处理标题
            var titleIndex = 0;
            if (!string.IsNullOrEmpty(title))
            {
                titleIndex = 1;
                worksheet.Cells[1, 1].Value = title;
                var titleRange = worksheet.Cells[1, 1, 1, columns.Count];
                titleRange.Merge = true;
                ApplyTitleRowFormatting(titleRange);
            }

            // 填充列头
            for (int i = 0; i < columns.Count; i++)
            {
                var cell = worksheet.Cells[titleIndex + 1, i + 1];
                cell.Value = string.IsNullOrEmpty(columns[i].Item2) ? columns[i].Item1 : columns[i].Item2;
                ApplyHeaderRowFormatting(cell);
            }

            // 判断是否需要并行处理
            bool useParallelProcessing = list.Count > Options.ParallelProcessingThreshold;

            if (useParallelProcessing)
            {
                logger?.LogDebug("使用并行处理导出 {Count} 条记录", list.Count);

                // 使用批处理提高性能
                int batchSize = Options.UseBatchWrite ? Options.BatchSize : list.Count;

                // 预计算所有值
                var cellValues = new object[list.Count, columns.Count];

                Parallel.For(0, list.Count, rowIndex =>
                {
                    for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                    {
                        var property = properties.FirstOrDefault(p => p.Name == columns[colIndex].Item1);
                        if (property != null)
                        {
                            var value = property.GetValue(list[rowIndex]);

                            // 处理特殊类型
                            if (value != null)
                            {
                                if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                                {
                                    var dateValue = value is DateTime dateTime ? dateTime : value.ToString().ToDateTime();
                                    if (dateValue != DateTime.MinValue)
                                    {
                                        cellValues[rowIndex, colIndex] = dateValue;
                                    }
                                    else
                                    {
                                        cellValues[rowIndex, colIndex] = DBNull.Value;
                                    }
                                }
                                else
                                {
                                    cellValues[rowIndex, colIndex] = value;
                                }
                            }
                            else
                            {
                                cellValues[rowIndex, colIndex] = DBNull.Value;
                            }
                        }
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
                            var cell = worksheet.Cells[titleIndex + i + 2, j + 1];
                            WriteValueToCell(cell, cellValues[i, j]);
                        }
                    }
                }
            }
            else
            {
                // 小数据集顺序处理
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < columns.Count; j++)
                    {
                        var cell = worksheet.Cells[titleIndex + i + 2, j + 1];
                        var property = properties.FirstOrDefault(p => p.Name == columns[j].Item1);

                        if (property != null)
                        {
                            var value = property.GetValue(list[i]);
                            WriteValueToCell(cell, value);
                        }
                        else
                        {
                            DrawBorder(cell);
                        }
                    }
                }
            }

            // 执行自定义操作
            action?.Invoke(worksheet, properties);

            // 根据配置决定是否自动调整列宽
            if (Options.AutoFitColumns)
            {
                //worksheet.Cells[titleIndex + 1, 1, titleIndex + list.Count + 1, columns.Count].AutoFitColumns();
                ApplyBasicFormatting(worksheet, titleIndex + list.Count + 1, columns.Count);
            }

            package.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }, "集合导出到Excel");
    }

    /// <summary>
    /// 将DataTable转换为内存流
    /// </summary>
    public override MemoryStream? ConvertDataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = "Sheet1",
        string title = "",
        Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable == null || dataTable.Columns.Count == 0)
        {
            logger?.LogWarning("要转换的DataTable为空或没有列");
            return null;
        }

        return ExecuteSafely(() =>
        {
            var memoryStream = new MemoryStream();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetsName);

            // 处理标题
            var titleIndex = 0;
            if (!string.IsNullOrEmpty(title))
            {
                titleIndex = 1;
                worksheet.Cells[1, 1].Value = title;
                var titleRange = worksheet.Cells[1, 1, 1, dataTable.Columns.Count];
                titleRange.Merge = true;
                ApplyTitleRowFormatting(titleRange);
            }

            // 填充列头
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                var cell = worksheet.Cells[titleIndex + 1, i + 1];
                cell.Value = dataTable.Columns[i].ColumnName;
                ApplyHeaderRowFormatting(cell);
            }

            // 判断是否需要并行处理
            bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

            if (useParallelProcessing)
            {
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
                            var cell = worksheet.Cells[titleIndex + i + 2, j + 1];
                            WriteValueToCell(cell, cellValues[i, j]);
                        }
                    }
                }
            }
            else
            {
                // 小数据集顺序处理
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var cell = worksheet.Cells[titleIndex + i + 2, j + 1];
                        var value = dataTable.Rows[i][j];
                        WriteValueToCell(cell, value != DBNull.Value ? value : null);
                    }
                }
            }

            // 执行自定义操作
            action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

            // 根据配置决定是否自动调整列宽
            if (Options.AutoFitColumns)
            {
                ApplyBasicFormatting(worksheet, titleIndex + dataTable.Rows.Count + 1, dataTable.Columns.Count);
            }

            package.SaveAs(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }, "DataTable导出到Excel");
    }

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

    protected override bool ProcessRow<T>(object worksheet, int rowNum, Dictionary<int, PropertyInfo> columnMappings, T item)
    {
        var excelWorksheet = (ExcelWorksheet)worksheet;
        bool hasData = false;

        foreach (var mapping in columnMappings)
        {
            var cell = excelWorksheet.Cells[rowNum, mapping.Key];
            if (cell?.Value != null)
            {
                var value = GetExcelCellValue(cell);

                if (value != DBNull.Value)
                {
                    SetPropertySafely(item, mapping.Value, value);
                    hasData = true;
                }
            }
        }

        return hasData;
    }

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
    /// 获取Excel列名
    /// </summary>
    private List<Tuple<string, string, int>> GetExcelColumns(IEnumerable<PropertyInfo> properties)
    {
        var columns = new List<Tuple<string, string, int>>();
        Type excelColumnAttributeType = typeof(ExcelColumnAttribute);

        foreach (PropertyInfo prop in properties)
        {
            var attrs = prop.GetCustomAttributesData();

            if (attrs.Any(a => a.AttributeType == excelColumnAttributeType))
            {
                var attr = prop.GetCustomAttributes(excelColumnAttributeType, true).FirstOrDefault() as ExcelColumnAttribute;
                if (attr != null)
                {
                    columns.Add(new Tuple<string, string, int>(
                        prop.Name,
                        attr.ColumnName.IsNullOrEmpty() ? prop.Name : attr.ColumnName,
                        attr.Index == int.MaxValue ? columns.Count : attr.Index));
                }
            }
        }

        return columns;
    }

    ///// <summary>
    ///// 绘制单元格边框
    ///// </summary>
    //private static void DrawBorder(ExcelStyle style, ExcelBorderStyle borderStyle = ExcelBorderStyle.Thin)
    //{
    //    style.Border.Top.Style = borderStyle;
    //    style.Border.Bottom.Style = borderStyle;
    //    style.Border.Left.Style = borderStyle;
    //    style.Border.Right.Style = borderStyle;
    //}

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
}