namespace Linger.Excel.Contracts;

/// <summary>
/// Excel服务非泛型接口 - 提供基本的Excel操作功能
/// </summary>
public interface IExcelService
{
    #region Import

    /// <summary>
    /// 将Excel文件转换为DataTable
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataTable</returns>
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为对象列表
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的对象列表</returns>
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    /// <summary>
    /// 将Stream转换为DataTable
    /// </summary>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataTable</returns>
    DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为对象列表
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的对象列表</returns>
    List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    /// <summary>
    /// 异步将Excel文件转换为DataTable
    /// </summary>
    Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 异步将Excel文件转换为对象列表
    /// </summary>
    Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    #endregion

    #region Export

    /// <summary>
    /// 数据表格转 Excel 文件
    /// </summary>
    string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "");

    /// <summary>
    /// 数据集转 Excel 文件(每个DataTable一个工作表)
    /// </summary>
    string DataSetToFile(DataSet dataSet, string fullFileName, string defaultSheetName = "Sheet");

    /// <summary>
    /// 列表转 Excel 文件
    /// </summary>
    string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "") where T : class;

    /// <summary>
    /// 列表转 Excel 内存流
    /// </summary>
    MemoryStream ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "") where T : class;

    /// <summary>
    /// 数据表格转 Excel 内存流
    /// </summary>
    MemoryStream ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "");

    /// <summary>
    /// 异步将DataTable导出为Excel文件
    /// </summary>
    Task<string> DataTableToFileAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "");

    /// <summary>
    /// 异步将对象列表导出为Excel文件
    /// </summary>
    Task<string> ListToFileAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "") where T : class;

    /// <summary>
    /// 创建Excel模板
    /// </summary>
    MemoryStream CreateExcelTemplate<T>() where T : class, new();

    #endregion
}
