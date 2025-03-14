using ClosedXML.Excel;

namespace Linger.Excel.ClosedXML;

/// <summary>
/// ClosedXML Excel 样式帮助类
/// </summary>
public static class ExcelStyleHelper
{
    /// <summary>
    /// 设置单元格样式
    /// </summary>
    public static void SetCellStyle(IXLCell cell,
        string? backgroundColor = null,  // 添加可空修饰符
        string? fontColor = null,        // 添加可空修饰符
        bool? bold = null,
        double? fontSize = null,
        string? fontName = null,         // 添加可空修饰符
        XLAlignmentHorizontalValues? horizontalAlignment = null,
        XLAlignmentVerticalValues? verticalAlignment = null,
        bool applyBorder = false)
    {
        if (!string.IsNullOrEmpty(backgroundColor))
        {
            try
            {
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(backgroundColor);
            }
            catch
            {
                // 如果颜色无效，忽略错误
            }
        }

        if (!string.IsNullOrEmpty(fontColor))
        {
            try
            {
                cell.Style.Font.FontColor = XLColor.FromHtml(fontColor);
            }
            catch
            {
                // 如果颜色无效，忽略错误
            }
        }

        if (bold.HasValue)
            cell.Style.Font.Bold = bold.Value;

        if (fontSize.HasValue)
            cell.Style.Font.FontSize = fontSize.Value;

        if (!string.IsNullOrEmpty(fontName))
            cell.Style.Font.FontName = fontName;

        if (horizontalAlignment.HasValue)
            cell.Style.Alignment.Horizontal = horizontalAlignment.Value;

        if (verticalAlignment.HasValue)
            cell.Style.Alignment.Vertical = verticalAlignment.Value;

        if (applyBorder)
        {
            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        }
    }

    /// <summary>
    /// 设置范围样式
    /// </summary>
    public static void SetRangeStyle(IXLRange range,
        string? backgroundColor = null,  // 添加可空修饰符
        string? fontColor = null,        // 添加可空修饰符
        bool? bold = null,
        double? fontSize = null,
        string? fontName = null,         // 添加可空修饰符
        XLAlignmentHorizontalValues? horizontalAlignment = null,
        XLAlignmentVerticalValues? verticalAlignment = null,
        bool applyBorder = false)
    {
        if (!string.IsNullOrEmpty(backgroundColor))
        {
            try
            {
                range.Style.Fill.BackgroundColor = XLColor.FromHtml(backgroundColor);
            }
            catch
            {
                // 如果颜色无效，忽略错误
            }
        }

        if (!string.IsNullOrEmpty(fontColor))
        {
            try
            {
                range.Style.Font.FontColor = XLColor.FromHtml(fontColor);
            }
            catch
            {
                // 如果颜色无效，忽略错误
            }
        }

        if (bold.HasValue)
            range.Style.Font.Bold = bold.Value;

        if (fontSize.HasValue)
            range.Style.Font.FontSize = fontSize.Value;

        if (!string.IsNullOrEmpty(fontName))
            range.Style.Font.FontName = fontName;

        if (horizontalAlignment.HasValue)
            range.Style.Alignment.Horizontal = horizontalAlignment.Value;

        if (verticalAlignment.HasValue)
            range.Style.Alignment.Vertical = verticalAlignment.Value;

        if (applyBorder)
        {
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            // range.Style.Border.InsideBorders = new XLBorders
            // {
            //     TopBorder = XLBorderStyleValues.Thin,
            //     BottomBorder = XLBorderStyleValues.Thin,
            //     LeftBorder = XLBorderStyleValues.Thin,
            //     RightBorder = XLBorderStyleValues.Thin
            // };
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
        }
    }
}
