namespace Linger.Excel.Contracts;

/// <summary>
/// 定义公开 Provider 工作表类型的高级导出操作。
/// </summary>
/// <typeparam name="TWorksheet">Provider 的工作表类型。</typeparam>
public interface IExcel<out TWorksheet> : IExcelService where TWorksheet : class
{
    /// <summary>
    /// 将数据集导出到 Excel 文件，并为每个创建的工作表调用委托。
    /// </summary>
    /// <param name="dataSet">要导出的数据集。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="worksheetAction">在默认工作表格式化完成后调用的委托。</param>
    /// <param name="defaultSheetName">未命名数据表使用的工作表名称前缀。</param>
    /// <returns>生成的文件路径。</returns>
    /// <remarks>
    /// 委托按源数据表顺序执行，包括没有列的数据表。委托抛出的异常会终止导出并向调用方传播。
    /// </remarks>
    string DataSetToExcel(
        DataSet dataSet,
        string fullFileName,
        Action<IWorksheetExportContext<TWorksheet>> worksheetAction,
        string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix);

    /// <summary>
    /// 使用 Provider 特定的工作表回调将对象集合导出到内存流。
    /// </summary>
    /// <typeparam name="T">要导出的对象类型。</typeparam>
    /// <param name="list">要导出的对象列表。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <param name="action">接收工作表和导出属性的单元格处理委托。</param>
    /// <param name="styleAction">默认格式化完成后执行的工作表样式委托。</param>
    /// <returns>定位在起始位置且包含 Excel 内容的内存流。</returns>
    MemoryStream CollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null,
        Action<TWorksheet>? styleAction = null)
        where T : class;

    /// <summary>
    /// 使用 Provider 特定的工作表回调将数据表导出到内存流。
    /// </summary>
    /// <param name="dataTable">要导出的数据表。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <param name="action">接收工作表、列集合和行集合的单元格处理委托。</param>
    /// <param name="styleAction">默认格式化完成后执行的工作表样式委托。</param>
    /// <returns>定位在起始位置且包含 Excel 内容的内存流。</returns>
    MemoryStream DataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null,
        Action<TWorksheet>? styleAction = null);
}
