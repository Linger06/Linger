using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;
using Microsoft.Extensions.Logging;
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

    #region 私有辅助方法

    /// <summary>
    /// 将值写入Excel单元格并设置适当的格式
    /// </summary>
    private void WriteValueToCell(IWorkbook workbook, IRow row, int columnIndex, object? value, Type valueType, Dictionary<Type, ICellStyle> styleCache)
    {
        var cell = row.CreateCell(columnIndex);

        if (value is null or DBNull)
        {
            cell.SetCellValue(string.Empty);
            return;
        }

        var actualType = valueType == typeof(object)
            ? value.GetType()
            : Nullable.GetUnderlyingType(valueType) ?? valueType;
        switch (Type.GetTypeCode(actualType))
        {
            case TypeCode.DateTime:
                WriteDateValue(workbook, cell, value, styleCache);
                break;
            case TypeCode.Boolean:
                cell.SetCellValue(value is bool boolean
                    ? boolean
                    : bool.TryParse(value.ToString(), out var parsedBoolean) && parsedBoolean);
                break;
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                cell.SetCellValue(value.ToLongOrDefault());
                cell.CellStyle = GetOrCreateDataStyle(
                    workbook,
                    styleCache,
                    typeof(int),
                    Options.StyleOptions.DataStyle.IntegerFormat);
                break;
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                cell.SetCellValue(value.ToDouble());
                cell.CellStyle = GetOrCreateDataStyle(
                    workbook,
                    styleCache,
                    typeof(double),
                    Options.StyleOptions.DataStyle.DecimalFormat);
                break;
            default:
                cell.SetCellValue(value.ToString());
                break;
        }

        DrawBorder(cell);
    }

    private void WriteDateValue(
        IWorkbook workbook,
        ICell cell,
        object value,
        Dictionary<Type, ICellStyle> styleCache)
    {
        var dateValue = value switch
        {
            DateTime dateTime => dateTime,
            double numericDate => DateTime.FromOADate(numericDate),
            _ when DateTime.TryParse(value.ToString(), out var parsedDate) => parsedDate,
            _ => DateTime.MinValue
        };

        if (dateValue == DateTime.MinValue)
        {
            cell.SetCellValue(string.Empty);
            return;
        }

        cell.SetCellValue(dateValue);
        cell.CellStyle = GetOrCreateDataStyle(
            workbook,
            styleCache,
            typeof(DateTime),
            Options.StyleOptions.DataStyle.DateFormat);
    }

    private static ICellStyle GetOrCreateDataStyle(
        IWorkbook workbook,
        Dictionary<Type, ICellStyle> styleCache,
        Type styleKey,
        string formatString)
    {
        if (styleCache.TryGetValue(styleKey, out var style))
        {
            return style;
        }

        style = workbook.CreateCellStyle();
        style.DataFormat = workbook.CreateDataFormat().GetFormat(formatString);
        styleCache[styleKey] = style;

        return style;
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
    /// 创建表头行样式
    /// </summary>
    private ICellStyle CreateHeaderStyle(IWorkbook workbook)
    {
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
        headerStyle.BorderTop = BorderStyle.Thin;
        headerStyle.BorderBottom = BorderStyle.Thin;
        headerStyle.BorderLeft = BorderStyle.Thin;
        headerStyle.BorderRight = BorderStyle.Thin;

        return headerStyle;
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
        var headerStyle = CreateHeaderStyle(worksheet.Workbook);

        for (var i = 0; i < columnNames.Length; i++)
        {
            var cell = headerRow.CreateCell(i);
            cell.SetCellValue(columnNames[i]);
            cell.CellStyle = headerStyle;
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
        var workbook = worksheet.Workbook;

        var styleCache = new Dictionary<Type, ICellStyle>();

        var columnTypes = new Type[dataTable.Columns.Count];
        for (var i = 0; i < dataTable.Columns.Count; i++)
        {
            columnTypes[i] = dataTable.Columns[i].DataType;
        }

        for (var rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
        {
            var dataRow = worksheet.CreateRow(rowIndex + startRowIndex + 1);
            for (var columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
            {
                var value = dataTable.Rows[rowIndex][columnIndex];
                WriteValueToCell(workbook, dataRow, columnIndex, value, columnTypes[columnIndex], styleCache);
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
        var workbook = worksheet.Workbook;
        var exportProperties = GetExportProperties(properties);

        var styleCache = new Dictionary<Type, ICellStyle>();

        for (var rowIndex = 0; rowIndex < list.Count; rowIndex++)
        {
            var dataRow = worksheet.CreateRow(rowIndex + startRowIndex + 1);
            for (var columnIndex = 0; columnIndex < exportProperties.Length; columnIndex++)
            {
                var property = exportProperties[columnIndex];
                WriteValueToCell(workbook, dataRow, columnIndex, property.GetValue(list[rowIndex]),
                    property.PropertyType,
                    styleCache);
            }
        }
    }

    /// <summary>
    /// 使用显式列定义直接写入集合数据行。
    /// </summary>
    protected override void ProcessCollectionRows<T>(ISheet worksheet, IReadOnlyList<T> items,
        IReadOnlyList<ExcelExportColumn<T>> columns, int startRowIndex)
    {
        var workbook = worksheet.Workbook;
        var styleCache = new Dictionary<Type, ICellStyle>();

        for (var rowIndex = 0; rowIndex < items.Count; rowIndex++)
        {
            var dataRow = worksheet.CreateRow(rowIndex + startRowIndex + 1);
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[columnIndex];
                WriteValueToCell(
                    workbook,
                    dataRow,
                    columnIndex,
                    GetExportValue(column, items[rowIndex]),
                    column.DataType,
                    styleCache);
            }
        }
    }
}
