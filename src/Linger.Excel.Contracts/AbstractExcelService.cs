namespace Linger.Excel.Contracts;

/// <summary>
/// Excel基础服务类，同时实现IExcelService和IExcel接口
/// </summary>
/// <typeparam name="TWorkbook">工作簿类型</typeparam>
/// <typeparam name="TWorksheet">工作表类型</typeparam>
public abstract class AbstractExcelService<TWorkbook, TWorksheet> : IExcelService, IExcel<TWorksheet>
    where TWorkbook : class
    where TWorksheet : class
{
    /// <summary>
    /// Excel配置选项
    /// </summary>
    protected readonly ExcelOptions Options;

    /// <summary>
    /// 日志记录器
    /// </summary>
    protected readonly ILogger? Logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">Excel配置选项</param>
    /// <param name="logger">日志记录器</param>
    protected AbstractExcelService(ExcelOptions? options = null, ILogger? logger = null)
    {
        Options = options ?? new ExcelOptions();
        Logger = logger;
    }

    #region IExcelService简单实现 - 调用IExcel实现

    /// <summary>
    /// 数据表格转 Excel 文件 - 简单版本
    /// </summary>
    string IExcelService.DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName, string title)
    {
        return DataTableToFile(dataTable, fullFileName, sheetsName, title);
    }

    /// <summary>
    /// 列表转 Excel 文件 - 简单版本
    /// </summary>
    string IExcelService.ListToFile<T>(List<T> list, string fullFileName, string sheetsName, string title)
    {
        return ListToFile(list, fullFileName, sheetsName, title);
    }

    /// <summary>
    /// 列表转 Excel 内存流 - 简单版本
    /// </summary>
    MemoryStream IExcelService.ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName, string title)
    {
        return ConvertCollectionToMemoryStream(list, sheetsName, title);
    }

    /// <summary>
    /// 数据表格转 Excel 内存流 - 简单版本
    /// </summary>
    MemoryStream IExcelService.ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName, string title)
    {
        return ConvertDataTableToMemoryStream(dataTable, sheetsName, title);
    }

    /// <summary>
    /// 异步将DataTable导出为Excel文件 - 简单版本
    /// </summary>
    Task<string> IExcelService.DataTableToFileAsync(DataTable dataTable, string fullFileName, string sheetsName, string title)
    {
        return DataTableToFileAsync(dataTable, fullFileName, sheetsName, title);
    }

    /// <summary>
    /// 异步将对象列表导出为Excel文件 - 简单版本
    /// </summary>
    Task<string> IExcelService.ListToFileAsync<T>(List<T> list, string fullFileName, string sheetsName, string title)
    {
        return ListToFileAsync<T>(list, fullFileName, sheetsName, title);
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
    /// 将Stream转换为DataTable
    /// </summary>
    public abstract DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为对象列表
    /// </summary>
    public abstract List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new();

    /// <summary>
    /// 数据表格转 Excel 文件
    /// </summary>
    public abstract string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 列表转 Excel 文件
    /// </summary>
    public abstract string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 列表转 Excel 内存流
    /// </summary>
    public abstract MemoryStream ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 数据表格转 Excel 内存流
    /// </summary>
    public abstract MemoryStream ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    #endregion

    #region 共享方法实现

    /// <summary>
    /// 异步将Excel文件转换为DataTable
    /// </summary>
    public virtual async Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            Logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            // 在Task.Run内部创建和使用文件流，确保流在使用完毕后才关闭
            return await Task.Run(() =>
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return ConvertStreamToDataTable(fileStream, sheetName, headerRowIndex, addEmptyRow);
            });
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取失败: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 异步将Excel文件转换为对象列表
    /// </summary>
    public virtual async Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            Logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            // 在Task.Run内部创建和使用文件流，确保流在使用完毕后才关闭
            return await Task.Run(() =>
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return ConvertStreamToList<T>(fileStream, sheetName, headerRowIndex, addEmptyRow);
            });
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件异步读取并转换为对象列表失败: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 异步将DataTable导出为Excel文件
    /// </summary>
    public virtual async Task<string> DataTableToFileAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null)
    {
        using var ms = ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action, styleAction);
        if (ms == null)
        {
            Logger?.LogError("转换DataTable到MemoryStream失败");
            throw new InvalidOperationException("转换DataTable到MemoryStream失败");
        }

        await ms.ToFileAsync(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 异步将对象列表导出为Excel文件
    /// </summary>
    public virtual async Task<string> ListToFileAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class
    {
        using var ms = ConvertCollectionToMemoryStream(list, sheetsName, title, action, styleAction);
        if (ms == null)
        {
            Logger?.LogError("转换对象列表到MemoryStream失败");
            throw new InvalidOperationException("转换对象列表到MemoryStream失败");
        }

        await ms.ToFileAsync(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 创建Excel模板
    /// </summary>
    public virtual MemoryStream CreateExcelTemplate<T>() where T : class, new()
    {
        var type = typeof(T);
        var properties = type.GetProperties().Where(p => p.CanRead).ToArray();

        // 创建一个空的列表，只包含列名
        var list = new List<T>(1);

        // 使用现有方法创建模板
        var result = ConvertCollectionToMemoryStream(list, type.Name, $"{type.Name} 模板");

        if (result == null)
            return new MemoryStream();

        return result;
    }

    #endregion

    /// <summary>
    /// 从属性数组中获取Excel列信息
    /// </summary>
    protected List<(string Name, string ColumnName, int Index)> GetExcelColumns(PropertyInfo[] properties)
    {
        var columns = new List<(string Name, string ColumnName, int Index)>();

        foreach (var property in properties)
        {
            var excelColumnAttr = property.GetCustomAttribute<Attributes.ExcelColumnAttribute>();
            if (excelColumnAttr != null)
            {
                columns.Add((property.Name, ColumnName: excelColumnAttr.ColumnName ?? property.Name, excelColumnAttr.Index
                ));
            }
        }

        return columns;
    }

    /// <summary>
    /// 获取Excl单元格值的通用方法
    /// </summary>
    protected object GetExcelCellValue(object value, bool isDateFormat = false)
    {
        return ExcelValueConverter.ConvertToDbValue(value, isDateFormat);
    }
}
