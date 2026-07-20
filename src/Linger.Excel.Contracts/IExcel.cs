namespace Linger.Excel.Contracts;

/// <summary>
/// Excel泛型接口 - 提供特定工作表类型的高级操作
/// 继承自IExcelService,在其基础上提供带Action委托的高级自定义功能
/// </summary>
/// <typeparam name="TWorksheet">工作表类型</typeparam>
public interface IExcel<out TWorksheet> : IExcelService where TWorksheet : class
{
    #region Export - 高级版本(支持自定义操作)

    /// <summary>
    /// 数据表格转 Excel 文件，支持自定义操作
    /// </summary>
    /// <param name="dataTable">数据表格</param>
    /// <param name="fullFileName">完整文件路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义单元格操作委托</param>
    /// <param name="styleAction">自定义样式操作委托</param>
    /// <returns>生成的文件路径</returns>
    string DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 数据集转 Excel 文件(每个DataTable一个工作表)，支持自定义操作
    /// </summary>
    /// <param name="dataSet">数据集</param>
    /// <param name="fullFileName">完整文件路径</param>
    /// <param name="defaultSheetName">默认工作表名称前缀</param>
    /// <param name="action">自定义单元格操作委托</param>
    /// <param name="styleAction">自定义样式操作委托</param>
    /// <returns>生成的文件路径</returns>
    string DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// Exports a data set to Excel and invokes an action for every created worksheet.
    /// </summary>
    /// <param name="dataSet">The data set to export.</param>
    /// <param name="fullFileName">The full output file path.</param>
    /// <param name="worksheetAction">The action invoked after default worksheet formatting.</param>
    /// <param name="defaultSheetName">The prefix used for unnamed tables.</param>
    /// <returns>The generated file path.</returns>
    /// <remarks>
    /// The action is invoked in source table order, including tables without columns. An exception
    /// thrown by the action stops the export and is propagated to the caller.
    /// </remarks>
    string DataSetToExcel(DataSet dataSet, string fullFileName, Action<IWorksheetExportContext<TWorksheet>> worksheetAction,
        string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix);

    /// <summary>
    /// 对象集合转 Excel 文件，支持自定义操作
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="fullFileName">完整文件路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义单元格操作委托</param>
    /// <param name="styleAction">自定义样式操作委托</param>
    /// <returns>生成的文件路径</returns>
    string CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 对象集合转 Excel 内存流，支持自定义操作
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义单元格操作委托</param>
    /// <param name="styleAction">自定义样式操作委托</param>
    /// <returns>包含Excel数据的内存流</returns>
    MemoryStream CollectionToMemoryStream<T>(List<T> list, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 数据表格转 Excel 内存流，支持自定义操作
    /// </summary>
    /// <param name="dataTable">数据表格</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义单元格操作委托</param>
    /// <param name="styleAction">自定义样式操作委托</param>
    /// <returns>包含Excel数据的内存流</returns>
    MemoryStream DataTableToMemoryStream(DataTable dataTable, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 异步将DataTable导出为Excel文件，支持自定义操作
    /// </summary>
    /// <param name="dataTable">数据表格</param>
    /// <param name="fullFileName">完整文件路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义单元格操作委托</param>
    /// <param name="styleAction">自定义样式操作委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>生成的文件路径</returns>
    Task<string> DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步将对象集合导出为Excel文件，支持自定义操作
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="fullFileName">完整文件路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义单元格操作委托</param>
    /// <param name="styleAction">自定义样式操作委托</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>生成的文件路径</returns>
    Task<string> CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 创建Excel模板
    /// </summary>
    /// <typeparam name="T">模板类型</typeparam>
    /// <returns>包含模板的内存流</returns>
    new MemoryStream CreateExcelTemplate<T>() where T : class, new();

    #endregion
}
