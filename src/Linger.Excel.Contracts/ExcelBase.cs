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
    public override List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
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
    /// 数据表格转 Excel 文件
    /// </summary>
    public override string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null)
    {
        using var ms = ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action, styleAction);
        ms.ToFile(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 列表转 Excel 文件
    /// </summary>
    public override string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "Sheet1", string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null)
    {
        using var ms = ConvertCollectionToMemoryStream(list, sheetsName, title, action, styleAction);
        ms.ToFile(fullFileName);
        return fullFileName;
    }

    /// <summary>
    /// 数据集转 Excel 文件(每个DataTable一个工作表)
    /// </summary>
    public override string DataSetToFile(DataSet dataSet, string fullFileName, string defaultSheetName = "Sheet",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null)
    {
        if (dataSet == null || dataSet.Tables.Count == 0)
        {
            Logger?.LogWarning("要导出的DataSet为空或不包含任何DataTable");
            dataSet = new DataSet();
            // 使用"Sheet1"作为空DataSet的默认工作表名称，而不是仅使用前缀
            dataSet.Tables.Add(new DataTable($"{defaultSheetName}1"));
        }

        try
        {
            MemoryStream ms;
            if (Options.EnablePerformanceMonitoring)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                ms = ExportDataSet(dataSet, defaultSheetName, action, styleAction);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger?.LogInformation("导出DataSet到Excel[表数:{TableCount}]耗时: {ElapsedMilliseconds}ms",
                        dataSet.Tables.Count, sw.ElapsedMilliseconds);
                }
            }
            else
            {
                ms = ExportDataSet(dataSet, defaultSheetName, action, styleAction);
            }

            ms.ToFile(fullFileName);
            ms.Dispose();
            return fullFileName;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "导出DataSet到Excel失败");
            throw new InvalidOperationException("导出DataSet到Excel失败", ex);
        }
    }

    /// <summary>
    /// 将Stream转换为DataTable
    /// </summary>
    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (stream == null || stream.Length == 0)
        {
            Logger?.LogWarning("Excel流为空");
            return null;
        }

        TWorkbook? workbook = null;

        try
        {
            // 打开Excel工作簿
            workbook = OpenWorkbook(stream);
            if (workbook == null)
            {
                Logger?.LogWarning("无法打开Excel工作簿");
                return null;
            }

            // 获取工作表
            var worksheet = GetWorksheet(workbook, sheetName);
            if (worksheet == null)
            {
                Logger?.LogWarning("工作表不存在: {SheetName}", sheetName ?? "默认");
                return null;
            }

            if (!HasData(worksheet))
            {
                Logger?.LogWarning("工作表为空: {SheetName}", sheetName ?? "默认");
                return null;
            }

            DataTable? dataTable;
            if (Options.EnablePerformanceMonitoring)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                dataTable = ImportFromWorksheet(worksheet, headerRowIndex, addEmptyRow);

                sw.Stop();
                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger?.LogInformation(
                        "从Excel流导入到DataTable[行数:{RowCount}, 列数:{ColumnCount}]耗时: {ElapsedMilliseconds}ms",
                        dataTable?.Rows.Count ?? 0,
                        dataTable?.Columns.Count ?? 0,
                        sw.ElapsedMilliseconds);
                }
            }
            else
            {
                dataTable = ImportFromWorksheet(worksheet, headerRowIndex, addEmptyRow);
            }

            return dataTable;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "从Excel流读取失败");
            return null;
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
    /// 将Stream转换为对象列表
    /// </summary>
    public override List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        // 首先转换为DataTable
        var dataTable = ConvertStreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
        if (dataTable == null)
        {
            return null;
        }

        // 然后将DataTable转换为对象列表
        try
        {
            var result = new List<T>(dataTable.Rows.Count);
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToArray();

            // 创建属性映射
            var propertyMapping = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in properties)
            {
                // 检查是否有ExcelColumn特性
                var excelAttr = prop.GetCustomAttribute<ExcelColumnAttribute>();
                if (excelAttr != null && excelAttr.ColumnName.IsNotNullAndWhiteSpace())
                {
                    propertyMapping[excelAttr.ColumnName] = prop;
                }

                // 同时添加属性名称映射
                propertyMapping[prop.Name] = prop;
            }

            // 创建列到属性的映射
            var columnToProperty = new Dictionary<int, PropertyInfo>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                string columnName = dataTable.Columns[i].ColumnName;
                if (propertyMapping.TryGetValue(columnName, out var property))
                {
                    columnToProperty[i] = property;
                }
            }

            // 转换每行数据
            foreach (DataRow row in dataTable.Rows)
            {
                T item = Activator.CreateInstance<T>();

                foreach (var kvp in columnToProperty)
                {
                    int columnIndex = kvp.Key;
                    PropertyInfo property = kvp.Value;

                    try
                    {
                        if (!row.IsNull(columnIndex))
                        {
                            object? value = row[columnIndex];
                            var convertedValue = ExcelValueConverter.TryConvertValue(value, property.PropertyType);
                            if (convertedValue != null)
                            {
                                property.SetValue(item, convertedValue);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogWarning(ex, "转换Excel单元格值到对象属性时出错: 列={Column}, 属性={Property}", dataTable.Columns[columnIndex].ColumnName, property.Name);
                    }
                }

                result.Add(item);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "将DataTable转换为对象列表时出错");
            return null;
        }
    }

    /// <summary>
    /// 列表转 Excel 内存流
    /// </summary>
    public override MemoryStream ConvertCollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null,
        Action<TWorksheet>? styleAction = null)
    {
        if (list == null)
        {
            Logger?.LogWarning("要导出的列表为空");
            list = [];
        }

        try
        {
            MemoryStream result;
            if (Options.EnablePerformanceMonitoring)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                result = ExportCollection(list, sheetsName, title, action, styleAction);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger?.LogInformation("导出列表到Excel[行数:{Count}]耗时: {ElapsedMilliseconds}ms",
                        list.Count, sw.ElapsedMilliseconds);
                }
            }
            else
            {
                result = ExportCollection(list, sheetsName, title, action, styleAction);
            }

            // ExportCollection方法不应该返回null，但为了健壮性仍进行检查
            if (result == null)
            {
                throw new InvalidOperationException("转换对象列表到MemoryStream失败");
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "导出列表到Excel失败");
            throw new InvalidOperationException("导出列表到Excel失败", ex);
        }
    }

    /// <summary>
    /// 数据表格转 Excel 内存流
    /// </summary>
    public override MemoryStream ConvertDataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = "Sheet1",
        string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null,
        Action<TWorksheet>? styleAction = null)
    {
        if (dataTable == null)
        {
            Logger?.LogWarning("要导出的DataTable为空");
            dataTable = new DataTable();
        }

        try
        {
            MemoryStream result;
            if (Options.EnablePerformanceMonitoring)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                result = ExportDataTable(dataTable, sheetsName, title, action, styleAction);
                sw.Stop();

                if (sw.ElapsedMilliseconds > Options.PerformanceThreshold)
                {
                    Logger?.LogInformation("导出DataTable到Excel[行数:{RowCount}, 列数:{ColumnCount}]耗时: {ElapsedMilliseconds}ms",
                        dataTable.Rows.Count, dataTable.Columns.Count, sw.ElapsedMilliseconds);
                }
            }
            else
            {
                result = ExportDataTable(dataTable, sheetsName, title, action, styleAction);
            }

            // ExportDataTable方法不应该返回null，但为了健壮性仍进行检查
            if (result == null)
            {
                throw new InvalidOperationException("转换DataTable到MemoryStream失败");
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "导出DataTable到Excel失败");
            throw new InvalidOperationException("导出DataTable到Excel失败", ex);
        }
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
        var worksheet = CreateWorksheet(workbook, sheetsName);

        // 获取所有列名
        string[] columnNames;
        var columns = GetExcelColumns(properties);

        if (columns.Any())
        {
            // 使用特性标记的列名
            columns = columns.OrderBy(c => c.Index);
            columnNames = columns.Select(c => c.ColumnName).ToArray();
        }
        else
        {
            // 使用属性名作为列名
            columnNames = properties.Select(p => p.Name).ToArray();
        }

        // 应用标题
        int startRowIndex = 0;
        if (title.IsNotNullAndEmpty())
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

        // 调用通用方法处理单个DataTable
        ExportDataTableToWorksheet(workbook, dataTable, sheetsName, title, action, styleAction);

        // 保存到流
        return SaveWorkbookToStream(workbook);
    }

    /// <summary>
    /// 导出DataSet到Excel
    /// </summary>
    private MemoryStream ExportDataSet(
        DataSet dataSet,
        string defaultSheetName,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action,
        Action<TWorksheet>? styleAction)
    {
        // 创建Excel工作簿
        var workbook = CreateWorkbook();

        // 遍历所有DataTable
        for (int i = 0; i < dataSet.Tables.Count; i++)
        {
            var dataTable = dataSet.Tables[i];
            var sheetName = !string.IsNullOrWhiteSpace(dataTable.TableName) ? dataTable.TableName : $"{defaultSheetName}{i + 1}";

            // 调用通用方法处理每个DataTable，不设置标题
            ExportDataTableToWorksheet(workbook, dataTable, sheetName, null, action, styleAction);
        }

        // 保存到流
        return SaveWorkbookToStream(workbook);
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
        Action<TWorksheet>? styleAction)
    {
        // 创建工作表
        var worksheet = CreateWorksheet(workbook, sheetName);

        if (dataTable.Columns.Count > 0)
        {
            // 获取所有列名
            var columnNames = dataTable.Columns.Cast<DataColumn>()
                .Select(col => col.ColumnName)
                .ToArray();
            int startRowIndex = 0;

            // 应用标题
            if (title.IsNotNullAndEmpty())
            {
                startRowIndex += ApplyTitle(worksheet, title, columnNames.Length);
            }

            // 创建表头行
            CreateHeaderRowCore(worksheet, columnNames, startRowIndex);

            // 填充数据行
            ProcessDataRows(worksheet, dataTable, startRowIndex);

            // 应用自定义处理
            action?.Invoke(worksheet, dataTable.Columns, dataTable.Rows);

            // 应用样式
            styleAction?.Invoke(worksheet);

            // 进行工作表格式化
            ApplyWorksheetFormatting(worksheet, dataTable.Rows.Count + startRowIndex + 1, columnNames.Length);
        }
    }

    /// <summary>
    /// 从工作表导入数据
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)，-1表示没有表头行</param>
    /// <param name="addEmptyRow">是否添加空行</param>
    /// <returns>转换后的DataTable</returns>
    /// <remarks>
    /// headerRowIndex的约定：
    /// 1. headerRowIndex传入为0-based索引（如同C#数组）
    /// 2. 如果headerRowIndex为-1，表示Excel没有表头行，直接从第一行开始读取数据
    /// 3. 各实现类需要根据自己的索引系统进行适当转换
    /// 4. 如果有表头行，数据行从表头的下一行开始
    /// </remarks>
    private DataTable ImportFromWorksheet(TWorksheet worksheet, int headerRowIndex, bool addEmptyRow)
    {
        DataTable dataTable = new DataTable(GetSheetName(worksheet));

        // 估计列数
        int columnCount = EstimateColumnCount(worksheet);
        if (columnCount <= 0)
        {
            Logger?.LogWarning("工作表为空或无法确定列数");
            return dataTable;
        }

        // 获取表头映射
        var headerMappings = CreateHeaderMappings(worksheet, headerRowIndex);

        // 如果headerRowIndex=-1且没有映射，自动生成列名(Column1, Column2...)
        if (headerMappings.Count == 0 && headerRowIndex < 0)
        {
            for (int i = 0; i < columnCount; i++)
            {
                headerMappings[i] = $"Column{i + 1}";
            }
        }

        // 添加列
        foreach (var mapping in headerMappings)
        {
            dataTable.Columns.Add(mapping.Value);
        }

        // 获取数据行范围 - 这里的startRow和endRow将使用各实现类的原生索引格式
        int startRow = GetDataStartRow(worksheet, headerRowIndex);
        int endRow = GetDataEndRow(worksheet);

        // 读取数据行
        for (int rowNum = startRow; rowNum <= endRow; rowNum++)
        {
            DataRow row = dataTable.NewRow();
            bool hasValue = false;

            foreach (var mapping in headerMappings)
            {
                int colIndex = mapping.Key;

                // GetCellValue期望接收原生索引格式 - rowNum已由GetDataStartRow转换，colIndex无需转换
                object cellValue = GetCellValue(worksheet, rowNum, colIndex);

                // 设置行值
                if (cellValue != DBNull.Value)
                {
                    row[mapping.Value] = cellValue;
                    hasValue = true;
                }
            }

            // 只添加有数据的行，除非指定了添加空行
            if (hasValue || addEmptyRow)
            {
                dataTable.Rows.Add(row);
            }
        }

        return dataTable;
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
    protected abstract TWorksheet? GetWorksheet(TWorkbook workbook, string? sheetName);

    /// <summary>
    /// 获取工作表名称
    /// </summary>
    protected abstract string GetSheetName(TWorksheet worksheet);

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
    /// 创建属性映射
    /// </summary>
    protected abstract Dictionary<int, PropertyInfo> CreatePropertyMappings<T>(TWorksheet worksheet, int headerRowIndex) where T : class, new();

    /// <summary>
    /// 获取数据开始行索引
    /// </summary>
    /// <param name="worksheet">工作表</param>
    /// <param name="headerRowIndex">表头行索引(0-based)</param>
    /// <returns>数据起始行索引(按库的原生索引格式)</returns>
    /// <remarks>
    /// 此方法应该处理headerRowIndex为负值的情况，通常返回第一个有效的数据行索引。
    /// 返回的索引需使用实现库的原生索引格式(NPOI为0-based，EPPlus/ClosedXML为1-based)。
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
    protected abstract Dictionary<int, string> CreateHeaderMappings(TWorksheet worksheet, int headerRowIndex);

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
    /// 应用工作表格式化
    /// </summary>
    protected abstract void ApplyWorksheetFormatting(TWorksheet worksheet, int rowCount, int columnCount);

    /// <summary>
    /// 保存工作簿到内存流
    /// </summary>
    protected abstract MemoryStream SaveWorkbookToStream(TWorkbook workbook);

    #endregion
}
