using System.Data;

namespace Linger.Excel.Contracts;

public interface IExcel
{
    #region 导入

    DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false);
    DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true);
    List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false) where T : new();
    List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true) where T : new();

    #endregion

    #region 导出

    string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "sheet1");
    MemoryStream? ConvertCollectionToMemoryStream<T>(List<T> list, string sheetsName = "sheet1", string title = "");
    MemoryStream? ConvertDataTableToMemoryStream(DataTable dataTable, string sheetsName = "sheet1", string title = "");
    #endregion
}