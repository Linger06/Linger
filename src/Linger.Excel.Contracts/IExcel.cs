namespace Linger.Excel.Contracts;

public interface IExcel<TWorksheet> where TWorksheet : class
{
    #region Import
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);
    DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为对象列表 - 适用于中小型Excel文件
    /// </summary>
    /// <typeparam name="T">要转换的对象类型</typeparam>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">表头行索引</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>对象列表</returns>
    /// <remarks>此方法将Excel数据一次性全部加载到内存，如果处理大文件请考虑使用StreamReadExcel</remarks>
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    /// <summary>
    /// 将Stream转换为对象列表 - 适用于中小型Excel文件
    /// </summary>
    /// <typeparam name="T">要转换的对象类型</typeparam>
    /// <param name="stream">Excel文件流</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">表头行索引</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>对象列表</returns>
    /// <remarks>此方法将Excel数据一次性全部加载到内存，如果处理大文件请考虑使用StreamReadExcel</remarks>
    List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();
    #endregion

    #region Export
    /// <summary>
    /// 数据表格转 Excel 文件
    /// </summary>
    string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "sheet1", string title = "", Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 列表转 Excel 文件
    /// </summary>
    string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "sheet1", string title = "", Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 列表转 Excel 内存流
    /// </summary>
    MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "sheet1", string title = "", Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 数据表格转 Excel 内存流
    /// </summary>
    MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "sheet1", string title = "", Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 异步将DataTable导出为Excel文件
    /// </summary>
    Task<string> DataTableToFileAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 异步将对象列表导出为Excel文件
    /// </summary>
    Task<string> ListToFileAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;
    #endregion

    /// <summary>
    /// 流式读取Excel文件 - 适用于大型Excel文件，内存占用低
    /// </summary>
    /// <typeparam name="T">要转换成的对象类型</typeparam>
    /// <param name="stream">Excel文件流</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">表头行索引</param>
    /// <returns>对象序列（惰性加载）</returns>
    /// <remarks>
    /// 此方法采用流式处理，一次只处理一行数据，适合处理大文件。
    /// 返回的IEnumerable采用惰性加载，每次迭代才会读取一行数据。
    /// </remarks>
    //IEnumerable<T> StreamReadExcel<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0) where T : class, new();
}