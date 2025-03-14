using Linger.Excel.Contracts.Attributes;

namespace Linger.Excel.Contracts;

public abstract class ExcelBase<TWorkbook, TWorksheet>(ExcelOptions? options = null, ILogger? logger = null) : IExcel
    where TWorkbook : class
    where TWorksheet : class
{
    /// <summary>
    /// Excel配置选项
    /// </summary>
    protected readonly ExcelOptions Options = options ?? new ExcelOptions();

    /// <summary>
    /// Excel内容类型
    /// </summary>
    public static string ContentType => "application/vnd.ms-excel";

    #region 格式化常量
    /// <summary>
    /// 整数格式
    /// </summary>
    protected const string INTEGER_FORMAT = "#,##0";

    /// <summary>
    /// 小数格式
    /// </summary>
    protected const string DECIMAL_FORMAT = "#,##0.00";

    /// <summary>
    /// 头部字体大小
    /// </summary>
    protected const int HEADER_FONT_SIZE = 14;

    /// <summary>
    /// 标题字体大小
    /// </summary>
    protected const int TITLE_FONT_SIZE = 10;
    #endregion

    #region Import
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
            logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ConvertStreamToDataTable(fileStream, sheetName, headerRowIndex, addEmptyRow);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "从Excel文件读取失败: {FilePath}", filePath);
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
            logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ConvertStreamToList<T>(fileStream, sheetName, headerRowIndex, addEmptyRow);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "从Excel文件读取并转换为对象列表失败: {FilePath}", filePath);
            return null;
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
    public virtual DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            logger?.LogWarning("无效的Stream: 不可读或长度为0");
            return null;
        }

        return ExecuteSafely(() =>
        {
            // 打开工作簿 - 不使用using，手动管理资源
            var workbook = OpenWorkbook(stream);
            if (workbook == null)
            {
                logger?.LogError("无法打开Excel工作簿");
                return null;
            }

            try
            {
                // 获取工作表
                var worksheet = GetWorksheet(workbook, sheetName);
                if (worksheet == null)
                {
                    logger?.LogError("无法获取Excel工作表");
                    return null;
                }

                // 验证工作表是否包含数据
                if (!HasData(worksheet))
                {
                    logger?.LogWarning("工作表 {SheetName} 不包含数据", GetSheetName(worksheet));
                    return new DataTable(GetSheetName(worksheet));
                }

                var dataTable = new DataTable(GetSheetName(worksheet));

                // 获取表头行索引和数据行范围
                int startRow = headerRowIndex < 0 ? 0 : headerRowIndex;
                int endRow = GetDataEndRow(worksheet);

                // 处理表头和列定义
                CreateDataTableColumns(worksheet, dataTable, headerRowIndex);

                // 处理数据行
                //Npoi 的 startRow 从 0 开始
                ProcessDataTableRows(worksheet, dataTable, startRow, endRow, addEmptyRow);

                return dataTable;
            }
            finally
            {
                // 确保无论如何都释放工作簿资源
                CloseWorkbook(workbook);
            }
        }, nameof(ConvertStreamToDataTable));
    }

    // 添加新的辅助方法以创建数据表列
    protected virtual void CreateDataTableColumns(TWorksheet worksheet, DataTable dataTable, int headerRowIndex)
    {
        if (headerRowIndex < 0)
        {
            // 没有表头，使用默认列名
            int columnCount = EstimateColumnCount(worksheet);
            for (int i = 0; i < columnCount; i++)
            {
                dataTable.Columns.Add($"Column{i + 1}");
            }
        }
        else
        {
            // 使用指定行作为表头
            var headerMappings = CreateHeaderMappings(worksheet, headerRowIndex);
            foreach (var mapping in headerMappings)
            {
                string columnName = mapping.Value;
                if (string.IsNullOrEmpty(columnName))
                    columnName = $"Column{mapping.Key + 1}";

                // 处理重复的列名
                if (dataTable.Columns.Contains(columnName))
                    columnName = $"{columnName}_{mapping.Key}";

                dataTable.Columns.Add(columnName);
            }
        }
    }

    // 添加处理数据行的方法
    protected virtual void ProcessDataTableRows(TWorksheet worksheet, DataTable dataTable, int startRow, int endRow, bool addEmptyRow)
    {
        for (int rowNum = startRow; rowNum <= endRow; rowNum++)
        {
            var dataRow = dataTable.NewRow();
            bool hasData = GetRowValues(worksheet, rowNum, dataRow);

            // 只添加有数据的行或指定添加空行的情况
            if (hasData || addEmptyRow)
            {
                dataTable.Rows.Add(dataRow);
            }

            // 内存优化
            OptimizeMemory(rowNum);
        }
    }

    // 添加获取行值的方法
    protected virtual bool GetRowValues(TWorksheet worksheet, int rowNum, DataRow dataRow)
    {
        // 这是默认实现，子类可以重写以提供更高效的实现
        bool hasData = false;

        // 根据行索引和DataRow的列来获取单元格值
        for (int colIndex = 0; colIndex < dataRow.Table.Columns.Count; colIndex++)
        {
            object cellValue = GetCellValue(worksheet, rowNum, colIndex);

            if (cellValue != DBNull.Value)
            {
                dataRow[colIndex] = cellValue;
                hasData = true;
            }
        }

        return hasData;
    }

    // 添加获取单元格值的方法
    protected virtual object GetCellValue(TWorksheet worksheet, int rowNum, int colIndex)
    {
        // 这是一个简单的默认实现，子类应该重写以提供实际的单元格值获取逻辑
        return DBNull.Value;
    }

    // 添加估算列数的方法
    protected virtual int EstimateColumnCount(TWorksheet worksheet)
    {
        // 默认返回一个基本值，子类应该重写以提供准确的列计数
        return 10;
    }

    // 添加创建表头映射的方法
    protected virtual Dictionary<int, string> CreateHeaderMappings(TWorksheet worksheet, int headerRowIndex)
    {
        // 默认返回空映射，子类应该重写以提供实际的表头映射
        return new Dictionary<int, string>();
    }

    /// <summary>
    /// 将Stream转换为对象列表 - 适用于中小型Excel文件
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="stream">要转换的Stream</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="headerRowIndex">列名所在行号,从0开始,默认0</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的对象列表</returns>
    /// <remarks>此方法将Excel数据一次性全部加载到内存，如果处理大文件请考虑使用StreamReadExcel</remarks>
    public List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        var dataTable = ExecuteSafely(() => ConvertStreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow), "读取Excel到DataTable");

        if (dataTable == null || dataTable.Columns.Count == 0)
        {
            logger?.LogWarning("无法从Stream转换为DataTable或结果为空表");
            return new List<T>();
        }

        // 如果数据量过大，建议使用StreamReadExcel
        if (dataTable.Rows.Count > 10000)
        {
            logger?.LogWarning("当前数据量较大({RowCount}行)，考虑使用StreamReadExcel方法以减少内存占用", dataTable.Rows.Count);
        }

        return ExecuteSafely(() => dataTable.ToList<T>(Options.ParallelProcessingThreshold),
            nameof(ConvertStreamToList));
    }

    /// <summary>
    /// 优化内存使用
    /// </summary>
    /// <param name="currentRowIndex">当前处理的行索引</param>
    protected void OptimizeMemory(int currentRowIndex)
    {
        if (Options.UseMemoryOptimization && currentRowIndex % Options.MemoryBufferSize == 0)
        {
            GC.Collect(0, GCCollectionMode.Optimized);
        }
    }

    #endregion

    #region Export
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
        using var ms = ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action);
        if (ms == null)
        {
            logger?.LogError("转换DataTable到MemoryStream失败");
            throw new InvalidOperationException("转换DataTable到MemoryStream失败");
        }

        ms.ToFile(fullFileName);
        return fullFileName;
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
        using var ms = ConvertCollectionToMemoryStream(list, sheetsName, title, action);
        if (ms == null)
        {
            logger?.LogError("转换对象列表到MemoryStream失败");
            throw new InvalidOperationException("转换对象列表到MemoryStream失败");
        }

        ms.ToFile(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 将DataTable转换为MemoryStream
    /// </summary>
    /// <param name="dataTable">数据表</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>内存流</returns>
    public virtual MemoryStream? ConvertDataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = "Sheet1",
        string title = "",
        Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable == null || dataTable.Columns.Count == 0)
        {
            logger?.LogWarning("要转换的DataTable为空或没有列");
            return null;
        }

        // 1. 创建工作簿和工作表
        var workbook = CreateWorkbook();
        var worksheet = CreateWorksheet(workbook, sheetsName);

        // 2. 应用标题(如果有)
        int titleRowsCount = 0;
        if (!string.IsNullOrEmpty(title))
        {
            titleRowsCount = ApplyTitle(worksheet, title, dataTable.Columns.Count);
        }

        // 3. 创建标题行
        CreateHeaderRow(worksheet, dataTable.Columns, titleRowsCount);

        // 4. 处理数据行
        ProcessDataRows(worksheet, dataTable, titleRowsCount);

        // 5. 执行自定义操作
        action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

        // 6. 应用工作表格式化
        if (Options.AutoFitColumns)
        {
            ApplyWorksheetFormatting(worksheet, titleRowsCount + dataTable.Rows.Count, dataTable.Columns.Count);
        }

        // 7. 保存并返回
        return SaveWorkbookToStream(workbook);
    }

    /// <summary>
    /// 将对象列表转换为MemoryStream
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="list">对象列表</param>
    /// <param name="sheetsName">工作表名称</param>
    /// <param name="title">标题</param>
    /// <param name="action">自定义操作</param>
    /// <returns>内存流</returns>
    public virtual MemoryStream? ConvertCollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = "Sheet1",
        string title = "",
        Action<object, PropertyInfo[]>? action = null) where T : class
    {
        if (list == null || list.Count == 0)
        {
            logger?.LogWarning("要转换的集合为空");
            return null;
        }

        // 1. 创建工作簿和工作表
        var workbook = CreateWorkbook();
        var worksheet = CreateWorksheet(workbook, sheetsName);

        // 2. 获取属性信息
        var properties = GetTypeProperties<T>();

        // 3. 应用标题(如果有)
        int titleRowsCount = 0;
        if (!string.IsNullOrEmpty(title))
        {
            titleRowsCount = ApplyTitle(worksheet, title, properties.Length);
        }

        // 4. 创建标题行
        CreateCollectionHeaderRow(worksheet, properties, titleRowsCount);

        // 5. 处理数据行
        ProcessCollectionRows(worksheet, list, properties, titleRowsCount);

        // 6. 执行自定义操作
        action?.Invoke(worksheet, properties);

        // 7. 应用工作表格式化
        if (Options.AutoFitColumns)
        {
            ApplyWorksheetFormatting(worksheet, titleRowsCount + list.Count + 1, properties.Length);
        }

        // 8. 保存并返回
        return SaveWorkbookToStream(workbook);
    }

    /// <summary>
    /// 创建空工作簿
    /// </summary>
    protected abstract TWorkbook CreateWorkbook();

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected abstract TWorksheet CreateWorksheet(TWorkbook workbook, string sheetName);

    /// <summary>
    /// 应用标题到工作表
    /// </summary>
    /// <returns>标题占用的行数</returns>
    protected abstract int ApplyTitle(TWorksheet worksheet, string title, int columnCount);

    /// <summary>
    /// 创建标题行
    /// </summary>
    protected virtual void CreateHeaderRow(TWorksheet worksheet, DataColumnCollection columns, int startRowIndex)
    {
        string[] columnNames = new string[columns.Count];

        for (int i = 0; i < columns.Count; i++)
        {
            columnNames[i] = columns[i].ColumnName;
        }
        CreateHeaderRowCore(worksheet, columnNames, startRowIndex);
    }

    /// <summary>
    /// 创建集合标题行
    /// </summary>
    protected virtual void CreateCollectionHeaderRow(TWorksheet worksheet, PropertyInfo[] properties, int startRowIndex)
    {
        // 获取有ExcelColumn特性的列，如果没有则使用所有列
        var columns = GetExcelColumns(properties);
        if (columns.Count == 0)
        {
            string[] columnNames = properties.Select(p => p.Name).ToArray();
            CreateHeaderRowCore(worksheet, columnNames, startRowIndex);
        }
        else
        {
            // 使用特性标记的属性及其顺序
            columns = columns.OrderBy(c => c.Item3).ToList();
            string[] columnNames = columns.Select(c =>
                string.IsNullOrEmpty(c.Item2) ? c.Item1 : c.Item2).ToArray();
            CreateHeaderRowCore(worksheet, columnNames, startRowIndex);
        }
    }

    /// <summary>
    /// 创建标题行的核心逻辑 - 供子类实现使用
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="columnNames">列名数组</param>
    /// <param name="startRowIndex">起始行索引</param>
    protected abstract void CreateHeaderRowCore(TWorksheet worksheet, string[] columnNames, int startRowIndex);

    /// <summary>
    /// 处理数据行
    /// </summary>
    protected abstract void ProcessDataRows(TWorksheet worksheet, DataTable dataTable, int startRowIndex);

    /// <summary>
    /// 处理集合数据行
    /// </summary>
    protected abstract void ProcessCollectionRows<T>(TWorksheet worksheet, List<T> list, PropertyInfo[] properties, int startRowIndex) where T : class;

    /// <summary>
    /// 应用工作表格式化
    /// </summary>
    protected abstract void ApplyWorksheetFormatting(TWorksheet worksheet, int rowCount, int columnCount);

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected abstract MemoryStream SaveWorkbookToStream(TWorkbook workbook);

    #endregion

    /// <summary>
    /// 执行操作并提供安全性和性能监控
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称（用于日志）</param>
    /// <param name="defaultValue">出错时返回的默认值</param>
    /// <returns>操作结果或默认值</returns>
    protected T? ExecuteSafely<T>(Func<T> operation, string operationName, T? defaultValue = default)
    {
        System.Diagnostics.Stopwatch? sw = null;
        if (Options.EnablePerformanceMonitoring)
        {
            sw = System.Diagnostics.Stopwatch.StartNew();
        }

        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Excel操作失败: {OperationName}", operationName);
            return defaultValue;
        }
        finally
        {
            if (sw != null)
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    logger?.LogInformation("{Operation} 耗时: {Time}ms", operationName, sw.ElapsedMilliseconds);
                }
            }
        }
    }

    /// <summary>
    /// 获取类型的可读属性
    /// </summary>
    protected virtual PropertyInfo[] GetTypeProperties<T>() where T : class
    {
        return typeof(T).GetProperties()
            .Where(p => p.CanRead)
            .ToArray();
    }

    /// <summary>
    /// 获取标记有ExcelColumn特性的属性信息
    /// </summary>
    protected List<(string Name, string ColumnName, int Index)> GetExcelColumns(IEnumerable<PropertyInfo> properties)
    {
        List<(string Name, string ColumnName, int Index)> columns = [];
        Type excelColumnAttributeType = typeof(ExcelColumnAttribute);

        foreach (var prop in properties)
        {
            var attrs = prop.GetCustomAttributesData();
            if (attrs.Any(a => a.AttributeType == excelColumnAttributeType))
            {
                var attr = prop.GetCustomAttributes(excelColumnAttributeType, true)
                    .FirstOrDefault() as ExcelColumnAttribute;

                if (attr != null)
                {
                    columns.Add(new(
                        prop.Name,
                        attr.ColumnName.IsNullOrEmpty() ? prop.Name : attr.ColumnName,
                        attr.Index == int.MaxValue ? columns.Count : attr.Index));
                }
            }
        }

        return columns;
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
        return await ProcessExcelFileAsync<DataTable?>(
            filePath,
            stream => ConvertStreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow),
            "从Excel文件异步读取");
    }

    /// <summary>
    /// 异步将Excel文件转换为对象列表
    /// </summary>
    public async Task<List<T>?> ExcelToListAsync<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false) where T : class, new()
    {
        return await ProcessExcelFileAsync<List<T>?>(
            filePath,
            stream => ConvertStreamToList<T>(stream, sheetName, headerRowIndex, addEmptyRow),
            "从Excel文件异步读取并转换为对象列表");
    }

    /// <summary>
    /// 异步处理Excel文件
    /// </summary>
    private async Task<TResult> ProcessExcelFileAsync<TResult>(string filePath, Func<Stream, TResult> operation, string operationName)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            logger?.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return default!;
        }

        try
        {
            // 在Task.Run内部创建和使用文件流，确保流在使用完毕后才关闭
            return await Task.Run(() =>
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return operation(fileStream);
            });
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{OperationName}失败: {FilePath}", operationName, filePath);
            return default!;
        }
    }

    /// <summary>
    /// 异步将DataTable导出为Excel文件
    /// </summary>
    public async Task<string> DataTableToFileAsync(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        using var ms = ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action);
        if (ms == null)
        {
            logger?.LogError("转换DataTable到MemoryStream失败");
            throw new InvalidOperationException("转换DataTable到MemoryStream失败");
        }

        await ms.ToFileAsync(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 异步将对象列表导出为Excel文件
    /// </summary>
    public async Task<string> ListToFileAsync<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null) where T : class
    {
        using var ms = ConvertCollectionToMemoryStream(list, sheetsName, title, action);
        if (ms == null)
        {
            logger?.LogError("转换对象列表到MemoryStream失败");
            throw new InvalidOperationException("转换对象列表到MemoryStream失败");
        }

        await ms.ToFileAsync(fullFileName);
        return fullFileName;
    }

    #endregion

    #region 由子类实现的抽象方法

    /// <summary>
    /// 打开Excel工作簿
    /// </summary>
    protected abstract TWorkbook OpenWorkbook(Stream stream);

    /// <summary>
    /// 获取工作表
    /// </summary>
    protected abstract TWorksheet GetWorksheet(TWorkbook workbook, string? sheetName);

    /// <summary>
    /// 获取工作表名称
    /// </summary>
    protected abstract string GetSheetName(TWorksheet worksheet);

    /// <summary>
    /// 检查工作表是否包含数据
    /// </summary>
    protected abstract bool HasData(TWorksheet worksheet);

    /// <summary>
    /// 创建属性映射关系
    /// </summary>
    protected abstract Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(TWorksheet worksheet, int headerRowIndex) where T : class, new();

    /// <summary>
    /// 获取数据开始行
    /// </summary>
    protected abstract int GetDataStartRow(TWorksheet worksheet, int headerRowIndex);

    /// <summary>
    /// 获取数据结束行
    /// </summary>
    protected abstract int GetDataEndRow(TWorksheet worksheet);

    /// <summary>
    /// 关闭工作簿并释放资源
    /// </summary>
    protected abstract void CloseWorkbook(TWorkbook workbook);

    #endregion
}