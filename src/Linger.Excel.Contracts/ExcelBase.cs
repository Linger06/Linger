using System.Data;
using System.Reflection;
using Linger.Extensions.Core;

namespace Linger.Excel.Contracts;

public abstract class ExcelBase : IExcel 
{
    public static string ContentType => "application/vnd.ms-excel";
    public DataTable? ExcelToDataTable(string filePath, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false)
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return ConvertStreamToDataTable(fileStream, sheetName, columnNameRowIndex, addEmptyRow);
    }

    public List<T>? ExcelToList<T>(string filePath, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false) where T : new()
    {
        if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
        {
            return null;
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return ConvertStreamToList<T>(fileStream, sheetName, columnNameRowIndex, addEmptyRow);
    }

    public string DataTableToFile(DataTable dataTable, string fullFileName, string sheetsName = "sheet1", string title = "", Action<object, DataColumnCollection, DataRowCollection>? action = null)
    {
        using var ms = ConvertDataTableToMemoryStream(dataTable, sheetsName, title, action);
        if (ms == null)
        {
            throw new NullReferenceException(nameof(ms));
        }

        using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
        var data = ms.ToArray();
        fs.Write(data, 0, data.Length);
        fs.Flush();

        return fullFileName;
    }

    public string ListToFile<T>(List<T> list, string fullFileName, string sheetsName = "sheet1", string title = "", Action<object, PropertyInfo[]>? action = null) where T : class
    {
        using var ms = ConvertCollectionToMemoryStream(list, sheetsName, title, action);
        if (ms == null)
        {
            throw new NullReferenceException(nameof(ms));
        }

        using var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
        var data = ms.ToArray();
        fs.Write(data, 0, data.Length);
        fs.Flush();

        return fullFileName;
    }

    public abstract DataTable? ConvertStreamToDataTable(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true);
    public abstract List<T>? ConvertStreamToList<T>(Stream stream, string? sheetName = null, int columnNameRowIndex = 0, bool addEmptyRow = false, bool dispose = true) where T : new();
        public abstract MemoryStream? ConvertDataTableToMemoryStream(
        DataTable dataTable,
        string sheetsName = "sheet1",
        string title = "",
        Action<object, DataColumnCollection, DataRowCollection>? action = null);

    public abstract MemoryStream? ConvertCollectionToMemoryStream<T>(
        List<T> list,
        string sheetsName = "sheet1",
        string title = "",
        Action<object, PropertyInfo[]>? action = null) where T : class;
}