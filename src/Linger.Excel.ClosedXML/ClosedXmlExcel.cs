using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Attributes;
using Linger.Extensions.Core;
using Microsoft.Extensions.Logging;

namespace Linger.Excel.ClosedXML;

public class ClosedXmlExcel(ExcelOptions? options = null, ILogger<ClosedXmlExcel>? logger = null) : ExcelBase(options, logger)
{
    // /// <summary>
    // /// 将对象集合转换为MemoryStream
    // /// </summary>
    // //[return: NotNullIfNotNull(nameof(list))]
    // public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T>? list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    // {
    //     if (list == null)
    //     {
    //         logger?.LogWarning("要转换的集合为空");
    //         return null;
    //     }

    //     return ExecuteSafely(() =>
    //     {
    //         using var workbook = new XLWorkbook();
    //         var worksheet = workbook.Worksheets.Add(sheetsName);

    //         // 获取可读属性
    //         var properties = typeof(T).GetProperties()
    //             .Where(p => p.CanRead)
    //             .ToArray();

    //         if (properties.Length == 0)
    //         {
    //             logger?.LogWarning("类型 {Type} 没有可读属性", typeof(T).Name);
    //             return new MemoryStream();
    //         }

    //         // 处理标题
    //         var currentRow = 1;
    //         if (!string.IsNullOrEmpty(title))
    //         {
    //             var cell = worksheet.Cell(currentRow, 1);
    //             cell.Value = title;
    //             var titleRange = worksheet.Range(1, 1, 1, properties.Length);
    //             titleRange.Merge();
    //             ApplyTitleRowFormatting(titleRange);
    //             currentRow++;
    //         }

    //         // 设置表头
    //         for (var i = 0; i < properties.Length; i++)
    //         {
    //             worksheet.Cell(currentRow, i + 1).Value = properties[i].Name;
    //             worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
    //         }

    //         // 使用基类的批处理方法处理数据行
    //         ProcessInBatches<IXLCell, object?>(
    //             list.Count,
    //             i => worksheet.Cell(i + currentRow + 1, 1),
    //             i => properties.Select(p => p.GetValue(list[i])).ToArray(),
    //             (cell, rowIndex, values, _) =>
    //             {
    //                 for (var j = 0; j < properties.Length; j++)
    //                 {
    //                     var currentCell = worksheet.Cell(rowIndex + currentRow + 1, j + 1);
    //                     WriteValueToCell(currentCell, values[j]);
    //                 }
    //             }
    //         );

    //         // 执行自定义操作
    //         action?.Invoke(worksheet, properties);

    //         // 根据配置应用格式化
    //         if (Options.AutoFitColumns)
    //         {
    //             ApplyBasicFormatting(worksheet, list.Count + currentRow, properties.Length);
    //         }

    //         var ms = new MemoryStream();
    //         workbook.SaveAs(ms);
    //         ms.Position = 0;
    //         return ms;
    //     }, "集合导出到Excel");
    // }

    // /// <summary>
    // /// 将DataTable转换为MemoryStream
    // /// </summary>
    // public override MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    // {
    //     if (dataTable == null || dataTable.Columns.Count == 0)
    //     {
    //         logger?.LogWarning("要转换的DataTable为空或没有列");
    //         return null;
    //     }

    //     return ExecuteSafely(() =>
    //     {
    //         using var workbook = new XLWorkbook();
    //         var worksheet = workbook.Worksheets.Add(sheetsName);

    //         // 处理标题
    //         var currentRow = 1;
    //         if (!string.IsNullOrEmpty(title))
    //         {
    //             var cell = worksheet.Cell(currentRow, 1);
    //             cell.Value = title;
    //             var titleRange = worksheet.Range(1, 1, 1, dataTable.Columns.Count);
    //             titleRange.Merge();
    //             ApplyTitleRowFormatting(titleRange);
    //             currentRow++;
    //         }

    //         // 设置表头
    //         for (var i = 0; i < dataTable.Columns.Count; i++)
    //         {
    //             var cell = worksheet.Cell(currentRow, i + 1);
    //             cell.Value = dataTable.Columns[i].ColumnName;
    //             ApplyHeaderRowFormatting(cell);
    //         }

