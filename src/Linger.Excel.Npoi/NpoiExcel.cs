using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using Linger.Excel.Contracts;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
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

            // 判断是否使用并行处理
            bool useParallelProcessing = list.Count > Options.ParallelProcessingThreshold;
            
            if (useParallelProcessing)
            {
                // 并行处理大数据集
                var cellValues = new object[list.Count, properties.Length];
                
                // 并行填充数据
                Parallel.For(0, list.Count, i =>
                {
                    var item = list[i];
                    for (int j = 0; j < properties.Length; j++)
                    {
                        var value = properties[j].GetValue(item);
                        cellValues[i, j] = value ?? DBNull.Value;
                        
                        // 计算列宽
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
                });
                
                // 批量写入
                int batchSize = Options.UseBatchWrite ? Options.BatchSize : list.Count;
                for (int batchStart = 0; batchStart < list.Count; batchStart += batchSize)
                {
                    int batchEnd = Math.Min(batchStart + batchSize, list.Count);
                    for (int i = batchStart; i < batchEnd; i++)
                    {
                        var dataRow = sheet.CreateRow(i + titleIndex);
                        for (int j = 0; j < properties.Length; j++)
                        {
                            var value = cellValues[i, j];
                            WriteValueToCell(workbook, dataRow, j, value, properties[j].PropertyType, styleCache);
                        }
                    }
                }
            }
            else
            {
                // 顺序处理小数据集
                for (int i = 0; i < list.Count; i++)
                {
                    var dataRow = sheet.CreateRow(i + titleIndex);
                    for (int j = 0; j < properties.Length; j++)
                    {
                        var value = properties[j].GetValue(list[i]);
                        WriteValueToCell(workbook, dataRow, j, value, properties[j].PropertyType, styleCache);
                        
                        // 计算列宽
                        if (value != null)
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
            IWorkbook workbook;
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
        }, null, nameof(ConvertStreamToDataTable));
    }

    /// <summary>
    /// 将Stream转换为对象列表
    /// </summary>
    public override List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        DataTable? dataTable = ConvertStreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
        if (dataTable == null)
        {
            return null;
        }

        return SafeExecute(() =>
        {
            var result = new List<T>(dataTable.Rows.Count);
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanWrite)
                .ToArray();
            
            // 创建列名到属性的映射（不区分大小写）
            var propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
            {
                propertyMap[prop.Name] = prop;
            }
            
            // 创建列索引到属性的映射
            var columnMappings = new Dictionary<int, PropertyInfo>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (propertyMap.TryGetValue(dataTable.Columns[i].ColumnName, out var property))
                {
                    columnMappings[i] = property;
                }
            }
            
            // 转换数据行为对象
            bool useParallel = dataTable.Rows.Count > Options.ParallelProcessingThreshold;
            
            if (useParallel)
            {
                var items = new T[dataTable.Rows.Count];
                
                Parallel.For(0, dataTable.Rows.Count, i =>
                {
                    var row = dataTable.Rows[i];
                    var item = new T();
                    
                    // 设置每个匹配的列
                    foreach (var mapping in columnMappings)
                    {
                        int columnIndex = mapping.Key;
                        PropertyInfo property = mapping.Value;
                        var value = row[columnIndex];
                        
                        if (value != DBNull.Value)
                        {
                            // 使用BaseClass的安全属性设置方法
                            SetPropertySafely(item, property, value);
                        }
                    }
                    
                    items[i] = item;
                });
                
                result.AddRange(items);
            }
            else
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    var item = new T();
                    
                    // 设置每个匹配的列
                    foreach (var mapping in columnMappings)
                    {
                        int columnIndex = mapping.Key;
                        PropertyInfo property = mapping.Value;
                        var value = row[columnIndex];
                        
                        if (value != DBNull.Value)
                        {
                            // 使用BaseClass的安全属性设置方法
                            SetPropertySafely(item, property, value);
                        }
                    }
                    
                    result.Add(item);
                }
            }
            
            return result;
        }, new List<T>(), nameof(ConvertStreamToList));
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
        
        // 根据数据类型设置单元格值和样式
        switch (value)
        {
            case string stringValue:
                cell.SetCellValue(stringValue);
                break;
                
            case DateTime dateTime:
                cell.SetCellValue(dateTime);
                // 应用日期样式
                if (!styleCache.TryGetValue(typeof(DateTime), out var dateStyle))
                {
                    dateStyle = workbook.CreateCellStyle();
                    var format = workbook.CreateDataFormat();
                    dateStyle.DataFormat = format.GetFormat(Options.DefaultDateFormat);
                    styleCache[typeof(DateTime)] = dateStyle;
                }
                cell.CellStyle = dateStyle;
                break;
                
            case bool boolValue:
                cell.SetCellValue(boolValue);
                break;
                
            case byte or short or int or long or float or double or decimal:
                cell.SetCellValue(Convert.ToDouble(value));
                break;
                
            default:
                cell.SetCellValue(value.ToString());
                break;
        }
    }

    /// <summary>
    /// 从Excel工作表导入数据到DataTable
    /// </summary>
    private DataTable ImportFromSheet(ISheet sheet, int headerRowIndex = 0, bool addEmptyRow = false)
    {
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
                
                if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
                {
                    columnName = $"Column{i}";
                }
                else
                {
                    columnName = cell.ToString();
                    if (dataTable.Columns.Contains(columnName))
                    {
                        columnName = $"{columnName}_{i}";
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
                        object cellValue = GetCellValue(cell);
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
    }

    /// <summary>
    /// 从Excel单元格获取适当类型的值
    /// </summary>
    private object GetCellValue(ICell cell)
    {
        switch (cell.CellType)
        {
            case CellType.Numeric:
                if (DateUtil.IsCellDateFormatted(cell))
                {
                    return cell.DateCellValue;
                }
                return cell.NumericCellValue;
                
            case CellType.String:
                return cell.StringCellValue;
                
            case CellType.Boolean:
                return cell.BooleanCellValue;
                
            case CellType.Formula:
                try
                {
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(cell))
                            {
                                return cell.DateCellValue;
                            }
                            return cell.NumericCellValue;
                            
                        case CellType.String:
                            return cell.StringCellValue;
                            
                        case CellType.Boolean:
                            return cell.BooleanCellValue;
                            
                        default:
                            return cell.ToString();
                    }
                }
                catch
                {
                    return cell.ToString();
                }
                
            case CellType.Error:
                return ErrorEval.GetText(cell.ErrorCellValue);
                
            case CellType.Blank:
            case CellType.Unknown:
            default:
                return DBNull.Value;
        }
    }

    #endregion
}
