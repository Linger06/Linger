using System.Data;
using System.Reflection;
using System.Text;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Attributes;
using Linger.Extensions.Core;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

public class NpoiExcel(ExcelOptions? options = null, ILogger<NpoiExcel>? logger = null) : ExcelBase(options, logger)
{
    // /// <summary>
    // /// 将对象集合转换为MemoryStream
    // /// </summary>
    // public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    // {
    //     if (list == null || list.Count == 0)
    //     {
    //         logger?.LogWarning("要转换的集合为空");
    //         return null;
    //     }

    //     return ExecuteSafely(() =>
    //     {
    //         var workbook = new XSSFWorkbook();
    //         var sheet = workbook.CreateSheet(sheetsName);

    //         // 获取属性信息
    //         var properties = typeof(T).GetProperties()
    //             .Where(p => p.CanRead)
    //             .ToArray();

    //         if (properties.Length == 0)
    //         {
    //             logger?.LogWarning("类型 {Type} 没有可读属性", typeof(T).Name);
    //             return new MemoryStream();
    //         }

    //         // 设置标题样式
    //         var titleIndex = 0;
    //         if (!string.IsNullOrEmpty(title))
    //         {
    //             var titleRow = sheet.CreateRow(titleIndex++);
    //             titleRow.HeightInPoints = 25;
    //             var titleCell = titleRow.CreateCell(0);
    //             titleCell.SetCellValue(title);

    //             ApplyTitleRowFormatting(titleCell);

    //             sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, properties.Length - 1));
    //         }

    //         // 创建表头
    //         var headerRow = sheet.CreateRow(titleIndex++);

    //         // 预创建样式字典，提高性能
    //         var styleCache = new Dictionary<Type, ICellStyle>();

    //         // 创建日期样式
    //         var dateStyle = workbook.CreateCellStyle();
    //         var format = workbook.CreateDataFormat();
    //         dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
    //         styleCache[typeof(DateTime)] = dateStyle;

    //         // 设置列标题并计算列宽
    //         var columnWidths = new int[properties.Length];
    //         for (var i = 0; i < properties.Length; i++)
    //         {
    //             var cell = headerRow.CreateCell(i);
    //             cell.SetCellValue(properties[i].Name);
    //             ApplyHeaderRowFormatting(cell);
    //             columnWidths[i] = Encoding.UTF8.GetBytes(properties[i].Name).Length;
    //         }

    //         // 使用基类的批处理方法处理数据行
    //         ProcessInBatches<IRow, object?>(
    //             list.Count,
    //             i => sheet.CreateRow(i + titleIndex),
    //             i =>
    //             {
    //                 var values = properties.Select(p => p.GetValue(list[i])).ToArray();
    //                 // 计算列宽
    //                 for (int j = 0; j < properties.Length; j++)
    //                 {
    //                     var value = values[j];
    //                     if (value != null)
    //                     {
    //                         var strValue = value.ToString() ?? string.Empty;
    //                         var length = Encoding.UTF8.GetBytes(strValue).Length;
    //                         lock (columnWidths)
    //                         {
    //                             columnWidths[j] = Math.Max(columnWidths[j], length);
    //                         }
    //                     }
    //                 }
    //                 return values;
    //             },
    //             (row, _, values, param) =>
    //             {
    //                 for (int j = 0; j < properties.Length; j++)
    //                 {
    //                     WriteValueToCell(workbook, row, j, values[j], properties[j].PropertyType, styleCache);
    //                 }
    //             }
    //         );

    //         // 设置列宽
    //         if (Options.AutoFitColumns)
    //         {
    //             for (int i = 0; i < properties.Length; i++)
    //             {
    //                 int width = Math.Min(255, columnWidths[i] + 2) * 256;
    //                 sheet.SetColumnWidth(i, width);
    //             }
    //         }

    //         // 执行自定义操作
    //         action?.Invoke(sheet, properties);