    //         // 判断是否需要并行处理
    //         bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

    //         if (useParallelProcessing)
    //         {
    //             logger?.LogDebug("使用并行处理导出 {Count} 行数据", dataTable.Rows.Count);

    //             // 使用批处理提高性能
    //             int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;

    //             // 预先计算所有值以避免在多线程中重复计算
    //             var cellValues = new object[dataTable.Rows.Count, dataTable.Columns.Count];

    //             Parallel.For(0, dataTable.Rows.Count, i =>
    //             {
    //                 for (var j = 0; j < dataTable.Columns.Count; j++)
    //                 {
    //                     cellValues[i, j] = dataTable.Rows[i][j];
    //                 }
    //             });

    //             // 批量写入
    //             for (int batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
    //             {
    //                 int batchEnd = Math.Min(batchStart + batchSize, dataTable.Rows.Count);

    //                 for (int i = batchStart; i < batchEnd; i++)
    //                 {
    //                     for (var j = 0; j < dataTable.Columns.Count; j++)
    //                     {
    //                         var cell = worksheet.Cell(i + currentRow + 1, j + 1);
    //                         WriteValueToCell(cell, cellValues[i, j]);
    //                     }
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             // 小数据集顺序处理
    //             for (var i = 0; i < dataTable.Rows.Count; i++)
    //             {
    //                 for (var j = 0; j < dataTable.Columns.Count; j++)
    //                 {
    //                     var cell = worksheet.Cell(i + currentRow + 1, j + 1);
    //                     WriteValueToCell(cell, dataTable.Rows[i][j]);
    //                 }
    //             }
    //         }

    //         // 执行自定义操作
    //         action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

    //         // 根据配置应用格式化
    //         if (Options.AutoFitColumns)
    //         {
    //             ApplyBasicFormatting(worksheet, dataTable.Rows.Count + currentRow, dataTable.Columns.Count);
    //         }

    //         var ms = new MemoryStream();
    //         workbook.SaveAs(ms);
    //         ms.Position = 0;
    //         return ms;
    //     }, "DataTable导出到Excel");
    // }

    // 添加基类要求的方法实现
    protected override object OpenWorkbook(Stream stream)
    {
        var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return new XLWorkbook(memoryStream);
    }

    protected override object GetWorksheet(object workbook, string? sheetName)
    {
        var xlWorkbook = (XLWorkbook)workbook;

        if (sheetName.IsNullOrEmpty())
        {
            return xlWorkbook.Worksheet(1);
        }
        else if (xlWorkbook.Worksheets.TryGetWorksheet(sheetName, out var namedSheet))
        {
            return namedSheet;
        }
        else
        {
            return xlWorkbook.Worksheet(1);
        }
    }

    protected override string GetSheetName(object worksheet)
    {
        return ((IXLWorksheet)worksheet).Name;
    }

