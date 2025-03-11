using System.Data;
using System.Reflection;

namespace Linger.Excel.Contracts;

public interface IExcel
{
    #region Import
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class,new();
    List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class,new();
    #endregion

    #region Export
    /// <summary>
    /// 数据表格转 Excel 文件
    /// </summary>
    string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null);

    /// <summary>
    /// 列表转 Excel 文件
    /// </summary>
    string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "sheet1", string title = "", Action<object, PropertyInfo[]>? action = null) where T : class;

    /// <summary>
    /// 列表转 Excel 内存流
    /// </summary>
    MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "sheet1", string title = "", Action<object, PropertyInfo[]>? action = null) where T : class;

    /// <summary>
    /// 数据表格转 Excel 内存流
    /// </summary>
    MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null);
    #endregion

    /// <summary>
    /// 流式读取Excel文件（内存优化方式）
    /// </summary>
    /// <typeparam name="T">要转换成的对象类型</typeparam>
    /// <param name="stream">Excel文件流</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">表头行索引</param>
    /// <returns>对象序列</returns>
    IEnumerable<T> StreamReadExcel<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0) where T : class, new();
}