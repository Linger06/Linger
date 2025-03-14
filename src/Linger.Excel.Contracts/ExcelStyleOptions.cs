namespace Linger.Excel.Contracts;

/// <summary>
/// Excel 样式配置选项
/// </summary>
public class ExcelStyleOptions
{
    // 标题样式
    public string TitleFontName { get; set; } = "Arial";
    public int TitleFontSize { get; set; } = 14;
    public bool TitleBold { get; set; } = true;
    public string TitleBackgroundColor { get; set; } = "#D0D0D0";
    public string TitleFontColor { get; set; } = "#000000";
    
    // 表头样式
    public string HeaderFontName { get; set; } = "Arial";
    public int HeaderFontSize { get; set; } = 11;
    public bool HeaderBold { get; set; } = true;
    public string HeaderBackgroundColor { get; set; } = "#E0E0E0";
    public string HeaderFontColor { get; set; } = "#000000";
    
    // 数据样式
    public string DataFontName { get; set; } = "Arial";
    public int DataFontSize { get; set; } = 10;
    public bool ShowGridlines { get; set; } = true;
    
    // 数字格式
    public string IntegerFormat { get; set; } = "#,##0";
    public string DecimalFormat { get; set; } = "#,##0.00";
    
    // 日期格式
    public string DefaultDateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
}
