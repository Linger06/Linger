using Linger.Extensions.Core;
using NPOI.OOXML.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

/// <summary>
/// NPOI Excel 样式帮助类
/// </summary>
public static class ExcelStyleHelper
{
    /// <summary>
    /// 设置单元格样式
    /// </summary>
    public static void SetCellStyle(ICell cell,
        string? backgroundColor = null,  // 添加可空修饰符
        string? fontColor = null,        // 添加可空修饰符
        bool? bold = null,
        short? fontSize = null,
        string? fontName = null,         // 添加可空修饰符
        HorizontalAlignment? horizontalAlignment = null,
        VerticalAlignment? verticalAlignment = null,
        bool applyBorder = false)
    {
        IWorkbook workbook = cell.Sheet.Workbook;
        ICellStyle style = workbook.CreateCellStyle();

        // 复制原始样式
        if (cell.CellStyle != null)
        {
            style.CloneStyleFrom(cell.CellStyle);
        }

        // 创建字体
        IFont font = workbook.CreateFont();

        if (bold.HasValue)
            font.IsBold = bold.Value;

        if (fontSize.HasValue)
            font.FontHeightInPoints = fontSize.Value;

        if (fontName.IsNotNullOrWhiteSpace())
            font.FontName = fontName;

        if (fontColor.IsNotNullOrWhiteSpace())
        {
            SetFontColor(font, fontColor);
        }

        style.SetFont(font);

        if (backgroundColor.IsNotNullOrWhiteSpace())
        {
            style.FillPattern = FillPattern.SolidForeground;
            SetFillForegroundColor(style, backgroundColor);
        }

        if (horizontalAlignment.HasValue)
            style.Alignment = horizontalAlignment.Value;

        if (verticalAlignment.HasValue)
            style.VerticalAlignment = verticalAlignment.Value;

        if (applyBorder)
        {
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
        }

        cell.CellStyle = style;
    }

    internal static void SetFontColor(IFont font, string color)
    {
        if (font is XSSFFont xssfFont)
        {
            xssfFont.SetColor(CreateXssfColor(color));
            return;
        }

        font.Color = GetColorIndex(color);
    }

    internal static void SetFillForegroundColor(ICellStyle style, string color)
    {
        if (style is XSSFCellStyle xssfStyle)
        {
            xssfStyle.FillForegroundXSSFColor = CreateXssfColor(color);
            return;
        }

        style.FillForegroundColor = GetColorIndex(color);
    }

    private static XSSFColor CreateXssfColor(string color)
    {
        var hexColor = color.TrimStart('#');
        if (hexColor.Length != 6 || hexColor.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(null, nameof(color));
        }

        return new XSSFColor(
        [
            Convert.ToByte(hexColor.Substring(0, 2), 16),
            Convert.ToByte(hexColor.Substring(2, 2), 16),
            Convert.ToByte(hexColor.Substring(4, 2), 16)
        ],
        new DefaultIndexedColorMap());
    }

    private static short GetColorIndex(string color)
    {
        var hexColor = color.TrimStart('#');
        if (hexColor.Length != 6 || hexColor.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException(null, nameof(color));
        }

        return GetClosestColorIndex(
            Convert.ToInt32(hexColor.Substring(0, 2), 16),
            Convert.ToInt32(hexColor.Substring(2, 2), 16),
            Convert.ToInt32(hexColor.Substring(4, 2), 16));
    }

    /// <summary>
    /// 获取最接近的颜色索引
    /// </summary>
    public static short GetClosestColorIndex(int r, int g, int b)
    {
        // 简化版本，仅返回一些常见的索引颜色
        if (r > 200 && g > 200 && b > 200) // 白色或浅色
            return NPOI.HSSF.Util.HSSFColor.White.Index;
        if (r < 50 && g < 50 && b < 50) // 黑色或深色
            return NPOI.HSSF.Util.HSSFColor.Black.Index;
        if (r > 200 && g < 100 && b < 100) // 红色
            return NPOI.HSSF.Util.HSSFColor.Red.Index;
        if (r < 100 && g > 200 && b < 100) // 绿色
            return NPOI.HSSF.Util.HSSFColor.Green.Index;
        if (r < 100 && g < 100 && b > 200) // 蓝色
            return NPOI.HSSF.Util.HSSFColor.Blue.Index;
        if (r > 200 && g > 200 && b < 100) // 黄色
            return NPOI.HSSF.Util.HSSFColor.Yellow.Index;
        if (r < 100 && g > 200 && b > 200) // 青色
            return NPOI.HSSF.Util.HSSFColor.Aqua.Index;
        if (r > 200 && g < 100 && b > 200) // 紫色
            return NPOI.HSSF.Util.HSSFColor.Violet.Index;
        // 默认灰色
        return NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
    }
}