    //         var ms = new MemoryStream();
    //         workbook.Write(ms, true);
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
    //         var workbook = new XSSFWorkbook();
    //         var sheet = workbook.CreateSheet(sheetsName);

    //         // 设置标题样式
    //         var titleIndex = 0;
    //         if (!string.IsNullOrEmpty(title))
    //         {
    //             var titleRow = sheet.CreateRow(titleIndex++);
    //             titleRow.HeightInPoints = 25;
    //             var titleCell = titleRow.CreateCell(0);
    //             titleCell.SetCellValue(title);

    //             //var titleStyle = workbook.CreateCellStyle();
    //             //titleStyle.Alignment = HorizontalAlignment.Center;
    //             //var titleFont = workbook.CreateFont();
    //             //titleFont.FontHeightInPoints = 14;
    //             //titleFont.IsBold = true;
    //             //titleStyle.SetFont(titleFont);
    //             //titleCell.CellStyle = titleStyle;

    //             sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dataTable.Columns.Count - 1));

    //             ApplyTitleRowFormatting(titleCell);
    //         }

    //         // 创建表头
    //         var headerRow = sheet.CreateRow(titleIndex++);
    //         //var headerStyle = workbook.CreateCellStyle();
    //         //headerStyle.Alignment = HorizontalAlignment.Center;
    //         //var headerFont = workbook.CreateFont();
    //         //headerFont.FontHeightInPoints = 10;
    //         //headerFont.IsBold = true;
    //         //headerStyle.SetFont(headerFont);

    //         // 预创建样式字典，提高性能
    //         var styleCache = new Dictionary<Type, ICellStyle>();

    //         // 创建日期样式
    //         var dateStyle = workbook.CreateCellStyle();
    //         var format = workbook.CreateDataFormat();
    //         dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
    //         styleCache[typeof(DateTime)] = dateStyle;

    //         // 设置列标题并计算列宽
    //         var columnWidths = new int[dataTable.Columns.Count];
    //         for (var i = 0; i < dataTable.Columns.Count; i++)
    //         {
    //             var cell = headerRow.CreateCell(i);
    //             cell.SetCellValue(dataTable.Columns[i].ColumnName);
    //             ApplyHeaderRowFormatting(cell);

    //             columnWidths[i] = Encoding.UTF8.GetBytes(dataTable.Columns[i].ColumnName).Length;
    //         }

    //         // 判断是否使用并行处理
    //         bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

    //         if (useParallelProcessing)
    //         {
    //             // 并行处理大数据集
    //             var cellValues = new object[dataTable.Rows.Count, dataTable.Columns.Count];
    //             var columnTypes = new Type[dataTable.Columns.Count];

    //             // 获取列类型
    //             for (int i = 0; i < dataTable.Columns.Count; i++)
    //             {
    //                 columnTypes[i] = dataTable.Columns[i].DataType;
    //             }

    //             // 并行填充数据
    //             Parallel.For(0, dataTable.Rows.Count, i =>
    //             {
    //                 for (int j = 0; j < dataTable.Columns.Count; j++)
    //                 {
    //                     cellValues[i, j] = dataTable.Rows[i][j];

    //                     // 计算列宽
    //                     var value = dataTable.Rows[i][j];
    //                     if (value != DBNull.Value)
    //                     {
    //                         var strValue = value.ToString() ?? string.Empty;
    //                         var length = Encoding.UTF8.GetBytes(strValue).Length;
    //                         lock (columnWidths)
    //                         {
    //                             columnWidths[j] = Math.Max(columnWidths[j], length);
    //                         }
    //                     }
    //                 }
    //             });

