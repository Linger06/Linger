using Linger.Extensions.Core;
using NPOI.SS.UserModel;

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

        if (fontName.IsNotNullAndWhiteSpace())
            font.FontName = fontName;

        if (fontColor.IsNotNullAndWhiteSpace())
        {
            try
            {
                // 尝试解析HTML颜色
                var colorStr = fontColor.TrimStart('#');
                if (colorStr.Length == 6)
                {
                    var r = Convert.ToInt32(colorStr.Substring(0, 2), 16);
                    var g = Convert.ToInt32(colorStr.Substring(2, 2), 16);
                    var b = Convert.ToInt32(colorStr.Substring(4, 2), 16);

                    // 获取最接近的颜色索引
                    font.Color = GetClosestColorIndex(r, g, b);
                }
            }
            catch
            {
                // 忽略颜色解析错误
            }
        }

        style.SetFont(font);

        if (backgroundColor.IsNotNullAndWhiteSpace())
        {
            try
            {
                // 尝试解析HTML颜色
                var colorStr = backgroundColor.TrimStart('#');
                if (colorStr.Length == 6)
                {
                    // 为NPOI设置背景色
                    style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index; // 默认使用灰色
                    style.FillPattern = FillPattern.SolidForeground;
                }
            }
            catch
            {
                // 忽略颜色解析错误
            }
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
