namespace Linger.Excel.Contracts;

/// <summary>
/// Excel扩展方法
/// </summary>
public static class ExcelExtensions
{
    /// <summary>
    /// 使用显式列定义创建 Excel 模板。
    /// 此重载不会对 <typeparamref name="T"/> 执行反射，适用于 AOT 和 trimming 场景。
    /// </summary>
    /// <typeparam name="T">模板数据类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="columns">显式列定义集合。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">标题。</param>
    /// <returns>包含模板内容的内存流。</returns>
    public static MemoryStream CreateExcelTemplate<T>(
        this IExcelService excelService,
        IEnumerable<ExcelExportColumn<T>> columns,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "")
    {
        ArgumentNullException.ThrowIfNull(excelService);

        return excelService.CollectionToMemoryStream(Array.Empty<T>(), columns, sheetsName, title);
    }

    /// <summary>
    /// 异步将数据表导出为Excel文件
    /// </summary>
    /// <param name="excel">Excel实现</param>
    /// <param name="dataTable">数据表</param>
    /// <param name="fullFileName">文件完整路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <param name="styleAction"></param>
    /// <param name="cancellationToken">用于取消导出操作的令牌。</param>
    /// <returns>文件路径</returns>
    public static async Task<string> DataTableToFileAsync<TWorkbook, TWorksheet>(
        this ExcelBase<TWorkbook, TWorksheet> excel,
        DataTable dataTable,
        string fullFileName,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null,
        Action<TWorksheet>? styleAction = null,
        CancellationToken cancellationToken = default)
        where TWorkbook : class
        where TWorksheet : class
    {
        ArgumentNullException.ThrowIfNull(excel);

        return await new RetryHelper().ExecuteAsync(async operationCancellationToken =>
        {
            using var ms = excel.DataTableToMemoryStream(dataTable, sheetsName, title, action, styleAction);
            var directoryName = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using var fs = new FileStream(
                fullFileName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                4096,
                useAsync: true);
            await ms.CopyToAsync(fs, 81920, operationCancellationToken).ConfigureAwait(false);
            await fs.FlushAsync(operationCancellationToken).ConfigureAwait(false);

            return fullFileName;
        }, "导出数据表到Excel文件", shouldRetry: exception => exception is IOException,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步将对象集合导出为Excel文件
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <typeparam name="TWorkbook"></typeparam>
    /// <typeparam name="TWorksheet"></typeparam>
    /// <param name="excel">Excel实现</param>
    /// <param name="list">对象集合</param>
    /// <param name="fullFileName">文件完整路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <param name="styleAction"></param>
    /// <param name="cancellationToken">用于取消导出操作的令牌。</param>
    /// <returns>文件路径</returns>
    public static async Task<string> ListToFileAsync<T, TWorkbook, TWorksheet>(
        this ExcelBase<TWorkbook, TWorksheet> excel,
        List<T> list,
        string fullFileName,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null,
        Action<TWorksheet>? styleAction = null,
        CancellationToken cancellationToken = default)
        where T : class
        where TWorkbook : class
        where TWorksheet : class
    {
        ArgumentNullException.ThrowIfNull(excel);

        return await new RetryHelper().ExecuteAsync(async operationCancellationToken =>
        {
            using var ms = excel.CollectionToMemoryStream(list, sheetsName, title, action, styleAction);
            var directoryName = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using var fs = new FileStream(
                fullFileName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                4096,
                useAsync: true);
            await ms.CopyToAsync(fs, 81920, operationCancellationToken).ConfigureAwait(false);
            await fs.FlushAsync(operationCancellationToken).ConfigureAwait(false);

            return fullFileName;
        }, "导出对象集合到Excel文件", shouldRetry: exception => exception is IOException,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

}
