using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Attributes;
using Linger.Excel.Contracts.Utils;
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Linger.Excel.EPPlus;

/// <summary>
/// 基于EPPlus的Excel处理实现
/// </summary>
public class EPPlusExcel : ExcelBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">Excel配置选项</param>
    /// <param name="logger">日志记录器</param>
    public EPPlusExcel(ExcelOptions? options = null, ILogger<EPPlusExcel>? logger = null)
        : base(options, logger)
    {
        // 设置EPPlus许可模式，避免商业版权问题
    }
    
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
            Logger?.LogWarning("要转换的集合为空");
            return null;
        }

        return SafeExecute(() =>
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
                Logger?.LogWarning("类型 {Type} 没有可读属性", typeof(T).Name);
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
            
            // 设置日期格式
            worksheet.Cells.Style.Numberformat.Format = Options.DefaultDateFormat;
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // 处理标题
            var titleIndex = 0;
            if (!string.IsNullOrEmpty(title))
            {
                titleIndex = 1;
                worksheet.Cells[1, 1, 1, columns.Count].Merge = true;
                worksheet.Cells[1, 1].Value = title;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
            }
            
            // 填充列头
            for (int i = 0; i < columns.Count; i++)
            {
                var cell = worksheet.Cells[titleIndex + 1, i + 1];
                cell.Value = string.IsNullOrEmpty(columns[i].Item2) ? columns[i].Item1 : columns[i].Item2;
                cell.Style.Font.Bold = true;
                DrawBorder(cell.Style);
            }
            
            // 判断是否需要并行处理
            bool useParallelProcessing = list.Count > Options.ParallelProcessingThreshold;
            
            if (useParallelProcessing)
            {
                Logger?.LogDebug("使用并行处理导出 {Count} 条记录", list.Count);
                
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
                            cell.Value = cellValues[i, j];
                            DrawBorder(cell.Style);
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
                            
                            // 处理特殊类型
                            if (value != null)
                            {
                                if (property.PropertyType == typeof(DateTime))
                                {
                                    var dateValue = value is DateTime dateTime ? dateTime : value.ToString().ToDateTime();
                                    cell.Value = dateValue != DateTime.MinValue ? dateValue : null;
                                    if (dateValue != DateTime.MinValue)
                                    {
                                        cell.Style.Numberformat.Format = Options.DefaultDateFormat;
                                    }
                                }
                                else if (property.PropertyType == typeof(DateTime?))
                                {
                                    if (value is DateTime dateValue)
                                    {
                                        cell.Value = dateValue;
                                        cell.Style.Numberformat.Format = Options.DefaultDateFormat;
                                    }
                                    else
                                    {
                                        cell.Value = null;
                                    }
                                }
                                else
                                {
                                    cell.Value = value;
                                }
                            }
                            else
                            {
                                cell.Value = null;
                            }
                        }
                        
                        DrawBorder(cell.Style);
                    }
                }
            }
            
            // 执行自定义操作
            action?.Invoke(worksheet, properties);
            
            // 根据配置决定是否自动调整列宽
            if (Options.AutoFitColumns)
            {
                worksheet.Cells[titleIndex + 1, 1, titleIndex + list.Count + 1, columns.Count].AutoFitColumns();
            }
            
            package.SaveAs(memoryStream);
            memoryStream.Position = 0;
            
            return memoryStream;
        }, null, nameof(ConvertCollectionToMemoryStream));
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
            Logger?.LogWarning("要转换的DataTable为空或没有列");
            return null;
        }

        return SafeExecute(() =>
        {
            var memoryStream = new MemoryStream();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetsName);
            
            // 设置日期格式
            worksheet.Cells.Style.Numberformat.Format = Options.DefaultDateFormat;
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // 处理标题
            var titleIndex = 0;
            if (!string.IsNullOrEmpty(title))
            {
                titleIndex = 1;
                worksheet.Cells[1, 1, 1, dataTable.Columns.Count].Merge = true;
                worksheet.Cells[1, 1].Value = title;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
            }
            
            // 填充列头
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                var cell = worksheet.Cells[titleIndex + 1, i + 1];
                cell.Value = dataTable.Columns[i].ColumnName;
                cell.Style.Font.Bold = true;
                DrawBorder(cell.Style);
            }
            
            // 判断是否需要并行处理
            bool useParallelProcessing = dataTable.Rows.Count > Options.ParallelProcessingThreshold;
            
            if (useParallelProcessing)
            {
                Logger?.LogDebug("使用并行处理导出 {Count} 行数据", dataTable.Rows.Count);
                
                // 使用批处理提高性能
                int batchSize = Options.UseBatchWrite ? Options.BatchSize : dataTable.Rows.Count;
                
                // 预计算所有值
                var cellValues = new object[dataTable.Rows.Count, dataTable.Columns.Count];
                
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
                            var value = cellValues[i, j];
                            
                            if (value != null)
                            {
                                // 特殊类型处理
                                if (value is DateTime dateTime)
                                {
                                    cell.Value = dateTime;
                                    cell.Style.Numberformat.Format = Options.DefaultDateFormat;
                                }
                                else
                                {
                                    cell.Value = value;
                                }
                            }
                            
                            DrawBorder(cell.Style);
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
                        
                        if (value != DBNull.Value)
                        {
                            // 特殊类型处理
                            if (value is DateTime dateTime)
                            {
                                cell.Value = dateTime;
                                cell.Style.Numberformat.Format = Options.DefaultDateFormat;
                            }
                            else
                            {
                                switch (dataTable.Columns[j].DataType.Name)
                                {
                                    case "String":
                                        cell.Value = value.ToString();
                                        break;
                                    case "Boolean":
                                        cell.Value = Convert.ToBoolean(value);
                                        break;
                                    case "Int16":
                                    case "Int32":
                                    case "Int64":
                                    case "Byte":
                                        cell.Value = Convert.ToInt64(value);
                                        break;
                                    case "Decimal":
                                    case "Double":
                                    case "Single":
                                        cell.Value = Convert.ToDouble(value);
                                        break;
                                    default:
                                        cell.Value = value;
                                        break;
                                }
                            }
                        }
                        
                        DrawBorder(cell.Style);
                    }
                }
            }
            
            // 执行自定义操作
            action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);
            
            // 根据配置决定是否自动调整列宽
            if (Options.AutoFitColumns)
            {
                worksheet.Cells[titleIndex + 1, 1, titleIndex + dataTable.Rows.Count + 1, dataTable.Columns.Count].AutoFitColumns();
            }
            
            package.SaveAs(memoryStream);
            memoryStream.Position = 0;
            
            return memoryStream;
        }, null, nameof(ConvertDataTableToMemoryStream));
    }
    
    /// <summary>
    /// 将流转换为DataTable
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
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            
            using var package = new ExcelPackage(memoryStream);
            
            var workbook = package.Workbook;
            if (workbook.Worksheets.Count == 0)
            {
                Logger?.LogWarning("Excel文件不包含任何工作表");
                return new DataTable();
            }
            
            ExcelWorksheet worksheet;
            if (!string.IsNullOrEmpty(sheetName))
            {
                worksheet = workbook.Worksheets[sheetName] ?? workbook.Worksheets[0];
                if (worksheet != workbook.Worksheets[sheetName])
                {
                    Logger?.LogWarning("未找到指定的工作表 {SheetName}, 使用第一个工作表代替", sheetName);
                }
            }
            else
            {
                worksheet = workbook.Worksheets[0];
                Logger?.LogDebug("使用第一个工作表, 名称: {SheetName}", worksheet.Name);
            }
            
            if (worksheet.Dimension == null)
            {
                Logger?.LogWarning("工作表为空");
                return new DataTable(worksheet.Name);
            }
            
            return ConvertWorksheetToDataTable(worksheet, headerRowIndex, addEmptyRow);
        }, new DataTable(), nameof(ConvertStreamToDataTable));
    }
    
    /// <summary>
    /// 流式读取（内存优化）
    /// </summary>
    public IEnumerable<T> StreamReadExcel<T>(Stream stream, string? sheetName = null, 
        int headerRowIndex = 0) where T : class, new()
    {
        if (stream == null || !stream.CanRead)
            yield break;
            
        using var package = new ExcelPackage(stream);
        
        var workbook = package.Workbook;
        if (workbook.Worksheets.Count == 0)
            yield break;
            
        ExcelWorksheet worksheet = GetWorksheet(workbook, sheetName);
        
        if (worksheet?.Dimension == null)
            yield break;
            
        // 读取表头
        var columnMappings = GetColumnMappings<T>(worksheet, headerRowIndex);
        
        // 流式读取数据行
        int startRow = headerRowIndex + 2; // EPPlus从1开始计数，加2表示从表头下一行开始
        for (int rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
        {
            var item = new T();
            bool hasData = false;
            
            foreach (var mapping in columnMappings)
            {
                var cell = worksheet.Cells[rowNum, mapping.Key];
                if (cell?.Value != null)
                {
                    hasData = true;
                    var value = GetTypedCellValue(cell);
                    
                    if (value != DBNull.Value)
                    {
                        SetPropertySafely(item, mapping.Value, value);
                    }
                }
            }
            
            if (hasData)
                yield return item;
        }
    }
    
    private ExcelWorksheet GetWorksheet(ExcelWorkbook workbook, string? sheetName)
    {
        if (!string.IsNullOrEmpty(sheetName))
        {
            return workbook.Worksheets[sheetName] ?? workbook.Worksheets[0];
        }
        return workbook.Worksheets[0];
    }
    
    private Dictionary<int, PropertyInfo> GetColumnMappings<T>(ExcelWorksheet worksheet, int headerRowIndex)
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
                if (!string.IsNullOrEmpty(columnName) && properties.TryGetValue(columnName, out var property))
                {
                    columnMappings[i] = property;
                }
            }
        }
        
        return columnMappings;
    }
    
    #region 私有辅助方法
    
    /// <summary>
    /// 从工作表获取数据表
    /// </summary>
    private DataTable ConvertWorksheetToDataTable(ExcelWorksheet worksheet, int headerRowIndex, bool addEmptyRow)
    {
        var dataTable = new DataTable(worksheet.Name);
        int startRow;
        
        // 处理表头
        if (headerRowIndex < 0)
        {
            // 没有表头，使用默认列名
            startRow = 1;
            int colCount = worksheet.Dimension.End.Column;
            
            for (int i = 1; i <= colCount; i++)
            {
                dataTable.Columns.Add($"Column{i}");
            }
        }
        else
        {
            // 使用指定行作为表头
            startRow = headerRowIndex + 2; // +1是因为EPPlus从1开始计数，再+1是因为我们需要跳过表头行
            int colCount = worksheet.Dimension.End.Column;
            
            for (int i = 1; i <= colCount; i++)
            {
                string columnName = worksheet.Cells[headerRowIndex + 1, i].Text.Trim();
                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = $"Column{i}";
                }
                else if (dataTable.Columns.Contains(columnName))
                {
                    columnName = $"{columnName}_{i}";
                }
                
                dataTable.Columns.Add(columnName);
            }
        }
        
        // 处理数据行
        for (int rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
        {
            var dataRow = dataTable.NewRow();
            bool hasData = false;
            
            for (int colNum = 1; colNum <= dataTable.Columns.Count; colNum++)
            {
                var cell = worksheet.Cells[rowNum, colNum];
                if (cell != null && cell.Value != null)
                {
                    hasData = true;
                    dataRow[colNum - 1] = GetTypedCellValue(cell);
                }
            }
            
            if (hasData || addEmptyRow)
            {
                dataTable.Rows.Add(dataRow);
            }
        }
        
        return dataTable;
    }
    
    /// <summary>
    /// 获取单元格值并转换为适当类型
    /// </summary>
    private static object GetTypedCellValue(ExcelRange cell)
    {
        if (cell.Value == null)
            return DBNull.Value;
            
        // 处理日期格式
        bool isDateFormat = cell.Style.Numberformat.Format.Contains("yy");
        
        // 使用通用转换器
        return ExcelValueConverter.ConvertToDbValue(cell.Value, isDateFormat);
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
                        string.IsNullOrEmpty(attr.ColumnName) ? prop.Name : attr.ColumnName, 
                        attr.Index == int.MaxValue ? columns.Count : attr.Index));
                }
            }
        }
        
        return columns;
    }
    
    /// <summary>
    /// 绘制单元格边框
    /// </summary>
    private static void DrawBorder(ExcelStyle style, ExcelBorderStyle borderStyle = ExcelBorderStyle.Thin)
    {
        style.Border.Top.Style = borderStyle;
        style.Border.Bottom.Style = borderStyle;
        style.Border.Left.Style = borderStyle;
        style.Border.Right.Style = borderStyle;
    }
    
    #endregion
}