    //             // 批量写入
    //             int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;
    //             for (int batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
    //             {
    //                 int batchEnd = Math.Min(batchStart + batchSize, dataTable.Rows.Count);
    //                 for (int i = batchStart; i < batchEnd; i++)
    //                 {
    //                     var dataRow = sheet.CreateRow(i + titleIndex);
    //                     for (int j = 0; j < dataTable.Columns.Count; j++)
    //                     {
    //                         var value = cellValues[i, j];
    //                         WriteValueToCell(workbook, dataRow, j, value, columnTypes[j], styleCache);
    //                     }
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             // 顺序处理小数据集
    //             for (int i = 0; i < dataTable.Rows.Count; i++)
    //             {
    //                 var dataRow = sheet.CreateRow(i + titleIndex);
    //                 for (int j = 0; j < dataTable.Columns.Count; j++)
    //                 {
    //                     var value = dataTable.Rows[i][j];
    //                     WriteValueToCell(workbook, dataRow, j, value, dataTable.Columns[j].DataType, styleCache);

    //                     // 计算列宽
    //                     if (value != DBNull.Value)
    //                     {
    //                         var strValue = value.ToString() ?? string.Empty;
    //                         var length = Encoding.UTF8.GetBytes(strValue).Length;
    //                         columnWidths[j] = Math.Max(columnWidths[j], length);
    //                     }
    //                 }
    //             }
    //         }

    //         //// 设置列宽
    //         //if (Options.AutoFitColumns)
    //         //{
    //         //    for (int i = 0; i < dataTable.Columns.Count; i++)
    //         //    {
    //         //        int width = Math.Min(255, columnWidths[i] + 2) * 256;
    //         //        sheet.SetColumnWidth(i, width);
    //         //    }
    //         //}

    //         // 执行自定义操作
    //         action?.Invoke(sheet, dataTable.Columns, dataTable.Rows);

    //         // 根据配置应用格式化
    //         if (Options.AutoFitColumns)
    //         {
    //             ApplyBasicFormatting(sheet, titleIndex + dataTable.Rows.Count + 1, dataTable.Columns.Count);
    //         }

    //         var ms = new MemoryStream();
    //         workbook.Write(ms, true);
    //         ms.Position = 0;

    //         return ms;
    //     }, "DataTable导出到Excel");
    // }

