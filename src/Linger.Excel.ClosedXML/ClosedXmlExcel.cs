using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ClosedXML.Excel;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;

namespace Linger.Excel.ClosedXML;

/// <summary>
/// 基于ClosedXML的Excel处理实现
/// </summary>
public class ClosedXmlExcel : ExcelBase
{
    #region 私有辅助方法

    /// <summary>
    /// 设置Excel单元格格式
    /// </summary>
    private void ApplyBasicFormatting(IXLWorksheet worksheet, int rowCount, int columnCount)
    {
        // 设置标题行样式
        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // 设置所有单元格自动适应宽度
        worksheet.Columns().AdjustToContents();

        // 设置表格边框
        worksheet.Range(1, 1, rowCount, columnCount).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range(1, 1, rowCount, columnCount).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    }

    /// <summary>
    /// 安全地设置单元格值
    /// </summary>
    private void SetCellValue(IXLCell cell, object value)
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
                cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                break;
            case bool boolean:
                cell.Value = boolean;
                break;
            case decimal or double or float:
                cell.Value = value.ToString();
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    /// <summary>
    /// 处理异常并记录日志
    /// </summary>
    private T HandleException<T>(Exception ex, string methodName) where T : class
    {
        // 这里可以添加日志记录逻辑，例如：
        Console.WriteLine($"错误发生在 {methodName}: {ex.Message}");
        return null;
    }

    #endregion

