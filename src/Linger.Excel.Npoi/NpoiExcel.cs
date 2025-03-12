using System.Data;
using System.Reflection;
using System.Text;
using Linger.Excel.Contracts;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

public class NpoiExcel(ExcelOptions? options = null, ILogger<NpoiExcel>? logger = null) : ExcelBase(options, logger)
{
    /// <summary>
    /// 将对象集合转换为MemoryStream
    /// </summary>
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    {
        if (list == null || list.Count == 0)
        {
            logger?.LogWarning("要转换的集合为空");
            return null;
        }

        return SafeExecute(() =>
        {
            return MonitorPerformance("集合导出到Excel", () =>
            {
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet(sheetsName);

                // 获取属性信息
                var properties = typeof(T).GetProperties()
                    .Where(p => p.CanRead)
                    .ToArray();

                if (properties.Length == 0)
                {
                    logger?.LogWarning("类型 {Type} 没有可读属性", typeof(T).Name);
                    return new MemoryStream();
                }

                // 设置标题样式
                var titleIndex = 0;
                if (!string.IsNullOrEmpty(title))
                {
                    var titleRow = sheet.CreateRow(titleIndex++);
                    titleRow.HeightInPoints = 25;
                    var titleCell = titleRow.CreateCell(0);
                    titleCell.SetCellValue(title);

                    var titleStyle = workbook.CreateCellStyle();
                    titleStyle.Alignment = HorizontalAlignment.Center;
                    var titleFont = workbook.CreateFont();
                    titleFont.FontHeightInPoints = 14;
                    titleFont.IsBold = true;
                    titleStyle.SetFont(titleFont);
                    titleCell.CellStyle = titleStyle;

                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, properties.Length - 1));
                }

                // 创建表头
                var headerRow = sheet.CreateRow(titleIndex++);
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.Alignment = HorizontalAlignment.Center;
                var headerFont = workbook.CreateFont();
                headerFont.FontHeightInPoints = 10;
                headerFont.IsBold = true;
                headerStyle.SetFont(headerFont);

                // 预创建样式字典，提高性能
                var styleCache = new Dictionary<Type, ICellStyle>();

                // 创建日期样式
                var dateStyle = workbook.CreateCellStyle();
                var format = workbook.CreateDataFormat();
                dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
                styleCache[typeof(DateTime)] = dateStyle;

                // 设置列标题并计算列宽
                var columnWidths = new int[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(properties[i].Name);
                    cell.CellStyle = headerStyle;
                    columnWidths[i] = Encoding.UTF8.GetBytes(properties[i].Name).Length;
                }

                // 使用基类的批处理方法处理数据行
                ProcessInBatches<IRow, object?>(
                    list.Count,
                    i => sheet.CreateRow(i + titleIndex),
                    i =>
                    {
                        var values = properties.Select(p => p.GetValue(list[i])).ToArray();
                        // 计算列宽
                        for (int j = 0; j < properties.Length; j++)
                        {
                            var value = values[j];
                            if (value != null)
                            {
                                var strValue = value.ToString() ?? string.Empty;
                                var length = Encoding.UTF8.GetBytes(strValue).Length;
                                lock (columnWidths)
                                {
                                    columnWidths[j] = Math.Max(columnWidths[j], length);
                                }
                            }
                        }
                        return values;
                    },
                    (row, _, values, param) =>
                    {
                        for (int j = 0; j < properties.Length; j++)
                        {
                            WriteValueToCell(workbook, row, j, values[j], properties[j].PropertyType, styleCache);
                        }
                    }
                );

                // 设置列宽
                if (Options.AutoFitColumns)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        int width = Math.Min(255, columnWidths[i] + 2) * 256;
                        sheet.SetColumnWidth(i, width);
                    }
                }

                // 执行自定义操作
                action?.Invoke(sheet, properties);

                var ms = new MemoryStream();
                workbook.Write(ms, true);
                ms.Position = 0;

                return ms;
            });
        }, null, nameof(ConvertCollectionToMemoryStream));
    }

    /// <summary>
    /// 将DataTable转换为MemoryStream
    /// </summary>
    public override MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable == null || dataTable.Columns.Count == 0)
        {
            logger?.LogWarning("要转换的DataTable为空或没有列");
            return null;
        }

        return SafeExecute(() =>
        {
            return MonitorPerformance("DataTable导出到Excel", () =>
            {
                var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet(sheetsName);

                // 设置标题样式
                var titleIndex = 0;
                if (!string.IsNullOrEmpty(title))
                {
                    var titleRow = sheet.CreateRow(titleIndex++);
                    titleRow.HeightInPoints = 25;
                    var titleCell = titleRow.CreateCell(0);
                    titleCell.SetCellValue(title);

                    var titleStyle = workbook.CreateCellStyle();
                    titleStyle.Alignment = HorizontalAlignment.Center;
                    var titleFont = workbook.CreateFont();
                    titleFont.FontHeightInPoints = 14;
                    titleFont.IsBold = true;
                    titleStyle.SetFont(titleFont);
                    titleCell.CellStyle = titleStyle;

                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dataTable.Columns.Count - 1));
                }

                // 创建表头
                var headerRow = sheet.CreateRow(titleIndex++);
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.Alignment = HorizontalAlignment.Center;
                var headerFont = workbook.CreateFont();
                headerFont.FontHeightInPoints = 10;
                headerFont.IsBold = true;
                headerStyle.SetFont(headerFont);

                // 预创建样式字典，提高性能
                var styleCache = new Dictionary<Type, ICellStyle>();

                // 创建日期样式
                var dateStyle = workbook.CreateCellStyle();
                var format = workbook.CreateDataFormat();
                dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
                styleCache[typeof(DateTime)] = dateStyle;

                // 设置列标题并计算列宽
                var columnWidths = new int[dataTable.Columns.Count];
                for (var i = 0; i < dataTable.Columns.Count; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(dataTable.Columns[i].ColumnName);
                    cell.CellStyle = headerStyle;
                    columnWidths[i] = Encoding.UTF8.GetBytes(dataTable.Columns[i].ColumnName).Length;
                }

                // 判断是否使用并行处理
                bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

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

                            // 计算列宽
                            var value = dataTable.Rows[i][j];
                            if (value != DBNull.Value)
                            {
                                var strValue = value.ToString() ?? string.Empty;
                                var length = Encoding.UTF8.GetBytes(strValue).Length;
                                lock (columnWidths)
                                {
                                    columnWidths[j] = Math.Max(columnWidths[j], length);
                                }
                            }
                        }
                    });

                    // 批量写入
                    int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;
                    for (int batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
                    {
                        int batchEnd = Math.Min(batchStart + batchSize, dataTable.Rows.Count);
                        for (int i = batchStart; i < batchEnd; i++)
                        {
                            var dataRow = sheet.CreateRow(i + titleIndex);
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
                        var dataRow = sheet.CreateRow(i + titleIndex);
                        for (int j = 0; j < dataTable.Columns.Count; j++)
                        {
                            var value = dataTable.Rows[i][j];
                            WriteValueToCell(workbook, dataRow, j, value, dataTable.Columns[j].DataType, styleCache);

                            // 计算列宽
                            if (value != DBNull.Value)
                            {
                                var strValue = value.ToString() ?? string.Empty;
                                var length = Encoding.UTF8.GetBytes(strValue).Length;
                                columnWidths[j] = Math.Max(columnWidths[j], length);
                            }
                        }
                    }
                }

                // 设置列宽
                if (Options.AutoFitColumns)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        int width = Math.Min(255, columnWidths[i] + 2) * 256;
                        sheet.SetColumnWidth(i, width);
                    }
                }

                // 执行自定义操作
                action?.Invoke(sheet, dataTable.Columns, dataTable.Rows);

                var ms = new MemoryStream();
                workbook.Write(ms, true);
                ms.Position = 0;

                return ms;
            });
        }, null, nameof(ConvertDataTableToMemoryStream));
    }

    // 删除原有的 public override DataTable? ConvertStreamToDataTable 方法

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
        DrawBorder(workbook, cell);
    }

    /// <summary>
    /// 为单元格添加边框
    /// </summary>
    private void DrawBorder(IWorkbook workbook, ICell cell)
    {
        // 如果单元格没有样式，创建一个新样式
        ICellStyle style = cell.CellStyle ?? workbook.CreateCellStyle();
        
        // 设置边框
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        // 应用样式到单元格
        cell.CellStyle = style;
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

    #endregion
}
