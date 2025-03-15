using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

public class NpoiExcel(ExcelOptions? options = null, ILogger<NpoiExcel>? logger = null)
    : ExcelBase<IWorkbook, ISheet>(options, logger)
{
    // 添加基类要求的方法实现
    protected override IWorkbook OpenWorkbook(Stream stream)
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

    protected override ISheet GetWorksheet(IWorkbook workbook, string? sheetName)
    {
        if (string.IsNullOrWhiteSpace(sheetName))
        {
            return workbook.GetSheetAt(0);
        }
        else
        {
            return workbook.GetSheet(sheetName) ?? workbook.GetSheetAt(0);
        }
    }

    protected override string GetSheetName(ISheet worksheet)
    {
        return worksheet.SheetName;
    }

    protected override bool HasData(ISheet worksheet)
    {
        return worksheet.LastRowNum > 0;
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

    protected override int GetDataStartRow(ISheet worksheet, int headerRowIndex)
    {
        return headerRowIndex + 1;
    }

    protected override int GetDataEndRow(ISheet worksheet)
    {
        return worksheet.LastRowNum;
    }

    protected override void CloseWorkbook(IWorkbook workbook)
    {
        try
        {
            workbook.Close();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "关闭NPOI工作簿时出错");
        }
    }

    protected override int EstimateColumnCount(ISheet worksheet)
    {
        int maxCellCount = 0;

        // 扫描所有行找到最大列数
        for (int i = worksheet.FirstRowNum; i <= worksheet.LastRowNum; i++)
        {
            var row = worksheet.GetRow(i);
            if (row != null && row.LastCellNum > maxCellCount)
            {
                maxCellCount = row.LastCellNum;
            }
        }

        return maxCellCount;
    }

    protected override Dictionary<int, string> CreateHeaderMappings(ISheet worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, string>();
        var headerRow = worksheet.GetRow(headerRowIndex);

        if (headerRow == null) return result;

        for (int i = headerRow.FirstCellNum; i < headerRow.LastCellNum; i++)
        {
            var cell = headerRow.GetCell(i);
            string columnName = cell?.ToString() ?? $"Column{i + 1}";
            result[i] = columnName;
        }

        return result;
    }

    protected override object GetCellValue(ISheet worksheet, int rowNum, int colIndex)
    {
        var row = worksheet.GetRow(rowNum);

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
                    dateStyle.DataFormat = format.GetFormat(Options.StyleOptions.DataStyle.DateFormat); // 更新为新路径
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
            titleFont.FontHeightInPoints = (short)Options.StyleOptions.TitleStyle.FontSize; // 更新为新路径
            titleFont.IsBold = Options.StyleOptions.TitleStyle.Bold; // 更新为新路径
            titleFont.FontName = Options.StyleOptions.TitleStyle.FontName; // 更新为新路径

            // 设置文字颜色
            if (!string.IsNullOrEmpty(Options.StyleOptions.TitleStyle.FontColor)) // 更新为新路径
            {
                try
                {
                    // 尝试解析HTML颜色代码
                    var colorStr = Options.StyleOptions.TitleStyle.FontColor.TrimStart('#'); // 更新为新路径
                    if (colorStr.Length == 6) // 标准RGB格式
                    {
                        int r = Convert.ToInt32(colorStr.Substring(0, 2), 16);
                        int g = Convert.ToInt32(colorStr.Substring(2, 2), 16);
                        int b = Convert.ToInt32(colorStr.Substring(4, 2), 16);

                        // 将RGB值转换为NPOI的最接近的索引颜色
                        titleFont.Color = ExcelStyleHelper.GetClosestColorIndex(r, g, b);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogDebug(ex, "设置标题文字颜色失败");
                }
            }

            titleStyle.SetFont(titleFont);
            titleRange.CellStyle = titleStyle;
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

            // 设置背景色
            if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.BackgroundColor)) // 更新为新路径
            {
                try
                {
                    // NPOI 使用索引色，这里使用灰色作为表头背景
                    headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                    headerStyle.FillPattern = FillPattern.SolidForeground;
                }
                catch (Exception ex)
                {
                    logger?.LogDebug(ex, "设置表头背景色失败");
                }
            }

            var headerFont = workbook.CreateFont();
            headerFont.FontHeightInPoints = (short)Options.StyleOptions.HeaderStyle.FontSize; // 更新为新路径
            headerFont.IsBold = Options.StyleOptions.HeaderStyle.Bold; // 更新为新路径
            headerFont.FontName = Options.StyleOptions.HeaderStyle.FontName; // 更新为新路径

            // 设置文字颜色
            if (!string.IsNullOrEmpty(Options.StyleOptions.HeaderStyle.FontColor)) // 更新为新路径
            {
                try
                {
                    // 尝试解析HTML颜色代码
                    var colorStr = Options.StyleOptions.HeaderStyle.FontColor.TrimStart('#'); // 更新为新路径
                    if (colorStr.Length == 6) // 标准RGB格式
                    {
                        int r = Convert.ToInt32(colorStr.Substring(0, 2), 16);
                        int g = Convert.ToInt32(colorStr.Substring(2, 2), 16);
                        int b = Convert.ToInt32(colorStr.Substring(4, 2), 16);

                        // 将RGB值转换为NPOI的最接近的索引颜色
                        headerFont.Color = ExcelStyleHelper.GetClosestColorIndex(r, g, b);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogDebug(ex, "设置表头文字颜色失败");
                }
            }

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

    // 在类中添加这些属性来替换常量
    private string INTEGER_FORMAT => Options.StyleOptions.DataStyle.IntegerFormat; // 更新为新路径
    private string DECIMAL_FORMAT => Options.StyleOptions.DataStyle.DecimalFormat; // 更新为新路径

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

        worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, columnCount - 1));
        return 1; // 标题占用1行
    }

    /// <summary>
    /// 创建标题行的核心方法 - 处理共通逻辑
    /// </summary>
    protected override void CreateHeaderRowCore(ISheet worksheet, string[] columnNames, int startRowIndex)
    {
        var headerRow = worksheet.CreateRow(startRowIndex);

        for (int i = 0; i < columnNames.Length; i++)
        {
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(columnNames[i]);
            ApplyHeaderRowFormatting(cell);
        }
    }

    /// <summary>
    /// 处理数据行
    /// </summary>
    protected override void ProcessDataRows(ISheet worksheet, DataTable dataTable, int startRowIndex)
    {
        var sheet = (ISheet)worksheet;
        var workbook = sheet.Workbook;
        bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

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
    protected override void ApplyWorksheetFormatting(ISheet worksheet, int rowCount, int columnCount)
    {
        // 设置所有单元格自动适应宽度
        if (Options.AutoFitColumns)
        {
            for (int i = 0; i < columnCount; i++)
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
            var cellRangeAddress = new CellRangeAddress(1, rowCount, 1, columnCount);
            //var cellRange = GetCellRange(worksheet, new CellRangeAddress(1, 1, rowCount, columnCount));
            SetRegionBorderStyle(BorderStyle.Thin, cellRangeAddress, worksheet);
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
    protected override void ProcessCollectionRows<T>(ISheet worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex)
    {
        var sheet = (ISheet)worksheet;
        var workbook = sheet.Workbook;
        bool useParallelProcessing = list.Count > Options.ParallelProcessingThreshold;

        // 预创建样式字典，提高性能
        var styleCache = new Dictionary<Type, ICellStyle>();

        // 创建日期样式
        var dateStyle = workbook.CreateCellStyle();
        var format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat(Options.StyleOptions.DataStyle.DateFormat); // 更新为新路径
        styleCache[typeof(DateTime)] = dateStyle;

        // 获取有ExcelColumn特性的列，如果没有则使用所有列
        var columns = GetExcelColumns(properties);
        if (columns.Count == 0)
        {
            columns = properties.Select((p, i) => (Name: p.Name, ColumnName: p.Name, Index: i)).ToList();
        }
        columns = columns.OrderBy(c => c.Index).ToList();

        if (useParallelProcessing)
        {
            // 并行处理大数据集
            logger?.LogDebug("使用并行处理导出 {Count} 条记录", list.Count);

            // 使用批处理提高性能
            int batchSize = Options.UseBatchWrite ? Options.BatchSize : list.Count;

            // 预计算所有值
            var cellValues = new object?[list.Count, columns.Count];

            Parallel.For(0, list.Count, rowIndex =>
            {
                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    var property = properties.FirstOrDefault(p => p.Name == columns[colIndex].Name);
                    cellValues[rowIndex, colIndex] = property?.GetValue(list[rowIndex]);
                }
            });

            // 批量写入
            for (int batchStart = 0; batchStart < list.Count; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, list.Count);
                for (int i = batchStart; i < batchEnd; i++)
                {
                    var dataRow = sheet.CreateRow(i + startRowIndex + 1);  // +1跳过表头行
                    for (int j = 0; j < columns.Count; j++)
                    {
                        WriteValueToCell(workbook, dataRow, j, cellValues[i, j],
                            properties.FirstOrDefault(p => p.Name == columns[j].Name)?.PropertyType ?? typeof(string),
                            styleCache);
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
                for (int j = 0; j < columns.Count; j++)
                {
                    var property = properties.FirstOrDefault(p => p.Name == columns[j].Name);
                    WriteValueToCell(workbook, dataRow, j, property?.GetValue(list[i]),
                        property?.PropertyType ?? typeof(string),
                        styleCache);
                }
            }
        }
    }
}