    protected override bool HasData(object worksheet)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        return xlWorksheet.RangeUsed() != null;
    }

    protected override Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(object worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, PropertyInfo>();
        var xlWorksheet = (IXLWorksheet)worksheet;

        if (headerRowIndex < 0) return result;

        var headerRow = xlWorksheet.Row(headerRowIndex + 1);
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

    protected override int GetDataStartRow(object worksheet, int headerRowIndex)
    {
        return headerRowIndex < 0 ? 1 : headerRowIndex + 2;
    }

    protected override int GetDataEndRow(object worksheet)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        var usedRange = xlWorksheet.RangeUsed();
        return usedRange?.LastRow()?.RowNumber() ?? 0;
    }

    protected override bool ProcessRow<T>(object worksheet, int rowNum, Dictionary<int, PropertyInfo> columnMappings, T item)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        var row = xlWorksheet.Row(rowNum);
        bool hasData = false;

        foreach (var cell in row.CellsUsed())
        {
            int colIndex = cell.Address.ColumnNumber;
            if (columnMappings.TryGetValue(colIndex, out var property))
            {
                var value = GetExcelCellValue(cell);

                if (value != DBNull.Value)
                {
                    SetPropertySafely(item, property, value);
                    hasData = true;
                }
            }
        }

        return hasData;
    }

    protected override void CloseWorkbook(object workbook)
    {
        if (workbook is XLWorkbook xlWorkbook)
        {
            try
            {
                xlWorkbook.Dispose();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "关闭ClosedXML工作簿时出错");
            }
        }
    }

    protected override int EstimateColumnCount(object worksheet)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        var usedRange = xlWorksheet.RangeUsed();
        return usedRange?.LastColumn()?.ColumnNumber() ?? 0;
    }

    protected override Dictionary<int, string> CreateHeaderMappings(object worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();
        var xlWorksheet = (IXLWorksheet)worksheet;

        if (headerRowIndex < 0)
            return result;

        var headerRow = xlWorksheet.Row(headerRowIndex + 1); // ClosedXML从1开始计数

        foreach (var cell in headerRow.CellsUsed())
        {
            int colIndex = cell.Address.ColumnNumber;
            string columnName = cell.Value.ToString() ?? $"Column{colIndex}";
            result[colIndex] = columnName;
        }

        return result;
    }

    protected override object GetCellValue(object worksheet, int rowNum, int colIndex)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;

        // ClosedXML从1开始计数
        var row = xlWorksheet.Row(rowNum);
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
                cell.Style.DateFormat.Format = Options.DefaultDateFormat;
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                cell.Style.NumberFormat.Format = DECIMAL_FORMAT;
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                cell.Style.NumberFormat.Format = doubleValue % 1 == 0 ? INTEGER_FORMAT : DECIMAL_FORMAT;
                break;
            case float floatValue:
                cell.Value = floatValue;
                cell.Style.NumberFormat.Format = floatValue % 1 == 0 ? INTEGER_FORMAT : DECIMAL_FORMAT;
                break;
            case int or long or short or byte or sbyte or ushort or uint or ulong:
                cell.Value = Convert.ToInt64(value);
                cell.Style.NumberFormat.Format = INTEGER_FORMAT;
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
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = TITLE_FONT_SIZE;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

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
    private void ApplyHeaderRowFormatting(IXLCell headerCell)
    {
        try
        {
            headerCell.Style.Font.Bold = true;
            headerCell.Style.Font.FontSize = HEADER_FONT_SIZE;
            headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerCell.Style.Fill.BackgroundColor = XLColor.LightGray;

            // 应用边框
            DrawBorder(headerCell);
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "设置表头行样式失败");
        }
    }

    private void ApplyBasicFormatting(IXLWorksheet worksheet, int rowCount, int columnCount)
    {
        // 设置所有单元格自动适应宽度
        worksheet.Columns().AdjustToContents();

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
            worksheet.Range(1, 1, rowCount, columnCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(1, 1, rowCount, columnCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
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
    protected override object CreateWorkbook()
    {
        return new XLWorkbook();
    }

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected override object CreateWorksheet(object workbook, string sheetName)
    {
        var xlWorkbook = (XLWorkbook)workbook;
        return xlWorkbook.Worksheets.Add(sheetName);
    }

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    protected override int ApplyTitle(object worksheet, string title, int columnCount)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        var cell = xlWorksheet.Cell(1, 1);
        cell.Value = title;
        var titleRange = xlWorksheet.Range(1, 1, 1, columnCount);
        titleRange.Merge();
        ApplyTitleRowFormatting(titleRange);
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法 - 处理共通逻辑
    /// </summary>
    protected void CreateHeaderRowCore(IXLWorksheet worksheet, string[] columnNames, int startRowIndex)
    {
        for (int i = 0; i < columnNames.Length; i++)
        {
            var cell = worksheet.Cell(startRowIndex + 1, i + 1);
            cell.Value = columnNames[i];
            ApplyHeaderRowFormatting(cell);
        }
    }

    /// <summary>
    /// 创建标题行
    /// </summary>
    protected override void CreateHeaderRow(object worksheet, DataColumnCollection columns, int startRowIndex)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        string[] columnNames = new string[columns.Count];
        
        for (int i = 0; i < columns.Count; i++)
        {
            columnNames[i] = columns[i].ColumnName;
        }
        
        CreateHeaderRowCore(xlWorksheet, columnNames, startRowIndex);
    }

    /// <summary>
    /// 创建集合标题行
    /// </summary>
    protected override void CreateCollectionHeaderRow(object worksheet, PropertyInfo[] properties, int startRowIndex)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        
        // 获取有ExcelColumn特性的列，如果没有则使用所有列
        var columns = GetExcelColumns(properties);
        if (columns.Count == 0)
        {
            string[] columnNames = properties.Select(p => p.Name).ToArray();
            CreateHeaderRowCore(xlWorksheet, columnNames, startRowIndex);
        }
        else
        {
            // 使用特性标记的属性及其顺序
            columns = columns.OrderBy(c => c.Item3).ToList();
            string[] columnNames = columns.Select(c => 
                string.IsNullOrEmpty(c.Item2) ? c.Item1 : c.Item2).ToArray();
            CreateHeaderRowCore(xlWorksheet, columnNames, startRowIndex);
        }
    }

    /// <summary>
    /// 处理数据行
    /// </summary>
    protected override void ProcessDataRows(object worksheet, DataTable dataTable, int startRowIndex)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
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
                        var cell = xlWorksheet.Cell(startRowIndex + i + 2, j + 1);
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
                    var cell = xlWorksheet.Cell(startRowIndex + i + 2, j + 1);
                    WriteValueToCell(cell, dataTable.Rows[i][j]);
                }
            }
        }
    }

    /// <summary>
    /// 应用工作表格式化
    /// </summary>
    protected override void ApplyWorksheetFormatting(object worksheet, int rowCount, int columnCount)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        ApplyBasicFormatting(xlWorksheet, rowCount, columnCount);
    }

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected override MemoryStream SaveWorkbookToStream(object workbook)
    {
        var xlWorkbook = (XLWorkbook)workbook;
        var ms = new MemoryStream();
        xlWorkbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    // /// <summary>
    // /// 内部实现：将对象列表转换为MemoryStream
    // /// </summary>
    // protected override MemoryStream InternalConvertCollectionToMemoryStream<T>(
    //     List<T> list,
    //     string sheetsName,
    //     string title,
    //     Action<object, PropertyInfo[]>? action)
    // {
    //     // 默认调用工作流模板方法
    //     return ExecuteCollectionExportWorkflow(list, sheetsName, title, action);
    // }

    /// <summary>
    /// 处理集合数据行
    /// </summary>
    protected override void ProcessCollectionRows<T>(object worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        var xlWorksheet = (IXLWorksheet)worksheet;
        
        // 优先查找带有ExcelColumn特性的属性
        var columns = GetExcelColumns(properties);
        if (columns.Count == 0)
        {
            columns = properties.Select((p, i) => new Tuple<string, string, int>(
                p.Name, p.Name, i)).ToList();
        }
        columns = columns.OrderBy(c => c.Item3).ToList();
        
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
                    var property = properties.FirstOrDefault(p => p.Name == columns[j].Item1);
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
                        var cell = xlWorksheet.Cell(startRowIndex + i + 2, j + 1);
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
                    var cell = xlWorksheet.Cell(startRowIndex + i + 2, j + 1);
                    var property = properties.FirstOrDefault(p => p.Name == columns[j].Item1);
                    WriteValueToCell(cell, property?.GetValue(list[i]));
                }
            }
        }
    }

    /// <summary>
    /// 获取标记有ExcelColumn特性的属性信息
    /// </summary>
    private List<Tuple<string, string, int>> GetExcelColumns(IEnumerable<PropertyInfo> properties)
    {
        var columns = new List<Tuple<string, string, int>>();
        Type excelColumnAttributeType = typeof(ExcelColumnAttribute);
        
        foreach (var prop in properties)
        {
            var attrs = prop.GetCustomAttributesData();
            if (attrs.Any(a => a.AttributeType == excelColumnAttributeType))
            {
                var attr = prop.GetCustomAttributes(excelColumnAttributeType, true)
                    .FirstOrDefault() as ExcelColumnAttribute;
                    
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
}
