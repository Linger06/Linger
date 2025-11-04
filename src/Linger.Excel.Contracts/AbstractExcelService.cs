namespace Linger.Excel.Contracts;

/// <summary>
/// Excel基础服务类，同时实现IExcelService和IExcel接口
/// </summary>
/// <typeparam name="TWorkbook">工作簿类型</typeparam>
/// <typeparam name="TWorksheet">工作表类型</typeparam>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="options">Excel配置选项</param>
/// <param name="logger">日志记录器</param>
public abstract class AbstractExcelService<TWorkbook, TWorksheet>(ExcelOptions? options = null, ILogger? logger = null) : IExcelService, IExcel<TWorksheet>
    where TWorkbook : class
    where TWorksheet : class
{
    /// <summary>
    /// Excel配置选项
    /// </summary>
    protected readonly ExcelOptions Options = options ?? new ExcelOptions();

    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger? Logger = logger;

    #region IExcelService简单实现 - 调用IExcel实现

    /// <summary>
    /// 数据表格转 Excel 文件 - 简单版本
    /// </summary>
    string IExcelService.DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName, string title)
    {
        return DataTableToExcel(dataTable, fullFileName, sheetsName, title, action: null, styleAction: null);
    }

    /// <summary>
    /// 数据集转 Excel 文件 - 简单版本
    /// </summary>
    string IExcelService.DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName)
    {
        return DataSetToExcel(dataSet, fullFileName, defaultSheetName, action: null, styleAction: null);
    }

    /// <summary>
    /// 对象集合转 Excel 文件 - 简单版本
    /// </summary>
    string IExcelService.CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName, string title)
    {
        return CollectionToExcel(list, fullFileName, sheetsName, title, action: null, styleAction: null);
    }

    /// <summary>
    /// 对象集合转 Excel 内存流 - 简单版本
    /// </summary>
    MemoryStream IExcelService.CollectionToMemoryStream<T>(List<T> list, string sheetsName, string title)
    {
        return CollectionToMemoryStream(list, sheetsName, title, action: null, styleAction: null);
    }

    /// <summary>
    /// 数据表格转 Excel 内存流 - 简单版本
    /// </summary>
    MemoryStream IExcelService.DataTableToMemoryStream(DataTable dataTable, string sheetsName, string title)
    {
        return DataTableToMemoryStream(dataTable, sheetsName, title, action: null, styleAction: null);
    }

    /// <summary>
    /// 将Stream转换为DataTable - 简单版本
    /// </summary>
    DataTable? IExcelService.StreamToDataTable(Stream stream, string? sheetName, int headerRowIndex, bool addEmptyRow)
    {
        return StreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
    }

    /// <summary>
    /// 将Stream转换为对象列表 - 简单版本
    /// </summary>
    List<T>? IExcelService.StreamToList<T>(Stream stream, string? sheetName, int headerRowIndex, bool addEmptyRow)
    {
        return StreamToList<T>(stream, sheetName, headerRowIndex, addEmptyRow);
    }

    /// <summary>
    /// 将Stream转换为DataSet - 简单版本
    /// </summary>
    DataSet? IExcelService.StreamToDataSet(Stream stream, int headerRowIndex, bool addEmptyRow)
    {
        return StreamToDataSet(stream, headerRowIndex, addEmptyRow);
    }

    DataSet? IExcelService.StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex, bool addEmptyRow)
    {
        return StreamToDataSet(stream, sheetNames, headerRowIndex, addEmptyRow);
    }

    DataSet? IExcelService.StreamToDataSet(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow)
    {
        return StreamToDataSet(stream, headerRowIndexSelector, addEmptyRow);
    }

    DataSet? IExcelService.StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow)
    {
        return StreamToDataSet(stream, sheetNames, headerRowIndexSelector, addEmptyRow);
    }

    /// <summary>
    /// 异步将DataTable导出为Excel文件 - 简单版本
    /// </summary>
    Task<string> IExcelService.DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName, string title)
    {
        return DataTableToExcelAsync(dataTable, fullFileName, sheetsName, title, action: null, styleAction: null);
    }

    /// <summary>
    /// 异步将对象集合导出为Excel文件 - 简单版本
    /// </summary>
    Task<string> IExcelService.CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName, string title)
    {
        return CollectionToExcelAsync<T>(list, fullFileName, sheetsName, title, action: null, styleAction: null);
    }

    #endregion

    #region IExcel抽象方法 - 由具体实现类实现

    /// <summary>
    /// 将Excel文件转换为DataTable
    /// </summary>
    public abstract DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为对象列表
    /// </summary>
    public abstract List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    /// <summary>
    /// 将Stream转换为DataTable（推荐 - 同步版本）
    /// </summary>
    public abstract DataTable? StreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataTable（推荐 - 异步版本）
    /// </summary>
    /// <remarks>
    /// 子类应实现真正的异步 I/O 操作,使用 Stream 的异步 API (ReadAsync 等)
    /// </remarks>
    public virtual async Task<DataTable?> StreamToDataTableAsync(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        // 默认实现: 使用 Task.Run 包裹同步方法
        // 子类应该覆盖此方法以提供真正的异步实现
        return await Task.Run(() => StreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow)).ConfigureAwait(false);
    }

    /// <summary>
    /// 将Stream转换为对象列表（推荐 - 同步版本）
    /// </summary>
    public abstract List<T>? StreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    /// <summary>
    /// 将Stream转换为对象列表（推荐 - 异步版本）
    /// </summary>
    /// <remarks>
    /// 子类应实现真正的异步 I/O 操作,使用 Stream 的异步 API (ReadAsync 等)
    /// </remarks>
    public virtual async Task<List<T>?> StreamToListAsync<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        // 默认实现: 使用 Task.Run 包裹同步方法
        // 子类应该覆盖此方法以提供真正的异步实现
        return await Task.Run(() => StreamToList<T>(stream, sheetName, headerRowIndex, addEmptyRow)).ConfigureAwait(false);
    }

    /// <summary>
    /// 将Excel文件转换为DataSet(所有工作表)
    /// </summary>
    public abstract DataSet? ExcelToDataSet(string filePath, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)
    /// </summary>
    public abstract DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    public abstract DataSet? ExcelToDataSet(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    public abstract DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(所有工作表)（推荐 - 同步版本）
    /// </summary>
    public abstract DataSet? StreamToDataSet(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(所有工作表)（推荐 - 异步版本）
    /// </summary>
    /// <remarks>
    /// 子类应实现真正的异步 I/O 操作,使用 Stream 的异步 API (ReadAsync 等)
    /// </remarks>
    public virtual async Task<DataSet?> StreamToDataSetAsync(Stream stream, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        // 默认实现: 使用 Task.Run 包裹同步方法
        // 子类应该覆盖此方法以提供真正的异步实现
        return await Task.Run(() => StreamToDataSet(stream, headerRowIndex, addEmptyRow)).ConfigureAwait(false);
    }

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)（推荐 - 同步版本）
    /// </summary>
    public abstract DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)（推荐 - 异步版本）
    /// </summary>
    /// <remarks>
    /// 子类应实现真正的异步 I/O 操作,使用 Stream 的异步 API (ReadAsync 等)
    /// </remarks>
    public virtual async Task<DataSet?> StreamToDataSetAsync(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        // 默认实现: 使用 Task.Run 包裹同步方法
        // 子类应该覆盖此方法以提供真正的异步实现
        return await Task.Run(() => StreamToDataSet(stream, sheetNames, headerRowIndex, addEmptyRow)).ConfigureAwait(false);
    }

    /// <summary>
    /// 将Stream转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行（推荐 - 同步版本）
    /// </summary>
    public abstract DataSet? StreamToDataSet(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行（推荐 - 异步版本）
    /// </summary>
    /// <remarks>
    /// 子类应实现真正的异步 I/O 操作,使用 Stream 的异步 API (ReadAsync 等)
    /// </remarks>
    public virtual async Task<DataSet?> StreamToDataSetAsync(Stream stream, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false)
    {
        // 默认实现: 使用 Task.Run 包裹同步方法
        // 子类应该覆盖此方法以提供真正的异步实现
        return await Task.Run(() => StreamToDataSet(stream, headerRowIndexSelector, addEmptyRow)).ConfigureAwait(false);
    }

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行（推荐 - 同步版本）
    /// </summary>
    public abstract DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行（推荐 - 异步版本）
    /// </summary>
    /// <remarks>
    /// 子类应实现真正的异步 I/O 操作,使用 Stream 的异步 API (ReadAsync 等)
    /// </remarks>
    public virtual async Task<DataSet?> StreamToDataSetAsync(Stream stream, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false)
    {
        // 默认实现: 使用 Task.Run 包裹同步方法
        // 子类应该覆盖此方法以提供真正的异步实现
        return await Task.Run(() => StreamToDataSet(stream, sheetNames, headerRowIndexSelector, addEmptyRow)).ConfigureAwait(false);
    }

    /// <summary>
    /// 数据表格转 Excel 文件（推荐）
    /// </summary>
    public abstract string DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 数据集转 Excel 文件（推荐）
    /// </summary>
    public abstract string DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 对象集合转 Excel 文件（推荐）
    /// </summary>
    public abstract string CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 对象集合转 Excel 内存流（推荐）
    /// </summary>
    public abstract MemoryStream CollectionToMemoryStream<T>(List<T> list, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 数据表格转 Excel 内存流（推荐）
    /// </summary>
    public abstract MemoryStream DataTableToMemoryStream(DataTable dataTable, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    #endregion

    #region 共享方法实现

    /// <summary>
    /// 异步将Excel文件转换为DataTable
    /// </summary>
    public virtual async Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        Logger?.LogTrace("开始异步读取Excel文件: {FilePath}", filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger?.LogWarning("文件路径为空");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Logger?.LogWarning("文件不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
#if NET5_0_OR_GREATER
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#else
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#endif
            return await StreamToDataTableAsync(fileStream, sheetName, headerRowIndex, addEmptyRow).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 异步将Excel文件转换为对象列表
    /// </summary>
    public virtual async Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        Logger?.LogTrace("开始异步读取Excel文件并转换为对象列表: {FilePath}", filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger?.LogWarning("文件路径为空");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Logger?.LogWarning("文件不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
#if NET5_0_OR_GREATER
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#else
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#endif
            return await StreamToListAsync<T>(fileStream, sheetName, headerRowIndex, addEmptyRow).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取并转换为对象列表失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 异步将Excel文件转换为DataSet(所有工作表)
    /// </summary>
    public virtual async Task<DataSet?> ExcelToDataSetAsync(string filePath, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        Logger?.LogTrace("开始异步读取Excel文件并转换为DataSet: {FilePath}", filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger?.LogWarning("文件路径为空");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Logger?.LogWarning("文件不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
#if NET5_0_OR_GREATER
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#else
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#endif
            return await StreamToDataSetAsync(fileStream, headerRowIndex, addEmptyRow).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取并转换为DataSet失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 异步将Excel文件转换为DataSet(指定工作表)
    /// </summary>
    public virtual async Task<DataSet?> ExcelToDataSetAsync(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        Logger?.LogTrace("开始异步读取Excel文件并转换为DataSet: {FilePath}", filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger?.LogWarning("文件路径为空");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Logger?.LogWarning("文件不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
#if NET5_0_OR_GREATER
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#else
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#endif
            return await StreamToDataSetAsync(fileStream, sheetNames, headerRowIndex, addEmptyRow).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取并转换为DataSet失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 异步将Excel文件转换为DataSet(所有工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    public virtual async Task<DataSet?> ExcelToDataSetAsync(string filePath, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false)
    {
        Logger?.LogTrace("开始异步读取Excel文件并转换为DataSet: {FilePath}", filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger?.LogWarning("文件路径为空");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Logger?.LogWarning("文件不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
#if NET5_0_OR_GREATER
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#else
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#endif
            return await StreamToDataSetAsync(fileStream, headerRowIndexSelector, addEmptyRow).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取并转换为DataSet失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 异步将Excel文件转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    public virtual async Task<DataSet?> ExcelToDataSetAsync(string filePath, IEnumerable<string>? sheetNames, Func<string, int?> headerRowIndexSelector, bool addEmptyRow = false)
    {
        Logger?.LogTrace("开始异步读取Excel文件并转换为DataSet: {FilePath}", filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger?.LogWarning("文件路径为空");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Logger?.LogWarning("文件不存在: {FilePath}", filePath);
            return null;
        }

        try
        {
#if NET5_0_OR_GREATER
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#else
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
#endif
            return await StreamToDataSetAsync(fileStream, sheetNames, headerRowIndexSelector, addEmptyRow).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取并转换为DataSet失败: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// 异步将DataTable导出为Excel文件（推荐）
    /// </summary>
    public virtual async Task<string> DataTableToExcelAsync(DataTable dataTable, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null)
    {
        using var ms = DataTableToMemoryStream(dataTable, sheetsName, title, action, styleAction);
        if (ms == null)
        {
            Logger?.LogError("转换DataTable到MemoryStream失败");
            throw new InvalidOperationException("转换DataTable到MemoryStream失败");
        }

        await ms.ToFileAsync(fullFileName).ConfigureAwait(false);
        return fullFileName;
    }

    /// <summary>
    /// 异步将对象集合导出为Excel文件（推荐）
    /// </summary>
    public virtual async Task<string> CollectionToExcelAsync<T>(List<T> list, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class
    {
        using var ms = CollectionToMemoryStream(list, sheetsName, title, action, styleAction);
        if (ms == null)
        {
            Logger?.LogError("转换对象列表到MemoryStream失败");
            throw new InvalidOperationException("转换对象列表到MemoryStream失败");
        }

        await ms.ToFileAsync(fullFileName).ConfigureAwait(false);
        return fullFileName;
    }

    /// <summary>
    /// 创建Excel模板
    /// </summary>
    public virtual MemoryStream CreateExcelTemplate<T>() where T : class, new()
    {
        var type = typeof(T);

        // 创建一个空的列表，只包含列名
        var list = new List<T>();

        // 使用现有方法创建模板
        var result = CollectionToMemoryStream(list, type.Name, $"{type.Name} 模板");

        if (result == null)
        {
            throw new InvalidOperationException($"创建 {type.Name} Excel模板失败");
        }

        return result;
    }

    #endregion

    /// <summary>
    /// 从属性数组中获取Excel列信息
    /// </summary>
    protected List<(string Name, string ColumnName, int Index)> GetExcelColumns(PropertyInfo[] properties)
    {
        return (from property in properties
                let excelColumnAttr = property.GetCustomAttribute<Attributes.ExcelColumnAttribute>()
                where excelColumnAttr != null
                select (property.Name, ColumnName: excelColumnAttr.ColumnName ?? property.Name, excelColumnAttr.Index))
               .ToList();
    }

    /// <summary>
    /// 获取Excl单元格值的通用方法
    /// </summary>
    protected object GetExcelCellValue(object value, bool isDateFormat = false)
    {
        return ExcelValueConverter.ConvertToDbValue(value, isDateFormat);
    }

    #region 私有辅助方法

    /// <summary>
    /// 解析逗号分隔的工作表名称字符串
    /// </summary>
    /// <param name="sheetNames">工作表名称，多个用逗号分隔，如 "Sheet1,Sheet2"</param>
    /// <returns>工作表名称集合，null表示所有工作表</returns>
    private static IEnumerable<string>? ParseSheetNames(string? sheetNames)
    {
        if (sheetNames is null || string.IsNullOrWhiteSpace(sheetNames))
        {
            return null;
        }

        return sheetNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim());
    }

    #endregion

    #region 兼容旧方法 - 提供与 NPOIHelper.ImportExcelToDs 相同的签名

    /// <summary>
    /// 将Excel文件转换为DataSet(兼容旧方法签名)
    /// </summary>
    /// <param name="strFileName">Excel文件完整路径</param>
    /// <param name="strSheetNames">工作表名称，多个用逗号分隔，如 "Sheet1,Sheet2"。为null时读取所有工作表</param>
    /// <param name="headerRowIndex">表头行索引，0表示第一行</param>
    /// <returns>包含指定工作表数据的DataSet</returns>
    /// <remarks>
    /// 此方法提供与 NPOIHelper.ImportExcelToDs 完全兼容的签名，便于迁移旧代码。
    /// 注意：此方法默认不包含空行(addEmptyRow=false)，与旧方法行为一致。
    /// </remarks>
    public virtual DataSet? ExcelToDataSet(string strFileName, string? strSheetNames, int headerRowIndex)
    {
        var sheetNames = ParseSheetNames(strSheetNames);
        return ExcelToDataSet(strFileName, sheetNames, headerRowIndex, addEmptyRow: false);
    }

    /// <summary>
    /// 异步将Excel文件转换为DataSet(兼容旧方法签名)
    /// </summary>
    /// <param name="strFileName">Excel文件完整路径</param>
    /// <param name="strSheetNames">工作表名称，多个用逗号分隔，如 "Sheet1,Sheet2"。为null时读取所有工作表</param>
    /// <param name="headerRowIndex">表头行索引，0表示第一行</param>
    /// <returns>包含指定工作表数据的DataSet</returns>
    /// <remarks>
    /// 此方法提供与 NPOIHelper.ImportExcelToDs 完全兼容的异步版本。
    /// 注意：此方法默认不包含空行(addEmptyRow=false)，与旧方法行为一致。
    /// </remarks>
    public virtual async Task<DataSet?> ExcelToDataSetAsync(string strFileName, string? strSheetNames, int headerRowIndex)
    {
        var sheetNames = ParseSheetNames(strSheetNames);
        return await ExcelToDataSetAsync(strFileName, sheetNames, headerRowIndex, addEmptyRow: false)
                     .ConfigureAwait(false);
    }

    #endregion
}
