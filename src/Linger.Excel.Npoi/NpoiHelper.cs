using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

public static class NpoiHelper
{
    #region Covert xls To xlsx
    public static string ConvertToXlsx(string xlsPath, string newExcelPath)
    {
        var oldWorkbook = new HSSFWorkbook(new FileStream(xlsPath, FileMode.Open));
        ISheet? oldWorkSheet = oldWorkbook.GetSheetAt(0);
        using (var fileStream = new FileStream(newExcelPath, FileMode.Create))
        {
            var newWorkBook = new XSSFWorkbook();
            //var newWorkSheet = newWorkBook.CreateSheet("Sheet1");
            //int i = 0;
            //foreach (HSSFRow oldRow in oldWorkSheet)
            //{
            //    var newRow = newWorkSheet.CreateRow(oldRow.RowNum);
            //    for (int ii = oldRow.FirstCellNum; ii < oldRow.Cells.Count; ii++)
            //    {
            //        m = ii;
            //        var newCell = newRow.CreateCell(ii);
            //        newCell.SetCellValue(GetValueType(oldRow.Cells[ii]).ToString());
            //    }
            //}

            //int sheetMergerCount = oldWorkSheet.NumMergedRegions;
            //for (int me = 0; me < sheetMergerCount; me++)
            //    newWorkSheet.AddMergedRegion(oldWorkSheet.GetMergedRegion(me));

            ISheet sheet = oldWorkSheet.CrossCloneSheet(newWorkBook, "Sheet1", false);
            newWorkBook.Add(sheet);

            newWorkBook.Write(fileStream);
            newWorkBook.Close();
        }

        oldWorkbook.Close();

        return newExcelPath;
    }
    /// <summary>
    /// 跨工作薄Workbook复制工作表Sheet
    /// </summary>
    /// <param name="sSheet">源工作表Sheet</param>
    /// <param name="dWb">目标工作薄Workbook</param>
    /// <param name="dSheetName">目标工作表Sheet名</param>
    /// <param name="clonePrintSetup">是否复制打印设置</param>
    public static ISheet CrossCloneSheet(this ISheet sSheet, IWorkbook dWb, string dSheetName, bool clonePrintSetup)
    {
        dSheetName = string.IsNullOrEmpty(dSheetName) ? sSheet.SheetName : dSheetName;
        dSheetName = dWb.GetSheet(dSheetName) == null ? dSheetName : dSheetName + "_Copy";
        ISheet dSheet = dWb.GetSheet(dSheetName) ?? dWb.CreateSheet(dSheetName);
        CopySheet(sSheet, dSheet);
        if (clonePrintSetup)
            ClonePrintSetup(sSheet, dSheet);
        dWb.SetActiveSheet(dWb.GetSheetIndex(dSheet));  //当前Sheet作为下次打开默认Sheet
        return dSheet;
    }
    /// <summary>
    /// 跨工作薄Workbook复制工作表Sheet
    /// </summary>
    /// <param name="sSheet">源工作表Sheet</param>
    /// <param name="dWb">目标工作薄Workbook</param>
    /// <param name="dSheetName">目标工作表Sheet名</param>
    public static ISheet CrossCloneSheet(this ISheet sSheet, IWorkbook dWb, string dSheetName)
    {
        var clonePrintSetup = true;
        return sSheet.CrossCloneSheet(dWb, dSheetName, clonePrintSetup);
    }
    /// <summary>
    /// 跨工作薄Workbook复制工作表Sheet
    /// </summary>
    /// <param name="sSheet">源工作表Sheet</param>
    /// <param name="dWb">目标工作薄Workbook</param>
    public static ISheet CrossCloneSheet(this ISheet sSheet, IWorkbook dWb)
    {
        var dSheetName = sSheet.SheetName;
        const bool ClonePrintSetup = true;
        return sSheet.CrossCloneSheet(dWb, dSheetName, ClonePrintSetup);
    }
    private static IFont? FindFont(this IWorkbook dWb, IFont font, List<IFont> dFonts)
    {
        IFont dFont = dWb.FindFont(font.IsBold, font.Color, (short)font.FontHeight, font.FontName, font.IsItalic, font.IsStrikeout, font.TypeOffset, font.Underline);
        //IFont? dFont = null;
        foreach (IFont currFont in dFonts)
        {
            //if (currFont.Charset != font.Charset) continue;
            //else
            //if (currFont.Color != font.Color) continue;
            //else
            if (currFont.FontName != font.FontName) continue;
            if (Math.Abs(currFont.FontHeight - font.FontHeight) > 0) continue;
            if (currFont.IsBold != font.IsBold) continue;
            if (currFont.IsItalic != font.IsItalic) continue;
            if (currFont.IsStrikeout != font.IsStrikeout) continue;
            if (currFont.Underline != font.Underline) continue;
            if (currFont.TypeOffset != font.TypeOffset) continue;
            dFont = currFont; break;
        }
        return dFont;
    }
    private static ICellStyle? FindStyle(this IWorkbook dWb, IWorkbook sWb, ICellStyle style, List<ICellStyle> dCellStyles, List<IFont> dFonts)
    {
        ICellStyle? dStyle = null;
        foreach (ICellStyle currStyle in dCellStyles)
        {
            if (currStyle.Alignment != style.Alignment) continue;
            if (currStyle.VerticalAlignment != style.VerticalAlignment) continue;
            if (currStyle.BorderTop != style.BorderTop) continue;
            if (currStyle.BorderBottom != style.BorderBottom) continue;
            if (currStyle.BorderLeft != style.BorderLeft) continue;
            if (currStyle.BorderRight != style.BorderRight) continue;
            if (currStyle.TopBorderColor != style.TopBorderColor) continue;
            if (currStyle.BottomBorderColor != style.BottomBorderColor) continue;
            if (currStyle.LeftBorderColor != style.LeftBorderColor) continue;
            if (currStyle.RightBorderColor != style.RightBorderColor) continue;
            //else if (currStyle.BorderDiagonal != style.BorderDiagonal) continue;
            //else if (currStyle.BorderDiagonalColor != style.BorderDiagonalColor) continue;
            //else if (currStyle.BorderDiagonalLineStyle != style.BorderDiagonalLineStyle) continue;
            //else if (currStyle.FillBackgroundColor != style.FillBackgroundColor) continue;
            //else if (currStyle.FillBackgroundColorColor != style.FillBackgroundColorColor) continue;
            //else if (currStyle.FillForegroundColor != style.FillForegroundColor) continue;
            //else if (currStyle.FillForegroundColorColor != style.FillForegroundColorColor) continue;
            //else if (currStyle.FillPattern != style.FillPattern) continue;
            if (currStyle.Indention != style.Indention) continue;
            if (currStyle.IsHidden != style.IsHidden) continue;
            if (currStyle.IsLocked != style.IsLocked) continue;
            if (currStyle.Rotation != style.Rotation) continue;
            if (currStyle.ShrinkToFit != style.ShrinkToFit) continue;
            if (currStyle.WrapText != style.WrapText) continue;
            if (!currStyle.GetDataFormatString().Equals(style.GetDataFormatString())) continue;
            IFont sFont = sWb.GetFontAt(style.FontIndex);
            IFont? dFont = dWb.FindFont(sFont, dFonts);
            if (dFont == null) continue;
            currStyle.SetFont(dFont);
            dStyle = currStyle;
            break;
        }
        return dStyle;
    }
    private static IFont CopyFont(this IFont dFont, IFont sFont, List<IFont> dFonts)
    {
        //dFont.Charset = sFont.Charset;
        //dFont.Color = sFont.Color;
        dFont.FontHeight = sFont.FontHeight;
        dFont.FontName = sFont.FontName;
        dFont.IsBold = sFont.IsBold;
        dFont.IsItalic = sFont.IsItalic;
        dFont.IsStrikeout = sFont.IsStrikeout;
        dFont.Underline = sFont.Underline;
        dFont.TypeOffset = sFont.TypeOffset;
        dFonts.Add(dFont);
        return dFont;
    }
    private static ICellStyle CopyStyle(this ICellStyle dCellStyle, ICellStyle sCellStyle, IWorkbook dWb, IWorkbook sWb, List<ICellStyle> dCellStyles, List<IFont> dFonts)
    {
        ICellStyle currCellStyle = dCellStyle;
        currCellStyle.Alignment = sCellStyle.Alignment;
        currCellStyle.VerticalAlignment = sCellStyle.VerticalAlignment;
        currCellStyle.BorderTop = sCellStyle.BorderTop;
        currCellStyle.BorderBottom = sCellStyle.BorderBottom;
        currCellStyle.BorderLeft = sCellStyle.BorderLeft;
        currCellStyle.BorderRight = sCellStyle.BorderRight;
        currCellStyle.TopBorderColor = sCellStyle.TopBorderColor;
        currCellStyle.LeftBorderColor = sCellStyle.LeftBorderColor;
        currCellStyle.RightBorderColor = sCellStyle.RightBorderColor;
        currCellStyle.BottomBorderColor = sCellStyle.BottomBorderColor;
        //dCellStyle.BorderDiagonal = sCellStyle.BorderDiagonal;
        //dCellStyle.BorderDiagonalColor = sCellStyle.BorderDiagonalColor;
        //dCellStyle.BorderDiagonalLineStyle = sCellStyle.BorderDiagonalLineStyle;
        //dCellStyle.FillBackgroundColor = sCellStyle.FillBackgroundColor;
        dCellStyle.FillForegroundColor = sCellStyle.FillForegroundColor;
        //dCellStyle.FillPattern = sCellStyle.FillPattern;
        currCellStyle.Indention = sCellStyle.Indention;
        currCellStyle.IsHidden = sCellStyle.IsHidden;
        currCellStyle.IsLocked = sCellStyle.IsLocked;
        currCellStyle.Rotation = sCellStyle.Rotation;
        currCellStyle.ShrinkToFit = sCellStyle.ShrinkToFit;
        currCellStyle.WrapText = sCellStyle.WrapText;
        currCellStyle.DataFormat = dWb.CreateDataFormat().GetFormat(sWb.CreateDataFormat().GetFormat(sCellStyle.DataFormat));
        IFont sFont = sCellStyle.GetFont(sWb);
        IFont dFont = dWb.FindFont(sFont, dFonts) ?? dWb.CreateFont().CopyFont(sFont, dFonts);
        currCellStyle.SetFont(dFont);
        dCellStyles.Add(currCellStyle);
        return currCellStyle;
    }
    private static void CopySheet(ISheet sSheet, ISheet dSheet)
    {
        var maxColumnNum = 0;
        var dCellStyles = new List<ICellStyle>();
        var dFonts = new List<IFont>();
        MergerRegion(sSheet, dSheet);
        for (var i = sSheet.FirstRowNum; i <= sSheet.LastRowNum; i++)
        {
            IRow sRow = sSheet.GetRow(i);
            IRow dRow = dSheet.CreateRow(i);
            if (sRow != null)
            {
                CopyRow(sRow, dRow, dCellStyles, dFonts);
                if (sRow.LastCellNum > maxColumnNum)
                    maxColumnNum = sRow.LastCellNum;
            }
        }
        for (var i = 0; i <= maxColumnNum; i++)
            dSheet.SetColumnWidth(i, sSheet.GetColumnWidth(i));
    }
    private static void CopyRow(IRow sRow, IRow dRow, List<ICellStyle> dCellStyles, List<IFont> dFonts)
    {
        dRow.Height = sRow.Height;
        for (int j = sRow.FirstCellNum; j <= sRow.LastCellNum; j++)
        {
            ICell sCell = sRow.GetCell(j);
            ICell dCell = dRow.GetCell(j);
            if (sCell != null)
            {
                dCell ??= dRow.CreateCell(j);
                CopyCell(sCell, dCell, dCellStyles, dFonts);
            }
        }
    }
    private static void CopyCell(ICell sCell, ICell dCell, List<ICellStyle> dCellStyles, List<IFont> dFonts)
    {
        ICellStyle? currCellStyle = dCell.Sheet.Workbook.FindStyle(sCell.Sheet.Workbook, sCell.CellStyle, dCellStyles, dFonts);
        currCellStyle ??= dCell.Sheet.Workbook.CreateCellStyle().CopyStyle(sCell.CellStyle, dCell.Sheet.Workbook, sCell.Sheet.Workbook, dCellStyles, dFonts);
        dCell.CellStyle = currCellStyle;
        switch (sCell.CellType)
        {
            case CellType.String:
                dCell.SetCellValue(sCell.StringCellValue);
                break;
            case CellType.Numeric:
                dCell.SetCellValue(sCell.NumericCellValue);
                break;
            case CellType.Blank:
                dCell.SetCellType(CellType.Blank);
                break;
            case CellType.Boolean:
                dCell.SetCellValue(sCell.BooleanCellValue);
                break;
            case CellType.Error:
                dCell.SetCellValue(sCell.ErrorCellValue);
                break;
            case CellType.Formula:
                dCell.SetCellFormula(sCell.CellFormula);
                break;
        }
    }
    private static void MergerRegion(ISheet sSheet, ISheet dSheet)
    {
        var sheetMergerCount = sSheet.NumMergedRegions;
        for (var i = 0; i < sheetMergerCount; i++)
            dSheet.AddMergedRegion(sSheet.GetMergedRegion(i));
    }
    private static void ClonePrintSetup(ISheet sSheet, ISheet dSheet)
    {
        //工作表Sheet页面打印设置
        dSheet.PrintSetup.Copies = 1;                               //打印份数
        dSheet.PrintSetup.PaperSize = sSheet.PrintSetup.PaperSize;  //纸张大小
        dSheet.PrintSetup.Landscape = sSheet.PrintSetup.Landscape;  //纸张方向：默认纵向false(横向true)
        dSheet.PrintSetup.Scale = sSheet.PrintSetup.Scale;          //缩放方式比例
        dSheet.PrintSetup.FitHeight = sSheet.PrintSetup.FitHeight;  //调整方式页高
        dSheet.PrintSetup.FitWidth = sSheet.PrintSetup.FitWidth;    //调整方式页宽
        dSheet.PrintSetup.FooterMargin = sSheet.PrintSetup.FooterMargin;
        dSheet.PrintSetup.HeaderMargin = sSheet.PrintSetup.HeaderMargin;
        //页边距
        dSheet.SetMargin(MarginType.TopMargin, sSheet.GetMargin(MarginType.TopMargin));
        dSheet.SetMargin(MarginType.BottomMargin, sSheet.GetMargin(MarginType.BottomMargin));
        dSheet.SetMargin(MarginType.LeftMargin, sSheet.GetMargin(MarginType.LeftMargin));
        dSheet.SetMargin(MarginType.RightMargin, sSheet.GetMargin(MarginType.RightMargin));
        dSheet.SetMargin(MarginType.HeaderMargin, sSheet.GetMargin(MarginType.HeaderMargin));
        dSheet.SetMargin(MarginType.FooterMargin, sSheet.GetMargin(MarginType.FooterMargin));
        //页眉页脚
        dSheet.Header.Left = sSheet.Header.Left;
        dSheet.Header.Center = sSheet.Header.Center;
        dSheet.Header.Right = sSheet.Header.Right;
        dSheet.Footer.Left = sSheet.Footer.Left;
        dSheet.Footer.Center = sSheet.Footer.Center;
        dSheet.Footer.Right = sSheet.Footer.Right;
        //工作表Sheet参数设置
        dSheet.IsPrintGridlines = sSheet.IsPrintGridlines;          //true: 打印整表网格线。不单独设置CellStyle时外框实线内框虚线。 false: 自己设置网格线
        dSheet.FitToPage = sSheet.FitToPage;                        //自适应页面
        dSheet.HorizontallyCenter = sSheet.HorizontallyCenter;      //打印页面为水平居中
        dSheet.VerticallyCenter = sSheet.VerticallyCenter;          //打印页面为垂直居中
        dSheet.RepeatingRows = sSheet.RepeatingRows;                //工作表顶端标题行范围
    }
    #endregion
}
