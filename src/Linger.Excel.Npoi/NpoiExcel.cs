using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

/// <summary>
/// 基于NPOI的Excel处理实现
/// </summary>
public class NpoiExcel : ExcelBase
{
    // 缓存样式数据，避免重复创建
    private readonly Dictionary<string, ICellStyle> _styleCache = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">Excel配置选项</param>
    /// <param name="logger">日志记录器</param>
    public NpoiExcel(ExcelOptions? options = null, ILogger<NpoiExcel>? logger = null)
        : base(options, logger)
    {
    }

    /// <summary>
    /// 将对象集合转换为MemoryStream
    /// </summary>
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    {
        if (list == null || list.Count == 0)
        {
            Logger?.LogWarning("要转换的集合为空");
            return null;
        }

        return SafeExecute(() =>
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet(sheetsName);

            // 获取属性信息
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .ToArray();

            if (properties.Length == 0)
            {
                Logger?.LogWarning("类型 {Type} 没有可读属性", typeof(T).Name);
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
        }, null, nameof(ConvertCollectionToMemoryStream));
    }

    /// <summary>
    /// 将DataTable转换为MemoryStream
    /// </summary>
    public override MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable == null || dataTable.Columns.Count == 0)
        {
            Logger?.LogWarning("要转换的DataTable为空或没有列");
            return null;
        }

        return SafeExecute(() =>
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
        }, null, nameof(ConvertDataTableToMemoryStream));
    }

    /// <summary>
    /// 将Stream转换为DataTable
    /// </summary>
    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (stream is not { CanRead: true, Length: > 0 })
        {
            Logger?.LogWarning("无效的Stream: 不可读或长度为0");
            return null;
        }

        return SafeExecute(() =>
        {
            IWorkbook? workbook = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    workbook = WorkbookFactory.Create(ms);
                }

                ISheet sheet;
                if (string.IsNullOrWhiteSpace(sheetName))
                {
                    sheet = workbook.GetSheetAt(0);
                    Logger?.LogDebug("使用第一个工作表, 名称: {SheetName}", sheet.SheetName);
                }
                else
                {
                    sheet = workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0);
                    if (sheet.SheetName != sheetName)
                    {
                        Logger?.LogWarning("未找到指定的工作表 {SheetName}, 使用第一个工作表代替", sheetName);
                    }
                }

                DataTable dataTable = ImportFromSheet(sheet, headerRowIndex, addEmptyRow);
                return dataTable;
            }
            finally
            {
                // NPOI中对Workbook的释放需特别处理
                workbook?.Close();
            }
        }, null, nameof(ConvertStreamToDataTable));
    }

    /// <summary>
    /// 流式读取大Excel文件
    /// </summary>
    public IEnumerable<T> StreamReadExcel<T>(Stream stream, string? sheetName = null,
        int headerRowIndex = 0) where T : class, new()
    {
        if (stream == null || !stream.CanRead)
            yield break;

        // 使用BufferedStream提高读取性能
        using var bufferedStream = new BufferedStream(stream);
        IWorkbook workbook;

        try
        {
            workbook = WorkbookFactory.Create(bufferedStream);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "无法创建工作簿");
            yield break;
        }

        ISheet sheet;
        if (string.IsNullOrWhiteSpace(sheetName))
        {
            sheet = workbook.GetSheetAt(0);
        }
        else
        {
            sheet = workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0);
        }

        // 读取表头并创建属性映射
        var propertyMap = CreatePropertyMap<T>(sheet, headerRowIndex);

        // 流式读取数据行
        for (int rowNum = headerRowIndex + 1; rowNum <= sheet.LastRowNum; rowNum++)
        {
            var row = sheet.GetRow(rowNum);
            if (row == null) continue;

            var item = new T();
            bool hasData = false;

            foreach (var mapping in propertyMap)
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

            if (hasData)
                yield return item;
        }
    }

    private Dictionary<int, PropertyInfo> CreatePropertyMap<T>(ISheet sheet, int headerRowIndex)
    {
        var propertyMap = new Dictionary<int, PropertyInfo>();
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

    #region 私有辅助方法

    /// <summary>
    /// 获取或创建样式
    /// </summary>
    private ICellStyle GetOrCreateStyle(IWorkbook workbook, string styleKey, Action<ICellStyle> styleInitializer)
    {
        if (!_styleCache.TryGetValue(styleKey, out var style))
        {
            style = workbook.CreateCellStyle();
            styleInitializer(style);
            _styleCache[styleKey] = style;
        }
        return style;
    }

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
                // 使用valueType作为样式缓存键
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
            // 整数类型
            try
            {
                long longValue = Convert.ToInt64(value);
                cell.SetCellValue((double)longValue);
                
                // 应用整数样式
                if (!styleCache.TryGetValue(typeof(int), out var intStyle))
                {
                    intStyle = workbook.CreateCellStyle();
                    var format = workbook.CreateDataFormat();
                    intStyle.DataFormat = format.GetFormat("#,##0");
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
            // 浮点类型
            try
            {
                double doubleValue = Convert.ToDouble(value);
                cell.SetCellValue(doubleValue);
                
                // 应用浮点数样式
                if (!styleCache.TryGetValue(typeof(double), out var doubleStyle))
                {
                    doubleStyle = workbook.CreateCellStyle();
                    var format = workbook.CreateDataFormat();
                    doubleStyle.DataFormat = format.GetFormat("#,##0.00");
                    styleCache[typeof(double)] = doubleStyle;
                }
                cell.CellStyle = doubleStyle;
            }
            catch
            {
                cell.SetCellValue(value.ToString());
            }
        }
        else if (valueType.IsEnum)
        {
            // 处理枚举类型 - 显示枚举名称而不是数值
            cell.SetCellValue(value.ToString());
        }
        else
        {
            // 默认处理为字符串
            cell.SetCellValue(value.ToString());
        }
    }

    /// <summary>
    /// 从Excel工作表导入数据到DataTable
    /// </summary>
    private DataTable ImportFromSheet(ISheet sheet, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        return MonitorPerformance("解析Excel工作表", () => {
            var dataTable = new DataTable();
            dataTable.TableName = sheet.SheetName;
            
            // 处理表头
            if (headerRowIndex < 0)
            {
                // 没有表头，使用默认列名
                int maxCellCount = 0;
                for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row != null && row.LastCellNum > maxCellCount)
                    {
                        maxCellCount = row.LastCellNum;
                    }
                }

                for (int i = 0; i < maxCellCount; i++)
                {
                    dataTable.Columns.Add($"Column{i}");
                }
            }
            else
            {
                // 使用指定行作为表头
                var headerRow = sheet.GetRow(headerRowIndex);
                if (headerRow == null)
                {
                    Logger?.LogWarning("未能在行 {Row} 找到表头", headerRowIndex);
                    return dataTable;
                }

                for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
                {
                    var cell = headerRow.GetCell(i);
                    string columnName;

                    if (cell == null)
                    {
                        columnName = $"Column{i}";
                    }
                    else
                    {
                        var colName = cell.ToString();
                        if (colName.IsNullOrWhiteSpace())
                        {
                            columnName = $"Column{i}";
                        }
                        else
                        {
                            if (dataTable.Columns.Contains(colName))
                            {
                                columnName = $"{colName}_{i}";
                            }
                            else
                            {
                                columnName = colName;
                            }
                        }
                    }
                    dataTable.Columns.Add(columnName);
                }
            }

            // 从表头行之后开始读取数据
            int startRow = headerRowIndex < 0 ? sheet.FirstRowNum : headerRowIndex + 1;

            for (int rowNum = startRow; rowNum <= sheet.LastRowNum; rowNum++)
            {
                var row = sheet.GetRow(rowNum);
                if (row == null) continue;

                var dataRow = dataTable.NewRow();
                bool hasData = false;

                for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                {
                    if (colIndex < row.LastCellNum)
                    {
                        var cell = row.GetCell(colIndex);
                        if (cell != null)
                        {
                            object cellValue = GetExcelCellValue(cell);
                            dataRow[colIndex] = cellValue;

                            if (cellValue != DBNull.Value)
                            {
                                hasData = true;
                            }
                        }
                    }
                }

                if (hasData || addEmptyRow)
                {
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        });
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
