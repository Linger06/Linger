using System.Data;
using System.Reflection;

namespace Linger.Excel.Contracts;

public interface IExcel
{
    #region Import
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false);
    DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true);
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false) where T : new();
    List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true) where T : new();
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
}