    [return: NotNullIfNotNull(nameof(list))]
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T>? list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null)
    {
        if (list.IsNull())
        {
            return null;
        }

        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetsName);

            // Add title if specified
            var currentRow = 1;
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead)  // 只处理可读属性
                .ToArray();

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

            // Add headers
            for (var i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(currentRow, i + 1).Value = properties[i].Name;
            }

            // 并行处理提升性能，适用于大数据集
            if (list.Count > 1000)
            {
                var rowDataArray = new object[list.Count, properties.Length];

                Parallel.For(0, list.Count, i =>
                {
                    var item = list[i];
                    for (var j = 0; j < properties.Length; j++)
                    {
                        rowDataArray[i, j] = properties[j].GetValue(item);
                    }
                });

                // 批量写入单元格值
                for (var i = 0; i < list.Count; i++)
                {
                    for (var j = 0; j < properties.Length; j++)
                    {
                        var cell = worksheet.Cell(i + currentRow + 1, j + 1);
                        SetCellValue(cell, rowDataArray[i, j]);
                    }
                }
            }
            else
            {
                // 对于小数据集，直接处理
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    for (var j = 0; j < properties.Length; j++)
                    {
                        var cell = worksheet.Cell(i + currentRow + 1, j + 1);
                        SetCellValue(cell, properties[j].GetValue(item));
                    }
                }
            }

            // 执行自定义操作
            action?.Invoke(worksheet, properties);

            // 应用基本格式
            ApplyBasicFormatting(worksheet, list.Count + currentRow, properties.Length);

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
        catch (Exception ex)
        {
            return HandleException<MemoryStream>(ex, nameof(ConvertCollectionToMemoryStream));
        }
    }

    public override MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable.IsNull() || dataTable.Rows.Count == 0)
        {
            return null;
        }

        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetsName);

            // Add title if specified
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

            // Add headers
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                worksheet.Cell(currentRow, i + 1).Value = dataTable.Columns[i].ColumnName;
            }

            // 对于大数据表使用并行处理
            if (dataTable.Rows.Count > 1000)
            {
                var rowDataArray = new object[dataTable.Rows.Count, dataTable.Columns.Count];

                Parallel.For(0, dataTable.Rows.Count, i =>
                {
                    for (var j = 0; j < dataTable.Columns.Count; j++)
                    {
                        rowDataArray[i, j] = dataTable.Rows[i][j];
                    }
                });

                // 批量写入单元格值
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (var j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var cell = worksheet.Cell(i + currentRow + 1, j + 1);
                        SetCellValue(cell, rowDataArray[i, j]);
                    }
                }
            }
            else
            {
                // 对于小数据集直接处理
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (var j = 0; j < dataTable.Columns.Count; j++)
                    {
                        var cell = worksheet.Cell(i + currentRow + 1, j + 1);
                        SetCellValue(cell, dataTable.Rows[i][j]);
                    }
                }
            }

            // 执行自定义操作
            action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

            // 应用基本格式
            ApplyBasicFormatting(worksheet, dataTable.Rows.Count + currentRow, dataTable.Columns.Count);

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return ms;
        }
        catch (Exception ex)
        {
            return HandleException<MemoryStream>(ex, nameof(ConvertDataTableToMemoryStream));
        }
    }

    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        try
        {
            using var workbook = new XLWorkbook(stream);
            IXLWorksheet worksheet;

            if (string.IsNullOrEmpty(sheetName))
            {
                worksheet = workbook.Worksheet(1);
            }
            else if (workbook.Worksheets.TryGetWorksheet(sheetName, out var namedSheet))
            {
                worksheet = namedSheet;
            }
            else
            {
                // 如果找不到指定名称的工作表，则使用第一个工作表
                worksheet = workbook.Worksheet(1);
            }

            var dt = new DataTable();
            var firstRow = true;
            var startRow = headerRowIndex + 1;
            var usedRange = worksheet.RangeUsed();

            if (usedRange == null)
            {
                return dt; // 返回空表
            }

            // 预先获取范围以提高性能
            var rows = usedRange.RowsUsed().ToList();
            if (!rows.Any() || rows[0].RowNumber() < startRow)
            {
                return dt;
            }

            // 处理表头
            var headerRow = rows.First(r => r.RowNumber() >= startRow);
            foreach (IXLCell cell in headerRow.CellsUsed())
            {
                var columnName = cell.Value.ToString();
                dt.Columns.Add(columnName.IsNullOrEmpty() ? $"Column{cell.Address.ColumnNumber}" : columnName);
            }

            // 处理数据行
            foreach (var row in rows.Where(r => r.RowNumber() > headerRow.RowNumber()))
            {
                var toInsert = dt.NewRow();
                var i = 0;
                foreach (IXLCell cell in row.Cells(1, dt.Columns.Count))
                {
                    if (i >= dt.Columns.Count) break;

                    // 根据单元格的数据类型进行转换
                    switch (cell.DataType)
                    {
                        case XLDataType.DateTime:
                            toInsert[i] = cell.GetDateTime();
                            break;
                        case XLDataType.Number:
                            toInsert[i] = cell.GetDouble();
                            break;
                        case XLDataType.Boolean:
                            toInsert[i] = cell.GetBoolean();
                            break;
                        case XLDataType.Text:
                            toInsert[i] = cell.GetString();
                            break;
                        default:
                            toInsert[i] = cell.Value;
                            break;
                    }
                    i++;
                }
                dt.Rows.Add(toInsert);
            }

            if (addEmptyRow && dt.Rows.Count > 0)
            {
                dt.Rows.Add(dt.NewRow());
            }

            return dt;
        }
        catch (Exception ex)
        {
            return HandleException<DataTable>(ex, nameof(ConvertStreamToDataTable));
        }
    }

    public override List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        var dt = ConvertStreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
        if (dt == null || dt.Rows.Count == 0) return new List<T>();

        try
        {
            var list = new List<T>(dt.Rows.Count);
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanWrite)  // 只处理可写属性
                .ToArray();

            var propertyDict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

            // 创建属性字典以提高查找效率 - 使用不区分大小写的比较器
            foreach (var prop in properties)
            {
                propertyDict[prop.Name] = prop;
            }

            // 创建列到属性的映射
            var columnMappings = new Dictionary<int, PropertyInfo>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string columnName = dt.Columns[i].ColumnName;
                if (propertyDict.TryGetValue(columnName, out var prop))
                {
                    columnMappings[i] = prop;
                }
            }

            // 使用并行处理提高大数据集性能
            if (dt.Rows.Count > 1000)
            {
                var resultList = new T[dt.Rows.Count];

                Parallel.For(0, dt.Rows.Count, i =>
                {
                    var item = Activator.CreateInstance<T>();
                    var row = dt.Rows[i];

                    foreach (var mapping in columnMappings)
                    {
                        int colIndex = mapping.Key;
                        PropertyInfo prop = mapping.Value;

                        if (row[colIndex] is DBNull) continue;

                        try
                        {
                            SetPropertyValue(item, prop, row[colIndex]);
                        }
                        catch
                        {
                            // 忽略转换错误
                        }
                    }

                    resultList[i] = item;
                });

                list.AddRange(resultList);
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    var item = Activator.CreateInstance<T>();

                    foreach (var mapping in columnMappings)
                    {
                        int colIndex = mapping.Key;
                        PropertyInfo prop = mapping.Value;

                        if (row[colIndex] is DBNull) continue;

                        try
                        {
                            SetPropertyValue(item, prop, row[colIndex]);
                        }
                        catch
                        {
                            // 忽略转换错误
                        }
                    }

                    list.Add(item);
                }
            }

            return list;
        }
        catch (Exception ex)
        {
            return HandleException<List<T>>(ex, nameof(ConvertStreamToList));
        }
    }

    /// <summary>
    /// 根据属性类型安全设置属性值
    /// </summary>
    private void SetPropertyValue<T>(T item, PropertyInfo prop, object value)
    {
        if (value == null || value is DBNull)
            return;

        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        try
        {
            if (propType.IsEnum)
            {
                // 处理枚举类型
                if (int.TryParse(value.ToString(), out int enumValue))
                {
                    prop.SetValue(item, Enum.ToObject(propType, enumValue));
                }
                else
                {
                    prop.SetValue(item, Enum.Parse(propType, value.ToString(), true));
                }
            }
            else if (propType == typeof(Guid))
            {
                // 处理GUID
                prop.SetValue(item, value is Guid guid ? guid : Guid.Parse(value.ToString()));
            }
            else if (propType == typeof(DateTime))
            {
                // 处理日期时间
                if (value is DateTime dateTime)
                {
                    prop.SetValue(item, dateTime);
                }
                else if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                {
                    prop.SetValue(item, parsedDate);
                }
            }
            else if (propType == typeof(bool))
            {
                // 处理布尔值
                if (value is bool boolValue)
                {
                    prop.SetValue(item, boolValue);
                }
                else
                {
                    string strValue = value.ToString().ToLower();
                    prop.SetValue(item, strValue == "true" || strValue == "1" || strValue == "yes" || strValue == "y");
                }
            }
            else
            {
                // 处理其他基本类型
                prop.SetValue(item, Convert.ChangeType(value, propType));
            }
        }
        catch
        {
            // 忽略转换错误
        }
    }
}
