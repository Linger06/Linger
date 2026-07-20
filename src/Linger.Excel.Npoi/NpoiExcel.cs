using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.Util;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

public class NpoiExcel(ExcelOptions? options = null, ILogger<NpoiExcel>? logger = null)
    : ExcelBase<IWorkbook, ISheet>(options, logger)
{
    // 添加基类要求的方法实现
    protected override IWorkbook OpenWorkbook(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
        {
            var seekableStream = CopyToMemoryStream(stream, cancellationToken);
            return WorkbookFactory.Create(seekableStream);
        }

        var bufferedStream = new BufferedStream(stream);
        cancellationToken.ThrowIfCancellationRequested();
        return WorkbookFactory.Create(bufferedStream);
    }
    protected override ISheet GetWorksheet(IWorkbook workbook, string? sheetName)
    {
        if (string.IsNullOrWhiteSpace(sheetName))
        {
            return workbook.GetSheetAt(0);
        }

        return workbook.GetSheet(sheetName); // 当指定的工作表不存在时返回null
    }

    protected override string GetSheetName(ISheet worksheet)
    {
        return worksheet.SheetName;
    }

    protected override List<string> GetAllSheetNames(IWorkbook workbook)
    {
        var sheetNames = new List<string>();
        for (var i = 0; i < workbook.NumberOfSheets; i++)
        {
            var sheetName = workbook.GetSheetName(i);
            if (!string.IsNullOrWhiteSpace(sheetName))
            {
                sheetNames.Add(sheetName);
            }
        }

        return sheetNames;
    }

    protected override bool HasData(ISheet worksheet)
    {
        return worksheet.PhysicalNumberOfRows > 0;
    }

    protected override Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(ISheet worksheet, int headerRowIndex)
    {
        var propertyMap = new Dictionary<int, PropertyInfo>();
        var headerRow = worksheet.GetRow(headerRowIndex);
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

    /// <summary>
    /// 获取数据开始行索引
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)，-1表示没有表头行</param>
    /// <returns>数据开始行索引(0-based)</returns>
    /// <remarks>
    /// NPOI使用0-based索引系统。
    /// 如果headerRowIndex为-1(无表头)，则从第0行开始读取数据。
    /// 否则从表头行的下一行(headerRowIndex+1)开始读取数据。
    /// </remarks>
    protected override int GetDataStartRow(ISheet worksheet, int headerRowIndex)
    {
        // 如果headerRowIndex为负值(无表头)，则从第0行开始
        return Math.Max(0, headerRowIndex + 1);
    }

    /// <summary>
    /// 获取数据结束行索引
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <returns>数据结束行索引</returns>
    /// <remarks>
    /// NPOI使用0-based索引，LastRowNum属性返回最后一行的索引(而非行数)。
    /// </remarks>
    protected override int GetDataEndRow(ISheet worksheet)
    {
        return worksheet.LastRowNum;
    }

    protected override void CloseWorkbook(IWorkbook workbook)
    {
        workbook.Close();
    }

    protected override int EstimateColumnCount(ISheet worksheet)
    {
        var maxCellCount = 0;

        // 扫描所有行找到最大列数
        for (var i = worksheet.FirstRowNum; i <= worksheet.LastRowNum; i++)
        {
            var row = worksheet.GetRow(i);
            if (row != null && row.LastCellNum > maxCellCount)
            {
                maxCellCount = row.LastCellNum;
            }
        }

        return maxCellCount;
    }

    /// <summary>
    /// 创建表头映射关系(列索引到列名的映射)
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)，-1表示没有表头行</param>
    /// <returns>列索引到列名的字典</returns>
    /// <remarks>
    /// NPOI使用0-based索引系统。
    /// 如果headerRowIndex为-1(无表头)，则返回空字典。
    /// 否则读取表头行，并将列名映射到列索引。
    /// </remarks>
    protected override Dictionary<int, string> ExtractRawHeaderMappings(ISheet worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();

        // 如果不存在表头行，返回空字典
        if (headerRowIndex < 0)
            return result;

        var headerRow = worksheet.GetRow(headerRowIndex);
        if (headerRow == null) return result;

        // NPOI的列索引从0开始，与基类约定一致
        for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
        {
            var cell = headerRow.GetCell(i);
            var columnName = cell?.ToString() ?? $"Column{i + 1}";
            result[i] = columnName;
        }

        return result;
    }

    /// <summary>
    /// 获取单元格的值
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="rowNum">行索引</param>
    /// <param name="colIndex">列索引</param>
    /// <returns>单元格值</returns>
    /// <remarks>
    /// NPOI使用0-based索引系统，rowNum和colIndex是NPOI原生的行列索引。
    /// 此方法直接使用传入的索引值，不需要进行转换。
    /// </remarks>
    protected override object GetCellValue(ISheet worksheet, int rowNum, int colIndex)
    {
        // NPOI从0开始计数，与基类约定一致，无需调整索引
        var row = worksheet.GetRow(rowNum);

        if (row == null) return DBNull.Value;

        var cell = row.GetCell(colIndex);
        if (cell == null) return DBNull.Value;

        return GetExcelCellValue(cell);
    }

    /// <summary>
    /// 检查指定行是否为空行
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="rowNum">行索引(0-based)</param>
    /// <returns>如果该行为空则返回true</returns>
    /// <remarks>
    /// NPOI提供了原生的FirstCellNum属性来快速判断行是否为空：
    /// - FirstCellNum t&lt; 0 表示该行所有单元格都为空
    /// - 也可以使用 PhysicalNumberOfCells == 0 来判断
    /// </remarks>
    protected override bool IsRowEmpty(ISheet worksheet, int rowNum)
    {
        var row = worksheet.GetRow(rowNum);

        // 如果行不存在，视为空行
        if (row == null)
            return true;

        // 使用NPOI原生判断：FirstCellNum < 0 表示所有单元格都为空
        // 这比遍历所有单元格要快得多
        return row.FirstCellNum < 0;
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
                    dateStyle.DataFormat = format.GetFormat(Options.StyleOptions.DataStyle.DateFormat);
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
            else if (bool.TryParse(value.ToString(), out var parsedBool))
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
            var longValue = value.ToLongOrDefault();// Convert.ToInt64(value);
            cell.SetCellValue(longValue);

            // 应用整数样式
            if (!styleCache.TryGetValue(typeof(int), out var intStyle))
            {
                intStyle = workbook.CreateCellStyle();
                var format = workbook.CreateDataFormat();
                intStyle.DataFormat = format.GetFormat(Options.StyleOptions.DataStyle.IntegerFormat);
                styleCache[typeof(int)] = intStyle;
            }
            cell.CellStyle = intStyle;
        }
        else if (valueType == typeof(double) || valueType == typeof(float) ||
                 valueType == typeof(decimal) || valueType == typeof(double?) ||
                 valueType == typeof(float?) || valueType == typeof(decimal?))
        {
            // 浮点类型 - 使用统一的小数格式
            var doubleValue = value.ToTarget<double>();
            cell.SetCellValue(doubleValue);

            // 应用浮点数样式
            if (!styleCache.TryGetValue(typeof(double), out var doubleStyle))
            {
                doubleStyle = workbook.CreateCellStyle();
                var format = workbook.CreateDataFormat();
                doubleStyle.DataFormat = format.GetFormat(Options.StyleOptions.DataStyle.DecimalFormat);
                styleCache[typeof(double)] = doubleStyle;
            }
            cell.CellStyle = doubleStyle;
        }
        else
        {
            // 默认处理为字符串
            cell.SetCellValue(value.ToString());
        }

        // 应用边框
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

                return GetExcelCellValue(cell.NumericCellValue);
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
                            return GetExcelCellValue(cell.NumericCellValue, true);
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
            default:
                return DBNull.Value;
        }
    }

    /// <summary>
    /// 设置标题行样式
    /// </summary>
    private void ApplyTitleRowFormatting(ICell titleRange)
    {
        var workbook = titleRange.Sheet.Workbook;

        var titleStyle = workbook.CreateCellStyle();
        titleStyle.Alignment = HorizontalAlignment.Center;
        titleStyle.VerticalAlignment = VerticalAlignment.Center;
        if (!string.IsNullOrEmpty(Options.StyleOptions.TitleStyle.BackgroundColor))
        {
            titleStyle.FillPattern = FillPattern.SolidForeground;
            ExcelStyleHelper.SetFillForegroundColor(titleStyle, Options.StyleOptions.TitleStyle.BackgroundColor);
        }

        var titleFont = workbook.CreateFont();
        titleFont.FontHeightInPoints = (short)Options.StyleOptions.TitleStyle.FontSize;
        titleFont.IsBold = Options.StyleOptions.TitleStyle.Bold;
        titleFont.FontName = Options.StyleOptions.TitleStyle.FontName;

        // 设置文字颜色
        if (!string.IsNullOrEmpty(Options.StyleOptions.TitleStyle.FontColor))
        {
            ExcelStyleHelper.SetFontColor(titleFont, Options.StyleOptions.TitleStyle.FontColor);
        }

        titleStyle.SetFont(titleFont);
        titleRange.CellStyle = titleStyle;
    }

    /// <summary>
    /// 设置表头行样式
    /// </summary>
    private void ApplyHeaderRowFormatting(ICell headerCell)
    {
        var workbook = headerCell.Sheet.Workbook;

        var headerStyle = workbook.CreateCellStyle();
        headerStyle.Alignment = HorizontalAlignment.Center;
        headerStyle.VerticalAlignment = VerticalAlignment.Center;

        // 设置背景色
        if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.BackgroundColor))
        {
            headerStyle.FillPattern = FillPattern.SolidForeground;
            ExcelStyleHelper.SetFillForegroundColor(headerStyle, Options.StyleOptions.HeaderStyle.BackgroundColor);
        }

        var headerFont = workbook.CreateFont();
        headerFont.FontHeightInPoints = (short)Options.StyleOptions.HeaderStyle.FontSize;
        headerFont.IsBold = Options.StyleOptions.HeaderStyle.Bold;
        headerFont.FontName = Options.StyleOptions.HeaderStyle.FontName;

        // 设置文字颜色
        if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.FontColor))
        {
            ExcelStyleHelper.SetFontColor(headerFont, Options.StyleOptions.HeaderStyle.FontColor);
        }

        headerStyle.SetFont(headerFont);
        headerCell.CellStyle = headerStyle;

        // 应用边框
        DrawBorder(headerCell);
    }

    /// <summary>
    /// 为单元格添加边框
    /// </summary>
    private static void DrawBorder(ICell cell)
    {
        var style = cell.CellStyle;
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
    }

    #endregion

    /// <summary>
    /// 创建空工作簿
    /// </summary>
    protected override IWorkbook CreateWorkbook()
    {
        return new XSSFWorkbook();
    }

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected override ISheet CreateWorksheet(IWorkbook workbook, string sheetName)
    {
        return workbook.CreateSheet(sheetName);
    }

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    protected override int ApplyTitle(ISheet worksheet, string title, int columnCount)
    {
        var titleRow = worksheet.CreateRow(0);
        titleRow.HeightInPoints = 25;
        var titleCell = titleRow.CreateCell(0);
        titleCell.SetCellValue(title);

        ApplyTitleRowFormatting(titleCell);

        if (columnCount > 1)
        {
            worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, columnCount - 1));
        }
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法 - 处理共通逻辑
    /// </summary>
    protected override void CreateHeaderRowCore(ISheet worksheet, string[] columnNames, int startRowIndex)
    {
        var headerRow = worksheet.CreateRow(startRowIndex);

        for (var i = 0; i < columnNames.Length; i++)
        {
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(columnNames[i]);
            ApplyHeaderRowFormatting(cell);
        }
    }

    /// <summary>
    /// 处理数据行
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="dataTable">数据表</param>
    /// <param name="startRowIndex">起始行索引</param>
    /// <remarks>
    /// NPOI使用0-based索引系统。
    /// 这里将数据写入从startRowIndex+1行开始的位置(跳过表头行)。
    /// 列索引从0开始。
    /// </remarks>
    protected override void ProcessDataRows(ISheet worksheet, DataTable dataTable, int startRowIndex)
    {
        var sheet = worksheet;
        var workbook = sheet.Workbook;
        var useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

        // 预创建样式字典，提高性能
        var styleCache = new Dictionary<Type, ICellStyle>();

        // 创建日期样式
        var dateStyle = workbook.CreateCellStyle();
        var format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat(Options.StyleOptions.DataStyle.DateFormat);
        styleCache[typeof(DateTime)] = dateStyle;

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            var cellValues = new object[dataTable.Rows.Count, dataTable.Columns.Count];
            var columnTypes = new Type[dataTable.Columns.Count];

            // 获取列类型
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                columnTypes[i] = dataTable.Columns[i].DataType;
            }

            // 并行填充数据
            Parallel.For(0, dataTable.Rows.Count, i =>
            {
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    cellValues[i, j] = dataTable.Rows[i][j];
                }
            });

            // 批量写入
            var batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;
            for (var batchStart = 0; batchStart < dataTable.Rows.Count; batchStart += batchSize)
            {
                var batchEnd = Math.Min(batchStart + batchSize, dataTable.Rows.Count);
                for (var i = batchStart; i < batchEnd; i++)
                {
                    var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                    for (var j = 0; j < dataTable.Columns.Count; j++)
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
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                for (var j = 0; j < dataTable.Columns.Count; j++)
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
    protected override void ApplyWorksheetFormatting(ISheet worksheet, int rowCount, int columnCount)
    {
        // 设置所有单元格自动适应宽度
        if (Options.AutoFitColumns)
        {
            for (var i = 0; i < columnCount; i++)
            {
                worksheet.AutoSizeColumn(i);
                // 确保最小列宽
                var width = worksheet.GetColumnWidth(i);
                if (width < 256 * 12) // 约12个字符宽
                {
                    worksheet.SetColumnWidth(i, 256 * 12);
                }
            }
        }

        // 设置表格边框
        if (rowCount > 1 && columnCount > 0)
        {
            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var row = worksheet.GetRow(rowIndex);
                if (row is null)
                {
                    continue;
                }

                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var cell = row.GetCell(columnIndex);
                    if (cell is not null)
                    {
                        DrawBorder(cell);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected override MemoryStream SaveWorkbookToStream(IWorkbook workbook)
    {
        var ms = new MemoryStream();
        workbook.Write(ms, true);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// 处理集合数据行
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <param name="worksheet">工作表</param>
    /// <param name="list">数据列表</param>
    /// <param name="properties">属性数组</param>
    /// <param name="startRowIndex">起始行索引</param>
    /// <remarks>
    /// NPOI使用0-based索引系统。
    /// 数据从startRowIndex+1行开始写入(跳过表头行)。
    /// 列索引从0开始。
    /// </remarks>
    protected override void ProcessCollectionRows<T>(ISheet worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        var sheet = worksheet;
        var workbook = sheet.Workbook;
        var useParallelProcessing = list.Count > Options.ParallelProcessingThreshold;
        var exportProperties = GetExportProperties(properties);

        // 预创建样式字典，提高性能
        var styleCache = new Dictionary<Type, ICellStyle>();

        // 创建日期样式
        var dateStyle = workbook.CreateCellStyle();
        var format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat(Options.StyleOptions.DataStyle.DateFormat);
        styleCache[typeof(DateTime)] = dateStyle;

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            Logger.LogDebug("使用并行处理导出 {Count} 条记录", list.Count);

            // 使用批处理提高性能
            var batchSize = Options.UseBatchWrite ? Options.BatchSize : list.Count;

            // 预计算所有值
            var cellValues = new object?[list.Count, exportProperties.Length];

            Parallel.For(0, list.Count, rowIndex =>
            {
                for (var colIndex = 0; colIndex < exportProperties.Length; colIndex++)
                {
                    cellValues[rowIndex, colIndex] = exportProperties[colIndex].GetValue(list[rowIndex]);
                }
            });

            // 批量写入
            for (var batchStart = 0; batchStart < list.Count; batchStart += batchSize)
            {
                var batchEnd = Math.Min(batchStart + batchSize, list.Count);
                for (var i = batchStart; i < batchEnd; i++)
                {
                    var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                    for (var j = 0; j < exportProperties.Length; j++)
                    {
                        WriteValueToCell(workbook, dataRow, j, cellValues[i, j],
                            exportProperties[j].PropertyType,
                            styleCache);
                    }
                }
            }
        }
        else
        {
            // 顺序处理小数据集
            for (var i = 0; i < list.Count; i++)
            {
                var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                for (var j = 0; j < exportProperties.Length; j++)
                {
                    var property = exportProperties[j];
                    WriteValueToCell(workbook, dataRow, j, property.GetValue(list[i]),
                        property.PropertyType,
                        styleCache);
                }
            }
        }
    }
}
