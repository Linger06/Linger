using System.Data;
using Linger.Extensions.Collection;
using Linger.Extensions.Core;

namespace Linger.Excel.Contracts;

public abstract class ExcelBase : IExcel
{
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
    public DataTable? ExcelToDataTable(string filePath, string? sheetName = null,
        int columnNameRowIndex = 0,
        bool addEmptyRow = false)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        DataTable? dt = ConvertStreamToDataTable(fileStream, sheetName, columnNameRowIndex, addEmptyRow);
        return dt;
    }

    #endregion 将指定路径的 Excel 文件读取到 DataTable

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
    public List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int columnNameRowIndex = 0,
        bool addEmptyRow = false) where T : new()
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        List<T>? list = ConvertStreamToList<T>(fileStream, sheetName, columnNameRowIndex, addEmptyRow);
        return list;
    }

    #endregion 将指定路径的 Excel 文件读取到 List<T>

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
    public abstract DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int columnNameRowIndex = 0,
        bool addEmptyRow = false, bool dispose = true);

    #endregion 将 Stream 对象读取到 DataTable

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
    public abstract List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int columnNameRowIndex = 0,
        bool addEmptyRow = false, bool dispose = true) where T : new();

    #endregion 将 Stream 对象读取到 List<T>

    #region 将 DataTable 对象写入到 MemoryStream 对象

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="MemoryStream" /> 对象
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <param name="title">标题</param>
    /// <returns>Excel形式的 <see cref="MemoryStream" /> 对象</returns>
    public abstract MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "sheet1", string title = "");

    #endregion 将 DataTable 对象写入到 MemoryStream 对象

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
    public abstract MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "sheet1", string title = "");

    #endregion 将 List 集合写入到 MemoryStream 对象

    #region 将 DataTable 对象导出生成文件

    /// <summary>
    ///     将 <see cref="DataTable" /> 对象写入到 <see cref="File" /> 对象
    /// </summary>
    /// <param name="dataTable">要写入的 <see cref="DataTable" /> 对象</param>
    /// <param name="fullFileName"></param>
    /// <param name="action">用于执行写入 Excel 单元格的委托</param>
    /// <param name="sheetsName">Excel 的工作簿名称</param>
    /// <returns>Excel形式的 <see cref="File" /> 对象</returns>
    public string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "sheet1")
    {
        MemoryStream? ms = ConvertDataTableToMemoryStream(dataTable, sheetsName);
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
}