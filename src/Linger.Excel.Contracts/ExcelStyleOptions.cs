namespace Linger.Excel.Contracts;

/// <summary>
/// Excel 样式配置选项
/// </summary>
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
