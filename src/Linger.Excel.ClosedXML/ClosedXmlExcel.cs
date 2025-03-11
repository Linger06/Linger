using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Utils;
using Linger.Extensions.Core;
using Microsoft.Extensions.Logging;

namespace Linger.Excel.ClosedXML;

/// <summary>
/// 基于ClosedXML的Excel处理实现
/// </summary>
public class ClosedXmlExcel : ExcelBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">Excel配置选项</param>
    /// <param name="logger">日志记录器</param>
    public ClosedXmlExcel(ExcelOptions? options = null, ILogger<ClosedXmlExcel>? logger = null)
        : base(options, logger)
    {
    }

    /// <summary>
    /// 将对象集合转换为MemoryStream
    /// </summary>
    //[return: NotNullIfNotNull(nameof(list))]
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T>? list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    {
        if (list == null //|| list.Count == 0
            )
        {
            Logger?.LogWarning("要转换的集合为空");
            return null;
        }

        return SafeExecute(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetsName);

            // 获取可读属性
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .ToArray();

            if (properties.Length == 0)
            {
                Logger?.LogWarning("类型 {Type} 没有可读属性", typeof(T).Name);
                return new MemoryStream();
            }

            // 处理标题
            var currentRow = 1;
            if (!string.IsNullOrEmpty(title))
            {
                var cell = worksheet.Cell(currentRow, 1);
                cell.Value = title;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 14;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(1, 1, 1, properties.Length).Merge();
                currentRow++;
            }

            // 设置表头
            for (var i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(currentRow, i + 1).Value = properties[i].Name;
                worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
            }

            // 使用基类的批处理方法处理数据行
            ProcessInBatches<IXLCell, object?>(
                list.Count,
                i => worksheet.Cell(i + currentRow + 1, 1),
                i => properties.Select(p => p.GetValue(list[i])).ToArray(),
                (cell, rowIndex, values, _) =>
                {
                    for (var j = 0; j < properties.Length; j++)
                    {
                        var currentCell = worksheet.Cell(rowIndex + currentRow + 1, j + 1);
                        WriteValueToCell(currentCell, values[j]);
                    }
                }
            );

            // 执行自定义操作
            action?.Invoke(worksheet, properties);

            // 根据配置应用格式化
            if (Options.AutoFitColumns)
            {
                ApplyBasicFormatting(worksheet, list.Count + currentRow, properties.Length);
            }

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
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
            return MonitorPerformance("DataTable导出到Excel", () => {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(sheetsName);

                // 处理标题
                var currentRow = 1;
                if (!string.IsNullOrEmpty(title))
                {
                    var cell = worksheet.Cell(currentRow, 1);
                    cell.Value = title;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontSize = 14;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(1, 1, 1, dataTable.Columns.Count).Merge();
                    currentRow++;
                }

                // 设置表头
                for (var i = 0; i < dataTable.Columns.Count; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = dataTable.Columns[i].ColumnName;
                    worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                }

                // 判断是否需要并行处理
                bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;

                if (useParallelProcessing)
                {
                    Logger?.LogDebug("使用并行处理导出 {Count} 行数据", dataTable.Rows.Count);

                    // 使用批处理提高性能
                    int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;

                    // 预先计算所有值以避免在多线程中重复计算
                    var cellValues = new object[dataTable.Rows.Count, dataTable.Columns.Count];

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
                                var cell = worksheet.Cell(i + currentRow + 1, j + 1);
                                WriteValueToCell(cell, cellValues[i, j]);
                            }
                        }
                    }
                }
                else
                {
                    // 小数据集顺序处理
                    for (var i = 0; i < dataTable.Rows.Count; i++)
                    {
                        for (var j = 0; j < dataTable.Columns.Count; j++)
                        {
                            var cell = worksheet.Cell(i + currentRow + 1, j + 1);
                            WriteValueToCell(cell, dataTable.Rows[i][j]);
                        }
                    }
                }

                // 执行自定义操作
                action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

                // 根据配置应用格式化
                if (Options.AutoFitColumns)
                {
                    ApplyBasicFormatting(worksheet, dataTable.Rows.Count + currentRow, dataTable.Columns.Count);
                }

                var ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;
                return ms;
            });
        }, null, nameof(ConvertDataTableToMemoryStream));
    }

    /// <summary>
    /// 将Stream转换为DataTable
    /// </summary>
    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            Logger?.LogWarning("无效的Stream: 不可读或长度为0");
            return null;
        }

        return SafeExecute(() =>
        {
            // 创建新的流以避免流位置问题
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);

            // 获取工作表
            IXLWorksheet worksheet;
            if (sheetName.IsNullOrEmpty())
            {
                worksheet = workbook.Worksheet(1);
                Logger?.LogDebug("使用第一个工作表, 名称: {SheetName}", worksheet.Name);
            }
            else if (workbook.Worksheets.TryGetWorksheet(sheetName, out var namedSheet))
            {
                worksheet = namedSheet;
            }
            else
            {
                worksheet = workbook.Worksheet(1);
                Logger?.LogWarning("未找到指定的工作表 {SheetName}, 使用第一个工作表代替", sheetName);
            }

            // 创建DataTable
            var dt = new DataTable(worksheet.Name);

            // 检查是否有数据
            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                Logger?.LogWarning("工作表 {SheetName} 没有数据", worksheet.Name);
                return dt;
            }

            // 处理行数据
            var rows = usedRange.RowsUsed().ToList();
            if (rows.Count <= headerRowIndex)
            {
                Logger?.LogWarning("工作表 {SheetName} 的行数 {RowCount} 小于或等于表头行号 {HeaderRowIndex}",
                    worksheet.Name, rows.Count, headerRowIndex);
                return dt;
            }

            // 处理表头
            var headerRow = headerRowIndex >= 0
                ? rows.FirstOrDefault(r => r.RowNumber() == headerRowIndex + 1)
                : null;

            if (headerRow == null && headerRowIndex >= 0)
            {
                // 如果找不到表头行但指定了表头行号，使用默认列名
                headerRowIndex = -1;
            }

            // 读取列名
            var columnCount = rows.Max(r => r.CellsUsed().Count());

            if (headerRowIndex < 0)
            {
                // 使用默认列名
                for (int i = 0; i < columnCount; i++)
                {
                    dt.Columns.Add($"Column{i + 1}");
                }
            }
            else
            {
                // 使用表头行作为列名
                foreach (var cell in headerRow!.CellsUsed())
                {
                    var columnName = cell.Value.ToString();
                    if (string.IsNullOrEmpty(columnName))
                    {
                        columnName = $"Column{cell.Address.ColumnNumber}";
                    }
                    else if (dt.Columns.Contains(columnName))
                    {
                        columnName = $"{columnName}_{cell.Address.ColumnNumber}";
                    }

                    dt.Columns.Add(columnName);
                }
            }

            // 读取数据行
            var startRow = headerRowIndex < 0 ? 0 : headerRowIndex + 1;
            foreach (var row in rows.Where(r => r.RowNumber() > startRow))
            {
                var dataRow = dt.NewRow();
                var hasData = false;

                foreach (var cell in row.CellsUsed())
                {
                    int colIndex = cell.Address.ColumnNumber - 1;
                    if (colIndex >= dt.Columns.Count) continue;

                    // 修改 ConvertStreamToDataTable 方法中的值转换逻辑，使用基类的 GetExcelCellValue 方法
                    object cellValue;
                    bool isDateFormat = cell.DataType == XLDataType.DateTime;
                    cellValue = GetExcelCellValue(
                        cell.DataType == XLDataType.DateTime ? cell.GetDateTime() :
                        cell.DataType == XLDataType.Number ? cell.GetDouble() :
                        cell.DataType == XLDataType.Boolean ? cell.GetBoolean() :
                        cell.DataType == XLDataType.Text ? cell.GetString() :
                        cell.Value,
                        isDateFormat);

                    dataRow[colIndex] = cellValue;
                    if (cellValue != DBNull.Value)
                    {
                        hasData = true;
                    }
                }

                // 只添加有数据的行和指定添加空行的情况
                if (hasData || addEmptyRow)
                {
                    dt.Rows.Add(dataRow);
                }
            }

            return dt;
        }, new DataTable(), nameof(ConvertStreamToDataTable));
    }

    /// <summary>
    /// 扩展的流式读取方法 - 适用于大文件
    /// </summary>
    public IEnumerable<T> StreamReadExcel<T>(Stream stream, string? sheetName = null,
        int headerRowIndex = 0) where T : class, new()
    {
        if (stream == null || stream.Length == 0)
            yield break;

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        using var workbook = new XLWorkbook(memoryStream);

        IXLWorksheet worksheet;
        if (sheetName.IsNullOrEmpty())
        {
            worksheet = workbook.Worksheet(1);
        }
        else if (workbook.Worksheets.TryGetWorksheet(sheetName, out var namedSheet))
        {
            worksheet = namedSheet;
        }
        else
        {
            worksheet = workbook.Worksheet(1);
        }

        // 获取使用范围
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null) yield break;

        // 读取表头并创建属性映射
        var columnMappings = GetPropertyMappings<T>(worksheet, headerRowIndex);

        // 流式读取数据
        var startRow = headerRowIndex < 0 ? 1 : headerRowIndex + 2;
        foreach (var row in worksheet.RowsUsed()
            .Where(r => r.RowNumber() >= startRow))
        {
            var item = new T();
            bool hasData = false;

            foreach (var cell in row.CellsUsed())
            {
                int colIndex = cell.Address.ColumnNumber;
                if (columnMappings.TryGetValue(colIndex, out var property))
                {
                    bool isDateFormat = cell.DataType == XLDataType.DateTime;
                    // 使用基类的 GetExcelCellValue 方法，而不是直接调用 ExcelValueConverter.ConvertToDbValue
                    var value = GetExcelCellValue(
                        cell.DataType == XLDataType.DateTime ? cell.GetDateTime() :
                        cell.DataType == XLDataType.Number ? cell.GetDouble() :
                        cell.DataType == XLDataType.Boolean ? cell.GetBoolean() :
                        cell.DataType == XLDataType.Text ? cell.GetString() :
                        cell.Value,
                        isDateFormat);

                    if (value != DBNull.Value)
                    {
                        SetPropertySafely(item, property, value);
                        hasData = true;
                    }
                }
            }

            if (hasData)
                yield return item;

            // 使用更温和的GC方式
            if (row.RowNumber() % Options.MemoryBufferSize == 0 && Options.UseMemoryOptimization)
            {
                GC.Collect(0, GCCollectionMode.Optimized);
            }
        }
    }

    private Dictionary<int, PropertyInfo> GetPropertyMappings<T>(IXLWorksheet worksheet, int headerRowIndex)
    {
        var result = new Dictionary<int, PropertyInfo>();
        if (headerRowIndex < 0) return result;

        var headerRow = worksheet.Row(headerRowIndex + 1);
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

    #region 私有辅助方法

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
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                cell.Style.NumberFormat.Format = doubleValue % 1 == 0 ? "#,##0" : "#,##0.00";
                break;
            case float floatValue:
                cell.Value = floatValue;
                cell.Style.NumberFormat.Format = floatValue % 1 == 0 ? "#,##0" : "#,##0.00";
                break;
            case int or long or short or byte or sbyte or ushort or uint or ulong:
                cell.Value = Convert.ToInt64(value);
                cell.Style.NumberFormat.Format = "#,##0";
                break;
            default:
                cell.Value = value.ToString();
                break;
        }

        // 应用边框
        DrawBorder(cell);
    }

    /// <summary>
    /// 设置Excel单元格格式和边框
    /// </summary>
    private void ApplyBasicFormatting(IXLWorksheet worksheet, int rowCount, int columnCount)
    {
        // 设置标题行样式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

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
}
