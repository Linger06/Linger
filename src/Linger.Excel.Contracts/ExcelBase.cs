using System.Data;
using System.Reflection;
using Linger.Excel.Contracts.Utils;
using Linger.Extensions.Core;
using Linger.Extensions.Data;
using Microsoft.Extensions.Logging;

namespace Linger.Excel.Contracts;

/// <summary>
/// Excel处理基类
/// </summary>
public abstract class ExcelBase : IExcel
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
    /// Excel内容类型
    /// </summary>
    public static string ContentType => "application/vnd.ms-excel";

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">Excel配置选项</param>
    /// <param name="logger">日志记录器</param>
    protected ExcelBase(ExcelOptions? options = null, ILogger? logger = null)
    {
        Options = options ?? new ExcelOptions();
        Logger = logger;
    }

    /// <summary>
    /// 将Excel文件转换为DataTable
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataTable</returns>
    public DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            Logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ConvertStreamToDataTable(fileStream, sheetName, headerRowIndex, addEmptyRow);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件读取失败: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 将Excel文件转换为对象列表
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的对象列表</returns>
    public List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            Logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ConvertStreamToList<T>(fileStream, sheetName, headerRowIndex, addEmptyRow);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel文件读取并转换为对象列表失败: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 将DataTable导出为Excel文件
    /// </summary>
    /// <param name="dataTable">数据表</param>
    /// <param name="fullFileName">文件完整路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>文件路径</returns>
    public string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        try
        {
            using var ms = ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action);
            if (ms == null)
            {
                Logger?.LogError("转换DataTable到MemoryStream失败");
                throw new InvalidOperationException("转换DataTable到MemoryStream失败");
            }

            EnsureDirectoryExists(fullFileName);
            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            ms.Position = 0; // 确保内存流位置在开头
            ms.CopyTo(fs);
            fs.Flush();

            return fullFileName;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "保存DataTable到Excel文件失败: {FilePath}", fullFileName);
            throw new ExcelException("保存DataTable到Excel文件失败", ex);
        }
    }

    /// <summary>
    /// 将对象列表导出为Excel文件
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="fullFileName">文件完整路径</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>文件路径</returns>
    public string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null) where T : class
    {
        try
        {
            using var ms = ConvertCollectionToMemoryStream(list, sheetsName, title, action);
            if (ms == null)
            {
                Logger?.LogError("转换对象列表到MemoryStream失败");
                throw new InvalidOperationException("转换对象列表到MemoryStream失败");
            }

            EnsureDirectoryExists(fullFileName);
            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            ms.Position = 0; // 确保内存流位置在开头
            ms.CopyTo(fs);
            fs.Flush();

            return fullFileName;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "保存对象列表到Excel文件失败: {FilePath}", fullFileName);
            throw new ExcelException("保存对象列表到Excel文件失败", ex);
        }
    }

    /// <summary>
    /// 确保目录存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    protected void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// 安全设置对象属性值
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">目标对象</param>
    /// <param name="property">属性信息</param>
    /// <param name="value">属性值</param>
    protected void SetPropertySafely<T>(T obj, PropertyInfo property, object? value) where T : class
    {
        if (value == null || value is DBNull)
            return;

        try
        {
            // 处理Nullable类型
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            // 特殊类型处理
            if (propertyType == typeof(string))
            {
                property.SetValue(obj, value.ToString());
                return;
            }

            if (propertyType == typeof(DateTime))
            {
                if (value is DateTime dateTime)
                {
                    property.SetValue(obj, dateTime);
                }
                else if (DateTime.TryParse(value.ToString(), out DateTime parsedDate))
                {
                    property.SetValue(obj, parsedDate);
                }
                else if (value is double numericDate)
                {
                    property.SetValue(obj, DateTime.FromOADate(numericDate));
                }
                return;
            }

            if (propertyType == typeof(bool))
            {
                if (value is bool boolValue)
                {
                    property.SetValue(obj, boolValue);
                }
                else
                {
                    string strValue = value.ToString()!.ToLower();
                    property.SetValue(obj, strValue == "true" || strValue == "yes" || strValue == "y" || strValue == "1");
                }
                return;
            }

            if (propertyType.IsEnum)
            {
                if (value is string strValue)
                {
                    try
                    {
                        var enumValue = Enum.Parse(propertyType, strValue, true);
                        property.SetValue(obj, enumValue);
                        return;
                    }
                    catch
                    {
                        // Parsing failed, continue with other conversion attempts
                    }
                }
                if (int.TryParse(value.ToString(), out int intValue))
                {
                    property.SetValue(obj, Enum.ToObject(propertyType, intValue));
                    return;
                }
            }

            // 常规类型转换
            property.SetValue(obj, Convert.ChangeType(value, propertyType));
        }
        catch (Exception ex)
        {
            // 转换失败记录日志
            Logger?.LogDebug(ex, "属性设置失败: {PropertyName}, 值: {Value}, 值类型: {ValueType}",
                property.Name, value, value.GetType().Name);
        }
    }

    /// <summary>
    /// 安全处理异常并返回默认值
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="operation">操作函数</param>
    /// <param name="defaultValue">默认值</param>
    /// <param name="operationName">操作名称</param>
    /// <returns>操作结果或默认值</returns>
    protected T? SafeExecute<T>(Func<T> operation, T? defaultValue = default, string? operationName = null)
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Excel操作失败: {OperationName}", operationName ?? "未知操作");
            return defaultValue;
        }
    }

    /// <summary>
    /// 使用性能监控执行操作
    /// </summary>
    protected T MonitorPerformance<T>(string operationName, Func<T> operation)
    {
        if (!Options.EnablePerformanceMonitoring)
            return operation();
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            return operation();
        }
        finally
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
            {
                Logger?.LogInformation("{Operation} 耗时: {Time}ms", operationName, sw.ElapsedMilliseconds);
            }
        }
    }

    /// <summary>
    /// 将Stream转换为DataTable
    /// </summary>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataTable</returns>
    public abstract DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将DataTable转换为MemoryStream
    /// </summary>
    /// <param name="dataTable">数据表</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>内存流</returns>
    public abstract MemoryStream? ConvertDataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = "Sheet1",
        string title = "",
        Action<object, DataColumnCollection, DataRowCollection>? action = null);

    /// <summary>
    /// 将对象列表转换为MemoryStream
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>内存流</returns>
    public abstract MemoryStream? ConvertCollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = "Sheet1",
        string title = "",
        Action<object, PropertyInfo[]>? action = null) where T : class;

    /// <summary>
    /// 将Stream转换为对象列表
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的对象列表</returns>
    public List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        var dataTable = MonitorPerformance("读取Excel到DataTable", () => 
            ConvertStreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow));
        
        if (dataTable == null || dataTable.Columns.Count == 0)
        {
            Logger?.LogWarning("无法从Stream转换为DataTable或结果为空表");
            return new List<T>();
        }

        return SafeExecute(() => dataTable.ToList<T>(Options.ParallelProcessingThreshold),
            new List<T>(),
            nameof(ConvertStreamToList));
    }

    /// <summary>
    /// 批量处理数据
    /// </summary>
    /// <typeparam name="TRow">行类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="totalCount">数据总数</param>
    /// <param name="createRowFunc">创建行的函数</param>
    /// <param name="getValuesFunc">获取行值的函数</param>
    /// <param name="processRowFunc">处理行的函数</param>
    protected void ProcessInBatches<TRow, TValue>(
        int totalCount,
        Func<int, TRow> createRowFunc,
        Func<int, TValue[]> getValuesFunc,
        Action<TRow, int, TValue[], object?> processRowFunc,
        object? additionalParam = null)
    {
        if (totalCount <= 0)
            return;

        bool useParallel = totalCount > Options.ParallelProcessingThreshold;
        int batchSize = Options.UseBatchWrite ? Options.BatchSize : totalCount;

        if (useParallel)
        {
            // 预计算所有值
            var batchValues = new TValue[totalCount][];

            Parallel.For(0, totalCount, i =>
            {
                batchValues[i] = getValuesFunc(i);
            });

            // 批量处理
            for (int batchStart = 0; batchStart < totalCount; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize, totalCount);

                for (int i = batchStart; i < batchEnd; i++)
                {
                    var row = createRowFunc(i);
                    var values = batchValues[i];
                    processRowFunc(row, i, values, additionalParam);
                }
            }
        }
        else
        {
            // 直接处理每一行
            for (int i = 0; i < totalCount; i++)
            {
                var row = createRowFunc(i);
                var values = getValuesFunc(i);
                processRowFunc(row, i, values, additionalParam);
            }
        }
    }

    /// <summary>
    /// 创建Excel模板
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="sheetName">工作表名称</param>
    /// <returns>Excel模板内存流</returns>
    public MemoryStream CreateExcelTemplate<T>() where T : class, new()
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

    /// <summary>
    /// 将对象转换为DataRow
    /// </summary>
    protected DataRow ObjectToDataRow<T>(T obj, DataTable dt) where T : class
    {
        var row = dt.NewRow();
        var properties = typeof(T).GetProperties().Where(p => p.CanRead);

        foreach (var prop in properties)
        {
            if (dt.Columns.Contains(prop.Name))
            {
                var value = prop.GetValue(obj);
                row[prop.Name] = value ?? DBNull.Value;
            }
        }

        return row;
    }

    /// <summary>
    /// 将流保存到文件
    /// </summary>
    protected async Task SaveStreamToFileAsync(Stream stream, string filePath)
    {
        EnsureDirectoryExists(filePath);
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        stream.Position = 0;
        await stream.CopyToAsync(fileStream);
        await fileStream.FlushAsync();
    }

    /// <summary>
    /// 获取Excel单元格值的通用方法
    /// </summary>
    protected object GetExcelCellValue(object cellValue, bool isDateFormat = false)
    {
        return ExcelValueConverter.ConvertToDbValue(cellValue, isDateFormat);
    }

    #region 异步方法

    /// <summary>
    /// 异步将Excel文件转换为DataTable
    /// </summary>
    public async Task<DataTable?> ExcelToDataTableAsync(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            Logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            // 使用TaskCompletionSource将同步操作包装为异步任务
            var tcs = new TaskCompletionSource<DataTable?>();
            await Task.Run(() =>
            {
                try
                {
                    var result = ConvertStreamToDataTable(fileStream, sheetName, headerRowIndex, addEmptyRow);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return await tcs.Task;
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
    public async Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            Logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            // 使用TaskCompletionSource将同步操作包装为异步任务
            var tcs = new TaskCompletionSource<List<T>?>();
            await Task.Run(() =>
            {
                try
                {
                    var result = ConvertStreamToList<T>(fileStream, sheetName, headerRowIndex, addEmptyRow);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return await tcs.Task;
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
    public async Task<string> DataTableToFileAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        try
        {
            using var ms = ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action);
            if (ms == null)
            {
                Logger?.LogError("转换DataTable到MemoryStream失败");
                throw new InvalidOperationException("转换DataTable到MemoryStream失败");
            }

            EnsureDirectoryExists(fullFileName);
            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            ms.Position = 0; // 确保内存流位置在开头
            await ms.CopyToAsync(fs);
            await fs.FlushAsync();

            return fullFileName;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "异步保存DataTable到Excel文件失败: {FilePath}", fullFileName);
            throw new ExcelException("异步保存DataTable到Excel文件失败", ex);
        }
    }

    /// <summary>
    /// 异步将对象列表导出为Excel文件
    /// </summary>
    public async Task<string> ListToFileAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null) where T : class
    {
        try
        {
            using var ms = ConvertCollectionToMemoryStream(list, sheetsName, title, action);
            if (ms == null)
            {
                Logger?.LogError("转换对象列表到MemoryStream失败");
                throw new InvalidOperationException("转换对象列表到MemoryStream失败");
            }

            EnsureDirectoryExists(fullFileName);
            using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
            ms.Position = 0; // 确保内存流位置在开头
            await ms.CopyToAsync(fs);
            await fs.FlushAsync();

            return fullFileName;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "异步保存对象列表到Excel文件失败: {FilePath}", fullFileName);
            throw new ExcelException("异步保存对象列表到Excel文件失败", ex);
        }
    }

    #endregion
}

/// <summary>
/// Excel配置选项
/// </summary>
public class ExcelOptions
{
    /// <summary>
    /// 并行处理阈值，超过此数量的数据将使用并行处理
    /// </summary>
    public int ParallelProcessingThreshold { get; set; } = 1000;

    /// <summary>
    /// 是否自动调整列宽
    /// </summary>
    public bool AutoFitColumns { get; set; } = true;

    /// <summary>
    /// 默认日期格式
    /// </summary>
    public string DefaultDateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 是否使用批量写入优化
    /// </summary>
    public bool UseBatchWrite { get; set; } = true;

    /// <summary>
    /// 批量写入大小
    /// </summary>
    public int BatchSize { get; set; } = 5000;

    /// <summary>
    /// 是否在错误时继续处理（不抛出异常）
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// 是否使用内存优化（处理大文件时）
    /// </summary>
    public bool UseMemoryOptimization { get; set; } = false;

    /// <summary>
    /// 内存优化缓冲区大小（行数）
    /// </summary>
    public int MemoryBufferSize { get; set; } = 1000;

    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;

    /// <summary>
    /// 性能监控阈值(毫秒)，超过此阈值才记录日志
    /// </summary>
    public int PerformanceThreshold { get; set; } = 500;
}

/// <summary>
/// Excel异常类
/// </summary>
public class ExcelException : Exception
{
    public ExcelException(string message) : base(message) { }

    public ExcelException(string message, Exception innerException) : base(message, innerException) { }
}