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

}
