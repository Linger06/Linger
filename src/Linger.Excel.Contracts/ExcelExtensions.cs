using Linger.Extensions.Data;

namespace Linger.Excel.Contracts;

/// <summary>
/// Excel扩展方法
/// </summary>
public static class ExcelExtensions
{
    /// <summary>
    /// 将Excel文件转换为对象列表，并使用调用方提供的映射委托完成对象构造。
    /// 此重载会先导入为 <see cref="DataTable"/>，再复用 AOT 友好的 DataTable 映射路径。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="map">将 <see cref="DataRow"/> 映射为 <typeparamref name="T"/> 的委托。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static List<T>? ExcelToList<T>(
        this IExcelService excelService,
        string filePath,
        Func<DataRow, T> map,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(map);

        var dataTable = excelService.ExcelToDataTable(filePath, sheetName, headerRowIndex, addEmptyRow);
        return dataTable?.ToList(map);
    }

    /// <summary>
    /// 将Excel文件转换为对象列表，并使用调用方提供的工厂与列 setter 映射完成对象构造。
    /// 此重载会先导入为 <see cref="DataTable"/>，再复用 AOT 友好的 DataTable 映射路径。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="factory">创建目标对象的工厂。</param>
    /// <param name="columnSetters">列名到 setter 的映射。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static List<T>? ExcelToList<T>(
        this IExcelService excelService,
        string filePath,
        Func<T> factory,
        IReadOnlyDictionary<string, Action<T, object?>> columnSetters,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(columnSetters);

        var dataTable = excelService.ExcelToDataTable(filePath, sheetName, headerRowIndex, addEmptyRow);
        return dataTable?.ToList(factory, columnSetters);
    }

    /// <summary>
    /// 将Excel流转换为对象列表，并使用调用方提供的映射委托完成对象构造。
    /// 此重载会先导入为 <see cref="DataTable"/>，再复用 AOT 友好的 DataTable 映射路径。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="stream">Excel 流。</param>
    /// <param name="map">将 <see cref="DataRow"/> 映射为 <typeparamref name="T"/> 的委托。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static List<T>? StreamToList<T>(
        this IExcelService excelService,
        Stream stream,
        Func<DataRow, T> map,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(map);

        var dataTable = excelService.StreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
        return dataTable?.ToList(map);
    }

    /// <summary>
    /// 将Excel流转换为对象列表，并使用调用方提供的工厂与列 setter 映射完成对象构造。
    /// 此重载会先导入为 <see cref="DataTable"/>，再复用 AOT 友好的 DataTable 映射路径。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="stream">Excel 流。</param>
    /// <param name="factory">创建目标对象的工厂。</param>
    /// <param name="columnSetters">列名到 setter 的映射。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static List<T>? StreamToList<T>(
        this IExcelService excelService,
        Stream stream,
        Func<T> factory,
        IReadOnlyDictionary<string, Action<T, object?>> columnSetters,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(columnSetters);

        var dataTable = excelService.StreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
        return dataTable?.ToList(factory, columnSetters);
    }

    /// <summary>
    /// 异步将Excel文件转换为对象列表，并使用调用方提供的映射委托完成对象构造。
    /// 此重载会先异步导入为 <see cref="DataTable"/>，再复用 AOT 友好的 DataTable 映射路径。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="map">将 <see cref="DataRow"/> 映射为 <typeparamref name="T"/> 的委托。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static async Task<List<T>?> ExcelToListAsync<T>(
        this IExcelService excelService,
        string filePath,
        Func<DataRow, T> map,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(map);

        var dataTable = await excelService.ExcelToDataTableAsync(filePath, sheetName, headerRowIndex, addEmptyRow, cancellationToken).ConfigureAwait(false);
        return dataTable?.ToList(map);
    }

    /// <summary>
    /// 异步将Excel文件转换为对象列表，并使用调用方提供的工厂与列 setter 映射完成对象构造。
    /// 此重载会先异步导入为 <see cref="DataTable"/>，再复用 AOT 友好的 DataTable 映射路径。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="factory">创建目标对象的工厂。</param>
    /// <param name="columnSetters">列名到 setter 的映射。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static async Task<List<T>?> ExcelToListAsync<T>(
        this IExcelService excelService,
        string filePath,
        Func<T> factory,
        IReadOnlyDictionary<string, Action<T, object?>> columnSetters,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(columnSetters);

        var dataTable = await excelService.ExcelToDataTableAsync(filePath, sheetName, headerRowIndex, addEmptyRow, cancellationToken).ConfigureAwait(false);
        return dataTable?.ToList(factory, columnSetters);
    }

    /// <summary>
    /// 异步将Excel流转换为对象列表，并使用调用方提供的映射委托完成对象构造。
    /// 默认实现会使用 <see cref="Task.Run(System.Action, CancellationToken)"/> 包裹同步导入过程。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="stream">Excel 流。</param>
    /// <param name="map">将 <see cref="DataRow"/> 映射为 <typeparamref name="T"/> 的委托。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static Task<List<T>?> StreamToListAsync<T>(
        this IExcelService excelService,
        Stream stream,
        Func<DataRow, T> map,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(map);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataTable = excelService.StreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
            return dataTable?.ToList(map);
        }, cancellationToken);
    }

    /// <summary>
    /// 异步将Excel流转换为对象列表，并使用调用方提供的工厂与列 setter 映射完成对象构造。
    /// 默认实现会使用 <see cref="Task.Run(System.Action, CancellationToken)"/> 包裹同步导入过程。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="stream">Excel 流。</param>
    /// <param name="factory">创建目标对象的工厂。</param>
    /// <param name="columnSetters">列名到 setter 的映射。</param>
    /// <param name="sheetName">工作表名称。</param>
    /// <param name="headerRowIndex">列头所在行索引。</param>
    /// <param name="addEmptyRow">是否保留空行。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>导入后的对象列表；导入失败时返回 <c>null</c>。</returns>
    public static Task<List<T>?> StreamToListAsync<T>(
        this IExcelService excelService,
        Stream stream,
        Func<T> factory,
        IReadOnlyDictionary<string, Action<T, object?>> columnSetters,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelService);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(columnSetters);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dataTable = excelService.StreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
            return dataTable?.ToList(factory, columnSetters);
        }, cancellationToken);
    }

    /// <summary>
    /// 使用显式列定义将对象集合导出为 Excel 文件。
    /// 此重载不会对 <typeparamref name="T"/> 执行反射，适用于 AOT 和 trimming 场景。
    /// </summary>
    /// <typeparam name="T">源数据类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="items">要导出的数据集合。</param>
    /// <param name="columns">显式列定义集合。</param>
    /// <param name="fullFileName">目标文件完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">标题。</param>
    /// <returns>导出的文件路径。</returns>
    public static string CollectionToExcel<T>(
        this IExcelService excelService,
        IEnumerable<T> items,
        IEnumerable<ExcelExportColumn<T>> columns,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "")
    {
        ArgumentNullException.ThrowIfNull(excelService);

        var dataTable = CreateExportDataTable(items, columns);
        return excelService.DataTableToExcel(dataTable, fullFileName, sheetsName, title);
    }

    /// <summary>
    /// 使用显式列定义将对象集合导出为 Excel 内存流。
    /// 此重载不会对 <typeparamref name="T"/> 执行反射，适用于 AOT 和 trimming 场景。
    /// </summary>
    /// <typeparam name="T">源数据类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="items">要导出的数据集合。</param>
    /// <param name="columns">显式列定义集合。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">标题。</param>
    /// <returns>包含导出内容的内存流。</returns>
    public static MemoryStream CollectionToMemoryStream<T>(
        this IExcelService excelService,
        IEnumerable<T> items,
        IEnumerable<ExcelExportColumn<T>> columns,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "")
    {
        ArgumentNullException.ThrowIfNull(excelService);

        var dataTable = CreateExportDataTable(items, columns);
        return excelService.DataTableToMemoryStream(dataTable, sheetsName, title);
    }

    /// <summary>
    /// 使用显式列定义异步将对象集合导出为 Excel 文件。
    /// 此重载不会对 <typeparamref name="T"/> 执行反射，适用于 AOT 和 trimming 场景。
    /// </summary>
    /// <typeparam name="T">源数据类型。</typeparam>
    /// <param name="excelService">Excel 服务实例。</param>
    /// <param name="items">要导出的数据集合。</param>
    /// <param name="columns">显式列定义集合。</param>
    /// <param name="fullFileName">目标文件完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">标题。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>导出的文件路径。</returns>
    public static Task<string> CollectionToExcelAsync<T>(
        this IExcelService excelService,
        IEnumerable<T> items,
        IEnumerable<ExcelExportColumn<T>> columns,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelService);

        var dataTable = CreateExportDataTable(items, columns);
        return excelService.DataTableToExcelAsync(dataTable, fullFileName, sheetsName, title, cancellationToken);
    }

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
            using var ms = excel.DataTableToMemoryStream(dataTable, sheetsName, title, action, styleAction);
            var directoryName = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            await ms.CopyToAsync(fs).ConfigureAwait(false);
            await fs.FlushAsync().ConfigureAwait(false);

            return fullFileName;
        }, "导出数据表到Excel文件").ConfigureAwait(false);
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
            using var ms = excel.CollectionToMemoryStream(list, sheetsName, title, action, styleAction);
            var directoryName = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            await ms.CopyToAsync(fs).ConfigureAwait(false);
            await fs.FlushAsync().ConfigureAwait(false);

            return fullFileName;
        }, "导出对象集合到Excel文件").ConfigureAwait(false);
    }

    private static DataTable CreateExportDataTable<T>(IEnumerable<T> items, IEnumerable<ExcelExportColumn<T>> columns)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(columns);

        var columnList = new List<ExcelExportColumn<T>>();
        foreach (var column in columns)
        {
            columnList.Add(column);
        }

        if (columnList.Count == 0)
        {
            throw new ArgumentException("At least one export column is required.", nameof(columns));
        }

        var dataTable = new DataTable();
        var headerSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in columnList)
        {
            if (!headerSet.Add(column.Header))
            {
                throw new ArgumentException("Duplicate export column headers are not allowed.", nameof(columns));
            }

            _ = dataTable.Columns.Add(column.Header, column.DataType);
        }

        foreach (var item in items)
        {
            var row = dataTable.NewRow();
            for (var i = 0; i < columnList.Count; i++)
            {
                var value = columnList[i].ValueSelector(item);
                row[i] = value ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}
