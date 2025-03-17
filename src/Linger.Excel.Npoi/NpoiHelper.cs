using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;
using Linger.Extensions.Core;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;

public static class NpoiHelper
{
    [Obsolete("导出Excel不再使用NPOI,使用EPPlus", true)]
    public static void ExportDTtoExcel(DataTable dtSource, string strHeaderText, string strFileName)
    {
        var temp = strFileName.Split('.');

        if (temp[temp.Length - 1] == "xls" && dtSource.Columns.Count < 256 && dtSource.Rows.Count < 65536)
        {
            using MemoryStream ms = ExportDt(dtSource, strHeaderText);
            using var fs = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
            var data = ms.ToArray();
            fs.Write(data, 0, data.Length);
            fs.Flush();
        }
        else
        {
            if (temp[temp.Length - 1] == "xls")
            {
                strFileName += "x";
            }

            using var fs = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
            ExportDti(dtSource, strHeaderText, fs);
        }
    }

    [Obsolete("导出Excel不再使用NPOI,使用EPPlus", true)]
    public static string ExportDStoExcel(DataSet dataSet, string sheetName, string strFileName)
    {
        var temp = strFileName.Split('.');

        var sheetNames = sheetName.Split(',');

        var xlsxFlag = false;
        for (var i = 0; i < sheetNames.Length; i++)
        {
            if (dataSet.Tables[i].Columns.Count >= 256 || dataSet.Tables[i].Rows.Count >= 65536)
            {
                xlsxFlag = true;
            }
        }

        if (temp[temp.Length - 1] == "xls" && !xlsxFlag)
        {
            using MemoryStream ms = ExportDsToMs(dataSet, sheetName);
            using var fs = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
            var data = ms.ToArray();
            fs.Write(data, 0, data.Length);
            fs.Flush();
        }
        else
        {
            if (temp[temp.Length - 1] == "xls")
            {
                strFileName += "x";
            }

            using var fs = new FileStream(strFileName, FileMode.Create, FileAccess.Write);
            ExportDsToMsI(dataSet, sheetName, fs);
        }

        return strFileName;
    }

