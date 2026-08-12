using Linger.Excel.Contracts.Attributes;

namespace Linger.Excel.Contracts;

/// <summary>
/// Excel基础实现类
/// </summary>
public abstract class ExcelBase<TWorkbook, TWorksheet>(ExcelOptions? options = null, ILogger? logger = null) : AbstractExcelService<TWorkbook, TWorksheet>(options, logger)
    where TWorkbook : class
    where TWorksheet : class
{

    #region Excel操作实现

    /// <summary>
    /// 将Excel文件转换为DataTable
    /// </summary>
    public override DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        return ImportFile(
            filePath,
            stream => StreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow));
    }

    /// <summary>
    /// 将Excel文件转换为对象列表
    /// </summary>
    public override List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        return ImportFile(
            filePath,
            stream => StreamToList<T>(stream, sheetName, headerRowIndex, addEmptyRow));
    }

    /// <summary>
    /// 使用行映射委托将 Excel 文件转换为对象列表。
    /// </summary>
    public override List<T>? ExcelToList<T>(string filePath, Func<ExcelRow, T> map, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        ArgumentNullException.ThrowIfNull(map);

        return ImportFile(
            filePath,
            stream => StreamToList(stream, map, sheetName, headerRowIndex, addEmptyRow));
    }

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)
    /// </summary>
    public override DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        return ImportFile(
            filePath,
            stream => StreamToDataSet(stream, sheetNames, headerRowIndex, addEmptyRow));
    }

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    public override DataSet? ExcelToDataSet(string filePath, Func<string, int?> headerRowIndexSelector, IEnumerable<string>? sheetNames = null, bool addEmptyRow = false)
    {
        return ImportFile(
            filePath,
            stream => StreamToDataSet(stream, headerRowIndexSelector, sheetNames, addEmptyRow));
    }

    private TResult? ImportFile<TResult>(string filePath, Func<Stream, TResult?> import)
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(import);

        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            Logger.LogWarning("Excel文件不存在或路径为空: {FilePath}", filePath);
            return null;
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return import(fileStream);
    }

    /// <summary>
    /// 数据表格转 Excel 文件
    /// </summary>
    protected override string DataTableToExcelCore(DataTable dataTable, string fullFileName, string sheetsName, string title,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null)
    {
        using var ms = DataTableToMemoryStream(dataTable, sheetsName, title, action, styleAction);
        ms.ToFile(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 对象集合转 Excel 文件
    /// </summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method relies on reflection-based property discovery. For AOT/trimming scenarios, use the explicit-column export overloads.")]
#endif
    protected override string CollectionToExcelCore<T>(List<T> list, string fullFileName, string sheetsName, string title,
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null)
    {
        using var ms = CollectionToMemoryStream(list, sheetsName, title, action, styleAction);
        ms.ToFile(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 使用显式列定义将对象集合导出为 Excel 文件。
    /// </summary>
    public override string CollectionToExcel<T>(IEnumerable<T> items, IEnumerable<ExcelExportColumn<T>> columns, string fullFileName,
        string sheetsName = "Sheet1", string title = "")
    {
        using var ms = CollectionToMemoryStream(items, columns, sheetsName, title);
        ms.ToFile(fullFileName);

        return fullFileName;
    }

    /// <summary>
    /// 数据集转 Excel 文件(每个DataTable一个工作表)
    /// </summary>
    protected override string DataSetToExcelCore(DataSet dataSet, string fullFileName, string defaultSheetName,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null)
    {
        return DataSetToExcelCore(dataSet, fullFileName, defaultSheetName, action, styleAction, worksheetAction: null);
    }

    /// <summary>
    /// Exports a data set and invokes an action for every created worksheet.
    /// </summary>
    /// <remarks>
    /// The action is invoked in source table order, including tables without columns. An exception
    /// thrown by the action stops the export and is propagated to the caller.
    /// </remarks>
    public override string DataSetToExcel(DataSet dataSet, string fullFileName, Action<IWorksheetExportContext<TWorksheet>> worksheetAction,
        string defaultSheetName = "Sheet")
    {
        ArgumentNullException.ThrowIfNull(worksheetAction);

        return DataSetToExcelCore(dataSet, fullFileName, defaultSheetName, action: null, styleAction: null, worksheetAction);
    }

    private string DataSetToExcelCore(
        DataSet dataSet,
        string fullFileName,
        string defaultSheetName,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action,
        Action<TWorksheet>? styleAction,
        Action<IWorksheetExportContext<TWorksheet>>? worksheetAction)
    {
        if (dataSet == null || dataSet.Tables.Count == 0)
        {
            Logger.LogWarning("要导出的DataSet为空或不包含任何DataTable");
            dataSet = new DataSet();
            // 使用"Sheet1"作为空DataSet的默认工作表名称，而不是仅使用前缀
            dataSet.Tables.Add(new DataTable($"{defaultSheetName}1"));
        }

        ValidateExportOptions();

        MemoryStream ms;
        if (Options.EnablePerformanceMonitoring)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            ms = ExportDataSet(dataSet, defaultSheetName, action, styleAction, worksheetAction);
            sw.Stop();

            if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
            {
                Logger.LogInformation("导出DataSet到Excel[表数:{TableCount}]耗时: {ElapsedMilliseconds}ms",
                    dataSet.Tables.Count, sw.ElapsedMilliseconds);
            }
        }
        else
        {
            ms = ExportDataSet(dataSet, defaultSheetName, action, styleAction, worksheetAction);
        }

        using (ms)
        {
            ms.ToFile(fullFileName);
            return fullFileName;
        }
    }

    /// <summary>
    /// 将Stream转换为DataTable（新方法）
    /// </summary>
    public override DataTable? StreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default)
    {
        return ImportFromStream(
            stream,
            sheetName,
            cancellationToken,
            worksheet =>
            {
                if (!Options.EnablePerformanceMonitoring)
                {
                    return ImportFromWorksheet(worksheet, headerRowIndex, addEmptyRow, cancellationToken);
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var dataTable = ImportFromWorksheet(worksheet, headerRowIndex, addEmptyRow, cancellationToken);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger.LogInformation(
                        "从Excel流导入到DataTable[行数:{RowCount}, 列数:{ColumnCount}]耗时: {ElapsedMilliseconds}ms",
                        dataTable.Rows.Count,
                        dataTable.Columns.Count,
                        sw.ElapsedMilliseconds);
                }

                return dataTable;
            });
    }

    /// <summary>
    /// 将Stream转换为对象列表（新方法）
    /// </summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to map properties. For AOT/trimming scenarios, use the ExcelRow mapper overload.")]
#endif
    public override List<T>? StreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default)
    {
        return ImportFromStream(
            stream,
            sheetName,
            cancellationToken,
            worksheet =>
            {
                if (!Options.EnablePerformanceMonitoring)
                {
                    return ImportWorksheetToList<T>(worksheet, headerRowIndex, addEmptyRow, cancellationToken);
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = ImportWorksheetToList<T>(worksheet, headerRowIndex, addEmptyRow, cancellationToken);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger.LogInformation(
                        "从Excel流导入到List[行数:{RowCount}]耗时: {ElapsedMilliseconds}ms",
                        result.Count,
                        sw.ElapsedMilliseconds);
                }

                return result;
            });
    }

    /// <summary>
    /// 使用行映射委托将 Excel 流转换为对象列表。
    /// </summary>
    public override List<T>? StreamToList<T>(Stream stream, Func<ExcelRow, T> map, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(map);

        return ImportFromStream(
            stream,
            sheetName,
            cancellationToken,
            worksheet =>
            {
                if (!Options.EnablePerformanceMonitoring)
                {
                    return ImportWorksheetToList(worksheet, map, headerRowIndex, addEmptyRow, cancellationToken);
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var result = ImportWorksheetToList(worksheet, map, headerRowIndex, addEmptyRow, cancellationToken);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger.LogInformation(
                        "从Excel流使用行映射导入到List[行数:{RowCount}]耗时: {ElapsedMilliseconds}ms",
                        result.Count,
                        sw.ElapsedMilliseconds);
                }

                return result;
            });
    }

    private TResult? ImportFromStream<TResult>(
        Stream stream,
        string? sheetName,
        CancellationToken cancellationToken,
        Func<TWorksheet, TResult> import)
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(import);
        cancellationToken.ThrowIfCancellationRequested();

        if (IsStreamEmpty(stream))
        {
            Logger.LogWarning("Excel流为空");
            return null;
        }

        TWorkbook? workbook = null;
        try
        {
            workbook = OpenWorkbook(stream, cancellationToken);
            if (workbook is null)
            {
                Logger.LogWarning("无法打开Excel工作簿");
                return null;
            }

            var worksheet = GetWorksheet(workbook, sheetName);
            if (worksheet is null)
            {
                Logger.LogWarning("工作表不存在: {SheetName}", sheetName ?? "默认");
                return null;
            }

            if (!HasData(worksheet))
            {
                Logger.LogWarning("工作表为空: {SheetName}", sheetName ?? "默认");
                return null;
            }

            return import(worksheet);
        }
        finally
        {
            if (workbook is not null)
            {
                CloseWorkbook(workbook);
            }
        }
    }

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)（新方法）
    /// </summary>
    public override DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsStreamEmpty(stream))
        {
            Logger.LogWarning("Excel流为空");
            return null;
        }

        TWorkbook? workbook = null;

        try
        {
            // 打开Excel工作簿
            workbook = OpenWorkbook(stream, cancellationToken);
            if (workbook == null)
            {
                Logger.LogWarning("无法打开Excel工作簿");
                return null;
            }

            // 获取要处理的工作表名称列表
            var targetSheetNames = GetTargetSheetNames(workbook, sheetNames);
            if (targetSheetNames == null || targetSheetNames.Count == 0)
            {
                Logger.LogWarning("工作簿中没有找到要处理的工作表");
                return new DataSet();
            }

            DataSet result;
            if (Options.EnablePerformanceMonitoring)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                result = ImportDataSetFromWorksheets(workbook, targetSheetNames, _ => headerRowIndex, addEmptyRow, cancellationToken);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger.LogInformation(
                        "从Excel流导入到DataSet[工作表数:{TableCount}]耗时: {ElapsedMilliseconds}ms",
                        result.Tables.Count,
                        sw.ElapsedMilliseconds);
                }
            }
            else
            {
                result = ImportDataSetFromWorksheets(workbook, targetSheetNames, _ => headerRowIndex, addEmptyRow, cancellationToken);
            }

            return result;
        }
        finally
        {
            // 确保释放资源
            if (workbook != null)
            {
                CloseWorkbook(workbook);
            }
        }
    }

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行（新方法）
    /// </summary>
    public override DataSet? StreamToDataSet(Stream stream, Func<string, int?> headerRowIndexSelector, IEnumerable<string>? sheetNames = null, bool addEmptyRow = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsStreamEmpty(stream))
        {
            Logger.LogWarning("Excel流为空");
            return null;
        }

        TWorkbook? workbook = null;

        try
        {
            // 打开Excel工作簿
            workbook = OpenWorkbook(stream, cancellationToken);
            if (workbook == null)
            {
                Logger.LogWarning("无法打开Excel工作簿");
                return null;
            }

            // 获取要处理的工作表名称列表
            var targetSheetNames = GetTargetSheetNames(workbook, sheetNames);
            if (targetSheetNames == null || targetSheetNames.Count == 0)
            {
                Logger.LogWarning("工作簿中没有找到要处理的工作表");
                return new DataSet();
            }

            DataSet result;
            if (Options.EnablePerformanceMonitoring)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                result = ImportDataSetFromWorksheets(workbook, targetSheetNames, sheetName => headerRowIndexSelector(sheetName) ?? 0, addEmptyRow, cancellationToken);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger.LogInformation(
                        "从Excel流导入到DataSet[工作表数:{TableCount}]耗时: {ElapsedMilliseconds}ms",
                        result.Tables.Count,
                        sw.ElapsedMilliseconds);
                }
            }
            else
            {
                result = ImportDataSetFromWorksheets(workbook, targetSheetNames, sheetName => headerRowIndexSelector(sheetName) ?? 0, addEmptyRow, cancellationToken);
            }

            return result;
        }
        finally
        {
            // 确保释放资源
            if (workbook != null)
            {
                CloseWorkbook(workbook);
            }
        }
    }

    /// <summary>
    /// 获取要处理的工作表名称列表
    /// </summary>
    private List<string> GetTargetSheetNames(TWorkbook workbook, IEnumerable<string>? requestedSheetNames)
    {
        // 获取所有可用的工作表名称
        var allSheetNames = GetAllSheetNames(workbook);

        // 如果没有指定工作表或指定的集合为空，则返回所有工作表
        if (requestedSheetNames is null)
        {
            return allSheetNames;
        }

        var requestedNames = requestedSheetNames.ToList();
        if (requestedNames.Count == 0)
        {
            return allSheetNames;
        }

        // 按请求的顺序返回存在的工作表（保持用户指定的顺序）
        var result = new List<string>();
        var sheetNamesByOrdinalIgnoreCase = allSheetNames.ToDictionary(name => name, StringComparer.OrdinalIgnoreCase);
        var selectedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var requestedName in requestedNames)
        {
            if (sheetNamesByOrdinalIgnoreCase.TryGetValue(requestedName, out var actualName))
            {
                if (selectedSheetNames.Add(actualName))
                {
                    result.Add(actualName);
                }
            }
            else
            {
                Logger.LogWarning("请求的工作表不存在: {SheetName}", requestedName);
            }
        }

        return result;
    }

    private static bool IsStreamEmpty(Stream? stream)
    {
        if (stream is null)
        {
            return true;
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException(null, nameof(stream));
        }

        return stream.CanSeek && stream.Length == 0;
    }

    /// <summary>
    /// 对象集合转 Excel 内存流（新方法）
    /// </summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method relies on reflection-based property discovery. For AOT/trimming scenarios, use the explicit-column export overloads.")]
