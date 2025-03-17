namespace Linger.Excel.Contracts;

/// <summary>
/// Excel扩展方法
/// </summary>
public static class ExcelExtensions
{
    /// <summary>
    /// 异步将数据表导出为Excel文件
    /// </summary>
    /// <param name="excel">Excel实现</param>
    /// <param name="dataTable">数据表</param>
    /// <param name="fullFileName">文件完整路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>文件路径</returns>
    public static async Task<string> DataTableToFileAsync<TWorkbook, TWorksheet>(
        this ExcelBase<TWorkbook, TWorksheet> excel,
        DataTable dataTable,
        string fullFileName,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null)
        where TWorkbook : class
        where TWorksheet : class
    {
        ArgumentNullException.ThrowIfNull(excel);

        return await new RetryHelper().ExecuteAsync(async () =>
        {
            using var ms = excel.ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action, styleAction);
            if (ms == null)
            {
                throw new InvalidOperationException("无法将数据表转换为Excel流");
            }

            var directoryName = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            await ms.CopyToAsync(fs);
            await fs.FlushAsync();

            return fullFileName;
        }, "导出数据表到Excel文件");
    }

    /// <summary>
    /// 异步将对象集合导出为Excel文件
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="excel">Excel实现</param>
    /// <param name="list">对象集合</param>
    /// <param name="fullFileName">文件完整路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>文件路径</returns>
    public static async Task<string> ListToFileAsync<T, TWorkbook, TWorksheet>(
        this ExcelBase<TWorkbook, TWorksheet> excel,
        List<T> list,
        string fullFileName,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null)
        where T : class
        where TWorkbook : class
        where TWorksheet : class
    {
        ArgumentNullException.ThrowIfNull(excel);

        return await new RetryHelper().ExecuteAsync(async () =>
        {
            using var ms = excel.ConvertCollectionToMemoryStream(list, sheetsName, title, action, styleAction);
            if (ms == null)
            {
                throw new InvalidOperationException("无法将对象集合转换为Excel流");
            }

            var directoryName = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            await ms.CopyToAsync(fs);
            await fs.FlushAsync();

            return fullFileName;
        }, "导出对象集合到Excel文件");
    }
}
