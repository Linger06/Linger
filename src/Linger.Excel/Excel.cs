using System.Collections;
using System.Data;
using System.Reflection;
using Linger.Excel.Contracts.Attributes;
using Linger.Excel.Extensions;
using Linger.Extensions.Collection;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Linger.Excel;

/// <summary>
///     提供 Excel 读取、写入功能。//此类不可被继承
/// </summary>
[Obsolete("请使用Linger.Excel.EPPlus")]
public class Excel : IExcel
{
    #region 构造函数

    /// <summary>
    ///     初始化 <see cref="Excel" /> 类的新实例
    /// </summary>
    public Excel() { }

    #endregion

    #region 静态属性

    /// <summary>
    ///     Excel 的 Content-Type
    /// </summary>
    public static string ContentType => "application/vnd.ms-excel";

    #endregion 静态属性

    #region 将指定路径的 Excel 文件读取到 DataTable

    /// <summary>
    ///     将指定路径的 Excel 文件读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="filePath">指定文件完整路径名</param>
    /// <param name="sheetName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 filePath 参数为 null 或者空字符串("")，则返回 null；
    ///     如果 filePath 参数值的磁盘中不存在 Excel 文件，则返回 null；
    ///     否则返回从指定 Excel 文件读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public DataTable? ReadExcelToDataTable(string filePath, string? sheetName = null,
        bool firstRowIsColumnName = true,
        bool addEmptyRow = false)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        DataTable? dt = ReadStreamToDataTable(fileStream, sheetName, firstRowIsColumnName, addEmptyRow);
        return dt;
    }

    #endregion 将指定路径的 Excel 文件读取到 DataTable

    #region 将指定路径的 Excel 文件读取到 ICollection<DataTable>

    /// <summary>
    ///     将指定路径的 Excel 文件读取到 <see cref="ICollection{DataTable}" />
    /// </summary>
    /// <param name="filePath">指定文件完整路径名</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 filePath 参数为 null 或者空字符串("")，则返回 null；
    ///     如果 filePath 参数值的磁盘中不存在 Excel 文件，则返回 null；
    ///     否则返回从指定 Excel 文件读取后的 <see cref="ICollection{DataTable}" /> 对象，
    ///     其中一个 <see cref="DataTable" /> 对应一个 Sheet 工作簿。
    /// </returns>
    public ICollection<DataTable>? ReadExcelToTables(string filePath, bool firstRowIsColumnName = true,
        bool addEmptyRow = false)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        ICollection<DataTable>? tables = ReadStreamToTables(fileStream, firstRowIsColumnName, addEmptyRow);
        return tables;
    }

    #endregion 将指定路径的 Excel 文件读取到 ICollection<DataTable>

    #region 将指定路径的 Excel 文件读取到 DataSet

    /// <summary>
    ///     将指定路径的 Excel 文件读取到 <see cref="DataSet" />
    /// </summary>
    /// <param name="filePath">指定文件完整路径名</param>
    /// <param name="sheetsName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 filePath 参数为 null 或者空字符串("")，则返回 null；
    ///     如果 filePath 参数值的磁盘中不存在 Excel 文件，则返回 null；
    ///     否则返回从指定 Excel 文件读取后的 <see cref="DataSet" /> 对象。
    /// </returns>
    public DataSet? ReadExcelToDataSet(string filePath, string? sheetsName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false)
    {
        ICollection<DataTable>? result = ReadExcelToTables(filePath, firstRowIsColumnName, addEmptyRow);
        if (result != null)
        {
            var dataSet = new DataSet();

            result.ForEach(x =>
            {
                if (sheetsName == null)
                {
                    dataSet.Tables.Add(x);
                }
                else
                {
                    var arraySheetNames = sheetsName.Split(',');
                    if (arraySheetNames.Contains(x.TableName))
                    {
                        dataSet.Tables.Add(x);
                    }
                }
            });
            return dataSet;
        }

        return null;
    }

    #endregion 将指定路径的 Excel 文件读取到 DataSet

    #region 将指定路径的 Excel 文件读取到 List<T>

    /// <summary>
    ///     将指定路径的 Excel 文件读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="filePath">指定文件完整路径名</param>
    /// <param name="sheetName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 filePath 参数为 null 或者空字符串("")，则返回 null；
    ///     如果 filePath 参数值的磁盘中不存在 Excel 文件，则返回 null；
    ///     否则返回从指定 Excel 文件读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public List<T>? ReadExcelToList<T>(string filePath, string? sheetName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false) where T : new()
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        List<T>? list = ReadStreamToList<T>(fileStream, sheetName, firstRowIsColumnName, addEmptyRow);
        return list;
    }

    #endregion 将指定路径的 Excel 文件读取到 List<T>

    #region 将指定路径的 Excel 文件用异步方式读取到 DataTable

    /// <summary>
    ///     将指定路径的 Excel 文件用异步方式读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="filePath">指定文件完整路径名</param>
    /// <param name="sheetName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 filePath 参数为 null 或者空字符串("")，则返回 null；
    ///     如果 filePath 参数值的磁盘中不存在 Excel 文件，则返回 null；
    ///     否则返回从指定 Excel 文件读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public async Task<DataTable?> ReadExcelToDataTableAsync(string filePath, string? sheetName = null,
        bool firstRowIsColumnName = true, bool addEmptyRow = false)
    {
        return await Task.Run(() => ReadExcelToDataTable(filePath, sheetName, firstRowIsColumnName, addEmptyRow))
            .ConfigureAwait(false);
    }

    #endregion 将指定路径的 Excel 文件用异步方式读取到 DataTable

    #region 将指定路径的 Excel 文件用异步方式读取到 ICollection<DataTable>

    /// <summary>
    ///     将指定路径的 Excel 文件用异步方式读取到 <see cref="ICollection{DataTable}" />
    /// </summary>
    /// <param name="filePath">指定文件完整路径名</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns>
    ///     如果 filePath 参数为 null 或者空字符串("")，则返回 null；
    ///     如果 filePath 参数值的磁盘中不存在 Excel 文件，则返回 null；
    ///     否则返回从指定 Excel 文件读取后的 <see cref="ICollection{DataTable}" /> 对象，
    ///     其中一个 <see cref="DataTable" /> 对应一个 Sheet 工作簿。
    /// </returns>
    public async Task<ICollection<DataTable>?> ReadExcelToTablesAsync(string filePath,
        bool firstRowIsColumnName = true,
        bool addEmptyRow = false)
    {
        return await Task.Run(() => ReadExcelToTables(filePath, firstRowIsColumnName, addEmptyRow))
            .ConfigureAwait(false);
    }

    #endregion 将指定路径的 Excel 文件用异步方式读取到 ICollection<DataTable>

    #region 将 Stream 对象读取到 DataTable

    /// <summary>
    ///     将 <see cref="Stream" /> 对象读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="stream">当前 <see cref="Stream" /> 对象</param>
    /// <param name="sheetName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <param name="dispose">是否释放 <see cref="Stream" /> 资源</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性为 小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public DataTable? ReadStreamToDataTable(Stream stream, string? sheetName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        using var pck = new ExcelPackage();
        pck.Load(stream);
        ExcelWorkbook? workbook = pck.Workbook;
        ExcelWorksheet? sheet = workbook.Worksheets.First();
        if (sheetName.IsNotNullAndEmpty())
        {
            sheet = workbook.Worksheets[sheetName];
        }

        if (sheet == null)
        {
            return null;
        }

        DataTable dataTable = ReadSheetToDataTable(sheet, firstRowIsColumnName, addEmptyRow);

        if (dispose)
        {
            stream.Flush();
            stream.Close();
        }

        return dataTable;
    }

    #endregion 将 Stream 对象读取到 DataTable

    #region 将 Stream 对象读取到 ICollection<DataTable>

    /// <summary>
    ///     将 <see cref="Stream" /> 对象读取到 <see cref="ICollection{DataTable}" />
    /// </summary>
    /// <param name="stream">要读取的 <see cref="Stream" /> 对象</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <param name="dispose">是否释放 <see cref="Stream" /> 资源</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="ICollection{DataTable}" /> 对象，
    ///     其中一个 <see cref="DataTable" /> 对应一个 Sheet 工作簿。
    /// </returns>
    public ICollection<DataTable>? ReadStreamToTables(Stream stream, bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        using var pck = new ExcelPackage();
        pck.Load(stream);
        ExcelWorksheets? ws = pck.Workbook.Worksheets;

        var tables = new HashSet<DataTable>();

        foreach (ExcelWorksheet? item in ws)
        {
            if (item == null)
            {
                continue;
            }

            DataTable dataTable = ReadSheetToDataTable(item, firstRowIsColumnName, addEmptyRow);
            tables.Add(dataTable);
        }

        //for (int i = 0; i < ws.Count; i++)
        //{
        //    var sheet = ws[i];
        //    if (sheet == null) continue;

        //    var dataTable = ReadSheetToDataTable(sheet, firstRowIsColumnName, addEmptyRow);
        //    tables.Add(dataTable);
        //}

        if (dispose)
        {
            stream.Flush();
            stream.Close();
        }

        return tables;

        //var workbook = WorkbookFactory.Create(stream);
        //    var tables = new HashSet<DataTable>();
        //    for (int i = 0; i < workbook.NumberOfSheets; i++)
        //    {
        //        var sheet = workbook.GetSheetAt(i);
        //        if (sheet == null) continue;

        //        var dataTable = ReadSheetToDataTable(sheet, firstRowIsColumnName, addEmptyRow);
        //        tables.Add(dataTable);
        //    }

        //    if (dispose)
        //    {
        //        stream.Flush();
        //        stream.Close();
        //    }
        //    return tables;
    }

    #endregion 将 Stream 对象读取到 ICollection<DataTable>

    #region 将 Stream 对象读取到 List<T>

    /// <summary>
    ///     将 <see cref="Stream" /> 对象读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="stream">当前 <see cref="Stream" /> 对象</param>
    /// <param name="sheetName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <param name="dispose">是否释放 <see cref="Stream" /> 资源</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性为 小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public List<T>? ReadStreamToList<T>(Stream stream, string? sheetName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true) where T : new()
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        using var pck = new ExcelPackage();
        pck.Load(stream);
        ExcelWorkbook? workbook = pck.Workbook;
        ExcelWorksheet? sheet = workbook.Worksheets.First();
        if (sheetName.IsNotNullAndEmpty())
        {
            sheet = workbook.Worksheets[sheetName];
        }

        if (sheet == null)
        {
            return null;
        }

        List<T> list = ReadSheetToList<T>(sheet, firstRowIsColumnName);

        if (dispose)
        {
            stream.Flush();
            stream.Close();
        }

        return list;
    }

    #endregion 将 Stream 对象读取到 List<T>

    #region 将 Stream 对象用异步方式读取到 DataTable

    /// <summary>
    ///     将 <see cref="Stream" /> 对象用异步方式读取到 <see cref="DataTable" />
    /// </summary>
    /// <param name="stream">要读取的 <see cref="Stream" /> 对象</param>
    /// <param name="sheetName">指定读取 Excel 工作薄 sheet 的名称</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <param name="dispose">是否释放 <see cref="Stream" /> 资源</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性为 小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="DataTable" /> 对象。
    /// </returns>
    public async Task<DataTable?> ReadStreamToDataTableAsync(Stream stream, string? sheetName = null,
        bool firstRowIsColumnName = true, bool addEmptyRow = false, bool dispose = true)
    {
        return await Task.Run(
                () => ReadStreamToDataTable(stream, sheetName, firstRowIsColumnName, addEmptyRow, dispose))
            .ConfigureAwait(false);
    }

    #endregion 将 Stream 对象用异步方式读取到 DataTable

    #region 将 Stream 对象用异步方式读取到 ICollection<DataTable>

    /// <summary>
    ///     将 <see cref="Stream" /> 对象用异步方式读取到 <see cref="ICollection{DataTable}" />
    /// </summary>
    /// <param name="stream">要读取的 <see cref="Stream" /> 对象</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <param name="dispose">是否释放 <see cref="Stream" /> 资源</param>
    /// <returns>
    ///     如果 stream 参数为 null，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.CanRead" /> 属性为 false，则返回 null；
    ///     如果 stream 参数的 <see cref="Stream.Length" /> 属性小于或者等于 0，则返回 null；
    ///     否则返回从 <see cref="Stream" /> 读取后的 <see cref="ICollection{DataTable}" /> 对象，
    ///     其中一个 <see cref="DataTable" /> 对应一个 Sheet 工作簿。
    /// </returns>
    public async Task<ICollection<DataTable>?> ReadStreamToTablesAsync(Stream stream,
        bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true)
    {
        return await Task.Run(() => ReadStreamToTables(stream, firstRowIsColumnName, addEmptyRow, dispose))
            .ConfigureAwait(false);
    }

    #endregion 将 Stream 对象用异步方式读取到 ICollection<DataTable>

    #region 将 ExcelWorksheet 写入到 List<T>

    /// <summary>
    ///     将 ExcelWorksheet 写入到 <see cref="List{T}" />
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="firstRowIsColumnName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<T> ReadSheetToList<T>(ExcelWorksheet worksheet, bool firstRowIsColumnName = true) where T : new()
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
        if (firstRowIsColumnName)
        {
            startRowIndex = 1;
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

    #endregion 将 ExcelWorksheet 写入到 List<T>

    #region 将 DataTable 对象写入到 ExcelWorksheet 对象

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="ExcelWorksheet" /> 对象
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="worksheet"></param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="title">Sheet的表头</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public ExcelWorksheet? WriteDataTableToSheet(DataTable dataTable, ExcelWorksheet worksheet,
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

    #endregion 将 DataTable 对象写入到 ExcelWorksheet 对象

    #region 将 DataTable 对象写入到 MemoryStream 对象

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="MemoryStream" /> 对象
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <param name="title">标题</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public MemoryStream? WriteToMemoryStream(DataTable dataTable,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string sheetsName = "sheet1",
        string title = "")
    {
        if (dataTable.IsNull() //|| dataTable.Rows.Count <= 0 //导出Excel时,如果DataTable只是没有数据,有结构,应该输出列名
           )
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        using var package = new ExcelPackage();
        ExcelWorksheet? worksheet = package.Workbook.Worksheets.Add(sheetsName);
        WriteDataTableToSheet(dataTable, worksheet, action, title);
        package.SaveAs(memoryStream);

        return memoryStream;
    }

    #endregion 将 DataTable 对象写入到 MemoryStream 对象

    #region 将 DataTable 对象用异步方式写入到 MemoryStream 对象

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象用异步方式写入到 <see cref="MemoryStream" /> 对象
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel 形式的 <see cref="MemoryStream" /> 对象</returns>
    public async Task<MemoryStream?> WriteToMemoryStreamAsync(DataTable dataTable,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string sheetsName = "sheet1")
    {
        return await Task.Run(() => WriteToMemoryStream(dataTable, action, sheetsName))
            .ConfigureAwait(false);
    }

    #endregion 将 DataTable 对象用异步方式写入到 MemoryStream 对象

    #region 将 DataTable 对象导出生成文件

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="File" /> 对象
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="fullFileName"></param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 <see cref="File" /> 对象</returns>
    public string ExportToFile(DataTable dataTable, string fullFileName,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string sheetsName = "sheet1")
    {
        MemoryStream? ms = WriteToMemoryStream(dataTable, action, sheetsName);
        if (ms == null)
        {
            throw new NullReferenceException(nameof(ms));
        }

        using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
        var data = ms.ToArray();
        fs.Write(data, 0, data.Length);
        fs.Flush();

        return fullFileName;
    }

    #endregion 将 DataTable 对象导出生成文件

    #region 将 DataSet 对象导出生成文件

    /// <summary>
    ///     将 <see cref="DataSet" /> 对象写入到 <see cref="File" /> 对象
    /// </summary>
    /// <param name="dataSet">要写入的 <see cref="DataSet" /> 对象</param>
    /// <param name="fullFileName"></param>
    /// <param name="actions">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 <see cref="File" /> 对象</returns>
    public string ExportToFile(DataSet dataSet, string fullFileName,
        List<Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>>? actions = null,
        string? sheetsName = null)
    {
        var sheetNames = new List<string>();
        if (sheetsName != null)
        {
            sheetNames = sheetsName.ToSplitArray().ToList();
        }

        var file = new FileInfo(fullFileName);

        using var excelPackage = new ExcelPackage(file);
        for (var index = 0; index < dataSet.Tables.Count; index++)
        {
            var sheetName = sheetNames.Count != 0 ? sheetNames[index] : dataSet.Tables[index].TableName;
            ExcelWorksheet? worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            WriteDataTableToSheet(dataSet.Tables[index], worksheet, actions?[index]);
        }

        excelPackage.Save();

        return fullFileName;
    }

    #endregion 将 DataSet 对象导出生成文件

    #region 将 DataSet 对象写入到 MemoryStream 对象

    /// <summary>
    ///     将 <see cref="DataSet" /> 对象写入到 <see cref="MemoryStream" /> 对象
    /// </summary>
    /// <param name="dataSet">要写入的 <see cref="DataSet" /> 对象</param>
    /// <param name="actions">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public MemoryStream? ExportToMemoryStream(DataSet dataSet, List<Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>>? actions = null, string? sheetsName = null)
    {
        if (dataSet.IsNull() //|| list.Count <= 0 //导出Excel时,如果DataSet只是没有数据,应该输出列名
           )
        {
            return null;
        }

        var sheetNames = new List<string>();
        if (sheetsName != null)
        {
            sheetNames = sheetsName.ToSplitArray().ToList();
        }

        var memoryStream = new MemoryStream();
        using var excelPackage = new ExcelPackage();
        for (var index = 0; index < dataSet.Tables.Count; index++)
        {
            var sheetName = sheetNames.Count != 0 ? sheetNames[index] : dataSet.Tables[index].TableName;
            ExcelWorksheet? worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            WriteDataTableToSheet(dataSet.Tables[index], worksheet, actions?[index]);
        }
        excelPackage.SaveAs(memoryStream);
        return memoryStream;
    }

    #endregion

    #region 将 List 集合写入到 MemoryStream 对象

    /// <summary>
    ///     将 <see cref="List{T}" /> 集合写入到 <see cref="MemoryStream" /> 对象
    /// </summary>
    /// <typeparam name="T">要写入 <see cref="MemoryStream" /> 的集合元素的类型</typeparam>
    /// <param name="list">要写入的 <see cref="List{T}" /> 集合</param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <param name="title">标题</param>
    /// <returns>Excel 形式的 <see cref="MemoryStream" /> 对象</returns>
    public MemoryStream? WriteToMemoryStream<T>(List<T> list, Action<ExcelWorksheet, PropertyInfo[]>? action = null,
        string sheetsName = "sheet1", string title = "")
    {
        if (list.IsNull() //|| list.Count <= 0 //导出Excel时,如果List只是没有数据,应该输出列名
           )
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        using var package = new ExcelPackage();
        ExcelWorksheet? worksheet = package.Workbook.Worksheets.Add(sheetsName);
        WriteListToSheet(list, worksheet, action, title);
        package.SaveAs(memoryStream);

        return memoryStream;
    }

    #endregion 将 List 集合写入到 MemoryStream 对象

    #region 将 List 集合用异步方式写入到 MemoryStream 对象

    /// <summary>
    ///     将 <see cref="List{T}" /> 集合用异步方式写入到 <see cref="MemoryStream" /> 对象
    /// </summary>
    /// <typeparam name="T">要写入 <see cref="MemoryStream" /> 的集合元素的类型</typeparam>
    /// <param name="list">要写入的 <see cref="List{T}" /> 集合</param>
    /// <param name="action"></param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <param name="title">标题</param>
    /// <returns>Excel 形式的 <see cref="MemoryStream" /> 对象</returns>
    public async Task<MemoryStream?> WriteToMemoryStreamAsync<T>(List<T> list,
        Action<ExcelWorksheet, PropertyInfo[]>? action = null, string sheetsName = "sheet1",
        string title = "")
    {
        return await Task.Run(() => WriteToMemoryStream(list, action, sheetsName, title))
            .ConfigureAwait(false);
    }

    #endregion 将 List 集合用异步方式写入到 MemoryStream 对象

    #region 将 ExcelWorksheet 写入到 DataTable

    /// <summary>
    ///     将 ExcelWorksheet 写入到 DataTable
    /// </summary>
    /// <param name="sheet">指定的 Sheet 工作簿</param>
    /// <param name="firstRowIsColumnName">首行是否为 <see cref="DataColumn.ColumnName" /></param>
    /// <param name="addEmptyRow">是否添加空行，默认为 false，不添加</param>
    /// <returns></returns>
    protected DataTable ReadSheetToDataTable(ExcelWorksheet sheet, bool firstRowIsColumnName = true,
        bool addEmptyRow = false)
    {
        var tbl = new DataTable();
        //bool hasHeader = true; // adjust it accordingly( I've mentioned that this is a simple approach)
        foreach (ExcelRangeBase? firstRowCell in sheet.Cells[1, 1, 1, sheet.Dimension.End.Column])
        {
            tbl.Columns.Add(firstRowIsColumnName
                ? firstRowCell.Text.Trim()
                : $"Column {firstRowCell.Start.Column}");
        }

        tbl.TableName = sheet.Name;

        //如果最后一行为空行,就删除最后一行
        sheet.TrimLastEmptyRows();

        var startRow = firstRowIsColumnName ? 2 : 1;
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

    #endregion 将 ExcelWorksheet 写入到 DataTable

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

    #region 将 List 集合写入到 WorkSheet 对象

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="ExcelWorksheet" /> 对象
    /// </summary>
    /// <param name="list">要写入的 <see cref="List{T}" /> 对象</param>
    /// <param name="worksheet"></param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="title">Sheet的表头</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public ExcelWorksheet? WriteListToSheet<T>(List<T> list, ExcelWorksheet worksheet,
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

    #endregion
}