    // 添加基类要求的方法实现
    protected override object OpenWorkbook(Stream stream)
    {
        var bufferedStream = new BufferedStream(stream);
        try
        {
            return WorkbookFactory.Create(bufferedStream);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "无法创建工作簿");
            return null!;
        }
    }

    protected override object GetWorksheet(object workbook, string? sheetName)
    {
        var workBook = (IWorkbook)workbook;

        if (string.IsNullOrWhiteSpace(sheetName))
        {
            return workBook.GetSheetAt(0);
        }
        else
        {
            return workBook.GetSheet(sheetName) ?? workBook.GetSheetAt(0);
        }
    }

    protected override string GetSheetName(object worksheet)
    {
        return ((ISheet)worksheet).SheetName;
    }

    protected override bool HasData(object worksheet)
    {
        var sheet = (ISheet)worksheet;
        return sheet.LastRowNum > 0;
    }

    protected override Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(object worksheet, int headerRowIndex)
    {
        var propertyMap = new Dictionary<int, PropertyInfo>();
        var sheet = (ISheet)worksheet;
        var headerRow = sheet.GetRow(headerRowIndex);
        if (headerRow == null) return propertyMap;

        var properties = typeof(T).GetProperties()
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
        {
            var cell = headerRow.GetCell(i);
            if (cell == null) continue;

            var columnName = cell.ToString();
            if (string.IsNullOrEmpty(columnName)) continue;

            if (properties.TryGetValue(columnName, out var property))
            {
                propertyMap[i] = property;
            }
        }

        return propertyMap;
    }

    protected override int GetDataStartRow(object worksheet, int headerRowIndex)
    {
        return headerRowIndex + 1;
    }

    protected override int GetDataEndRow(object worksheet)
    {
        var sheet = (ISheet)worksheet;
        return sheet.LastRowNum;
    }

    protected override bool ProcessRow<T>(object worksheet, int rowNum, Dictionary<int, PropertyInfo> columnMappings, T item)
    {
        var sheet = (ISheet)worksheet;
        var row = sheet.GetRow(rowNum);
        if (row == null) return false;

        bool hasData = false;

        foreach (var mapping in columnMappings)
        {
            int colIndex = mapping.Key;
            if (colIndex >= row.LastCellNum) continue;

            var cell = row.GetCell(colIndex);
            if (cell != null)
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
        if (workbook is IWorkbook npoiWorkbook)
        {
            try
            {
                npoiWorkbook.Close();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "关闭NPOI工作簿时出错");
            }
        }
    }

    protected override int EstimateColumnCount(object worksheet)
    {
        var sheet = (ISheet)worksheet;
        int maxCellCount = 0;

        // 扫描所有行找到最大列数
        for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row != null && row.LastCellNum > maxCellCount)
            {
                maxCellCount = row.LastCellNum;
            }
        }

        return maxCellCount;
    }

    protected override Dictionary<int, string> CreateHeaderMappings(object worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();
        var sheet = (ISheet)worksheet;
        var headerRow = sheet.GetRow(headerRowIndex);

        if (headerRow == null) return result;

        for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
        {
            var cell = headerRow.GetCell(i);
            string columnName = cell?.ToString() ?? $"Column{i + 1}";
            result[i] = columnName;
        }

        return result;
    }

    protected override object GetCellValue(object worksheet, int rowNum, int colIndex)
    {
        var sheet = (ISheet)worksheet;
        var row = sheet.GetRow(rowNum);

        if (row == null) return DBNull.Value;

        var cell = row.GetCell(colIndex);
        if (cell == null) return DBNull.Value;

        return GetExcelCellValue(cell);
    }

    #region 私有辅助方法

    /// <summary>
    /// 将值写入Excel单元格并设置适当的格式
    /// </summary>
    private void WriteValueToCell(IWorkbook workbook, IRow row, int columnIndex, object? value, Type valueType, Dictionary<Type, ICellStyle> styleCache)
    {
        var cell = row.CreateCell(columnIndex);

        if (value == null || value is DBNull)
        {
            cell.SetCellValue(string.Empty);
            return;
        }

        // 使用提供的类型参数处理单元格值和样式
        if (valueType == typeof(DateTime) || valueType == typeof(DateTime?))
        {
            DateTime dateValue;
            if (value is DateTime dt)
            {
                dateValue = dt;
            }
            else
            {
                // 尝试转换其他类型为日期
                if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                    dateValue = parsedDate;
                else if (value is double numericDate)
                    dateValue = DateTime.FromOADate(numericDate);
                else
                    dateValue = DateTime.MinValue;
            }

            if (dateValue != DateTime.MinValue)
            {
                cell.SetCellValue(dateValue);
                // 使用统一的日期格式
                if (!styleCache.TryGetValue(valueType, out var dateStyle))
                {
                    dateStyle = workbook.CreateCellStyle();
                    var format = workbook.CreateDataFormat();
                    dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
                    styleCache[valueType] = dateStyle;
                }
                cell.CellStyle = dateStyle;
            }
            else
            {
                cell.SetCellValue(string.Empty);
            }
        }
        else if (valueType == typeof(bool) || valueType == typeof(bool?))
        {
            bool boolValue;
            if (value is bool b)
                boolValue = b;
            else if (bool.TryParse(value.ToString(), out bool parsedBool))
                boolValue = parsedBool;
            else
                boolValue = false;

            cell.SetCellValue(boolValue);
        }
        else if (valueType == typeof(int) || valueType == typeof(long) ||
                 valueType == typeof(short) || valueType == typeof(byte) ||
                 valueType == typeof(sbyte) || valueType == typeof(ushort) ||
                 valueType == typeof(uint) || valueType == typeof(ulong) ||
                 valueType == typeof(int?) || valueType == typeof(long?) ||
                 valueType == typeof(short?) || valueType == typeof(byte?) ||
                 valueType == typeof(sbyte?) || valueType == typeof(ushort?) ||
                 valueType == typeof(uint?) || valueType == typeof(ulong?))
        {
            // 整数类型 - 使用统一的整数格式
            try
            {
                long longValue = Convert.ToInt64(value);
                cell.SetCellValue((double)longValue);

                // 应用整数样式
                if (!styleCache.TryGetValue(typeof(int), out var intStyle))
                {
                    intStyle = workbook.CreateCellStyle();
                    var format = workbook.CreateDataFormat();
                    intStyle.DataFormat = format.GetFormat(INTEGER_FORMAT);
                    styleCache[typeof(int)] = intStyle;
                }
                cell.CellStyle = intStyle;
            }
            catch
            {
                cell.SetCellValue(value.ToString());
            }
        }
        else if (valueType == typeof(double) || valueType == typeof(float) ||
                 valueType == typeof(decimal) || valueType == typeof(double?) ||
                 valueType == typeof(float?) || valueType == typeof(decimal?))
        {
            // 浮点类型 - 使用统一的小数格式
            try
            {
                double doubleValue = Convert.ToDouble(value);
                cell.SetCellValue(doubleValue);

                // 应用浮点数样式
                if (!styleCache.TryGetValue(typeof(double), out var doubleStyle))
                {
                    doubleStyle = workbook.CreateCellStyle();
                    var format = workbook.CreateDataFormat();
                    doubleStyle.DataFormat = format.GetFormat(DECIMAL_FORMAT);
                    styleCache[typeof(double)] = doubleStyle;
                }
                cell.CellStyle = doubleStyle;
            }
            catch
            {
                cell.SetCellValue(value.ToString());
            }
        }
        else
        {
            // 默认处理为字符串
            cell.SetCellValue(value.ToString());
        }

        // 应用边框 - 单元格样式应该已经由上面的代码设置，需要在这里修改它
        DrawBorder(cell);
    }

    /// <summary>
    /// 从Excel单元格获取适当类型的值
    /// </summary>
    private object GetExcelCellValue(ICell cell)
    {
        switch (cell.CellType)
        {
            case CellType.String:
                return GetExcelCellValue(cell.StringCellValue);
            case CellType.Numeric:
                if (DateUtil.IsCellDateFormatted(cell))
                {
                    return GetExcelCellValue(cell.NumericCellValue, true);
                }
                else
                {
                    return GetExcelCellValue(cell.NumericCellValue);
                }
            case CellType.Boolean:
                return GetExcelCellValue(cell.BooleanCellValue);
            case CellType.Formula:
                switch (cell.CachedFormulaResultType)
                {
                    case CellType.String:
                        return GetExcelCellValue(cell.StringCellValue);
                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(cell))
                        {
                            return GetExcelCellValue(cell.DateCellValue, true);
                        }
                        return GetExcelCellValue(cell.NumericCellValue);
                    case CellType.Boolean:
                        return GetExcelCellValue(cell.BooleanCellValue);
                    case CellType.Error:
                        return GetExcelCellValue(ErrorEval.GetText(cell.ErrorCellValue));
                    default:
                        return GetExcelCellValue(string.Empty);
                }
            case CellType.Error:
                return GetExcelCellValue(ErrorEval.GetText(cell.ErrorCellValue));
            case CellType.Blank:
            case CellType.Unknown:
            default:
                return DBNull.Value;
        }
    }

    /// <summary>
    /// 设置标题行样式
    /// </summary>
    private void ApplyTitleRowFormatting(ICell titleRange)
    {
        try
        {
            var workbook = titleRange.Sheet.Workbook;

            var titleStyle = workbook.CreateCellStyle();
            titleStyle.Alignment = HorizontalAlignment.Center;
            titleStyle.VerticalAlignment = VerticalAlignment.Center;
            var titleFont = workbook.CreateFont();
            titleFont.FontHeightInPoints = TITLE_FONT_SIZE;
            titleFont.IsBold = true;
            titleStyle.SetFont(titleFont);
            titleRange.CellStyle = titleStyle;

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
    private void ApplyHeaderRowFormatting(ICell headerCell)
    {
        try
        {
            var workbook = headerCell.Sheet.Workbook;

            var headerStyle = workbook.CreateCellStyle();
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            var headerFont = workbook.CreateFont();
            headerFont.FontHeightInPoints = HEADER_FONT_SIZE;
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);
            headerCell.CellStyle = headerStyle;

            // 应用边框
            DrawBorder(headerCell);
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "设置表头行样式失败");
        }
    }

    private void ApplyBasicFormatting(ISheet worksheet, int rowCount, int columnCount)
    {
        // 设置所有单元格自动适应宽度
        for (int i = 0; i < columnCount; i++)
        {
            worksheet.AutoSizeColumn(i);
        }

        // 对标题行进行特殊处理，最小宽度为12
        for (int i = 1; i <= columnCount; i++)
        {
            var columnWidth = worksheet.GetColumnWidth(i);
            if (columnWidth < 12)
            {
                columnWidth = 12;
                worksheet.SetColumnWidth(i, columnWidth);
            }

        }

        // 设置表格边框
        if (rowCount > 1 && columnCount > 0)
        {
            var cellRangeAddress = new CellRangeAddress(1, rowCount, 1, columnCount);
            //var cellRange = GetCellRange(worksheet, new CellRangeAddress(1, 1, rowCount, columnCount));
            SetRegionBorderStyle(BorderStyle.Thin, cellRangeAddress, worksheet);
        }
    }

    /// <summary>
    /// 这个只能合并单元格使用吗？？？？？？？？？？？？？？？
    /// </summary>
    /// <param name="borderStyle"></param>
    /// <param name="region"></param>
    /// <param name="sheet"></param>
    private static void SetRegionBorderStyle(BorderStyle borderStyle, CellRangeAddress region, ISheet sheet)
    {
        RegionUtil.SetBorderBottom(borderStyle, region, sheet);
        RegionUtil.SetBorderLeft(borderStyle, region, sheet);
        RegionUtil.SetBorderRight(borderStyle, region, sheet);
        RegionUtil.SetBorderTop(borderStyle, region, sheet);
    }

    //返回指定范围单元格
    private ICellRange<ICell> GetCellRange(ISheet ws, CellRangeAddress range)
    {
        int firstRow = range.FirstRow;
        int firstColumn = range.FirstColumn;
        int lastRow = range.LastRow;
        int lastColumn = range.LastColumn;
        int height = lastRow - firstRow + 1;
        int width = lastColumn - firstColumn + 1;
        List<ICell> temp = new List<ICell>(height * width);
        for (int rowIn = firstRow; rowIn <= lastRow; rowIn++)
        {
            for (int colIn = firstColumn; colIn <= lastColumn; colIn++)
            {
                IRow row = ws.GetRow(rowIn);
                if (row == null)
                {
                    row = ws.CreateRow(rowIn);
                }
                ICell cell = row.GetCell(colIn);
                if (cell == null)
                {
                    cell = row.CreateCell(colIn);
                }
                temp.Add(cell);
            }
        }
        return SSCellRange<ICell>.Create(firstRow, firstColumn, height, width, temp, typeof(HSSFCell));
    }

    /// <summary>
    /// 为单元格添加边框
    /// </summary>
    private static void DrawBorder(ICell cell)
    {
        // 如果单元格没有样式，创建一个新样式
        ICellStyle style = cell.CellStyle ?? cell.Sheet.Workbook.CreateCellStyle();

        // 设置边框
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;

        // 应用样式到单元格
        cell.CellStyle = style;
    }

    ///// <summary>
    ///// 为单元格添加边框
    ///// </summary>
    //private void DrawBorder(IWorkbook workbook, ICell cell)
    //{
    //    // 如果单元格没有样式，创建一个新样式
    //    ICellStyle style = cell.CellStyle ?? workbook.CreateCellStyle();

    //    // 设置边框
    //    style.BorderTop = BorderStyle.Thin;
    //    style.BorderBottom = BorderStyle.Thin;
    //    style.BorderLeft = BorderStyle.Thin;
    //    style.BorderRight = BorderStyle.Thin;

    //    // 应用样式到单元格
    //    cell.CellStyle = style;
    //}

    #endregion

    /// <summary>
    /// 创建空工作簿
    /// </summary>
    protected override object CreateWorkbook()
    {
        return new XSSFWorkbook();
    }

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected override object CreateWorksheet(object workbook, string sheetName)
    {
        var xssfWorkbook = (XSSFWorkbook)workbook;
        return xssfWorkbook.CreateSheet(sheetName);
    }

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    protected override int ApplyTitle(object worksheet, string title, int columnCount)
    {
        var sheet = (ISheet)worksheet;
        var titleRow = sheet.CreateRow(0);
        titleRow.HeightInPoints = 25;
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue(title);

        ApplyTitleRowFormatting(titleCell);
        
        sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, columnCount - 1));
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法 - 处理共通逻辑
    /// </summary>
    protected void CreateHeaderRowCore(ISheet sheet, string[] columnNames, int startRowIndex)
    {
        var headerRow = sheet.CreateRow(startRowIndex);
        
        for (int i = 0; i < columnNames.Length; i++)
        {
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(columnNames[i]);
            ApplyHeaderRowFormatting(cell);
        }
    }

    /// <summary>
    /// 创建标题行
    /// </summary>
    protected override void CreateHeaderRow(object worksheet, DataColumnCollection columns, int startRowIndex)
    {
        var sheet = (ISheet)worksheet;
        string[] columnNames = new string[columns.Count];
        
        for (int i = 0; i < columns.Count; i++)
        {
            columnNames[i] = columns[i].ColumnName;
        }
        
        CreateHeaderRowCore(sheet, columnNames, startRowIndex);
    }

    /// <summary>
    /// 创建集合标题行
    /// </summary>
    protected override void CreateCollectionHeaderRow(object worksheet, PropertyInfo[] properties, int startRowIndex)
    {
        var sheet = (ISheet)worksheet;
        
        // 获取有ExcelColumn特性的列，如果没有则使用所有列
        var columns = GetExcelColumns(properties);
        if (columns.Count == 0)
        {
            string[] columnNames = properties.Select(p => p.Name).ToArray();
            CreateHeaderRowCore(sheet, columnNames, startRowIndex);
        }
        else
        {
            // 使用特性标记的属性及其顺序
            columns = columns.OrderBy(c => c.Item3).ToList();
            string[] columnNames = columns.Select(c => 
                string.IsNullOrEmpty(c.Item2) ? c.Item1 : c.Item2).ToArray();
            CreateHeaderRowCore(sheet, columnNames, startRowIndex);
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

    /// <summary>
    /// 处理数据行
    /// </summary>
    protected override void ProcessDataRows(object worksheet, DataTable dataTable, int startRowIndex)
    {
        var sheet = (ISheet)worksheet;
        var workbook = sheet.Workbook;
        bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;
        
        // 预创建样式字典，提高性能
        var styleCache = new Dictionary<Type, ICellStyle>();
        
        // 创建日期样式
        var dateStyle = workbook.CreateCellStyle();
        var format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
        styleCache[typeof(DateTime)] = dateStyle;

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            var cellValues = new object[dataTable.Rows.Count, dataTable.Columns.Count];
            var columnTypes = new Type[dataTable.Columns.Count];

            // 获取列类型
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                columnTypes[i] = dataTable.Columns[i].DataType;
            }

            // 并行填充数据
            Parallel.For(0, dataTable.Rows.Count, i =>
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    cellValues[i, j] = dataTable.Rows[i][j];
                }
            });

            // 批量写入
            int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;
            for (int batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, dataTable.Rows.Count);
                for (int i = batchStart; i < batchEnd; i++)
                {
                    var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var value = cellValues[i, j];
                        WriteValueToCell(workbook, dataRow, j, value, columnTypes[j], styleCache);
                    }
                }
            }
        }
        else
        {
            // 顺序处理小数据集
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    var value = dataTable.Rows[i][j];
                    WriteValueToCell(workbook, dataRow, j, value, dataTable.Columns[j].DataType, styleCache);
                }
            }
        }
    }

    /// <summary>
    /// 应用工作表格式化
    /// </summary>
    protected override void ApplyWorksheetFormatting(object worksheet, int rowCount, int columnCount)
    {
        var sheet = (ISheet)worksheet;
        
        // 设置所有单元格自动适应宽度
        if (Options.AutoFitColumns)
        {
            for (int i = 0; i < columnCount; i++)
            {
                sheet.AutoSizeColumn(i);
                // 确保最小列宽
                var width = sheet.GetColumnWidth(i);
                if (width < 256 * 12) // 约12个字符宽
                {
                    sheet.SetColumnWidth(i, 256 * 12);
                }
            }
        }
    }

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected override MemoryStream SaveWorkbookToStream(object workbook)
    {
        var xssfWorkbook = (XSSFWorkbook)workbook;
        var ms = new MemoryStream();
        xssfWorkbook.Write(ms, true);
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

    // /// <summary>
    // /// 创建集合标题行
    // /// </summary>
    // protected override void CreateCollectionHeaderRow(object worksheet, PropertyInfo[] properties, int startRowIndex)
    // {
    //     var sheet = (ISheet)worksheet;
    //     var headerRow = sheet.CreateRow(startRowIndex);
        
    //     for (int i = 0; i < properties.Length; i++)
    //     {
    //         var cell = headerRow.CreateCell(i);
    //         cell.SetCellValue(properties[i].Name);
    //         ApplyHeaderRowFormatting(cell);
    //     }
    // }

    /// <summary>
    /// 处理集合数据行
    /// </summary>
    protected override void ProcessCollectionRows<T>(object worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        var sheet = (ISheet)worksheet;
        var workbook = sheet.Workbook;
        bool useParallelProcessing = list.Count > Options.ParallelProcessingThreshold;
        
        // 预创建样式字典，提高性能
        var styleCache = new Dictionary<Type, ICellStyle>();
        
        // 创建日期样式
        var dateStyle = workbook.CreateCellStyle();
        var format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
        styleCache[typeof(DateTime)] = dateStyle;

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            logger?.LogDebug("使用并行处理导出 {Count} 条记录", list.Count);
            
            // 使用批处理提高性能
            int batchSize = Options.UseBatchWrite ? Options.BatchSize : list.Count;
            
            // 预计算所有值
            var cellValues = new object?[list.Count, properties.Length];
            
            Parallel.For(0, list.Count, rowIndex =>
            {
                for (int colIndex = 0; colIndex < properties.Length; colIndex++)
                {
                    cellValues[rowIndex, colIndex] = properties[colIndex].GetValue(list[rowIndex]);
                }
            });
            
            // 批量写入
            for (int batchStart = 0; batchStart < list.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, list.Count);
                for (int i = batchStart; i < batchEnd; i++)
                {
                    var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                    for (int j = 0; j < properties.Length; j++)
                    {
                        WriteValueToCell(workbook, dataRow, j, cellValues[i, j], properties[j].PropertyType, styleCache);
                    }
                }
            }
        }
        else
        {
            // 顺序处理小数据集
            for (int i = 0; i < list.Count; i++)
            {
                var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                for (int j = 0; j < properties.Length; j++)
                {
                    var value = properties[j].GetValue(list[i]);
                    WriteValueToCell(workbook, dataRow, j, value, properties[j].PropertyType, styleCache);
                }
            }
        }
    }
}
