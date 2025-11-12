namespace Linger.Excel.Contracts;

/// <summary>
/// Excel 样式配置选项
/// </summary>
/// <example>
/// 自定义样式配置示例：
/// <code>
/// var styleOptions = new ExcelStyleOptions
/// {
///     TitleStyle = new TitleStyle
///     {
///         FontName = "Microsoft YaHei",
///         FontSize = 16,
///         Bold = true,
///         BackgroundColor = "#2E75B6",
///         FontColor = "#FFFFFF"
///     },
///     HeaderStyle = new HeaderStyle
///     {
///         FontName = "Microsoft YaHei",
///         FontSize = 11,
///         Bold = true,
///         BackgroundColor = "#4472C4",
///         FontColor = "#FFFFFF"
///     },
///     DataStyle = new DataStyle
///     {
///         FontName = "Calibri",
///         FontSize = 10,
///         DateFormat = "yyyy-MM-dd",
///         DecimalFormat = "#,##0.00",
///         IntegerFormat = "#,##0"
///     },
///     ShowGridlines = true
/// };
/// </code>
/// </example>
public class ExcelStyleOptions
{
    /// <summary>
    /// 标题样式配置
    /// </summary>
    public TitleStyle TitleStyle { get; set; } = new();

    /// <summary>
    /// 表头样式配置
    /// </summary>
    public HeaderStyle HeaderStyle { get; set; } = new();

    /// <summary>
    /// 数据行样式配置
    /// </summary>
    public DataStyle DataStyle { get; set; } = new();

    /// <summary>
    /// 是否显示网格线
    /// </summary>
    public bool ShowGridlines { get; set; } = true;
}

/// <summary>
/// 标题样式配置
/// </summary>
/// <example>
/// <code>
/// var titleStyle = new TitleStyle
/// {
///     FontName = "Microsoft YaHei",
///     FontSize = 18,
///     Bold = true,
///     BackgroundColor = "#2E75B6",  // 深蓝色
///     FontColor = "#FFFFFF"          // 白色
/// };
/// </code>
/// </example>
public class TitleStyle
{
    /// <summary>
    /// 字体名称
    /// </summary>
    public string FontName { get; set; } = "Arial";

    /// <summary>
    /// 字体大小
    /// </summary>
    public int FontSize { get; set; } = 14;

    /// <summary>
    /// 是否粗体
    /// </summary>
    public bool Bold { get; set; } = true;

    /// <summary>
    /// 背景色 (Hex格式: #RRGGBB)
    /// </summary>
    public string BackgroundColor { get; set; } = "#D0D0D0";

    /// <summary>
    /// 字体颜色 (Hex格式: #RRGGBB)
    /// </summary>
    public string FontColor { get; set; } = "#000000";
}

/// <summary>
/// 表头样式配置
/// </summary>
/// <example>
/// <code>
/// var headerStyle = new HeaderStyle
/// {
///     FontName = "Arial",
///     FontSize = 11,
///     Bold = true,
///     BackgroundColor = "#4472C4",  // 中蓝色
///     FontColor = "#FFFFFF"          // 白色
/// };
/// </code>
/// </example>
public class HeaderStyle
{
    /// <summary>
    /// 字体名称
    /// </summary>
    public string FontName { get; set; } = "Arial";

    /// <summary>
    /// 字体大小
    /// </summary>
    public int FontSize { get; set; } = 11;

    /// <summary>
    /// 是否粗体
    /// </summary>
    public bool Bold { get; set; } = true;

    /// <summary>
    /// 背景色 (Hex格式: #RRGGBB)
    /// </summary>
    public string BackgroundColor { get; set; } = "#E0E0E0";

    /// <summary>
    /// 字体颜色 (Hex格式: #RRGGBB)
    /// </summary>
    public string FontColor { get; set; } = "#000000";
}

/// <summary>
/// 数据行样式配置
/// </summary>
/// <example>
/// <code>
/// var dataStyle = new DataStyle
/// {
///     FontName = "Calibri",
///     FontSize = 10,
///     IntegerFormat = "#,##0",              // 整数格式：1,234
///     DecimalFormat = "#,##0.00",           // 小数格式：1,234.56
///     DateFormat = "yyyy-MM-dd HH:mm:ss"    // 日期格式：2025-01-01 12:30:45
/// };
/// </code>
/// </example>
public class DataStyle
{
    /// <summary>
    /// 字体名称
    /// </summary>
    public string FontName { get; set; } = "Arial";

    /// <summary>
    /// 字体大小
    /// </summary>
    public int FontSize { get; set; } = 10;

    /// <summary>
    /// 数字格式 - 整数
    /// </summary>
    public string IntegerFormat { get; set; } = "#,##0";

    /// <summary>
    /// 数字格式 - 小数
    /// </summary>
    public string DecimalFormat { get; set; } = "#,##0.00";

    /// <summary>
    /// 日期格式
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
}
