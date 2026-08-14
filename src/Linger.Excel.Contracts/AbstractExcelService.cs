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
/// <param name="logger">日志记录器，为 <c>null</c> 时使用 <see cref="NullLogger"/>。</param>
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
    protected readonly ILogger Logger = logger ?? NullLogger.Instance;

    /// <summary>
    /// 规范化后的 Excel 单元格值类型。
    /// </summary>
    protected enum ExcelCellValueKind
    {
        Empty,
        Text,
        Boolean,
        Integer,
        Decimal,
        DateTime
    }

    /// <summary>
    /// 规范化后的 Excel 单元格值。
    /// </summary>
    protected readonly struct ExcelCellValue(ExcelCellValueKind kind, object? value)
    {
        public ExcelCellValueKind Kind { get; } = kind;

        public object? Value { get; } = value;
    }

    #region IExcelService简单实现 - 调用IExcel实现

    /// <inheritdoc />
    public string DataTableToExcel(DataTable dataTable, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "")
    {
        return DataTableToExcelCore(dataTable, fullFileName, sheetsName, title, action: null, styleAction: null);
    }

    /// <inheritdoc />
    public string DataSetToExcel(DataSet dataSet, string fullFileName, string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix)
    {
        return DataSetToExcelCore(dataSet, fullFileName, defaultSheetName, action: null, styleAction: null);
    }

    /// <inheritdoc />
    public string CollectionToExcel<T>(List<T> list, string fullFileName, string sheetsName = ExcelOptions.DefaultSheetName, string title = "") where T : class
    {
        return CollectionToExcelCore(list, fullFileName, sheetsName, title, action: null, styleAction: null);
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
    /// 使用行映射委托将 Excel 文件转换为对象列表。
    /// </summary>
    public abstract List<T>? ExcelToList<T>(string filePath, Func<ExcelRow, T> map, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataTable（推荐 - 同步版本）
    /// </summary>
    public abstract DataTable? StreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将Stream转换为对象列表（推荐 - 同步版本）
    /// </summary>
    public abstract List<T>? StreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default) where T : class, new();

    /// <summary>
    /// 使用行映射委托将 Excel 流转换为对象列表。
    /// </summary>
    public abstract List<T>? StreamToList<T>(Stream stream, Func<ExcelRow, T> map, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)
    /// </summary>
    public abstract DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 将Excel文件转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行
    /// </summary>
    public abstract DataSet? ExcelToDataSet(string filePath, Func<string, int?> headerRowIndexSelector, IEnumerable<string>? sheetNames = null, bool addEmptyRow = false);

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)（推荐 - 同步版本）
    /// </summary>
    public abstract DataSet? StreamToDataSet(Stream stream, IEnumerable<string>? sheetNames, int headerRowIndex = 0, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将Stream转换为DataSet(指定工作表)，支持为每个工作表指定不同的表头行（推荐 - 同步版本）
    /// </summary>
    public abstract DataSet? StreamToDataSet(Stream stream, Func<string, int?> headerRowIndexSelector, IEnumerable<string>? sheetNames = null, bool addEmptyRow = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 数据表格转 Excel 文件（推荐）
    /// </summary>
    protected abstract string DataTableToExcelCore(DataTable dataTable, string fullFileName, string sheetsName, string title,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 数据集转 Excel 文件（推荐）
    /// </summary>
    protected abstract string DataSetToExcelCore(DataSet dataSet, string fullFileName, string defaultSheetName,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// Exports a data set to Excel and invokes an action for every created worksheet.
    /// </summary>
    /// <remarks>
    /// The action is invoked in source table order, including tables without columns. An exception
    /// thrown by the action stops the export and is propagated to the caller.
    /// </remarks>
    public abstract string DataSetToExcel(DataSet dataSet, string fullFileName, Action<IWorksheetExportContext<TWorksheet>> worksheetAction,
        string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix);

    /// <summary>
    /// 使用 Provider 特定的工作表回调将数据表直接导出到 Excel 文件。
    /// </summary>
    public abstract string DataTableToExcel(
        DataTable dataTable,
        string fullFileName,
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        Action<TWorksheet>? styleAction = null);

    /// <summary>
    /// 使用 Provider 特定的工作表回调将对象集合直接导出到 Excel 文件。
    /// </summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method relies on reflection-based property discovery. For AOT/trimming scenarios, use the explicit-column export overloads.")]
#endif
    public abstract string CollectionToExcel<T>(
        List<T> list,
        string fullFileName,
        Action<TWorksheet, PropertyInfo[]>? action,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        Action<TWorksheet>? styleAction = null)
        where T : class;

    /// <summary>
    /// 对象集合转 Excel 文件（推荐）
    /// </summary>
    protected abstract string CollectionToExcelCore<T>(List<T> list, string fullFileName, string sheetsName, string title,
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 使用显式列定义将对象集合导出为 Excel 文件。
    /// </summary>
    public abstract string CollectionToExcel<T>(IEnumerable<T> items, IEnumerable<ExcelExportColumn<T>> columns, string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName, string title = "");

    /// <summary>
    /// 对象集合转 Excel 内存流（推荐）
    /// </summary>
    public abstract MemoryStream CollectionToMemoryStream<T>(List<T> list, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, PropertyInfo[]>? action = null, Action<TWorksheet>? styleAction = null) where T : class;

    /// <summary>
    /// 使用显式列定义将对象集合导出为 Excel 内存流。
    /// </summary>
    public abstract MemoryStream CollectionToMemoryStream<T>(IEnumerable<T> items, IEnumerable<ExcelExportColumn<T>> columns,
        string sheetsName = ExcelOptions.DefaultSheetName, string title = "");

    /// <summary>
    /// 数据表格转 Excel 内存流（推荐）
    /// </summary>
    public abstract MemoryStream DataTableToMemoryStream(DataTable dataTable, string sheetsName = ExcelOptions.DefaultSheetName, string title = "",
        Action<TWorksheet, DataColumnCollection, DataRowCollection>? action = null, Action<TWorksheet>? styleAction = null);

    #endregion

    #region 共享方法实现

    /// <summary>
    /// 创建Excel模板
    /// </summary>
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection-based collection export for template generation. For AOT/trimming scenarios, use ExcelExtensions.CreateExcelTemplate with explicit ExcelExportColumn definitions.")]
#endif
    public virtual MemoryStream CreateExcelTemplate<T>() where T : class, new()
    {
        var type = typeof(T);

        // 创建一个空的列表，只包含列名
        var list = new List<T>();

        // 使用现有方法创建模板
        var result = CollectionToMemoryStream(list, type.Name, $"{type.Name} 模板");

        return result ?? throw new InvalidOperationException($"创建 {type.Name} Excel模板失败");
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

    /// <summary>
    /// 将用户数据规范化为各 Excel Provider 可一致写入的值。
    /// </summary>
    protected static ExcelCellValue NormalizeExcelCellValue(object? value, Type declaredType)
    {
        ArgumentNullException.ThrowIfNull(declaredType);

        if (value is null or DBNull)
        {
            return new ExcelCellValue(ExcelCellValueKind.Empty, null);
        }

        var actualType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;
        if (actualType == typeof(object))
        {
            actualType = value.GetType();
        }
        else if (!actualType.IsInstanceOfType(value))
        {
            if (!TypeConverter.TryConvert(value, actualType, out var convertedValue))
            {
                throw new InvalidOperationException(
                    $"Excel 单元格值无法从 {value.GetType().FullName} 转换为 {actualType.FullName}。");
            }

            value = convertedValue!;
        }

        if (actualType.IsEnum)
        {
            return new ExcelCellValue(ExcelCellValueKind.Text, ConvertToInvariantText(value));
        }

        return Type.GetTypeCode(actualType) switch
        {
            TypeCode.Boolean => new ExcelCellValue(ExcelCellValueKind.Boolean, (bool)value),
            TypeCode.Byte or
            TypeCode.SByte or
            TypeCode.Int16 or
            TypeCode.UInt16 or
            TypeCode.Int32 or
            TypeCode.UInt32 or
            TypeCode.Int64 or
            TypeCode.UInt64 => new ExcelCellValue(
                ExcelCellValueKind.Integer,
                Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture)),
            TypeCode.Decimal or TypeCode.Double or TypeCode.Single => new ExcelCellValue(
                ExcelCellValueKind.Decimal,
                Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture)),
            TypeCode.DateTime => new ExcelCellValue(ExcelCellValueKind.DateTime, (DateTime)value),
            _ => new ExcelCellValue(ExcelCellValueKind.Text, ConvertToInvariantText(value))
        };
    }

    /// <summary>
    /// 判断 Excel 数字格式是否表示日期或时间。
    /// </summary>
    protected static bool IsDateCellFormat(string? numberFormat)
    {
        if (numberFormat is null || string.IsNullOrWhiteSpace(numberFormat))
        {
            return false;
        }

        for (var index = 0; index < numberFormat.Length; index++)
        {
            var character = numberFormat[index];
            if (character == '"')
            {
                index++;
                while (index < numberFormat.Length && numberFormat[index] != '"')
                {
                    index++;
                }

                continue;
            }

            if (character is '\\' or '_' or '*')
            {
                index++;
                continue;
            }

            if (character == '[')
            {
                var closingBracket = numberFormat.IndexOf(']', index + 1);
                if (closingBracket < 0)
                {
                    return false;
                }

                var bracketContent = numberFormat.Substring(index + 1, closingBracket - index - 1);
                if (bracketContent.Length > 0 && bracketContent.All(value =>
                        char.ToLowerInvariant(value) is 'h' or 'm' or 's'))
                {
                    return true;
                }

                index = closingBracket;
                continue;
            }

            if (char.ToLowerInvariant(character) is 'y' or 'm' or 'd' or 'h' or 's')
            {
                return true;
            }
        }

        return false;
    }

    private static string ConvertToInvariantText(object value)
    {
        return value is IFormattable formattable
            ? formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            : value.ToString() ?? string.Empty;
    }

    #region 私有辅助方法

    /// <summary>
    /// Copies a stream to a seekable memory stream while observing cancellation between reads.
    /// </summary>
    protected static MemoryStream CopyToMemoryStream(Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var memoryStream = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            memoryStream.Write(buffer, 0, bytesRead);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    #endregion

    #region 兼容旧方法 - 提供与 NPOIHelper.ImportExcelToDs 相同的签名

    #endregion
}
