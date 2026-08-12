namespace Linger.Excel.Contracts;

/// <summary>
/// 定义与具体 Provider 无关的 Excel 导入和导出契约。
/// </summary>
/// <remarks>
/// 文件导入在路径为空或文件不存在时返回 <see langword="null"/>；文件存在后的访问、格式和解析异常会向调用方传播。
/// 流导入采用协作式取消。取消令牌会在读取和转换检查点被观察，但不能强制中断 Provider 内部的同步解析阶段。
/// </remarks>
public interface IExcelService
{
    /// <summary>
    /// 将文件中的一个工作表导入数据表。
    /// </summary>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="sheetName">工作表名称；为 <see langword="null"/> 时使用第一个工作表。</param>
    /// <param name="headerRowIndex">表头行的从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <returns>导入的数据表；路径为空或文件不存在时返回 <see langword="null"/>。</returns>
    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 使用基于反射的对象映射导入文件中的一个工作表。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="sheetName">工作表名称；为 <see langword="null"/> 时使用第一个工作表。</param>
    /// <param name="headerRowIndex">表头行的从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <returns>映射后的对象列表；路径为空或文件不存在时返回 <see langword="null"/>。</returns>
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
        where T : class, new();

    /// <summary>
    /// 使用显式行映射委托导入文件中的一个工作表。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="map">将当前 Excel 行转换为目标对象的委托。</param>
    /// <param name="sheetName">工作表名称；为 <see langword="null"/> 时使用第一个工作表。</param>
    /// <param name="headerRowIndex">表头行的从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <returns>映射后的对象列表；路径为空或文件不存在时返回 <see langword="null"/>。</returns>
    List<T>? ExcelToList<T>(string filePath, Func<ExcelRow, T> map, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 使用相同的表头行索引从文件导入指定工作表。
    /// </summary>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="sheetNames">要导入的工作表名称；为 <see langword="null"/> 或空集合时导入全部工作表。</param>
    /// <param name="headerRowIndex">所有工作表共用的表头行从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <returns>每个工作表对应一个数据表的数据集；路径为空或文件不存在时返回 <see langword="null"/>。</returns>
    DataSet? ExcelToDataSet(string filePath, IEnumerable<string>? sheetNames = null, int headerRowIndex = 0, bool addEmptyRow = false);

    /// <summary>
    /// 使用按工作表确定的表头行索引从文件导入指定工作表。
    /// </summary>
    /// <param name="filePath">Excel 文件路径。</param>
    /// <param name="headerRowIndexSelector">根据工作表名称返回表头行索引的委托；返回 <see langword="null"/> 时使用索引 0。</param>
    /// <param name="sheetNames">要导入的工作表名称；为 <see langword="null"/> 或空集合时导入全部工作表。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <returns>每个工作表对应一个数据表的数据集；路径为空或文件不存在时返回 <see langword="null"/>。</returns>
    DataSet? ExcelToDataSet(
        string filePath,
        Func<string, int?> headerRowIndexSelector,
        IEnumerable<string>? sheetNames = null,
        bool addEmptyRow = false);

    /// <summary>
    /// 将流中的一个工作表导入数据表。
    /// </summary>
    /// <param name="stream">包含 Excel 内容的可读流。</param>
    /// <param name="sheetName">工作表名称；为 <see langword="null"/> 时使用第一个工作表。</param>
    /// <param name="headerRowIndex">表头行的从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <param name="cancellationToken">用于取消读取和转换的令牌。</param>
    /// <returns>导入的数据表；无法选择工作表时返回 <see langword="null"/>。</returns>
    DataTable? StreamToDataTable(
        Stream stream,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用基于反射的对象映射导入流中的一个工作表。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="stream">包含 Excel 内容的可读流。</param>
    /// <param name="sheetName">工作表名称；为 <see langword="null"/> 时使用第一个工作表。</param>
    /// <param name="headerRowIndex">表头行的从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <param name="cancellationToken">用于取消读取和转换的令牌。</param>
    /// <returns>映射后的对象列表；无法选择工作表时返回 <see langword="null"/>。</returns>
    List<T>? StreamToList<T>(
        Stream stream,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default)
        where T : class, new();

    /// <summary>
    /// 使用显式行映射委托导入流中的一个工作表。
    /// </summary>
    /// <typeparam name="T">目标对象类型。</typeparam>
    /// <param name="stream">包含 Excel 内容的可读流。</param>
    /// <param name="map">将当前 Excel 行转换为目标对象的委托。</param>
    /// <param name="sheetName">工作表名称；为 <see langword="null"/> 时使用第一个工作表。</param>
    /// <param name="headerRowIndex">表头行的从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <param name="cancellationToken">用于取消读取和转换的令牌。</param>
    /// <returns>映射后的对象列表；无法选择工作表时返回 <see langword="null"/>。</returns>
    List<T>? StreamToList<T>(
        Stream stream,
        Func<ExcelRow, T> map,
        string? sheetName = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用相同的表头行索引从流中导入指定工作表。
    /// </summary>
    /// <param name="stream">包含 Excel 内容的可读流。</param>
    /// <param name="sheetNames">要导入的工作表名称；为 <see langword="null"/> 或空集合时导入全部工作表。</param>
    /// <param name="headerRowIndex">所有工作表共用的表头行从零开始索引。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <param name="cancellationToken">用于取消读取和转换的令牌。</param>
    /// <returns>每个工作表对应一个数据表的数据集。</returns>
    DataSet? StreamToDataSet(
        Stream stream,
        IEnumerable<string>? sheetNames = null,
        int headerRowIndex = 0,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用按工作表确定的表头行索引从流中导入指定工作表。
    /// </summary>
    /// <param name="stream">包含 Excel 内容的可读流。</param>
    /// <param name="headerRowIndexSelector">根据工作表名称返回表头行索引的委托；返回 <see langword="null"/> 时使用索引 0。</param>
    /// <param name="sheetNames">要导入的工作表名称；为 <see langword="null"/> 或空集合时导入全部工作表。</param>
    /// <param name="addEmptyRow">是否在结果中保留空行。</param>
    /// <param name="cancellationToken">用于取消读取和转换的令牌。</param>
    /// <returns>每个工作表对应一个数据表的数据集。</returns>
    DataSet? StreamToDataSet(
        Stream stream,
        Func<string, int?> headerRowIndexSelector,
        IEnumerable<string>? sheetNames = null,
        bool addEmptyRow = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 将数据表导出到 Excel 文件。
    /// </summary>
    /// <param name="dataTable">要导出的数据表。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <returns>生成的文件路径。</returns>
    string DataTableToExcel(
        DataTable dataTable,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "");

    /// <summary>
    /// 将数据集导出到 Excel 文件，每个数据表对应一个工作表。
    /// </summary>
    /// <param name="dataSet">要导出的数据集。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="defaultSheetName">未命名数据表使用的工作表名称前缀。</param>
    /// <returns>生成的文件路径。</returns>
    string DataSetToExcel(
        DataSet dataSet,
        string fullFileName,
        string defaultSheetName = ExcelOptions.DefaultDataSetSheetPrefix);

    /// <summary>
    /// 使用基于反射的列发现将对象集合导出到 Excel 文件。
    /// </summary>
    /// <typeparam name="T">要导出的对象类型。</typeparam>
    /// <param name="list">要导出的对象列表。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <returns>生成的文件路径。</returns>
    string CollectionToExcel<T>(
        List<T> list,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "")
        where T : class;

    /// <summary>
    /// 使用显式列定义将对象集合导出到 Excel 文件。
    /// </summary>
    /// <typeparam name="T">要导出的对象类型。</typeparam>
    /// <param name="items">要导出的对象序列。</param>
    /// <param name="columns">导出列定义。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <returns>生成的文件路径。</returns>
    string CollectionToExcel<T>(
        IEnumerable<T> items,
        IEnumerable<ExcelExportColumn<T>> columns,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "");

    /// <summary>
    /// 使用基于反射的列发现将对象集合导出到内存流。
    /// </summary>
    /// <typeparam name="T">要导出的对象类型。</typeparam>
    /// <param name="list">要导出的对象列表。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <returns>定位在起始位置且包含 Excel 内容的内存流。</returns>
    MemoryStream CollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "")
        where T : class;

    /// <summary>
    /// 使用显式列定义将对象集合导出到内存流。
    /// </summary>
    /// <typeparam name="T">要导出的对象类型。</typeparam>
    /// <param name="items">要导出的对象序列。</param>
    /// <param name="columns">导出列定义。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <returns>定位在起始位置且包含 Excel 内容的内存流。</returns>
    MemoryStream CollectionToMemoryStream<T>(
        IEnumerable<T> items,
        IEnumerable<ExcelExportColumn<T>> columns,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "");

    /// <summary>
    /// 将数据表导出到内存流。
    /// </summary>
    /// <param name="dataTable">要导出的数据表。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <returns>定位在起始位置且包含 Excel 内容的内存流。</returns>
    MemoryStream DataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "");

    /// <summary>
    /// 异步将数据表导出到 Excel 文件。
    /// </summary>
    /// <param name="dataTable">要导出的数据表。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <param name="cancellationToken">用于取消文件写入的令牌。</param>
    /// <returns>其结果为生成文件路径的任务。</returns>
    Task<string> DataTableToExcelAsync(
        DataTable dataTable,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用基于反射的列发现异步将对象集合导出到 Excel 文件。
    /// </summary>
    /// <typeparam name="T">要导出的对象类型。</typeparam>
    /// <param name="list">要导出的对象列表。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <param name="cancellationToken">用于取消文件写入的令牌。</param>
    /// <returns>其结果为生成文件路径的任务。</returns>
    Task<string> CollectionToExcelAsync<T>(
        List<T> list,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// 使用显式列定义异步将对象集合导出到 Excel 文件。
    /// </summary>
    /// <typeparam name="T">要导出的对象类型。</typeparam>
    /// <param name="items">要导出的对象序列。</param>
    /// <param name="columns">导出列定义。</param>
    /// <param name="fullFileName">输出文件的完整路径。</param>
    /// <param name="sheetsName">工作表名称。</param>
    /// <param name="title">工作表标题。</param>
    /// <param name="cancellationToken">用于取消文件写入的令牌。</param>
    /// <returns>其结果为生成文件路径的任务。</returns>
    Task<string> CollectionToExcelAsync<T>(
        IEnumerable<T> items,
        IEnumerable<ExcelExportColumn<T>> columns,
        string fullFileName,
        string sheetsName = ExcelOptions.DefaultSheetName,
        string title = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用基于反射的列发现创建空 Excel 模板。
    /// </summary>
    /// <typeparam name="T">用于定义模板列的对象类型。</typeparam>
    /// <returns>定位在起始位置且包含 Excel 模板的内存流。</returns>
    MemoryStream CreateExcelTemplate<T>() where T : class, new();
}