    public static void InsertSheet(string outputFile, string sheetname, DataTable dt)
    {
        var readfile = new FileStream(outputFile, FileMode.Open, FileAccess.Read);
        IWorkbook? hssfworkbook = WorkbookFactory.Create(readfile);
        //HSSFWorkbook hssfworkbook = new HSSFWorkbook(readfile);
        var num = hssfworkbook.GetSheetIndex(sheetname);
        ISheet sheet1;
        if (num >= 0)
        {
            sheet1 = hssfworkbook.GetSheet(sheetname);
        }
        else
        {
            sheet1 = hssfworkbook.CreateSheet(sheetname);
        }

        if (sheet1.GetRow(0) == null)
        {
            sheet1.CreateRow(0);
        }

        for (var coluid = 0; coluid < dt.Columns.Count; coluid++)
        {
            if (sheet1.GetRow(0).GetCell(coluid) == null)
            {
                sheet1.GetRow(0).CreateCell(coluid);
            }

            sheet1.GetRow(0).GetCell(coluid).SetCellValue(dt.Columns[coluid].ColumnName);
        }

        for (var i = 1; i <= dt.Rows.Count; i++)
        {
            if (sheet1.GetRow(i) == null)
            {
                sheet1.CreateRow(i);
            }

            for (var coluid = 0; coluid < dt.Columns.Count; coluid++)
            {
                if (sheet1.GetRow(i).GetCell(coluid) == null)
                {
                    sheet1.GetRow(i).CreateCell(coluid);
                }

                sheet1.GetRow(i).GetCell(coluid).SetCellValue(dt.Rows[i - 1][coluid].ToString());
            }
        }

        readfile.Close();

        var writefile = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write);
        hssfworkbook.Write(writefile);
        writefile.Close();
    }

    /// <summary>
    ///     获取Excel中Sheet的总数量
    /// </summary>
    /// <param name="outputFile"></param>
    /// <returns></returns>
    public static int GetSheetNumber(string outputFile)
    {
        var readfile = new FileStream(outputFile, FileMode.Open, FileAccess.Read);

        var hssfworkbook = new HSSFWorkbook(readfile);
        var number = hssfworkbook.NumberOfSheets;
        return number;
    }

    /// <summary>
    ///     获取Excel中所有的Sheet名称
    /// </summary>
    /// <param name="outputFile"></param>
    /// <returns></returns>
    public static ArrayList GetSheetName(string outputFile)
    {
        var arrayList = new ArrayList();
        var readfile = new FileStream(outputFile, FileMode.Open, FileAccess.Read);

        var hssfworkbook = new HSSFWorkbook(readfile);
        for (var i = 0; i < hssfworkbook.NumberOfSheets; i++)
        {
            arrayList.Add(hssfworkbook.GetSheetName(i));
        }

        return arrayList;
    }

    [Obsolete("导出Excel不再使用NPOI,使用EPPlus", true)]
    public static MemoryStream Export(DataTable dtSource, string strHeaderText)
    {
        var workbook = new HSSFWorkbook();
        workbook = GetWorkbook(workbook, dtSource, null, strHeaderText);
        var ms = new MemoryStream();

        workbook.Write(ms);
        ms.Flush();
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    ///     由DataSet导出MemoryStream
    /// </summary>
    /// <param name="sourceDs">要导出数据的DataSet</param>
    /// <param name="sheetName">工作表名称</param>
    /// <returns>Excel工作表</returns>
    private static MemoryStream ExportDataSetToMs(DataSet sourceDs, string sheetName)
    {
        var workbook = new HSSFWorkbook();
        var ms = new MemoryStream();
        var sheetNames = sheetName.Split(',');

        for (var i = 0; i < sheetNames.Length; i++)
        {
            ISheet? sheet = workbook.CreateSheet(sheetNames[i]);

            #region 列头

            IRow? headerRow = sheet.CreateRow(0);
            ICellStyle? headStyle = workbook.CreateCellStyle();
            IFont? font = workbook.CreateFont();
            font.FontHeightInPoints = 10;
            font.IsBold = true;
            headStyle.SetFont(font);

            //取得列宽
            var arrColWidth = new int[sourceDs.Tables[i].Columns.Count];
            foreach (DataColumn item in sourceDs.Tables[i].Columns)
            {
                arrColWidth[item.Ordinal] = Encoding.GetEncoding(936).GetBytes(item.ColumnName).Length;
            }

            // 处理列头
            foreach (DataColumn column in sourceDs.Tables[i].Columns)
            {
                headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                headerRow.GetCell(column.Ordinal).CellStyle = headStyle;
                //设置列宽
                sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
            }

            #endregion 列头

            #region 填充值

            var rowIndex = 1;
            foreach (DataRow row in sourceDs.Tables[i].Rows)
            {
                IRow? dataRow = sheet.CreateRow(rowIndex);
                foreach (DataColumn column in sourceDs.Tables[i].Columns)
                {
                    dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                }

                rowIndex++;
            }

            #endregion 填充值
        }

        workbook.Write(ms);
        ms.Flush();
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    ///     由DataSet导出MemoryStream
    /// </summary>
    /// <param name="ds">要导出数据的DataSet</param>
    /// <param name="sheetName">工作表名称</param>
    /// <returns>Excel工作表</returns>
    private static MemoryStream ExportDsToMs(DataSet ds, string sheetName)
    {
        var workbook = new HSSFWorkbook();
        var ms = new MemoryStream();
        var sheetNames = sheetName.Split(',');

        for (var i = 0; i < sheetNames.Length; i++)
        {
            workbook = GetWorkbook(workbook, ds.Tables[i], sheetNames[i]);
        }

        workbook.Write(ms);
        ms.Flush();
        ms.Position = 0;
        return ms;
    }

    private static void ExportDsToMsI(DataSet ds, string sheetName, FileStream fs)
    {
        var workbook = new XSSFWorkbook();

        var sheetNames = sheetName.Split(',');

        for (var i = 0; i < sheetNames.Length; i++)
        {
            workbook = GetWorkbookI(workbook, ds.Tables[i], sheetNames[i], null);
        }

        workbook.Write(fs);
        fs.Close();
    }

    /// <summary>
    ///     验证导入的Excel是否有数据
    /// </summary>
    /// <param name="excelFileStream"></param>
    /// <returns></returns>
    public static bool HasData(Stream excelFileStream)
    {
        using (excelFileStream)
        {
            var workBook = new HSSFWorkbook(excelFileStream);
            if (workBook.NumberOfSheets <= 0)
            {
                return false;
            }

            ISheet? sheet = workBook.GetSheetAt(0);
            return sheet.PhysicalNumberOfRows > 0;
        }
    }

    #region 从datatable中将数据导出到excel

    /// <summary>
    ///     DataTable导出到Excel的MemoryStream
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    /// <param name="strHeaderText">表头文本</param>
    private static MemoryStream ExportDt(DataTable dtSource, string strHeaderText)
    {
        var workbook = new HSSFWorkbook();
        workbook = GetWorkbook(workbook, dtSource, null, strHeaderText);
        var ms = new MemoryStream();
        workbook.Write(ms);
        ms.Flush();
        ms.Position = 0;

        //sheet.Dispose();
        //workbook.Dispose();

        return ms;
    }

    /// <summary>
    /// </summary>
    /// <param name="workbook"></param>
    /// <param name="dt"></param>
    /// <param name="sheetName"></param>
    /// <param name="headerText">表头内容</param>
    /// <param name="columnsName">使用自定义列名</param>
    /// <returns></returns>
    public static HSSFWorkbook GetWorkbook(HSSFWorkbook workbook, DataTable dt, string? sheetName = null,
        string? headerText = null, string? columnsName = null)
    {
        // HSSFWorkbook workbook = new HSSFWorkbook();
        ISheet? sheet = workbook.CreateSheet();
        if (!string.IsNullOrEmpty(sheetName))
        {
            sheet = workbook.CreateSheet(sheetName);
        }

        #region 右击文件 属性信息

        //{
        //    DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
        //    dsi.Company = "http://www.yongfa365.com/";
        //    workbook.DocumentSummaryInformation = dsi;

        //    SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
        //    si.Author = "柳永法"; //填加xls文件作者信息
        //    si.ApplicationName = "NPOI测试程序"; //填加xls文件创建程序信息
        //    si.LastAuthor = "柳永法2"; //填加xls文件最后保存者信息
        //    si.Comments = "说明信息"; //填加xls文件作者信息
        //    si.Title = "NPOI测试"; //填加xls文件标题信息
        //    si.Subject = "NPOI测试Demo"; //填加文件主题信息
        //    si.CreateDateTime = DateTime.Now;
        //    workbook.SummaryInformation = si;
        //}

        #endregion 右击文件 属性信息

        ICellStyle? dateStyle = workbook.CreateCellStyle();
        IDataFormat? format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd HH:mm:ss");

        //取得列宽
        var arrColWidth = new int[dt.Columns.Count];
        foreach (DataColumn item in dt.Columns)
        {
            arrColWidth[item.Ordinal] = Encoding.GetEncoding(936).GetBytes(item.ColumnName).Length;
        }

        for (var i = 0; i < dt.Rows.Count; i++)
        {
            for (var j = 0; j < dt.Columns.Count; j++)
            {
                var cellValue = dt.Rows[i][j].ToString()!;
                var intTemp = Encoding.GetEncoding(936).GetBytes(cellValue).Length;
                if (intTemp > arrColWidth[j])
                {
                    arrColWidth[j] = intTemp;
                }
            }
        }

        var rowIndex = 0;

        foreach (DataRow row in dt.Rows)
        {
            #region 新建表，填充表头，填充列头，样式

            if (rowIndex is 65535 or 0)
            {
                if (rowIndex != 0)
                {
                    sheet = workbook.CreateSheet();
                }

                #region 表头及样式

                if (!string.IsNullOrEmpty(headerText))
                {
                    IRow? headerRow = sheet.CreateRow(0);
                    headerRow.HeightInPoints = 25;
                    headerRow.CreateCell(0).SetCellValue(headerText);

                    ICellStyle? headStyle = workbook.CreateCellStyle();
                    headStyle.Alignment = HorizontalAlignment.Center;
                    IFont? font = workbook.CreateFont();
                    font.FontHeightInPoints = 20;
                    font.IsBold = true;
                    headStyle.SetFont(font);

                    headerRow.GetCell(0).CellStyle = headStyle;

                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count - 1));
                    //headerRow.Dispose();
                }

                #endregion 表头及样式

                #region 列头及样式

                {
                    IRow? headerRow = sheet.CreateRow(rowIndex);

                    ICellStyle? headStyle = workbook.CreateCellStyle();
                    headStyle.Alignment = HorizontalAlignment.Center;
                    IFont? font = workbook.CreateFont();
                    font.FontHeightInPoints = 10;
                    font.IsBold = true;
                    headStyle.SetFont(font);

                    if (columnsName != null)
                    {
                        var columns = columnsName.Split(',');
                        for (var i = 0; i < columns.Length; i++)
                        {
                            headerRow.CreateCell(i).SetCellValue(columns[i]);
                            headerRow.GetCell(i).CellStyle = headStyle;

                            //设置列宽
                            //sheet.SetColumnWidth(i, (arrColWidth[i] + 1) * 256);

                            if (arrColWidth[i] >= 255)
                            {
                                arrColWidth[i] = 255;
                            }
                            else
                            {
                                sheet.SetColumnWidth(i, (arrColWidth[i] + 1) * 256);
                            }
                        }
                    }
                    else
                    {
                        foreach (DataColumn column in dt.Columns)
                        {
                            headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                            headerRow.GetCell(column.Ordinal).CellStyle = headStyle;

                            //设置列宽
                            //sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);

                            if (arrColWidth[column.Ordinal] >= 255)
                            {
                                arrColWidth[column.Ordinal] = 255;
                            }
                            else
                            {
                                sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
                            }
                        }
                    }

                    //headerRow.Dispose();
                }

                #endregion 列头及样式

                rowIndex++;
            }

            #endregion 新建表，填充表头，填充列头，样式

            #region 填充内容

            IRow? dataRow = sheet.CreateRow(rowIndex);
            foreach (DataColumn column in dt.Columns)
            {
                ICell? newCell = dataRow.CreateCell(column.Ordinal);

                var drValue = row[column].ToString()!;

                switch (column.DataType.ToString())
                {
                    case "System.String": //字符串类型
                        if (drValue.IsDouble())
                        {
                            newCell.SetCellValue(drValue.ToDouble());
                            break;
                        }

                        newCell.SetCellValue(drValue);
                        break;

                    case "System.DateTime": //日期类型
                        var dateV = drValue.ToDateTime();
                        newCell.SetCellValue(dateV);

                        newCell.CellStyle = dateStyle; //格式化显示
                        break;

                    case "System.Boolean": //布尔型
                        var boolV = drValue.ToBool();
                        newCell.SetCellValue(boolV);
                        break;

                    case "System.Int16": //整型
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Byte":
                        var intV = drValue.ToInt();
                        newCell.SetCellValue(intV);
                        break;

                    case "System.Decimal": //浮点型
                    case "System.Double":
                        var doubV = drValue.ToDouble();
                        newCell.SetCellValue(doubV);
                        break;

                    case "System.DBNull": //空值处理
                        newCell.SetCellValue(string.Empty);
                        break;

                    default:
                        newCell.SetCellValue(string.Empty);
                        break;
                }
            }

            #endregion 填充内容

            rowIndex++;
        }

        return workbook;
    }

    /// <summary>
    ///     DataTable导出到Excel的MemoryStream
    /// </summary>
    /// <param name="dtSource">源DataTable</param>
    /// <param name="strHeaderText">表头文本</param>
    /// <param name="fs"></param>
    private static void ExportDti(DataTable dtSource, string strHeaderText, FileStream fs)
    {
        var workbook = new XSSFWorkbook();
        workbook = GetWorkbookI(workbook, dtSource, "Sheet1", strHeaderText);
        workbook.Write(fs);
        fs.Close();
    }

    private static XSSFWorkbook GetWorkbookI(XSSFWorkbook workbook, DataTable dt, string sheetName,
        string? strHeaderText)
    {
        ISheet? sheet = workbook.CreateSheet(sheetName);

        #region 右击文件 属性信息

        //{
        //    DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
        //    dsi.Company = "http://www.yongfa365.com/";
        //    workbook.DocumentSummaryInformation = dsi;

        //    SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
        //    si.Author = "柳永法"; //填加xls文件作者信息
        //    si.ApplicationName = "NPOI测试程序"; //填加xls文件创建程序信息
        //    si.LastAuthor = "柳永法2"; //填加xls文件最后保存者信息
        //    si.Comments = "说明信息"; //填加xls文件作者信息
        //    si.Title = "NPOI测试"; //填加xls文件标题信息
        //    si.Subject = "NPOI测试Demo"; //填加文件主题信息
        //    si.CreateDateTime = DateTime.Now;
        //    workbook.SummaryInformation = si;
        //}

        #endregion 右击文件 属性信息

        ICellStyle? dateStyle = workbook.CreateCellStyle();
        IDataFormat? format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

        //取得列宽
        var arrColWidth = new int[dt.Columns.Count];
        foreach (DataColumn item in dt.Columns)
        {
            arrColWidth[item.Ordinal] = Encoding.GetEncoding(936).GetBytes(item.ColumnName).Length;
        }

        for (var i = 0; i < dt.Rows.Count; i++)
        {
            for (var j = 0; j < dt.Columns.Count; j++)
            {
                var intTemp = Encoding.GetEncoding(936).GetBytes(dt.Rows[i][j].ToString()!).Length;
                if (intTemp > arrColWidth[j])
                {
                    arrColWidth[j] = intTemp;
                }
            }
        }

        var rowIndex = 0;

        foreach (DataRow row in dt.Rows)
        {
            #region 新建表，填充表头，填充列头，样式

            if (rowIndex == 0)
            {
                #region 表头及样式

                if (!string.IsNullOrEmpty(strHeaderText))
                {
                    IRow? headerRow = sheet.CreateRow(0);
                    headerRow.HeightInPoints = 25;
                    headerRow.CreateCell(0).SetCellValue(strHeaderText);

                    ICellStyle? headStyle = workbook.CreateCellStyle();
                    headStyle.Alignment = HorizontalAlignment.Center;
                    IFont? font = workbook.CreateFont();
                    font.FontHeightInPoints = 20;
                    font.IsBold = true;
                    headStyle.SetFont(font);

                    headerRow.GetCell(0).CellStyle = headStyle;

                    //sheet.AddMergedRegion(new Region(0, 0, 0, dtSource.Columns.Count - 1));
                    //headerRow.Dispose();
                }

                #endregion 表头及样式

                #region 列头及样式

                {
                    IRow? headerRow = sheet.CreateRow(rowIndex);

                    ICellStyle? headStyle = workbook.CreateCellStyle();
                    headStyle.Alignment = HorizontalAlignment.Center;
                    IFont? font = workbook.CreateFont();
                    font.FontHeightInPoints = 10;
                    font.IsBold = true;
                    headStyle.SetFont(font);

                    foreach (DataColumn column in dt.Columns)
                    {
                        headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                        headerRow.GetCell(column.Ordinal).CellStyle = headStyle;

                        //设置列宽
                        //sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);

                        if (arrColWidth[column.Ordinal] >= 255)
                        {
                            arrColWidth[column.Ordinal] = 255;
                        }
                        else
                        {
                            sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
                        }
                    }

                    //headerRow.Dispose();
                }

                #endregion 列头及样式

                rowIndex++;
            }

            #endregion 新建表，填充表头，填充列头，样式

            #region 填充内容

            IRow? dataRow = sheet.CreateRow(rowIndex);
            foreach (DataColumn column in dt.Columns)
            {
                ICell? newCell = dataRow.CreateCell(column.Ordinal);

                var drValue = row[column].ToString()!;

                switch (column.DataType.ToString())
                {
                    case "System.String": //字符串类型
                        if (drValue.IsDouble())
                        {
                            newCell.SetCellValue(drValue.ToDouble());
                            break;
                        }

                        newCell.SetCellValue(drValue);
                        break;

                    case "System.DateTime": //日期类型
                        var dateV = drValue.ToDateTime();
                        newCell.SetCellValue(dateV);

                        newCell.CellStyle = dateStyle; //格式化显示
                        break;

                    case "System.Boolean": //布尔型
                        var boolV = drValue.ToBool();
                        newCell.SetCellValue(boolV);
                        break;

                    case "System.Int16": //整型
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Byte":
                        var intV = drValue.ToInt();
                        newCell.SetCellValue(intV);
                        break;

                    case "System.Decimal": //浮点型
                    case "System.Double":
                        var doubV = drValue.ToDouble();
                        newCell.SetCellValue(doubV);
                        break;

                    case "System.DBNull": //空值处理
                        newCell.SetCellValue(string.Empty);
                        break;

                    default:
                        newCell.SetCellValue(string.Empty);
                        break;
                }
            }

            #endregion 填充内容

            rowIndex++;
        }

        return workbook;
    }

    #endregion 从datatable中将数据导出到excel

    #region 从excel中将数据导出到datatable

    /// <summary>
    ///     读取excel 默认第一行为标头
    /// </summary>
    /// <param name="strFileName">excel文档路径</param>
    /// <returns></returns>
    public static DataTable ImportExceltoDt(string strFileName)
    {
        IWorkbook wb;
        using (var file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
        {
            wb = WorkbookFactory.Create(file);
        }

        ISheet? sheet = wb.GetSheetAt(0);
        DataTable dt = ImportDt(sheet, 0);
        return dt;
    }

    /// <summary>
    ///     读取Excel流到DataTable
    /// </summary>
    /// <param name="stream">Excel流</param>
    /// <returns>第一个sheet中的数据</returns>
    public static DataTable ImportExceltoDt(Stream stream)
    {
        IWorkbook wb;
        using (stream)
        {
            wb = WorkbookFactory.Create(stream);
        }

        ISheet? sheet = wb.GetSheetAt(0);
        DataTable dt = ImportDt(sheet, 0);
        return dt;
    }

    /// <summary>
    ///     读取Excel流到DataTable
    /// </summary>
    /// <param name="stream">Excel流</param>
    /// <param name="sheetName">表单名</param>
    /// <param name="headerRowIndex">列头所在行号，-1表示没有列头</param>
    /// <returns>指定sheet中的数据</returns>
    public static DataTable ImportExceltoDt(Stream stream, string sheetName, int headerRowIndex)
    {
        IWorkbook wb;
        using (stream)
        {
            wb = WorkbookFactory.Create(stream);
        }

        ISheet? sheet = wb.GetSheet(sheetName);
        DataTable dt = ImportDt(sheet, headerRowIndex);
        return dt;
    }

    /// <summary>
    ///     读取Excel流到DataSet
    /// </summary>
    /// <param name="stream">Excel流</param>
    /// <returns>Excel中的数据</returns>
    public static DataSet ImportExceltoDs(Stream stream)
    {
        var ds = new DataSet();
        IWorkbook wb;
        using (stream)
        {
            wb = WorkbookFactory.Create(stream);
        }

        for (var i = 0; i < wb.NumberOfSheets; i++)
        {
            ISheet? sheet = wb.GetSheetAt(i);
            DataTable dt = ImportDt(sheet, 0);
            ds.Tables.Add(dt);
        }

        return ds;
    }

    /// <summary>
    ///     读取Excel流到DataSet
    /// </summary>
    /// <param name="stream">Excel流</param>
    /// <param name="dict">字典参数，key：sheet名，value：列头所在行号，-1表示没有列头</param>
    /// <returns>Excel中的数据</returns>
    public static DataSet ImportExceltoDs(Stream stream, Dictionary<string, int> dict)
    {
        var ds = new DataSet();
        IWorkbook wb;
        using (stream)
        {
            wb = WorkbookFactory.Create(stream);
        }

        foreach (var key in dict.Keys)
        {
            ISheet? sheet = wb.GetSheet(key);
            DataTable dt = ImportDt(sheet, dict[key]);
            ds.Tables.Add(dt);
        }

        return ds;
    }

    /// <summary>
    ///     读取excel
    /// </summary>
    /// <param name="strFileName">excel文件路径</param>
    /// <param name="sheetName">需要导出的sheet名称</param>
    /// <param name="headerRowIndex">列头所在行号，-1表示没有列头</param>
    /// <returns></returns>
    public static DataTable ImportExcelToDt(string strFileName, string sheetName, int headerRowIndex)
    {
        DataSet ds = ImportExcelToDs(strFileName, null, headerRowIndex);

        if (ds.Tables.Contains(sheetName))
        {
            throw new Exception($"Not Found the {sheetName} in {nameof(ds)}");
        }

        DataTable dt = ds.Tables[sheetName]!;
        return dt;
    }

    /// <summary>
    ///     读取excel
    /// </summary>
    /// <param name="strFileName">excel文件路径</param>
    /// <param name="sheetIndex">需要导出的sheet序号</param>
    /// <param name="headerRowIndex">列头所在行号，-1表示没有列头</param>
    /// <returns></returns>
    public static DataTable ImportExceltoDt(string strFileName, int sheetIndex, int headerRowIndex)
    {
        IWorkbook wb;
        using (var file = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            wb = WorkbookFactory.Create(file);
        }

        ISheet? sheet = wb.GetSheetAt(sheetIndex);
        DataTable table = ImportDt(sheet, headerRowIndex);
        return table;
    }

    /// <summary>
    ///     读取excel
    /// </summary>
    /// <param name="strFileName">excel文件路径</param>
    /// <param name="strSheetNames">需要导出的sheet名称,全部导出设为null</param>
    /// <param name="headerRowIndex">列头所在行号，-1表示没有列头</param>
    /// <returns></returns>
    public static DataSet ImportExcelToDs(string strFileName, string? strSheetNames, int headerRowIndex)
    {
        IWorkbook wb;
        using (var file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
        {
            wb = WorkbookFactory.Create(file);
        }

        var dataSet = new DataSet();

        if (strSheetNames == null)
        {
            for (var i = 0; i < wb.NumberOfSheets; i++)
            {
                ISheet? sheet = wb.GetSheetAt(i);
                DataTable table = ImportDt(sheet, headerRowIndex);
                table.TableName = wb.GetSheetName(i);
                dataSet.Tables.Add(table);
            }
        }
        else
        {
            var sheetNames = strSheetNames.Split(',');
            for (var i = 0; i < wb.NumberOfSheets; i++)
            {
                var sheetName = wb.GetSheetName(i).Trim();
                if (Array.IndexOf(sheetNames, sheetName) >= 0)
                {
                    ISheet? sheet = wb.GetSheetAt(i);
                    DataTable table = ImportDt(sheet, headerRowIndex);
                    table.TableName = wb.GetSheetName(i);
                    dataSet.Tables.Add(table);
                }
            }
        }

        return dataSet;
    }

    /// <summary>
    ///     将制定sheet中的数据导出到datatable中
    /// </summary>
    /// <param name="sheet">需要导出的sheet</param>
    /// <param name="headerRowIndex">列名所在行号，-1表示没有列名,0表示第一行</param>
    /// <returns></returns>
    private static DataTable ImportDt(ISheet sheet, int headerRowIndex)
    {
        var table = new DataTable();
        int cellCount;
        if (headerRowIndex < 0) //没有列名或不需要Header
        {
            //headerRow = sheet.GetRow(0);
            //cellCount = headerRow.LastCellNum;
            cellCount = 0;
            for (var rowCnt = sheet.FirstRowNum; rowCnt <= sheet.LastRowNum; rowCnt++) //迭代所有行
            {
                IRow? row = sheet.GetRow(rowCnt);
                if (row != null && row.LastCellNum > cellCount)
                {
                    cellCount = row.LastCellNum;
                }
            }

            for (var i = 0; i <= cellCount; i++)
            {
                var column = new DataColumn(Convert.ToString("Column" + i));
                table.Columns.Add(column);
            }
        }
        else
        {
            IRow? headerRow = sheet.GetRow(headerRowIndex);
            cellCount = headerRow.LastCellNum;

            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                if (headerRow.GetCell(i) == null)
                {
                    if (table.Columns.IndexOf(Convert.ToString(i)) > 0)
                    {
                        var column = new DataColumn(Convert.ToString("重复列名" + i));
                        table.Columns.Add(column);
                    }
                    else
                    {
                        var column = new DataColumn(Convert.ToString(i));
                        table.Columns.Add(column);
                    }
                }
                else if (table.Columns.IndexOf(headerRow.GetCell(i).ToString()) > 0)
                {
                    var column = new DataColumn(Convert.ToString("重复列名" + i));
                    table.Columns.Add(column);
                }
                else
                {
                    var column = new DataColumn(headerRow.GetCell(i).ToString());
                    table.Columns.Add(column);
                }
            }
        }

        for (var i = headerRowIndex + 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i) ?? sheet.CreateRow(i);

            if (row.FirstCellNum >= 0)
            {
                DataRow dataRow = table.NewRow();

                for (int j = row.FirstCellNum; j <= cellCount; j++)
                {
                    ICell? cell = row.GetCell(j);
                    if (cell != null)
                    {
                        switch (cell.CellType)
                        {
                            case CellType.String:
                                var str = cell.StringCellValue;
                                if (!string.IsNullOrEmpty(str))
                                {
                                    dataRow[j] = str;
                                }
                                else
                                {
                                    dataRow[j] = null;
                                }

                                break;

                            case CellType.Numeric:
                                if (DateUtil.IsCellDateFormatted(cell))
                                {
                                    dataRow[j] = DateTime.FromOADate(cell.NumericCellValue);
                                }
                                else
                                {
                                    dataRow[j] = Convert.ToDouble(cell.NumericCellValue);
                                }

                                break;

                            case CellType.Boolean:
                                dataRow[j] = Convert.ToString(cell.BooleanCellValue);
                                break;

                            case CellType.Error:
                                dataRow[j] = ErrorEval.GetText(cell.ErrorCellValue);
                                break;

                            case CellType.Formula:
                                switch (cell.CachedFormulaResultType)
                                {
                                    case CellType.String:
                                        var strFormula = cell.StringCellValue;
                                        if (!string.IsNullOrEmpty(strFormula))
                                        {
                                            dataRow[j] = strFormula;
                                        }
                                        else
                                        {
                                            dataRow[j] = null;
                                        }

                                        break;

                                    case CellType.Numeric:
                                        dataRow[j] = Convert.ToString(cell.NumericCellValue, CultureInfo.CurrentCulture);
                                        break;

                                    case CellType.Boolean:
                                        dataRow[j] = Convert.ToString(cell.BooleanCellValue);
                                        break;

                                    case CellType.Error:
                                        dataRow[j] = ErrorEval.GetText(cell.ErrorCellValue);
                                        break;

                                    default:
                                        dataRow[j] = string.Empty;
                                        break;
                                }

                                break;
                            //case CellType.Blank:
                            //    dataRow[j] = null;
                            //    break;
                            default:
                                dataRow[j] = string.Empty;
                                break;
                        }
                    }
                }

                table.Rows.Add(dataRow);
            }
        }

        table.TableName = sheet.SheetName;
        return table;
    }

    #endregion 从excel中将数据导出到datatable

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
        var clonePrintSetup = true;
        return sSheet.CrossCloneSheet(dWb, dSheetName, clonePrintSetup);
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