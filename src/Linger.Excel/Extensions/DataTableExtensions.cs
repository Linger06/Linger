using System.Data;
using System.Text;
using Linger.Extensions.Core;
using OfficeOpenXml;

namespace Linger.Excel.Extensions;

/// <summary>
///     <see cref="DataTable" /> 扩展
/// </summary>
public static class DataTableExtensions
{
    #region 将当前 DataTable 对象写入 MemoryStream

    /// <summary>
    ///     将当前 <see cref="DataTable" /> 写入 <see cref="MemoryStream" />
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public static MemoryStream? WriteToMemoryStream(this DataTable dataTable,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string sheetsName = "sheet1")
    {
        return new Excel().WriteToMemoryStream(dataTable, action, sheetsName);
    }

    #endregion 将当前 DataTable 对象写入 MemoryStream

    #region 将当前 DataTable 对象用异步方式写入 MemoryStream

    /// <summary>
    ///     将当前 <see cref="DataTable" /> 对象用异步方式写入 <see cref="MemoryStream" />
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public static async Task<MemoryStream?> WriteToMemoryStreamAsync(this DataTable dataTable,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string sheetsName = "sheet1")
    {
        return await new Excel().WriteToMemoryStreamAsync(dataTable, action, sheetsName).ConfigureAwait(false);
    }

    #endregion 将当前 DataTable 对象用异步方式写入 MemoryStream

    #region 将当前 DataTable 对象写入 File

    /// <summary>
    ///     将当前 <see cref="DataTable" /> 写入 <see cref="File" />
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="fullFileName"></param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 <see cref="File" /> 对象</returns>
    public static string ExportToFile(this DataTable dataTable, string fullFileName,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string sheetName = "sheet1")
    {
        return new Excel().ExportToFile(dataTable, fullFileName, action, sheetName);
    }

    #endregion 将当前 DataTable 对象写入 File

    #region 将 DataTable 写入 Flat File

    /// <summary>
    ///     将DataTable里面的内容写入 txt 文件
    /// </summary>
    /// <param name="dt">数据表</param>
    /// <param name="fileName">文件名，全路径，建议以.txt为后缀</param>
    /// <param name="encoding">文件编码格式</param>
    /// <param name="fieldsTerminated">字段分隔符，默认为\t制表符</param>
    /// <returns></returns>
    public static void ExportToFlatFile(this DataTable dt, string fileName, Encoding? encoding = null,
        string fieldsTerminated = "\t")
    {
        if (dt.IsNull())
        {
            return;
        }

        var columnNames = new List<string>();
        //写数据文件
        using var streamWriter = new StreamWriter(fileName, false, encoding ?? Encoding.UTF8);
        for (var i = 0; i < dt.Columns.Count; i++)
        {
            if (i > 0 && i < dt.Columns.Count)
            {
                streamWriter.Write(fieldsTerminated);
            }

            var data = dt.Columns[i].ColumnName; //写出列名称
            columnNames.Add(data);
            streamWriter.Write(data);
        }

        streamWriter.WriteLine();

        foreach (DataRow dr in dt.Rows)
        {
            var col = 0;
            foreach (var column in columnNames)
            {
                if (col > 0)
                {
                    streamWriter.Write(fieldsTerminated);
                }

                var columnValue = dr[column];

                if (dt.Columns[column]!.DataType == typeof(DateTime))
                {
                    if (columnValue == DBNull.Value)
                    {
                        //streamWriter.Write("0000-00-00 00:00:00");
                    }
                    else
                    {
                        streamWriter.Write(Convert.ToString(columnValue)?.Replace('\t', ' ').Replace('\r', ' ')
                            .Replace('\n', ' '));

                        //streamWriter.Write(((DateTime)dr[column]).ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
                else
                {
                    streamWriter.Write(Convert.ToString(columnValue)
                        ?.Replace('\t', ' ').Replace('\r', ' ')
                        .Replace('\n', ' '));
                }

                col++;
            }

            streamWriter.WriteLine();
            streamWriter.Flush();
        }

        streamWriter.Close();
    }

    #endregion 将 DataTable 写入 Flat File
}