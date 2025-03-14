using OfficeOpenXml;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;

namespace Linger.Excel.EPPlus;

/// <summary>
/// EPPlus Excel 样式帮助类
/// </summary>
public static class ExcelStyleHelper
{
    /// <summary>
    /// 设置单元格样式
    /// </summary>
    public static void SetCellStyle(ExcelRange cell, 
        Color? backgroundColor = null,
        Color? fontColor = null, 
        bool? bold = null,
        float? fontSize = null,
        string? fontName = null,
        ExcelHorizontalAlignment? horizontalAlignment = null,
        ExcelVerticalAlignment? verticalAlignment = null,
        bool applyBorder = false)
    {
        if (backgroundColor.HasValue)
        {
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(backgroundColor.Value);
        }

        if (fontColor.HasValue)
            cell.Style.Font.Color.SetColor(fontColor.Value);

        if (bold.HasValue)
            cell.Style.Font.Bold = bold.Value;

        if (fontSize.HasValue)
            cell.Style.Font.Size = fontSize.Value;

        if (!string.IsNullOrEmpty(fontName))
            cell.Style.Font.Name = fontName;

        if (horizontalAlignment.HasValue)
            cell.Style.HorizontalAlignment = horizontalAlignment.Value;

        if (verticalAlignment.HasValue)
            cell.Style.VerticalAlignment = verticalAlignment.Value;

        if (applyBorder)
        {
            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
    }
}
