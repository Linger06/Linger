using System.Data;
using System.Reflection;
using OfficeOpenXml;

namespace Linger.Excel;

public interface IExcel
{
    ExcelWorksheet? WriteDataTableToSheet(DataTable dataTable, ExcelWorksheet worksheet,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string title = "");

    MemoryStream? WriteToMemoryStream(DataTable dataTable,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>? action = null, string sheetsName = "sheet1",
        string title = "");

    Task<MemoryStream?> WriteToMemoryStreamAsync(DataTable dataTable,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection> action, string sheetsName = "sheet1");

    #region 导入

    DataSet? ReadExcelToDataSet(string filePath, string? sheetsName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false);

    DataTable? ReadExcelToDataTable(string filePath, string? sheetName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false);

    Task<DataTable?> ReadExcelToDataTableAsync(string filePath, string? sheetName = null,
        bool firstRowIsColumnName = true, bool addEmptyRow = false);

    ICollection<DataTable>? ReadExcelToTables(string filePath, bool firstRowIsColumnName = true,
        bool addEmptyRow = false);

    Task<ICollection<DataTable>?> ReadExcelToTablesAsync(string filePath, bool firstRowIsColumnName = true,
        bool addEmptyRow = false);


    DataTable? ReadStreamToDataTable(Stream stream, string? sheetName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true);

    Task<DataTable?> ReadStreamToDataTableAsync(Stream stream, string? sheetName = null,
        bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true);


    ICollection<DataTable>? ReadStreamToTables(Stream stream, bool firstRowIsColumnName = true,
        bool addEmptyRow = false,
        bool dispose = true);

    Task<ICollection<DataTable>?> ReadStreamToTablesAsync(Stream stream, bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true);


    List<T>? ReadExcelToList<T>(string filePath, string? sheetName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false) where T : new();

    List<T>? ReadSheetToList<T>(ExcelWorksheet worksheet, bool firstRowIsColumnName = true) where T : new();

    List<T>? ReadStreamToList<T>(Stream stream, string? sheetName = null, bool firstRowIsColumnName = true,
        bool addEmptyRow = false, bool dispose = true) where T : new();

    #endregion

    #region 导出

    string ExportToFile(DataSet dataSet, string fullFileName,
        List<Action<ExcelWorksheet, DataColumnCollection, DataRowCollection>>? actions = null,
        string? sheetsName = null);

    string ExportToFile(DataTable dataTable, string fullFileName,
        Action<ExcelWorksheet, DataColumnCollection, DataRowCollection> action, string sheetsName = "sheet1");


    ExcelWorksheet? WriteListToSheet<T>(List<T> list, ExcelWorksheet worksheet,
        Action<ExcelWorksheet, PropertyInfo[]>? action = null, string title = "");

    MemoryStream? WriteToMemoryStream<T>(List<T> list, Action<ExcelWorksheet, PropertyInfo[]> action,
        string sheetsName = "sheet1", string title = "");

    Task<MemoryStream?> WriteToMemoryStreamAsync<T>(List<T> list, Action<ExcelWorksheet, PropertyInfo[]>? action = null,
        string sheetsName = "sheet1", string title = "");

    #endregion
}