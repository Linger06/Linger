using System.Data;
using System.Reflection;
using Linger.Excel.Contracts;
using Linger.Excel.Contracts.Attributes;
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Linger.Excel.EPPlus;

public class EPPlusExcel : ExcelBase
{
    private ExcelWorksheet? GetWorksheet(Stream stream, string? sheetName)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        using var pck = new ExcelPackage();
        pck.Load(stream);
        var sheet = sheetName.IsNotNullAndEmpty()
            ? pck.Workbook.Worksheets[sheetName]
            : pck.Workbook.Worksheets.First();

        return sheet;
    }

    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        var sheet = GetWorksheet(stream, sheetName);
        if (sheet == null)
        {
            return null;
        }

        return ConvertSheetToDataTable(sheet, headerRowIndex, addEmptyRow);
    }

    public override List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        var sheet = GetWorksheet(stream, sheetName);
        if (sheet == null)
        {
            return null;
        }

        return ConvertSheetToList<T>(sheet, headerRowIndex);
    }

    public override MemoryStream? ConvertDataTableToMemoryStream(
    DataTable dataTable,
    string sheetsName = "sheet1",
    string title = "",
    Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable.IsNull())
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        using var package = new ExcelPackage();
        ExcelWorksheet? worksheet = package.Workbook.Worksheets.Add(sheetsName);
        ConvertDataTableToSheet(dataTable, worksheet,
            action == null ? null : (ws, cols, rows) => action(ws, cols, rows),
            title);
        package.SaveAs(memoryStream);

        return memoryStream;
    }

    public override MemoryStream? ConvertCollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = "sheet1",
        string title = "",
        Action<object, PropertyInfo[]>? action = null)
    {
        if (list.IsNull())
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        using var package = new ExcelPackage();
        ExcelWorksheet? worksheet = package.Workbook.Worksheets.Add(sheetsName);
        ConvertListToSheet(list, worksheet,
            action == null ? null : (ws, props) => action(ws, props),
            title);
        package.SaveAs(memoryStream);

        return memoryStream;
    }

    /// <summary>
    ///     获取 Excel 列名
    /// </summary>
    /// <param name="properties">要获取列名的 <see cref="PropertyInfo" /> 数组</param>
    /// <returns><see cref="Tuple{String, String, Int}" />属性名,列名(没有设置,就为""),索引</returns>
    private List<Tuple<string, string, int>> GetExcelColumns(IEnumerable<PropertyInfo> properties)
    {
        Type excelColumnAttributeType = typeof(ExcelColumnAttribute);
        var columns = new List<Tuple<string, string, int>>();
        foreach (PropertyInfo item in properties)
        {
            IList<CustomAttributeData> attrs = item.GetCustomAttributesData();

            if (attrs.HasAttribute(excelColumnAttributeType))
            {
                Tuple<string, int> tuple = GetExcelColumnAttributeValue(attrs, excelColumnAttributeType);
                columns.Add(new Tuple<string, string, int>(item.Name, tuple.Item1, tuple.Item2));
            }
        }

        return columns;
    }

    /// <summary>
    ///     获取 <see cref="ExcelColumnAttribute" /> 的值
    /// </summary>
    /// <param name="customs">要获取值的 <see cref="IEnumerable" /></param>
    /// <param name="type">要匹配的类型</param>
    /// <returns>
    ///     如果匹配成功，返回 <see cref="ExcelColumnAttribute" /> 的值；
    ///     如果匹配失败，则返回 <see cref="string.Empty" /> 和 <see cref="int.MaxValue" />。
    /// </returns>
    private static Tuple<string, int> GetExcelColumnAttributeValue(IEnumerable<CustomAttributeData> customs, Type type)
    {
        foreach (CustomAttributeData? attr in customs)
        {
            if (attr.AttributeType == type || attr.ConstructorArguments.Count > 0)
            {
                if (attr.NamedArguments.IsNotNull())
                {
                    CustomAttributeNamedArgument nameMember =
                        attr.NamedArguments.FirstOrDefault(a => a.MemberName == "Name");
                    CustomAttributeNamedArgument indexMember =
                        attr.NamedArguments.FirstOrDefault(a => a.MemberName == "Index");
                    var value = nameMember.TypedValue.Value;
                    var name = value == null ? string.Empty : value.ToString();
                    var index = indexMember.TypedValue.Value?.ToInt() ?? int.MaxValue;
                    return new Tuple<string, int>(name ?? string.Empty, index);
                }
            }
        }

        return new Tuple<string, int>(string.Empty, int.MaxValue);
    }

    #region 获取列名称再Sheet中对应的Index

    /// <summary>
    ///     获取列名称再Sheet中对应的Index
    /// </summary>
    /// <param name="ws"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static int GetColumnByName(ExcelWorksheet ws, string columnName)
    {
        if (ws == null)
        {
            throw new ArgumentNullException(nameof(ws));
        }

        return ws.Cells["1:1"].First(c => c.Value.ToString() == columnName).Start.Column;
    }

    #endregion

    #region 绘制边框

    /// <summary>
    ///     绘制边框
    /// </summary>
    /// <param name="excelStyle">要绘制边框的 <see cref="ExcelWorksheet.Cells" /> 的 <see cref="ExcelBorderStyle" /></param>
    /// <param name="excelBorderStyle"><see cref="ExcelBorderStyle" /> 值</param>
    public static void DrawBorder(ExcelStyle excelStyle, ExcelBorderStyle excelBorderStyle = ExcelBorderStyle.Thin)
    {
        excelStyle.Border.Left.Style = excelBorderStyle;
        excelStyle.Border.Right.Style = excelBorderStyle;
        excelStyle.Border.Top.Style = excelBorderStyle;
        excelStyle.Border.Bottom.Style = excelBorderStyle;
    }

    #endregion 绘制边框

    #region DataTable ==> ExcelWorksheet

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="ExcelWorksheet" /> 对象
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="worksheet"></param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="title">Sheet的表头</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public ExcelWorksheet? ConvertDataTableToSheet(DataTable dataTable, ExcelWorksheet worksheet,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string title = "")
    {
        if (dataTable.IsNull() //|| dataTable.Rows.Count <= 0 //导出Excel时,如果DataTable只是没有数据,有结构,应该输出列名
           )
        {
            return null;
        }

        worksheet.Cells.LoadFromDataTable(dataTable, true);

        worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        var titleIndex = 0;
        DataColumnCollection columns = dataTable.Columns;
        if (title.IsNotNullAndEmpty())
        {
            titleIndex = 1;
            worksheet.Cells[1, 1, 1, columns.Count].Merge = true;
            worksheet.Cells[1, 1].Value = title;
        }

        columns.ForEach((_, columnIndex) =>
        {
            ExcelRange? cell = worksheet.Cells[titleIndex + 1, columnIndex + 1];
            //cell.Value = column.Item2.IsNullOrEmpty() ? column.Item1 : column.Item2;
            DrawBorder(cell.Style);
        });
        var rowIndex = 0;
        foreach (DataRow row in dataTable.Rows)
        {
            var columnIndex = 0;
            foreach (DataColumn column in dataTable.Columns)
            {
                ExcelRange? cell = worksheet.Cells[titleIndex + rowIndex + 2, columnIndex + 1];
                var drValue = row[column].ToString();

                if (string.IsNullOrEmpty(drValue))
                {
                    cell.Value = string.Empty;
                }
                else
                {
                    switch (column.DataType.ToString())
                    {
                        case "System.String": //字符串类型

                            //长数字会被转换为科学计数法,暂时先拿掉
                            //if (drValue.IsDouble())
                            //{
                            //    cell.Value = drValue.ToDouble();
                            //    break;
                            //}

                            cell.Value = drValue;
                            break;

                        case "System.DateTime": //日期类型
                            cell.Value = drValue.ToDateTime(); //.ToFormatDateTime();
                            cell.Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                            break;

                        case "System.Boolean": //布尔型
                            cell.Value = drValue.ToBool();
                            break;

                        case "System.Int16": //整型
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Byte":
                            cell.Value = drValue.ToInt();
                            break;

                        case "System.Decimal": //浮点型
                        case "System.Double":
                            cell.Value = drValue.ToDouble();
                            break;

                        case "System.DBNull": //空值处理
                            cell.Value = string.Empty;
                            break;

                        default:
                            cell.Value = string.Empty;
                            break;
                    }
                }

                columnIndex++;
            }

            rowIndex++;
        }

        action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

        worksheet.Cells.AutoFitColumns();

        return worksheet;
    }

    #endregion

    #region ExcelWorksheet ==> DataTable

    /// <summary>
    ///     ExcelWorksheet ==> DataTable
    /// </summary>
    /// <param name="sheet">指定的 Sheet 工作簿</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns></returns>
    protected DataTable ConvertSheetToDataTable(ExcelWorksheet sheet, int headerRowIndex = 0,
        bool addEmptyRow = false)
    {
        var tbl = new DataTable();

        int startRow;
        if (headerRowIndex < 0)
        {
            foreach (ExcelRangeBase? firstRowCell in sheet.Cells[1, 1, 1, sheet.Dimension.End.Column])
            {
                tbl.Columns.Add($"Column {firstRowCell.Start.Column}");
            }
            startRow = 1;
        }
        else
        {
            //var row = sheet.Row(headerRowIndex + 1);
            //bool hasHeader = true; // adjust it accordingly( I've mentioned that this is a simple approach)
            foreach (ExcelRangeBase? firstRowCell in sheet.Cells[headerRowIndex + 1, 1, headerRowIndex + 1, sheet.Dimension.End.Column])
            {
                tbl.Columns.Add(firstRowCell.Text.Trim());
            }
            startRow = headerRowIndex + 2;
        }


        tbl.TableName = sheet.Name;

        //如果最后一行为空行,就删除最后一行
        sheet.TrimLastEmptyRows();

        for (var rowNum = startRow; rowNum <= sheet.Dimension.End.Row; rowNum++)
        {
            ExcelRange? wsRow = sheet.Cells[rowNum, 1, rowNum, sheet.Dimension.End.Column];
            DataRow row = tbl.NewRow();
            if (wsRow == null)
            {
                if (addEmptyRow)
                {
                    tbl.Rows.Add(row);
                }

                continue;
            }
            foreach (ExcelRangeBase? cell in wsRow)
            {
                row[cell.Start.Column - 1] = cell.Value;
            }

            tbl.Rows.Add(row);
        }

        return tbl;
    }

    #endregion

    #region ExcelWorksheet ==> List<T>

    /// <summary>
    ///     将 ExcelWorksheet 写入到 <see cref="List{T}" />
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="firstRowIsColumnName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<T> ConvertSheetToList<T>(ExcelWorksheet worksheet, int headerRowIndex = 0) where T : new()
    {
        //Func<CustomAttributeData, bool> columnOnly = y => y.AttributeType == typeof(ExcelColumn);
        var columns = typeof(T)
            .GetProperties()
            //.Where(x => x.CustomAttributes.Any(columnOnly))
            .Select(p => new { Property = p, Column = p.Name }).ToList();

        IOrderedEnumerable<int> rows = worksheet.Cells
            .Select(cell => cell.Start.Row)
            .Distinct()
            .OrderBy(x => x);
        var startRowIndex = 0;

        if (headerRowIndex < 0)
        {
            startRowIndex = 0;
        }
        else
        {
            startRowIndex = headerRowIndex + 1;
        }

        IEnumerable<T> collection = rows.Skip(startRowIndex)
            .Select(row =>
            {
                var tNew = new T();
                columns.ForEach(col =>
                {
                    ExcelRange? val = worksheet.Cells[row, GetColumnByName(worksheet, col.Column)];
                    if (val.Value == null)
                    {
                        col.Property.SetValue(tNew, null);
                        return;
                    }

                    if (col.Property.PropertyType == typeof(int))
                    {
                        col.Property.SetValue(tNew, val.GetValue<int>());
                        return;
                    }

                    if (col.Property.PropertyType == typeof(double))
                    {
                        col.Property.SetValue(tNew, val.GetValue<double>());
                        return;
                    }

                    if (col.Property.PropertyType == typeof(DateTime?))
                    {
                        col.Property.SetValue(tNew, val.GetValue<DateTime?>());
                        return;
                    }

                    if (col.Property.PropertyType == typeof(DateTime))
                    {
                        col.Property.SetValue(tNew, val.GetValue<DateTime>());
                        return;
                    }

                    if (col.Property.PropertyType == typeof(bool))
                    {
                        col.Property.SetValue(tNew, val.GetValue<bool>());
                        return;
                    }

                    col.Property.SetValue(tNew, val.GetValue<string>());
                });

                return tNew;
            });
        return collection.ToList();
    }

    #endregion

    #region List<T> ==> ExcelWorksheet

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="ExcelWorksheet" /> 对象
    /// </summary>
    /// <param name="list">要写入的 <see cref="List{T}" /> 对象</param>
    /// <param name="worksheet"></param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="title">Sheet的表头</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public ExcelWorksheet? ConvertListToSheet<T>(List<T> list, ExcelWorksheet worksheet,
        Action<ExcelWorksheet, PropertyInfo[]>? action = null, string title = "")
    {
        if (list.IsNull() //|| list.Count <= 0 //导出Excel时,如果List只是没有数据,应该输出列名
           )
        {
            return null;
        }

        PropertyInfo[] properties = typeof(T).GetProperties();
        IOrderedEnumerable<Tuple<string, string, int>> columns = GetExcelColumns(properties).OrderBy(a => a.Item3);
        var titleIndex = 0;

        worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        if (title.IsNotNullAndEmpty())
        {
            titleIndex = 1;
            worksheet.Cells[1, 1, 1, columns.Count()].Merge = true;
            worksheet.Cells[1, 1].Value = title;
        }

        columns.ForEach((column, columnIndex) =>
        {
            ExcelRange? cell = worksheet.Cells[titleIndex + 1, columnIndex + 1];
            cell.Value = column.Item2.IsNullOrEmpty() ? column.Item1 : column.Item2;
            DrawBorder(cell.Style);
        });

        list.ForEach((row, rowIndex) =>
        {
            columns.ForEach((column, columnIndex) =>
            {
                ExcelRange? cell = worksheet.Cells[titleIndex + rowIndex + 2, columnIndex + 1];
                PropertyInfo? property = properties.Find(a => a.Name == column.Item1);
                var value = property!.GetValue(row);

                if (value != null && property.PropertyType == typeof(DateTime))
                {
                    var date = value.ToString().ToDateTime();
                    if (date == DateTime.MinValue)
                    {
                        value = string.Empty;
                    }
                    else
                    {
                        value = date.ToFormatDateTime();
                    }
                }

                if (value != null && property.PropertyType == typeof(DateTime?))
                {
                    var date = (DateTime?)value;
                    value = date.IsNotNull() ? date.Value.ToFormatDateTime() : string.Empty;
                }

                cell.Value = value;
                DrawBorder(cell.Style);
            });
        });

        action?.Invoke(worksheet, properties);

        worksheet.Cells.AutoFitColumns();

        return worksheet;
    }
    #endregion
}