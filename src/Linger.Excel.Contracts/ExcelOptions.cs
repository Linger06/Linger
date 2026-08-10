namespace Linger.Excel.Contracts;

/// <summary>
/// Excel配置选项
/// </summary>
/// <example>
/// 基本配置示例：
/// <code>
/// var options = new ExcelOptions
/// {
///     AutoFitColumns = true,
///     EnablePerformanceMonitoring = true,
///     PerformanceThreshold = 1000
/// };
/// </code>
///
/// 带样式配置的完整示例：
/// <code>
/// var options = new ExcelOptions
/// {
///     AutoFitColumns = true,
///     StyleOptions = new ExcelStyleOptions
///     {
///         HeaderStyle = new HeaderStyle
///         {
///             BackgroundColor = "#4472C4",
///             FontColor = "#FFFFFF",
///             Bold = true,
///             FontSize = 12
///         },
///         TitleStyle = new TitleStyle
///         {
///             BackgroundColor = "#2E75B6",
///             FontColor = "#FFFFFF",
///             Bold = true,
///             FontSize = 16
///         },
///         DataStyle = new DataStyle
///         {
///             DateFormat = "yyyy/MM/dd",
///             DecimalFormat = "#,##0.00",
///             FontSize = 10
///         }
///     }
/// };
/// </code>
/// </example>
public class ExcelOptions
{
    /// <summary>
    /// 默认工作表名称
    /// </summary>
    public const string DefaultSheetName = "Sheet1";

    /// <summary>
    /// DataSet导出时的默认工作表名称前缀
    /// </summary>
    public const string DefaultDataSetSheetPrefix = "Sheet";

    /// <summary>
    /// 是否自动调整列宽
    /// </summary>
    public bool AutoFitColumns { get; set; } = true;

    // 移除此处的DefaultDateFormat, 改为使用StyleOptions中的配置

    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; }

    /// <summary>
    /// 性能监控阈值(毫秒)，超过此阈值才记录日志
    /// </summary>
    public int PerformanceThreshold { get; set; } = 500;

    /// <summary>
    /// 样式配置选项
    /// </summary>
    public ExcelStyleOptions StyleOptions { get; set; } = new();
}