#endif
    public override MemoryStream CollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null,
        Action<TWorksheet>? styleAction = null)
    {
        if (list == null)
        {
            Logger.LogWarning("要导出的列表为空");
            list = [];
        }

        ValidateExportOptions();

        MemoryStream result;
        if (Options.EnablePerformanceMonitoring)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            result = ExportCollection(list, sheetsName, title, action, styleAction);
            sw.Stop();

            if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
            {
                Logger.LogInformation("导出列表到Excel[行数:{Count}]耗时: {ElapsedMilliseconds}ms",
                    list.Count, sw.ElapsedMilliseconds);
            }
        }
        else
        {
            result = ExportCollection(list, sheetsName, title, action, styleAction);
        }

        return result;
    }

    /// <summary>
    /// 使用显式列定义将对象集合直接导出为 Excel 内存流。
    /// </summary>
    public override MemoryStream CollectionToMemoryStream<T>(
        IEnumerable<T> items,
        IEnumerable<ExcelExportColumn<T>> columns,
        string sheetsName = "Sheet1",
        string title = "")
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(columns);

        var itemList = items as IReadOnlyList<T> ?? items.ToList();
        var columnList = columns as IReadOnlyList<ExcelExportColumn<T>> ?? columns.ToList();
        ValidateExplicitExportColumns(columnList);
        ValidateExportOptions();

        if (!Options.EnablePerformanceMonitoring)
        {
            return ExportCollection(itemList, columnList, sheetsName, title);
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = ExportCollection(itemList, columnList, sheetsName, title);
        sw.Stop();

        if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
        {
            Logger.LogInformation(
                "使用显式列导出列表到Excel[行数:{RowCount}, 列数:{ColumnCount}]耗时: {ElapsedMilliseconds}ms",
                itemList.Count,
                columnList.Count,
                sw.ElapsedMilliseconds);
        }

        return result;
    }

    /// <summary>
    /// 数据表格转 Excel 内存流（新方法）
    /// </summary>
    public override MemoryStream DataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null,
        Action<TWorksheet>? styleAction = null)
    {
        if (dataTable == null)
        {
            Logger.LogWarning("要导出的DataTable为空");
            dataTable = new DataTable();
        }

        ValidateExportOptions();

        MemoryStream result;
        if (Options.EnablePerformanceMonitoring)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            result = ExportDataTable(dataTable, sheetsName, title, action, styleAction);
            sw.Stop();

            if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
            {
                Logger.LogInformation("导出DataTable到Excel[行数:{RowCount}, 列数:{ColumnCount}]耗时: {ElapsedMilliseconds}ms",
                    dataTable.Rows.Count, dataTable.Columns.Count, sw.ElapsedMilliseconds);
            }
        }
        else
        {
            result = ExportDataTable(dataTable, sheetsName, title, action, styleAction);
        }

        return result;
    }

    /// <summary>
    /// 导出集合到Excel
    /// </summary>
    private MemoryStream ExportCollection<T>(
        List<T> list,
        string sheetsName,
        string title,
        Action<TWorksheet, PropertyInfo[]>? action,
        Action<TWorksheet>? styleAction) where T : class
    {
        // 获取所有属性
        var properties = typeof(T).GetProperties().Where(p => p.CanRead).ToArray();

        // 创建Excel工作簿和工作表
        var workbook = CreateWorkbook();
        try
        {
            var worksheet = CreateWorksheet(workbook, sheetsName);

            var exportColumns = GetExportColumns(properties);
            var columnNames = exportColumns.Select(column => column.ColumnName).ToArray();

            // 应用标题
            var startRowIndex = 0;
            if (title.IsNotNullOrEmpty())
            {
                startRowIndex += ApplyTitle(worksheet, title, columnNames.Length);
            }

            // 创建表头行
            CreateHeaderRowCore(worksheet, columnNames, startRowIndex);

            // 填充数据行
            ProcessCollectionRows(worksheet, list, properties, startRowIndex);

            // 应用自定义处理
            action?.Invoke(worksheet, properties);

            // 应用样式
            styleAction?.Invoke(worksheet);

            // 进行工作表格式化
            ApplyWorksheetFormatting(worksheet, list.Count + startRowIndex + 1, columnNames.Length);

            // 保存到流
            return SaveWorkbookToStream(workbook);
        }
        finally
        {
            CloseWorkbook(workbook);
        }
    }

    private MemoryStream ExportCollection<T>(
        IReadOnlyList<T> items,
        IReadOnlyList<ExcelExportColumn<T>> columns,
        string sheetsName,
        string title)
    {
        var workbook = CreateWorkbook();
        try
        {
            var worksheet = CreateWorksheet(workbook, sheetsName);
            var columnNames = columns.Select(static column => column.Header).ToArray();

            var startRowIndex = 0;
            if (title.IsNotNullOrEmpty())
            {
                startRowIndex += ApplyTitle(worksheet, title, columnNames.Length);
            }

            CreateHeaderRowCore(worksheet, columnNames, startRowIndex);
            ProcessCollectionRows(worksheet, items, columns, startRowIndex);
            ApplyWorksheetFormatting(worksheet, items.Count + startRowIndex + 1, columnNames.Length);

            return SaveWorkbookToStream(workbook);
        }
        finally
        {
            CloseWorkbook(workbook);
        }
    }

    /// <summary>
    /// 导出DataTable到Excel
    /// </summary>
    private MemoryStream ExportDataTable(
        DataTable dataTable,
        string sheetsName,
        string title,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action,
        Action<TWorksheet>? styleAction)
    {
        // 创建Excel工作簿
        var workbook = CreateWorkbook();
        try
        {
            // 调用通用方法处理单个DataTable
            ExportDataTableToWorksheet(workbook, dataTable, sheetsName, title, action, styleAction, worksheetAction: null, tableIndex: 0);

            // 保存到流
            return SaveWorkbookToStream(workbook);
        }
        finally
        {
            CloseWorkbook(workbook);
        }
    }

    private void ValidateExportOptions()
    {
        if (Options.PerformanceThreshold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ExcelOptions.PerformanceThreshold), Options.PerformanceThreshold, "Performance threshold cannot be negative.");
        }

        var styleOptions = Options.StyleOptions;
        ValidateHexColor(styleOptions.TitleStyle.BackgroundColor);
        ValidateHexColor(styleOptions.TitleStyle.FontColor);
        ValidateHexColor(styleOptions.HeaderStyle.BackgroundColor);
        ValidateHexColor(styleOptions.HeaderStyle.FontColor);
    }

    private static void ValidateExplicitExportColumns<T>(IReadOnlyList<ExcelExportColumn<T>> columns)
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException("至少需要一个导出列。", nameof(columns));
        }

        var headers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in columns)
        {
            if (!headers.Add(column.Header))
            {
                throw new ArgumentException($"存在重复的导出列名: '{column.Header}'。", nameof(columns));
            }
        }
    }

    private static void ValidateHexColor(string? color)
    {
        if (color is null || color.Length == 0)
        {
            return;
        }

        var hexColor = color[0] == '#' ? color.Substring(1) : color;
        if (hexColor.Length != 6 || hexColor.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(null, nameof(color));
        }
    }

    private IReadOnlyList<(PropertyInfo Property, string ColumnName)> GetExportColumns(PropertyInfo[] properties)
    {
        var attributedColumns = GetExcelColumns(properties)
            .OrderBy(column => column.Index)
            .ToArray();

        if (attributedColumns.Length == 0)
        {
            return properties.Select(property => (property, property.Name)).ToArray();
        }

        return attributedColumns
            .Select(column => (properties.First(property => property.Name == column.Name), column.ColumnName))
            .ToArray();
    }

    protected PropertyInfo[] GetExportProperties(PropertyInfo[] properties)
    {
        return GetExportColumns(properties)
            .Select(column => column.Property)
            .ToArray();
    }

    /// <summary>
    /// 获取显式导出列的原始值，类型转换由统一的单元格写入入口处理。
    /// </summary>
    protected static object? GetExportValue<T>(ExcelExportColumn<T> column, T item)
    {
        return column.ValueSelector(item);
    }

    /// <summary>
    /// 导出DataSet到Excel
    /// </summary>
    private MemoryStream ExportDataSet(
        DataSet dataSet,
        string defaultSheetName,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action,
        Action<TWorksheet>? styleAction,
        Action<IWorksheetExportContext<TWorksheet>>? worksheetAction)
    {
        // 创建Excel工作簿
        var workbook = CreateWorkbook();
        try
        {
            // 遍历所有DataTable
            for (var i = 0; i < dataSet.Tables.Count; i++)
            {
                var dataTable = dataSet.Tables[i];
                var sheetName = !string.IsNullOrWhiteSpace(dataTable.TableName) ? dataTable.TableName : $"{defaultSheetName}{i + 1}";

                // 调用通用方法处理每个DataTable，不设置标题
                ExportDataTableToWorksheet(workbook, dataTable, sheetName, null, action, styleAction, worksheetAction, i);
            }

            // 保存到流
            return SaveWorkbookToStream(workbook);
        }
        finally
        {
            CloseWorkbook(workbook);
        }
    }

    /// <summary>
    /// 将单个DataTable导出到工作表的通用方法
    /// </summary>
    /// <param name="workbook">工作簿</param>
    /// <param name="dataTable">数据表</param>
    /// <param name="sheetName">工作表名称</param>
    /// <param name="title">标题(可选)</param>
    /// <param name="action">自定义处理</param>
    /// <param name="styleAction">样式处理</param>
    private void ExportDataTableToWorksheet(
        TWorkbook workbook,
        DataTable dataTable,
        string sheetName,
        string? title,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action,
        Action<TWorksheet>? styleAction,
        Action<IWorksheetExportContext<TWorksheet>>? worksheetAction,
        int tableIndex)
    {
        // 创建工作表
        var worksheet = CreateWorksheet(workbook, sheetName);

        if (dataTable.Columns.Count > 0)
        {
            // 获取所有列名
            var columnNames = dataTable.Columns.Cast<DataColumn>()
                .Select(col => col.ColumnName)
                .ToArray();
            var startRowIndex = 0;

            // 应用标题
            if (title.IsNotNullOrEmpty())
            {
                startRowIndex += ApplyTitle(worksheet, title, columnNames.Length);
            }

            // 创建表头行
            CreateHeaderRowCore(worksheet, columnNames, startRowIndex);

            // 填充数据行
            ProcessDataRows(worksheet, dataTable, startRowIndex);

            // 进行工作表格式化
            ApplyWorksheetFormatting(worksheet, dataTable.Rows.Count + startRowIndex + 1, columnNames.Length);
        }

        action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);
        styleAction?.Invoke(worksheet);
        worksheetAction?.Invoke(new WorksheetExportContext<TWorksheet>(worksheet, dataTable, tableIndex, sheetName));
    }

    /// <summary>
    /// Imports the requested worksheets into a data set.
    /// </summary>
    private DataSet ImportDataSetFromWorksheets(
        TWorkbook workbook,
        IEnumerable<string> sheetNames,
        Func<string, int> headerRowIndexSelector,
        bool addEmptyRow,
        CancellationToken cancellationToken)
    {
        var dataSet = new DataSet();
        foreach (var sheetName in sheetNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var worksheet = GetWorksheet(workbook, sheetName);
            if (worksheet is null || !HasData(worksheet))
            {
                Logger.LogDebug("跳过空工作表: {SheetName}", sheetName);
                continue;
            }

            var dataTable = ImportFromWorksheet(worksheet, headerRowIndexSelector(sheetName), addEmptyRow, cancellationToken);
            dataTable.TableName = sheetName;
            dataSet.Tables.Add(dataTable);
        }

        return dataSet;
    }

    /// <summary>
    /// Imports one worksheet using the provider's native row and column indexes.
    /// </summary>
    private DataTable ImportFromWorksheet(TWorksheet worksheet, int headerRowIndex, bool addEmptyRow, CancellationToken cancellationToken)
    {
        var dataTable = new DataTable(GetSheetName(worksheet));
        var definition = PrepareWorksheetImport(worksheet, headerRowIndex, cancellationToken);
        if (definition is null)
        {
            return dataTable;
        }

        // DataColumn 在写入行前必须确定类型；先扫描工作表可避免为类型推断额外缓存整张表。
        var columnTypes = InferColumnTypes(
            worksheet,
            definition.HeaderMappings,
            definition.StartRow,
            definition.EndRow,
            cancellationToken);

        foreach (var mapping in definition.HeaderMappings)
        {
            dataTable.Columns.Add(mapping.Value, columnTypes[mapping.Key]);
        }

        ReadWorksheetRows(
            worksheet,
            definition,
            addEmptyRow,
            cancellationToken,
            values => dataTable.Rows.Add(values));

        return dataTable;
    }

    private List<T> ImportWorksheetToList<T>(
        TWorksheet worksheet,
        int headerRowIndex,
        bool addEmptyRow,
        CancellationToken cancellationToken)
        where T : class, new()
    {
        var definition = PrepareWorksheetImport(worksheet, headerRowIndex, cancellationToken);
        if (definition is null)
        {
            return [];
        }

        var propertyMappings = CreateColumnPropertyMappings<T>(definition.HeaderMappings);
        var estimatedRowCount = Math.Max(0, definition.EndRow - definition.StartRow + 1);
        var result = new List<T>(estimatedRowCount);

        ReadWorksheetRows(
            worksheet,
            definition,
            addEmptyRow,
            cancellationToken,
            values =>
            {
                var item = new T();
                foreach (var mapping in propertyMappings)
                {
                    var value = values[mapping.Key];
                    if (value is not DBNull &&
                        TypeConverter.TryConvert(value, mapping.Value.PropertyType, out var convertedValue) &&
                        convertedValue is not null)
                    {
                        mapping.Value.SetValue(item, convertedValue);
                    }
                }

                result.Add(item);
            });

        return result;
    }

    private List<T> ImportWorksheetToList<T>(
        TWorksheet worksheet,
        Func<ExcelRow, T> map,
        int headerRowIndex,
        bool addEmptyRow,
        CancellationToken cancellationToken)
    {
        var definition = PrepareWorksheetImport(worksheet, headerRowIndex, cancellationToken);
        if (definition is null)
        {
            return [];
        }

        var columnIndexes = new Dictionary<string, int>(definition.HeaderMappings.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in definition.HeaderMappings)
        {
            columnIndexes.Add(mapping.Value, mapping.Key);
        }

        var estimatedRowCount = Math.Max(0, definition.EndRow - definition.StartRow + 1);
        var result = new List<T>(estimatedRowCount);

        ReadWorksheetRows(
            worksheet,
            definition,
            addEmptyRow,
            cancellationToken,
            values => result.Add(map(new ExcelRow(columnIndexes, values))));

        return result;
    }

    private WorksheetImportDefinition? PrepareWorksheetImport(
        TWorksheet worksheet,
        int headerRowIndex,
        CancellationToken cancellationToken)
    {
        var columnCount = EstimateColumnCount(worksheet);
        if (columnCount <= 0)
        {
            Logger.LogWarning("工作表为空或无法确定列数");
            return null;
        }

        var headerMappings = ExtractRawHeaderMappings(worksheet, headerRowIndex);
        var normalizedHeaderMappings = NormalizeHeaderMappings(headerMappings, columnCount, headerRowIndex);
        var startRow = GetDataStartRow(worksheet, headerRowIndex);
        var endRow = GetDataEndRow(worksheet);

        return new WorksheetImportDefinition(
            normalizedHeaderMappings,
            startRow,
            endRow);
    }

    private void ReadWorksheetRows(
        TWorksheet worksheet,
        WorksheetImportDefinition definition,
        bool addEmptyRow,
        CancellationToken cancellationToken,
        Action<object[]> addRow)
    {
        ArgumentNullException.ThrowIfNull(addRow);

        for (var rowNum = definition.StartRow; rowNum <= definition.EndRow; rowNum++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = new object[definition.HeaderMappings.Count];
            var hasValue = false;

            foreach (var mapping in definition.HeaderMappings)
            {
                var cellValue = GetCellValue(worksheet, rowNum, mapping.Key);
                values[mapping.Key] = cellValue;
                hasValue |= cellValue is not DBNull;
            }

            if (hasValue || addEmptyRow)
            {
                addRow(values);
            }
        }
    }

    private static Dictionary<int, PropertyInfo> CreateColumnPropertyMappings<T>(
        IReadOnlyDictionary<int, string> headerMappings)
    {
        var propertyLookup = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in typeof(T).GetProperties().Where(static property => property.CanWrite))
        {
            var excelAttribute = property.GetCustomAttribute<ExcelColumnAttribute>();
            if (excelAttribute is not null && excelAttribute.ColumnName.IsNotNullOrWhiteSpace())
            {
                propertyLookup[excelAttribute.ColumnName] = property;
            }

            propertyLookup[property.Name] = property;
        }

        var propertyMappings = new Dictionary<int, PropertyInfo>();
        foreach (var mapping in headerMappings)
        {
            if (propertyLookup.TryGetValue(mapping.Value, out var property))
            {
                propertyMappings[mapping.Key] = property;
            }
        }

        return propertyMappings;
    }

    private sealed class WorksheetImportDefinition(
        IReadOnlyDictionary<int, string> headerMappings,
        int startRow,
        int endRow)
    {
        public IReadOnlyDictionary<int, string> HeaderMappings { get; } = headerMappings;

        public int StartRow { get; } = startRow;

        public int EndRow { get; } = endRow;
    }

    private Dictionary<int, Type> InferColumnTypes(
        TWorksheet worksheet,
        IReadOnlyDictionary<int, string> headerMappings,
        int startRow,
        int endRow,
        CancellationToken cancellationToken)
    {
        var inferredTypes = new Dictionary<int, Type?>(headerMappings.Count);
        foreach (var mapping in headerMappings)
        {
            inferredTypes[mapping.Key] = null;
        }

        for (var rowNum = startRow; rowNum <= endRow; rowNum++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var mapping in headerMappings)
            {
                var columnIndex = mapping.Key;
                if (inferredTypes[columnIndex] == typeof(object))
                {
                    continue;
                }

                var cellValue = GetCellValue(worksheet, rowNum, columnIndex);
                if (cellValue is DBNull)
                {
                    continue;
                }

                var valueType = GetSupportedColumnType(cellValue.GetType());
                inferredTypes[columnIndex] = MergeColumnTypes(inferredTypes[columnIndex], valueType);
            }
        }

        var columnTypes = new Dictionary<int, Type>(inferredTypes.Count);
        foreach (var inferredType in inferredTypes)
        {
            columnTypes[inferredType.Key] = inferredType.Value ?? typeof(object);
        }

        return columnTypes;
    }

    private static Type GetSupportedColumnType(Type valueType)
    {
        if (valueType == typeof(Guid) || valueType == typeof(TimeSpan) || valueType == typeof(byte[]))
        {
            return valueType;
        }

        return Type.GetTypeCode(valueType) switch
        {
            TypeCode.Boolean or
            TypeCode.Byte or
            TypeCode.SByte or
            TypeCode.Int16 or
            TypeCode.UInt16 or
            TypeCode.Int32 or
            TypeCode.UInt32 or
            TypeCode.Int64 or
            TypeCode.UInt64 or
            TypeCode.Single or
            TypeCode.Double or
            TypeCode.Decimal or
            TypeCode.Char or
            TypeCode.String or
            TypeCode.DateTime => valueType,
            _ => typeof(object)
        };
    }

    private static Type MergeColumnTypes(Type? currentType, Type valueType)
    {
        if (currentType is null || currentType == valueType)
        {
            return valueType;
        }

        if (currentType == typeof(object) || valueType == typeof(object))
        {
            return typeof(object);
        }

        return IsNumericType(currentType) && IsNumericType(valueType)
            ? GetCommonNumericType(currentType, valueType)
            : typeof(object);
    }

    private static bool IsNumericType(Type type)
    {
        return Type.GetTypeCode(type) is
            TypeCode.Byte or
            TypeCode.SByte or
            TypeCode.Int16 or
            TypeCode.UInt16 or
            TypeCode.Int32 or
            TypeCode.UInt32 or
            TypeCode.Int64 or
            TypeCode.UInt64 or
            TypeCode.Single or
            TypeCode.Double or
            TypeCode.Decimal;
    }

    private static Type GetCommonNumericType(Type leftType, Type rightType)
    {
        if (leftType == typeof(double) || rightType == typeof(double) ||
            leftType == typeof(float) || rightType == typeof(float))
        {
            return typeof(double);
        }

        if (leftType == typeof(decimal) || rightType == typeof(decimal))
        {
            return typeof(decimal);
        }

        if (leftType == typeof(ulong) || rightType == typeof(ulong))
        {
            return IsSignedIntegralType(leftType) || IsSignedIntegralType(rightType)
                ? typeof(decimal)
                : typeof(ulong);
        }

        if (leftType == typeof(long) || rightType == typeof(long) ||
            leftType == typeof(uint) || rightType == typeof(uint))
        {
            return typeof(long);
        }

        return typeof(int);
    }

    private static bool IsSignedIntegralType(Type type)
    {
        return Type.GetTypeCode(type) is
            TypeCode.SByte or
            TypeCode.Int16 or
            TypeCode.Int32 or
            TypeCode.Int64;
    }

    /// <summary>
    /// 归一化表头映射，统一不同提供者的列行为。
    /// </summary>
    /// <remarks>
    /// 规则：
    /// 1. 按列索引补齐缺失列；
    /// 2. 空白列名回退为 ColumnN；
    /// 3. 重复列名自动追加后缀，避免 DataTable 重名异常；
    /// 4. headerRowIndex 小于 0 时总是使用 ColumnN。
    /// </remarks>
    private static IReadOnlyDictionary<int, string> NormalizeHeaderMappings(
        IReadOnlyDictionary<int, string> headerMappings,
        int estimatedColumnCount,
        int headerRowIndex)
    {
        var normalizedMappings = new SortedDictionary<int, string>();

        var maxMappedColumnIndex = -1;
        foreach (var mappedColumnIndex in headerMappings.Keys)
        {
            if (mappedColumnIndex > maxMappedColumnIndex)
            {
                maxMappedColumnIndex = mappedColumnIndex;
            }
        }

        var effectiveColumnCount = Math.Max(estimatedColumnCount, maxMappedColumnIndex + 1);
        if (effectiveColumnCount <= 0)
        {
            return normalizedMappings;
        }

        var usedColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var columnIndex = 0; columnIndex < effectiveColumnCount; columnIndex++)
        {
            var baseColumnName = headerRowIndex < 0
                ? $"Column{columnIndex + 1}"
                : GetHeaderNameOrDefault(headerMappings, columnIndex);

            var uniqueColumnName = baseColumnName;
            var duplicateSuffix = 2;
            while (!usedColumnNames.Add(uniqueColumnName))
            {
                uniqueColumnName = $"{baseColumnName}_{duplicateSuffix}";
                duplicateSuffix++;
            }

            normalizedMappings[columnIndex] = uniqueColumnName;
        }

        return normalizedMappings;

        static string GetHeaderNameOrDefault(IReadOnlyDictionary<int, string> mappings, int index)
        {
            if (!mappings.TryGetValue(index, out var mappedName) || string.IsNullOrWhiteSpace(mappedName))
            {
                return $"Column{index + 1}";
            }

            return mappedName.Trim();
        }
    }

    #endregion

    #region 由子类实现的抽象方法

    /// <summary>
    /// 打开Excel工作簿
    /// </summary>
    protected abstract TWorkbook OpenWorkbook(Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// 获取工作表
    /// </summary>
    protected abstract TWorksheet? GetWorksheet(TWorkbook workbook, string? sheetName);

    /// <summary>
    /// 获取工作表名称
    /// </summary>
    protected abstract string GetSheetName(TWorksheet worksheet);

    /// <summary>
    /// 获取所有工作表名称
    /// </summary>
    protected abstract List<string> GetAllSheetNames(TWorkbook workbook);

    /// <summary>
    /// 检查工作表是否包含数据
    /// </summary>
    protected abstract bool HasData(TWorksheet worksheet);

    /// <summary>
    /// 创建工作簿
    /// </summary>
    protected abstract TWorkbook CreateWorkbook();

    /// <summary>
    /// 创建工作表
    /// </summary>
    protected abstract TWorksheet CreateWorksheet(TWorkbook workbook, string sheetName);

    /// <summary>
    /// 获取数据开始行索引
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)</param>
    /// <returns>数据起始行索引(按库的原生索引格式)</returns>
    /// <remarks>
    /// 此方法应该处理headerRowIndex为负值的情况，通常返回第一个有效的数据行索引。
    /// 返回的索引需使用实现库的原生索引格式（NPOI 为 0-based，ClosedXML 为 1-based）。
    /// </remarks>
    protected abstract int GetDataStartRow(TWorksheet worksheet, int headerRowIndex);

    /// <summary>
    /// 获取数据结束行索引
    /// </summary>
    protected abstract int GetDataEndRow(TWorksheet worksheet);

    /// <summary>
    /// 关闭工作簿
    /// </summary>
    protected abstract void CloseWorkbook(TWorkbook workbook);

    /// <summary>
    /// 估计列数
    /// </summary>
    protected abstract int EstimateColumnCount(TWorksheet worksheet);

    /// <summary>
    /// 创建表头映射
    /// </summary>
    protected abstract Dictionary<int, string> ExtractRawHeaderMappings(TWorksheet worksheet, int headerRowIndex);

    /// <summary>
    /// 获取单元格值
    /// </summary>
    protected abstract object GetCellValue(TWorksheet worksheet, int rowNum, int colIndex);

    /// <summary>
    /// 应用标题
    /// </summary>
    protected abstract int ApplyTitle(TWorksheet worksheet, string title, int columnCount);

    /// <summary>
    /// 创建标题行的核心方法
    /// </summary>
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
    /// 使用显式列定义处理集合数据行。
    /// </summary>
    protected abstract void ProcessCollectionRows<T>(TWorksheet worksheet, IReadOnlyList<T> items,
        IReadOnlyList<ExcelExportColumn<T>> columns, int startRowIndex);

    /// <summary>
    /// 应用工作表格式化
    /// </summary>
    protected abstract void ApplyWorksheetFormatting(TWorksheet worksheet, int rowCount, int columnCount);

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected abstract MemoryStream SaveWorkbookToStream(TWorkbook workbook);

    #endregion

}
