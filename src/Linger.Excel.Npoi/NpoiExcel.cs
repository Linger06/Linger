using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using Linger.Excel.Contracts;
using Linger.Extensions.Core;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Linger.Excel.Npoi;
public class NpoiExcel : ExcelBase
{
    public override MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "Sheet1", string title = "", Action<object, PropertyInfo[]>? action = null) where T : class
    {
        if (list.IsNull())
        {
            return null;
        }

        var dataTable = new DataTable();
        var properties = typeof(T).GetProperties();

        // Add columns
        foreach (var prop in properties)
        {
            dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }

        // Add rows
        foreach (var item in list)
        {
            var row = dataTable.NewRow();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                row[prop.Name] = value ?? DBNull.Value;
            }
            dataTable.Rows.Add(row);
        }

        var memoryStream = new MemoryStream();
        var workbook = new XSSFWorkbook();
        workbook = GetWorkbookI(workbook, dataTable, sheetsName, title);
        workbook.Write(memoryStream);
        memoryStream.Flush();
        memoryStream.Position = 0;
        return memoryStream;
    }

    public override MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "Sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        if (dataTable.IsNull() //|| dataTable.Rows.Count <= 0 //导出Excel时,如果DataTable只是没有数据,有结构,应该输出列名
)
        {
            return null;
        }

        var memoryStream = new MemoryStream();
        var workbook = new XSSFWorkbook();
        workbook = GetWorkbookI(workbook, dataTable, sheetsName, title);

        workbook.Write(memoryStream);
        //memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public override DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        IWorkbook wb;
        using (stream)
        {
            wb = WorkbookFactory.Create(stream);
        }

        ISheet? sheet = sheetName.IsNullOrWhiteSpace() ? wb.GetSheetAt(0) : wb.GetSheet(sheetName);
        DataTable dt = ImportDt(sheet, headerRowIndex);
        return dt;
    }

    public override List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int headerRowIndex = 0, bool addEmptyRow = false)
    {
        if (stream is not { CanRead: true } || stream.Length <= 0)
        {
            return null;
        }

        try
        {
            // First convert stream to DataTable
            var dataTable = ConvertStreamToDataTable(stream, sheetName, headerRowIndex, addEmptyRow);
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return null;
            }

            var list = new List<T>();
            var properties = typeof(T).GetProperties();

            // Create a map of property names to column indices
            var propertyMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn column in dataTable.Columns)
            {
                propertyMap[column.ColumnName] = column.Ordinal;
            }

            // Convert each row to object
            foreach (DataRow row in dataTable.Rows)
            {
                var item = new T();
                foreach (var prop in properties)
                {
                    if (propertyMap.TryGetValue(prop.Name, out int colIndex))
                    {
                        var value = row[colIndex];
                        if (value != DBNull.Value)
                        {
                            try
                            {
                                // Handle type conversion
                                if (prop.PropertyType == typeof(string))
                                {
                                    prop.SetValue(item, value.ToString());
                                }
                                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                                {
                                    if (value is DateTime dt)
                                    {
                                        prop.SetValue(item, dt);
                                    }
                                    else if (DateTime.TryParse(value.ToString(), out dt))
                                    {
                                        prop.SetValue(item, dt);
                                    }
                                }
                                else
                                {
                                    prop.SetValue(item, Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
                                }
                            }
                            catch
                            {
                                // Ignore conversion errors and continue with next property
                                continue;
                            }
                        }
                    }
                }
                list.Add(item);
            }

            return list;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="workbook"></param>
    /// <param name="dt"></param>
    /// <param name="sheetName"></param>
    /// <param name="headerText">表头内容</param>
    /// <param name="columnsName">使用自定义列名</param>
    /// <returns></returns>
    public static HSSFWorkbook GetWorkbook(HSSFWorkbook workbook, DataTable dt, string? sheetName = null, string? headerText = null, string? columnsName = null)
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
            arrColWidth[item.Ordinal] = Encoding.Default.GetBytes(item.ColumnName).Length;
        }

        for (var i = 0; i < dt.Rows.Count; i++)
        {
            for (var j = 0; j < dt.Columns.Count; j++)
            {
                var cellValue = dt.Rows[i][j].ToString()!;
                var intTemp = Encoding.Default.GetBytes(cellValue).Length;
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
                                    var cellValue = cell.NumericCellValue;
                                    if (cellValue.ToString().IsScientificNotation())
                                    {
                                        dataRow[j] = cellValue.ToString().ChangeToDecimal();
                                    }
                                    else
                                    {
                                        dataRow[j] = cellValue;
                                    }
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

    private static XSSFWorkbook GetWorkbookI(XSSFWorkbook workbook, DataTable dt, string? sheetName = null, string? headerText = null)
    {
        ISheet? sheet;
        if (sheetName.IsNotNullAndEmpty())
        {
            sheet = workbook.CreateSheet(sheetName);
        }
        else
        {
            sheet = workbook.CreateSheet();
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
        dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

        //取得列宽
        var arrColWidth = new int[dt.Columns.Count];
        foreach (DataColumn item in dt.Columns)
        {
            arrColWidth[item.Ordinal] = Encoding.Default.GetBytes(item.ColumnName).Length;
        }

        for (var i = 0; i < dt.Rows.Count; i++)
        {
            for (var j = 0; j < dt.Columns.Count; j++)
            {
                var intTemp = Encoding.Default.GetBytes(dt.Rows[i][j].ToString()!).Length;
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
}
