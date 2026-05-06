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
    /// 将Excel文件转换为DataSet(所有工作表)
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="headerRowIndex">列名所在行号(所有工作表使用相同值)</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? ExcelToDataSet(string filePath, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetNames">要处理的工作表名称集合，为null或空集合时处理所有工作表</param>
    /// <param name="headerRowIndex">列名所在行号(所有工作表使用相同值)</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="headerRowIndexSelector">根据工作表名称返回对应的表头行索引的委托，如果返回null则使用默认值0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? ExcelToDataSet(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetNames">要处理的工作表名称集合，为null或空集合时处理所有工作表</param>
    /// <param name="headerRowIndexSelector">根据工作表名称返回对应的表头行索引的委托，如果返回null则使用默认值0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataTable
    /// </summary>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataTable</returns>
    DataTable? StreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为对象列表
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的对象列表</returns>
    List<T>? StreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    /// <summary>
    /// 将Stream转换为DataSet(所有工作表)
    /// </summary>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0(所有工作表使用相同值)</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? StreamToDataSet(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)
    /// </summary>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetNames">要处理的工作表名称集合，为null或空集合时处理所有工作表</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0(所有工作表使用相同值)</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="headerRowIndexSelector">根据工作表名称返回对应的表头行索引的委托，如果返回null则使用默认值0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? StreamToDataSet(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetNames">要处理的工作表名称集合，为null或空集合时处理所有工作表</param>
    /// <param name="headerRowIndexSelector">根据工作表名称返回对应的表头行索引的委托，如果返回null则使用默认值0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataSet，每个工作表对应一个DataTable</returns>
    DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 异步将Excel文件转换为DataTable
    /// </summary>
    Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Excel文件转换为对象列表
    /// </summary>
    Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// 异步将Excel文件转换为DataSet(所有工作表)
    /// </summary>
    Task<DataSet?> ExcelToDataSetAsync(string filePath, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Excel文件转换为DataSet(指定工作表)
    /// </summary>
    Task<DataSet?> ExcelToDataSetAsync(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Excel文件转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    Task<DataSet?> ExcelToDataSetAsync(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Excel文件转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    Task<DataSet?> ExcelToDataSetAsync(string filePath, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Stream转换为DataTable
    /// </summary>
    Task<DataTable?> StreamToDataTableAsync(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Stream转换为对象列表
    /// </summary>
    Task<List<T>?> StreamToListAsync<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// 异步将Stream转换为DataSet(所有工作表)
    /// </summary>
    Task<DataSet?> StreamToDataSetAsync(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Stream转换为DataSet(指定工作表)
    /// </summary>
    Task<DataSet?> StreamToDataSetAsync(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Stream转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    Task<DataSet?> StreamToDataSetAsync(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将Stream转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    Task<DataSet?> StreamToDataSetAsync(Stream stream, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    #endregion

    #region Export

    /// <summary>
    /// 数据表格转 Excel 文件
    /// </summary>
    string DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "");

    /// <summary>
    /// 数据集转 Excel 文件(每个DataTable一个工作表)
    /// </summary>
    string DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix);

    /// <summary>
    /// 对象集合转 Excel 文件
    /// </summary>
    string CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "") where T : class;

    /// <summary>
    /// 对象集合转 Excel 内存流
    /// </summary>
    MemoryStream CollectionToMemoryStream<T>(List<T> list, string sheetsName = ExcelOptions.DefaultSheetName, string title = "") where T : class;

    /// <summary>
    /// 数据表格转 Excel 内存流
    /// </summary>
    MemoryStream DataTableToMemoryStream(DataTable dataTable, string sheetsName = ExcelOptions.DefaultSheetName, string title = "");

    /// <summary>
    /// 异步将DataTable导出为Excel文件
    /// </summary>
    Task<string> DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将对象集合导出为Excel文件
    /// </summary>
    Task<string> CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "", CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 创建Excel模板
    /// </summary>
    MemoryStream CreateExcelTemplate<T>() where T : class, new();

    #endregion
}
